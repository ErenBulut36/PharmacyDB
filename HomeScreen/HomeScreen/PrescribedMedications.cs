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
    public partial class PrescribedMedications : Form
    {
        // Database Connection String
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public PrescribedMedications()
        {
            InitializeComponent();

            // Load the list immediately when form opens
            LoadPrescriptionHistory("");
        }

        // METHOD: Load List Fetches Hidden ID + Display Data
        private void LoadPrescriptionHistory(string searchTerm)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();


                    // We need 'PrescriptionID' to update the specific row accurately.
                    // To display the patient's name, Turkish Republic Identity Number, and medication information on a single line
                    // I combined 3 different tables (Prescriptions, Patients, PrescriptionDetails) using a JOIN operation.
                    string query = @"
                        SELECT 
                            PR.PrescriptionID,             
                            P.PatientTC AS 'TC Identity',
                            P.PatientName AS 'Patient Name',
                            PD.MedicineName AS 'Medicine',
                            PR.Barcode AS 'Barcode',
                            PR.IsSold AS 'Sold Status',    
                            PR.PrescriptionDate AS 'Date Issued'
                        FROM Prescriptions PR
                        INNER JOIN Patients P ON PR.PatientID = P.PatientID
                        INNER JOIN PrescriptionDetails PD ON PR.PrescriptionID = PD.PrescriptionID
                        WHERE 
                            P.PatientName LIKE @search OR
                            P.PatientTC LIKE @search       
                        ORDER BY PR.PrescriptionDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    // I'm adding the search parameter.
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + searchTerm + "%");

                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                    // Hide the ID column so the user doesn't get confused
                    if (dataGridView1.Columns["PrescriptionID"] != null)
                    {
                        dataGridView1.Columns["PrescriptionID"].Visible = false;
                    }

                    // Automatically adjust column widths.
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading list: " + ex.Message);
            }
        }

        
        private void textBox1_TextChanged(object sender, EventArgs e) { }

        // BUTTON 1: BACK
        private void button1_Click(object sender, EventArgs e)
        {
            // I'm returning to the admin panel.
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();
            this.Close();
        }

        // BUTTON 2: SEARCH
        private void button2_Click(object sender, EventArgs e)
        {
            // I'm filtering the list based on the ID in the textbox.
            LoadPrescriptionHistory(textBox1.Text.Trim());
        }

        // BUTTON 3: UPDATE STATUS
        // Checks the database first, then toggles the status.
        private void button3_Click(object sender, EventArgs e)
        {
            // 1. Selection Check: Has a row been selected from the list?
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Index == -1)
            {
                MessageBox.Show("Please select a row from the list.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 2. Getting the ID: I'm retrieving the ID and barcode from the hidden column.
                int prescriptionID = Convert.ToInt32(dataGridView1.CurrentRow.Cells["PrescriptionID"].Value);
                string barcode = dataGridView1.CurrentRow.Cells["Barcode"].Value.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // STEP 2: The data on the grid might be outdated. I'm querying the database for the most current data.
                    string checkQuery = "SELECT IsSold FROM Prescriptions WHERE PrescriptionID = @id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@id", prescriptionID);

                    object result = checkCmd.ExecuteScalar();

                    // If DB is NULL or 0 False. If 1 True.
                    bool currentStatus = (result != DBNull.Value) && Convert.ToBoolean(result);

                    // STEP 3: (((Toggle the status)))
                    bool newStatus = !currentStatus;

                    // User Confirmation Message
                    string actionText = newStatus ? "MARK AS SOLD (1)" : "RESET / UNSOLD (0)";

                    DialogResult dialogResult = MessageBox.Show(
                        $"Prescription ID: {prescriptionID}\n" +
                        $"Current Status: {currentStatus}\n" +
                        $"New Action: {actionText}\n\n" +
                        "Do you confirm this change?",
                        "Confirm Status Update",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (dialogResult == DialogResult.Yes)
                    {
                        // STEP 4: Update
                        string updateQuery = "UPDATE Prescriptions SET IsSold = @status WHERE PrescriptionID = @id";
                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);

                        // Send 1 or 0 to SQL
                        updateCmd.Parameters.AddWithValue("@status", newStatus ? 1 : 0);
                        updateCmd.Parameters.AddWithValue("@id", prescriptionID);

                        int rows = updateCmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Operation Successful! Status updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Refresh the list to show changes
                            LoadPrescriptionHistory(textBox1.Text.Trim());
                        }
                        else
                        {
                            MessageBox.Show("Error: Could not update the record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
            }
        }

        // Unused Events
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
    }
}