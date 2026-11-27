using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeScreen
{
    // This form allows Admins to manage Pharmacist accounts (Add, Update, Delete).
    public partial class EditPharmacyScreen : Form
    {
        // I keep the database connection string here to manage connectivity.
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public EditPharmacyScreen()
        {
            InitializeComponent();

            // When the form starts, I immediately load the pharmacist list and update the statistics
            // so the admin sees the current data right away.
            LoadPharmacistList();
            UpdatePharmacistCount();
        }

        private void EditPharmacyScreen_Load(object sender, EventArgs e)
        {
            // I moved the loading logic to the Constructor for better performance on startup.
        }

        // HELPER METHOD: Fetches the list of pharmacists from the database
        void LoadPharmacistList()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I select specific columns to display in the grid (excluding passwords for security).
                    SqlDataAdapter da = new SqlDataAdapter("SELECT PharmacistID, UserName, FirstName, LastName FROM Pharmacists", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading list: " + ex.Message);
            }
        }

        // HELPER METHOD: Updates the total count in the window title
        void UpdatePharmacistCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I use the custom SQL function 'fn_GetTotalPharmacists' to get the real-time count.
                    SqlCommand cmd = new SqlCommand("SELECT dbo.fn_GetTotalPharmacists()", conn);

                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // I display the result in the Form's title bar.
                        this.Text = "Pharmacist Management - Total Pharmacists: " + result.ToString();
                    }
                }
            }
            catch { }
        }

        // HELPER METHOD: Retrieves the latest log entry created by the SQL Trigger
        void ShowLastLogMessage()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Since the database triggers automatically log actions, I fetch the most recent log
                    // to show the user that the system is tracking their actions.
                    string query = "SELECT TOP 1 Description FROM SystemLogs ORDER BY LogID DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // I show the log content in a message box.
                        MessageBox.Show("System Log Notification:\n\n" + result.ToString(), "Operation Logged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent catch
            }
        }

        // BUTTON 1: ADD PHARMACIST (Uses Stored Procedure)
        private void button1_Click(object sender, EventArgs e)
        {
            // I validate that the required fields are not empty.
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Username and Password are required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // I use the Stored Procedure 'sp_AddPharmacist' for secure insertion.
                    using (SqlCommand cmd = new SqlCommand("sp_AddPharmacist", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // I map the textboxes to the SQL parameters.
                        cmd.Parameters.AddWithValue("@UserName", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", textBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@FirstName", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", textBox4.Text.Trim());

                        // I capture the return value to check if the username already exists.
                        SqlParameter returnVal = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        returnVal.Direction = ParameterDirection.ReturnValue;

                        // Execution: The Trigger 'trg_LogNewPharmacist' fires here.
                        cmd.ExecuteNonQuery();

                        int result = (int)returnVal.Value;

                        if (result == -1)
                        {
                            MessageBox.Show("This username is already taken!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // Success logic:
                            // 1. Show the trigger's log message.
                            ShowLastLogMessage();

                            // 2. Clear the input fields.
                            textBox1.Clear();
                            textBox2.Clear();
                            textBox3.Clear();
                            textBox4.Clear();

                            // 3. Refresh the list and the counter.
                            LoadPharmacistList();
                            UpdatePharmacistCount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding pharmacist: " + ex.Message);
            }
        }

        // BUTTON 2: DELETE PHARMACIST
        private void button2_Click(object sender, EventArgs e)
        {
            // I verify that a row is selected.
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pharmacist to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["PharmacistID"].Value);
            string name = dataGridView1.SelectedRows[0].Cells["FirstName"].Value.ToString();

            // I ask for confirmation before deleting.
            if (MessageBox.Show($"Are you sure you want to delete Pharmacist {name}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // Execution: This fires the DELETE Trigger 'trg_LogDeletePharmacist'.
                        SqlCommand cmd = new SqlCommand("DELETE FROM Pharmacists WHERE PharmacistID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    // I show the database log confirming the deletion.
                    ShowLastLogMessage();

                    // I refresh the grid.
                    LoadPharmacistList();
                    UpdatePharmacistCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete Error: " + ex.Message);
                }
            }
        }

        // BUTTON 3: BACK
        private void button3_Click(object sender, EventArgs e)
        {
            // I navigate back to the main Admin Panel.
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();
            this.Close();
        }

        // GRID CLICK: Fills the textboxes for editing
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // When the admin clicks a row, I populate the input fields with that pharmacist's details.
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                textBox1.Text = row.Cells["UserName"].Value.ToString();
                // Password is intentionally not loaded for security reasons.
                textBox3.Text = row.Cells["FirstName"].Value.ToString();
                textBox4.Text = row.Cells["LastName"].Value.ToString();
            }
        }

        // BUTTON 4: UPDATE PHARMACIST
        private void button4_Click(object sender, EventArgs e)
        {
            // 1. Selection Check
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a pharmacist to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get ID
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["PharmacistID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I prepare the SQL Update statement to modify the record.
                    string query = @"UPDATE Pharmacists 
                                     SET UserName=@user, Password=@pass, FirstName=@first, LastName=@last 
                                     WHERE PharmacistID=@id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@pass", textBox2.Text.Trim()); // Updates password as well
                    cmd.Parameters.AddWithValue("@first", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@last", textBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", id);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Pharmacist updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // I refresh the UI elements.
                        LoadPharmacistList();
                        UpdatePharmacistCount();

                        // I clear the textboxes.
                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        textBox4.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Error: " + ex.Message);
            }
        }
    }
}