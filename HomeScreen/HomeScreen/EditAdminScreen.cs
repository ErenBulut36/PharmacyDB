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
    public partial class EditAdminScreen : Form
    {
        // Database Connection String
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public EditAdminScreen()
        {
            InitializeComponent();
            // ((Load data when form starts))
            LoadAdminList();
            UpdateAdminCount();
        }

        // //Retrieves the administrator list from the database and populates the DataGridView with it.
        void LoadAdminList()
        {
            try
            {
                // I opened the connection using a 'using' block for memory management.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    //I prepared the SQL query to retrieve the administrators' IDs, Usernames, and Names.
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT AdminID, UserName, FirstName, LastName FROM Admins", conn);
                    DataTable dt = new DataTable();

                    // I populated the extracted data into a virtual table.
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                // I notify the user if an error occurs while loading the list.
                MessageBox.Show("Error loading list: " + ex.Message);
            }
        }

        // HELPER METHOD: Calculates and displays the total number of administrators.
        void UpdateAdminCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I get the number by calling the 'fn_GetTotalAdmins' function that I wrote on the SQL side.
                    SqlCommand cmd = new SqlCommand("SELECT dbo.fn_GetTotalAdmins()", conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // I display the result to the user by printing it in the form's title.
                        this.Text = "Admin Management - Total Admins: " + result.ToString();
                    }
                }
            }
            catch { }
        }

        // BUTTON 1: ADD NEW ADMINISTRATOR Using Stored Procedure
        private void button1_Click(object sender, EventArgs e)
        {
            // I'm checking if the login fields (Username and Password) are filled in.
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

                    // STEP 1: I'm running the 'sp_AddAdmin' procedure to register the data.
                    using (SqlCommand cmd = new SqlCommand("sp_AddAdmin", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure; //I indicated that this is a SP.

                        // I'm retrieving parameters from textboxes and adding them securely.
                        cmd.Parameters.AddWithValue("@UserName", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", textBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@FirstName", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", textBox4.Text.Trim());

                        // I defined a return parameter to capture the success/error code returned from the stored procedure.
                        SqlParameter returnVal = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        returnVal.Direction = ParameterDirection.ReturnValue;

                        cmd.ExecuteNonQuery();

                        int result = (int)returnVal.Value;

                        // If SP returns -1, it means the username already exists.
                        if (result == -1)
                        {
                            MessageBox.Show("This username is already taken!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // STEP 2: I'm reading the log message written by the trigger for feedback.
                            string logQuery = "SELECT TOP 1 Description FROM SystemLogs ORDER BY LogID DESC";
                            SqlCommand cmdLog = new SqlCommand(logQuery, conn);
                            object logResult = cmdLog.ExecuteScalar();
                            string logMessage = logResult != null ? logResult.ToString() : "Log not found.";

                            // STEP 3: I show the user the success message and the database log.
                            MessageBox.Show("Admin added successfully.\n\n[DATABASE LOG ENTRY]:\n" + logMessage,
                                            "Success",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);

                            // Clear and Refresh
                            textBox1.Clear();
                            textBox2.Clear();
                            textBox3.Clear();
                            textBox4.Clear();
                            LoadAdminList();
                            UpdateAdminCount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding admin: " + ex.Message);
            }
        }

        // BUTTON 2: DELETE ADMIN (Trigger Activates & Show Log)
        private void button2_Click(object sender, EventArgs e)
        {
            // I'm checking if a row has been selected from the list.
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an admin to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I'm retrieving the Admin ID and Name information from the selected row.
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["AdminID"].Value);
            string name = dataGridView1.SelectedRows[0].Cells["FirstName"].Value.ToString();

            // I'm requesting deletion confirmation from the user.
            if (MessageBox.Show($"Are you sure you want to delete Admin '{name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // STEP 1: I'm running the delete command.
                        SqlCommand cmd = new SqlCommand("DELETE FROM Admins WHERE AdminID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // STEP 2: I'm reading the log left by Trigger after the deletion.
                            SqlCommand cmdLog = new SqlCommand("SELECT TOP 1 Description FROM SystemLogs ORDER BY LogID DESC", conn);
                            object logResult = cmdLog.ExecuteScalar();
                            string logMessage = logResult != null ? logResult.ToString() : "Log not found.";

                            // STEP 3: I show the user the result of the operation.
                            MessageBox.Show("Deletion successful!\n\n[DATABASE LOG ENTRY]:\n" + logMessage,
                                            "Process Completed",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);

                            // Update Grid and Count
                            LoadAdminList();
                            UpdateAdminCount();
                        }
                    }
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
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();
            this.Close();
        }

        // GRID CLICK: FILL TEXTBOXES
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells["UserName"].Value.ToString();
                // Password usually hidden for security
                 
                textBox3.Text = row.Cells["FirstName"].Value.ToString();
                textBox4.Text = row.Cells["LastName"].Value.ToString();
            }
        }

        // BUTTON 4: UPDATE ADMIN
        private void button4_Click(object sender, EventArgs e)
        {
            // 1. Selection Check: Is the person to be updated selected?
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an admin to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get ID: I'm capturing the ID of the selected row.
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["AdminID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // SQL Update querry
                    string query = @"UPDATE Admins 
                                     SET UserName=@user, Password=@pass, FirstName=@first, LastName=@last 
                                     WHERE AdminID=@id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@pass", textBox2.Text.Trim()); // It also updates the password.
                    cmd.Parameters.AddWithValue("@first", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@last", textBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", id); // I specify which record to update.

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Admin updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh List and Counter
                        LoadAdminList();
                        UpdateAdminCount();

                        // Clear Boxes
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