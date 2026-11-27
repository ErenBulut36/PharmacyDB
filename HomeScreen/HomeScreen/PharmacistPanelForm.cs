using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient; // SQL Library
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class PharmacistPanelForm : Form
    {
        // Database Connection String
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        // Store the current prescription ID in memory
        int currentPrescriptionId = 0;

        // I'm storing the patient's ID here to check their past medication history.
        int currentPatientId = 0;

        public PharmacistPanelForm()
        {
            InitializeComponent();
            // Reset form on startup
            ResetForm();

            // I'm clearing the previous settings to prevent column conflicts.
            dataGridView1.Columns.Clear();

            // FIX: This ensures columns are created automatically from SQL query
            dataGridView1.AutoGenerateColumns = true;

            // I'm linking the double-click event to remove medication.
            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
        }

        // HELPER METHOD: RESET FORM
        private void ResetForm()
        {
            currentPrescriptionId = 0;
            currentPatientId = 0; // I am resetting the patient's information.
            textBox1.Clear(); // Barcode box clear
            label1.Text = "Total Price: 0.00 TL";
            label2.Text = "Patient: -";
            label2.ForeColor = Color.Black;
            dataGridView1.DataSource = null;
            button2.Enabled = false; // Disable sale button if no prescription
            textBox1.Focus();
        }

        // BUTTON 1: SEARCH PRESCRIPTION (BY BARCODE)
        private void button1_Click(object sender, EventArgs e)
        {
            string barcodeInput = textBox1.Text.Trim();

            // I give a warning if a barcode hasn't been entered.
            if (string.IsNullOrEmpty(barcodeInput))
            {
                MessageBox.Show("Please enter a barcode!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // STEP 1: I'm retrieving the prescription title and patient information.
                    string queryPrescription = @"
                        SELECT P.PrescriptionID, P.IsSold, PT.PatientName, PT.PatientID 
                        FROM Prescriptions P
                        JOIN Patients PT ON P.PatientID = PT.PatientID
                        WHERE P.Barcode = @barcode";

                    using (SqlCommand cmd = new SqlCommand(queryPrescription, conn))
                    {
                        cmd.Parameters.AddWithValue("@barcode", barcodeInput);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Prescription found, I'm saving the IDs to memory.
                                currentPrescriptionId = Convert.ToInt32(reader["PrescriptionID"]);
                                currentPatientId = Convert.ToInt32(reader["PatientID"]);

                                string patientName = reader["PatientName"].ToString();

                                // I'm checking if the prescription has been sold before.
                                bool isSold = reader["IsSold"] != DBNull.Value && Convert.ToBoolean(reader["IsSold"]);

                                label2.Text = "Patient: " + patientName;

                                // If it's sold, I give a warning and (((lock the button))).
                                if (isSold)
                                {
                                    label2.Text += " (ALREADY DISPENSED)";
                                    label2.ForeColor = Color.Red;
                                    button2.Enabled = false; // Lock sale button
                                    MessageBox.Show("WARNING: This prescription has already been used!", "System Alert", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                }
                                else
                                {
                                    label2.ForeColor = Color.Black;
                                    button2.Enabled = true; //I authorize the sale.
                                }
                            }
                            else
                            {
                                MessageBox.Show("Prescription not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ResetForm();
                                return;
                            }
                        }
                    }

                    // STEP 2: List Medicines and Calculate Price
                    // I'm also retrieving the unique (((DetailID))) of each row for the deletion process.
                    string queryDetails = @"
                        SELECT 
                            PD.DetailID,   
                            PD.MedicineName, 
                            M.Category, 
                            M.StockQuantity,    
                            M.Price,
                            M.UsageDays,        
                            M.DosageInfo        
                        FROM PrescriptionDetails PD
                        JOIN PharmacyData M ON PD.MedicineName = M.MedicineName
                        WHERE PD.PrescriptionID = @id";


                    SqlDataAdapter da = new SqlDataAdapter(queryDetails, conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", currentPrescriptionId);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Reset and re-issue the grid source
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = dt;

                    // I'm hiding DetailID because it's technical data that the user doesn't need to see.
                    if (dataGridView1.Columns.Contains("DetailID"))
                    {
                        dataGridView1.Columns["DetailID"].Visible = false;
                    }

                    // I'm editing the table's appearance.
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    // Calculate Total Price
                    CalculateAndShowPrice();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Search Error: " + ex.Message);
            }
        }

        // Pricing Method
        private void CalculateAndShowPrice()
        {
            if (dataGridView1.DataSource is DataTable dt)
            {
                decimal totalPrice = 0;
                foreach (DataRow row in dt.Rows)
                {
                    // I'm not counting deleted lines.
                    if (row.RowState != DataRowState.Deleted && row["Price"] != DBNull.Value)
                    {
                        totalPrice += Convert.ToDecimal(row["Price"]);
                    }
                }
                label1.Text = $"Total Price: {totalPrice:N2} TL";
            }
        }

        // Double-click deletion process.
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    string medName = dataGridView1.Rows[e.RowIndex].Cells["MedicineName"].Value.ToString();

                    // I'm getting a (((unique ID))) so that other medications with the same name aren't deleted.
                    var idValue = dataGridView1.Rows[e.RowIndex].Cells["DetailID"].Value;

                    if (idValue == null || idValue == DBNull.Value) return;

                    int uniqueDetailId = Convert.ToInt32(idValue);

                    // I'm requesting confirmation from the user.
                    DialogResult result = MessageBox.Show(
                        $"Do you want to remove '{medName}' from the prescription list?\n(Only THIS specific box will be deleted)",
                        "Remove Medicine",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            // I'm only deleting the selected row based on DetailID. (((uniqueID))
                            string deleteQuery = "DELETE FROM PrescriptionDetails WHERE DetailID = @uniqueId";

                            using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@uniqueId", uniqueDetailId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // I'm triggering the search button again to update the list.
                        button1_Click(sender, e);

                        MessageBox.Show("Medicine removed from prescription.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing medicine: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // BUTTON 2: COMPLETE SALE  
        private void button2_Click(object sender, EventArgs e)
        {
            // First, I check if a prescription has been searched.
            if (currentPrescriptionId == 0)
            {
                MessageBox.Show("Please search for a prescription first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //STAGE 1: TIME CONTROL
                    // With this query, I'm checking if the patient still has any medication that hasn't expired.
                    //I'm blocking the sale if it exists.

                    string checkQuery = @"
                        SELECT TOP 1 PD.MedicineName
                        FROM PrescriptionDetails PD
                        INNER JOIN Prescriptions P ON PD.PrescriptionID = P.PrescriptionID
                        INNER JOIN PharmacyData Ph ON PD.MedicineName = Ph.MedicineName
                        WHERE P.PatientID = @patID 
                          AND P.IsSold = 1 
                          AND PD.MedicineName IN (
                              SELECT MedicineName FROM PrescriptionDetails WHERE PrescriptionID = @currPrescID
                          )
                          AND DATEADD(day, ISNULL(Ph.UsageDays, 0), P.PrescriptionDate) > GETDATE()";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@patID", currentPatientId);
                        checkCmd.Parameters.AddWithValue("@currPrescID", currentPrescriptionId);

                        object duplicateMedicine = checkCmd.ExecuteScalar();

                        // If the query returns a drug name, that means there's been a rule violation.
                        if (duplicateMedicine != null)
                        {
                            // If the query returns a drug name, it means it hasn't expired.
                            MessageBox.Show($"SYSTEM ALERT (Medula Rule):\n\nThe patient has an active supply of '{duplicateMedicine.ToString()}'.\nUsage period has not expired yet.\n\nSale cancelled.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);

                            // I'm canceling the transaction and exiting.
                            return;
                        }
                    }



                    //2. SALES STAGE: Normal sale if the control is passed.
                    // If the checks are passed, I reduce the stock by calling the Stored Procedure.
                    SqlCommand cmd = new SqlCommand("sp_ProcessSale", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrescriptionID", currentPrescriptionId);

                    string result = cmd.ExecuteScalar()?.ToString();

                    if (result == "SUCCESS")
                    {
                        MessageBox.Show("Sale completed successfully! Stock updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Failed: " + result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("System Error: " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Back Form1
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Back PharmacistMainForm
            PharmacistMainForm mainForm = new PharmacistMainForm();
            mainForm.Show();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
    }
}