using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class DoctorPanelForm : Form
    {
        // Connection string
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public DoctorPanelForm()
        {
            InitializeComponent();
            // When the form opens, I retrieve the drug categories from the database and load them into the list.
            LoadCategories();

            // To avoid an empty list at startup, I'm listing all the medications.
            SearchMedicines();

            // I'm configuring the settings for the second grid (basket) where the medications to be added to the prescription will be stored.
            SetupPrescriptionGrid();
        }

        // DataGridView2 Settings
        private void SetupPrescriptionGrid()
        {
            // I'm clearing the previous columns and adding only the ((Drug Name)) and ((Duration)) of Use columns.
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("MedicineName", "Medicine Name");
            dataGridView2.Columns.Add("UsageDays", "Usage Days"); 
            dataGridView2.ReadOnly = true;

            // I'm binding the double-click event to remove the incorrectly added medication.
            this.dataGridView2.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView2_CellDoubleClick);
        }

        private void DoctorPanelForm_Load(object sender, EventArgs e) { }

        // My method adds an "Add" button to the grid on the left where the search results are listed.
        private void AddCheckBoxColumn()
        {
            // I'm adding the button column if it hasn't been added before.
            if (!dataGridView1.Columns.Contains("Select"))
            {
                DataGridViewButtonColumn btnAdd = new DataGridViewButtonColumn();
                btnAdd.HeaderText = "Add";
                btnAdd.Text = "Add +";   //The button will say "Add +".
                btnAdd.Name = "Select";
                btnAdd.UseColumnTextForButtonValue = true;
                btnAdd.Width = 60;

                // I'm putting the button at the very beginning (index 0) so the doctor can easily see it.
                // Index 0 categories index eg. index 0 all categories
                dataGridView1.Columns.Insert(0, btnAdd);
            }
        }

        // I'm retrieving drug categories (Syrup, Tablet, etc.) from the database and filling them into a filtering box (Combobox).
        private void LoadCategories()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // I'm clearing the list and adding "All Categories" to the very beginning.
                    conn.Open();
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("All Categories");

                    // I'm retrieving unique (distinct) category names from the database.
                    string query = "SELECT DISTINCT Category FROM PharmacyData WHERE Category IS NOT NULL ORDER BY Category";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["Category"].ToString());
                    }
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        // My main function that performs drug search and filtering operations.
        private void SearchMedicines()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //I'm preparing an SQL query that can search by both ((drug name)) and ((ATCCode)).
                    // search medicines name

                    string query = @"SELECT MedicineName, Category, ATCCode, ActiveIngredient, Manufacturer, UsageDays, DosageInfo 
                                     FROM PharmacyData 
                                     WHERE (MedicineName LIKE @search OR ATCCode LIKE @search)";

                    // If the doctor has selected a specific category, I add the category filter to the query.
                    if (comboBox1.SelectedIndex > 0)
                    {
                        query += " AND Category = @category";
                    }

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    // I'm adding the text typed in the search box as a parameter. The (% sign is for partial match).
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + textBox3.Text + "%");

                    if (comboBox1.SelectedIndex > 0)
                    {
                        da.SelectCommand.Parameters.AddWithValue("@category", comboBox1.SelectedItem.ToString());
                    }

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // I'm binding the results to the following grid (dataGridView1).
                    // connection result dt1 to dt2
                    dataGridView1.DataSource = dt;

                    // I'm checking to ensure the "Add" button doesn't disappear or become duplicated after the data is loaded.
                    if (dataGridView1.Columns.Contains("Select")) dataGridView1.Columns.Remove("Select");
                    AddCheckBoxColumn();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading medicines: " + ex.Message);
            }
        }

        // A helpful method to prevent code repetition.
        private void LoadMedicines() { SearchMedicines(); }

        // I return to the main screen when I press the logout button.
        private void button1_Click(object sender, EventArgs e)
        {
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }

        // Event that will run when the "Add +" button is clicked in the list in datagrid1.
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // I'm checking if the click occurred on a valid row and on the "Select" button.
            if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex].Name == "Select")
            {
                // If the data is valid, I'm adding the medication to the prescription list below (dataGridView2).
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                string medName = row.Cells["MedicineName"].Value?.ToString();
                string usageDaysStr = row.Cells["UsageDays"].Value?.ToString();

                if (!string.IsNullOrEmpty(medName))
                {
                    // I'm adding this directly for now; I'll do the main check on the "Generate Prescription" button.
                    dataGridView2.Rows.Add(medName, usageDaysStr);
                }
            }
        }

        // I'm enabling the removal of a drug from the list below when it's double-clicked.
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView2.Rows.RemoveAt(e.RowIndex);
            }
        }

        
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        // I reset the search when the search button is pressed.
        private void button2_Click(object sender, EventArgs e) { SearchMedicines(); }

        // I automatically apply filtering when the category changes.
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { SearchMedicines(); }

        // PRESCRIPTION CREATION BUTTON
        private void button3_Click(object sender, EventArgs e)
        {
            // 1. ACCESS CONTROLS
            string patientName = textBox1.Text.Trim();
            string patientTC = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(patientName) || string.IsNullOrEmpty(patientTC))
            {
                // I'm checking if the Name and Turkish ID number fields are empty.
                MessageBox.Show("Please enter patient name and ID number!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I'm checking if there are numbers in the name field.
            foreach (char c in patientName)
            {
                if (char.IsDigit(c))
                {
                    MessageBox.Show("Name cannot contain numbers!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Is the TC a digital system?
            foreach (char c in patientTC)
            {
                if (!char.IsDigit(c))
                {
                    MessageBox.Show("TC ID must be numeric!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // I'm checking if the medication is on the prescription list. A blank prescription cannot be created.
            if (dataGridView2.Rows.Count == 0 || (dataGridView2.Rows.Count == 1 && dataGridView2.Rows[0].IsNewRow))
            {
                MessageBox.Show("Prescription list is empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I'm generating an 8-digit random and unique barcode for the prescription.
            string barcode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
            int patientId = 0;
            int prescriptionId = 0; // I will store the ID of the new recipe here.

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 2. PATIENT OPERATIONS 
                    // I am searching for the patient in the database by calling the procedure called 'sp_GetOrCreatePatient'.
                    // If it doesn't exist, it creates it; if it does exist, it retrieves its ID.
                    using (SqlCommand cmdPatient = new SqlCommand("sp_GetOrCreatePatient", conn))
                    {
                        cmdPatient.CommandType = CommandType.StoredProcedure;
                        cmdPatient.Parameters.AddWithValue("@PatientTC", patientTC);
                        cmdPatient.Parameters.AddWithValue("@PatientName", patientName);

                        // I'm defining an output parameter to capture the ((PatientID)) that will be returned from the procedure.
                        SqlParameter outParam = new SqlParameter("@PatientID", SqlDbType.Int);
                        outParam.Direction = ParameterDirection.Output;
                        cmdPatient.Parameters.Add(outParam);

                        cmdPatient.ExecuteNonQuery();

                        if (outParam.Value != DBNull.Value) patientId = Convert.ToInt32(outParam.Value);
                    }

                    if (patientId == 0) return;

                    // I check if the patient's medication has expired so they can take it again.
                    HashSet<string> uniqueMedsToCheck = new HashSet<string>();
                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        if (!row.IsNewRow) uniqueMedsToCheck.Add(row.Cells["MedicineName"].Value.ToString());
                    }

                    foreach (string medicineName in uniqueMedsToCheck)
                    {
                        // I am requesting the LAST prescription date and duration of use for this patient.
                        string checkQuery = @"
                            SELECT TOP 1 P.PrescriptionID, P.PrescriptionDate, M.UsageDays
                            FROM Prescriptions P
                            JOIN PrescriptionDetails PD ON P.PrescriptionID = PD.PrescriptionID
                            JOIN PharmacyData M ON PD.MedicineName = M.MedicineName
                            WHERE P.PatientID = @pid AND PD.MedicineName = @medName
                            ORDER BY P.PrescriptionDate DESC";

                        using (SqlCommand cmdCheck = new SqlCommand(checkQuery, conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@pid", patientId);
                            cmdCheck.Parameters.AddWithValue("@medName", medicineName);

                            using (SqlDataReader reader = cmdCheck.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    int lastPresID = Convert.ToInt32(reader["PrescriptionID"]);
                                    DateTime lastDate = Convert.ToDateTime(reader["PrescriptionDate"]);
                                    int oneBoxDays = Convert.ToInt32(reader["UsageDays"]);
                                    reader.Close(); // I'm closing the reader so I can run the query below.

                                    /// How many boxes of this medication were prescribed in the last prescription? (I will multiply the time by the number of boxes)
                                    string countQuery = "SELECT COUNT(*) FROM PrescriptionDetails WHERE PrescriptionID = @presID AND MedicineName = @medName";
                                    using (SqlCommand cmdCount = new SqlCommand(countQuery, conn))
                                    {
                                        cmdCount.Parameters.AddWithValue("@presID", lastPresID);
                                        cmdCount.Parameters.AddWithValue("@medName", medicineName);

                                        int boxCount = Convert.ToInt32(cmdCount.ExecuteScalar());
                                        int totalWaitDays = oneBoxDays * boxCount;
                                        DateTime allowedDate = lastDate.AddDays(totalWaitDays);

                                        // I'm preventing prescriptions from being written if today's date is before the permitted date.
                                        if (DateTime.Now < allowedDate)
                                        {
                                            int remaining = (allowedDate - DateTime.Now).Days;
                                            MessageBox.Show($"WARNING: Medicine '{medicineName}' cannot be prescribed!\n" +
                                                            $"Previous prescription had {boxCount} boxes ({totalWaitDays} days).\n" +
                                                            $"Wait {remaining} more days.",
                                                            "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                            return;// I'm stopping the entire process here.
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // 4. SAVING THE PRESCRIPTION TITLE
                    // If the checks are passed, I save the prescription to the 'Prescriptions' table.
                    // I instantly retrieve the ID of the prescription I added using SCOPE_IDENTITY().
                    string insertQuery = @"
                        INSERT INTO Prescriptions (PatientID, Barcode, PrescriptionDate) 
                        VALUES (@pid, @barcode, GETDATE());     
                        SELECT SCOPE_IDENTITY();";

                    // By saving the Patient ID (@pid) I found above and the Barcode (@barcode)
                    //I generated on the same line, I am assigning it to the patient. ((BARCODE SAVE))
                    using (SqlCommand cmdInsert = new SqlCommand(insertQuery, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@pid", patientId);
                        cmdInsert.Parameters.AddWithValue("@barcode", barcode);

                        // ExecuteScalar, SELECT SCOPE_IDENTITY()
                        object result = cmdInsert.ExecuteScalar();
                        if (result != null) prescriptionId = Convert.ToInt32(result);
                    }

                    if (prescriptionId == 0) return;

                    // 5. SAVING PRESCRIPTION DETAILS (Details Insert)
                    // Now I'm saving the medications in the list to the database one by one by looping through them.
                    string insertDetailQuery = "INSERT INTO PrescriptionDetails (PrescriptionID, MedicineName) VALUES (@presId, @medName)";

                    foreach (DataGridViewRow row in dataGridView2.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string medName = row.Cells["MedicineName"].Value.ToString();

                        using (SqlCommand cmdDetail = new SqlCommand(insertDetailQuery, conn))
                        {
                            cmdDetail.Parameters.AddWithValue("@presId", prescriptionId);
                            cmdDetail.Parameters.AddWithValue("@medName", medName);
                            cmdDetail.ExecuteNonQuery();
                        }
                    }

                    // I'm displaying a message saying the operation was completed successfully.
                    MessageBox.Show($"Prescription Created Successfully!\nBarcode: {barcode}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear
                    dataGridView2.Rows.Clear();
                    textBox1.Clear();
                    textBox2.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}