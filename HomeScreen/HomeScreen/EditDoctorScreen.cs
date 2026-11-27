using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HomeScreen
{
    // This form is designed for Admins to manage Doctor accounts Add, Update, Delete.
    public partial class EditDoctorScreen : Form
    {
        // I defined the connection string here to manage the database connection centrally.
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public EditDoctorScreen()
        {
            InitializeComponent();

            // When the form initializes, I immediately load the doctor list and update the total count 
            LoadDoctorList();
            UpdateDoctorCount();
        }

        private void EditDoctorScreen_Load(object sender, EventArgs e)
        {
            // I moved the loading logic to the Constructor to ensure data is ready when the form opens.
        }

        // HELPER METHOD: Loads the list of doctors into the DataGridView
        void LoadDoctorList()
        {
            try
            {
                // I used a 'using' block for the connection to ensure memory is released properly.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I fetched the Doctor ID, Username, Name, and Specialty to display in the grid.
                    SqlDataAdapter da = new SqlDataAdapter("SELECT DoctorID, UserName, FirstName, LastName, Specialty FROM Doctors", conn);
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

        // HELPER METHOD: Updates the total count of doctors in the Form Title
        void UpdateDoctorCount()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I called my custom SQL Function 'fn_GetTotalDoctors' to get the current count.
                    SqlCommand cmd = new SqlCommand("SELECT dbo.fn_GetTotalDoctors()", conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // I updated the form's title bar to show the statistics.
                        this.Text = "Doctor Management - Total Doctors: " + result.ToString();
                    }
                }
            }
            catch { }
        }

        // HELPER METHOD: Fetches the latest log entry created by the SQL Trigger
        void ShowLastLogMessage()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Since my Triggers automatically write to SystemLogs, I fetch the very last entry here
                    // to show the user that the background process worked.
                    string query = "SELECT TOP 1 Description FROM SystemLogs ORDER BY LogID DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        // I display the trigger's log message to the user as a confirmation.
                        MessageBox.Show("System Log Notification:\n\n" + result.ToString(), "Operation Logged", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent catch for log errors.
            }
        }

        // BUTTON 1: ADD DOCTOR (Uses Stored Procedure)
        private void button1_Click(object sender, EventArgs e)
        {
            // I validated that the username and password fields are not empty.
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

                    // I used the Stored Procedure 'sp_AddDoctor' to handle the insertion securely.
                    using (SqlCommand cmd = new SqlCommand("sp_AddDoctor", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // I passed the input values as parameters to prevent SQL Injection.
                        cmd.Parameters.AddWithValue("@UserName", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", textBox2.Text.Trim());
                        cmd.Parameters.AddWithValue("@FirstName", textBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", textBox4.Text.Trim());
                        cmd.Parameters.AddWithValue("@Specialty", textBox5.Text.Trim());

                        // I defined a return parameter to check if the Stored Procedure returns an error (like -1).
                        SqlParameter returnVal = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        returnVal.Direction = ParameterDirection.ReturnValue;

                        // Execution: The Trigger 'trg_LogNewDoctor' fires automatically here.
                        cmd.ExecuteNonQuery();

                        int result = (int)returnVal.Value;

                        if (result == -1)
                        {
                            MessageBox.Show("This username is already taken!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // If successful, I show the log message generated by the trigger.
                            ShowLastLogMessage();

                            // I cleared the fields to prepare for the next entry.
                            textBox1.Clear(); textBox2.Clear(); textBox3.Clear(); textBox4.Clear(); textBox5.Clear();

                            // I refreshed the grid and the counter.
                            LoadDoctorList();
                            UpdateDoctorCount();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        // BUTTON 2: DELETE DOCTOR
        private void button2_Click(object sender, EventArgs e)
        {
            // I checked if the user actually selected a row to delete.
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a doctor to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DoctorID"].Value);
            string name = dataGridView1.SelectedRows[0].Cells["FirstName"].Value.ToString();

            // I asked for confirmation before deleting to prevent accidental data loss.
            if (MessageBox.Show($"Are you sure you want to delete Dr. {name}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // I executed the DELETE command. The Trigger 'trg_LogDeletedDoctor' fires here.
                        SqlCommand cmd = new SqlCommand("DELETE FROM Doctors WHERE DoctorID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    // I displayed the log message from the database to confirm the action.
                    ShowLastLogMessage();

                    LoadDoctorList();
                    UpdateDoctorCount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete Error: " + ex.Message);
                }
            }
        }

        // BUTTON 3: GO BACK
        private void button3_Click(object sender, EventArgs e)
        {
            // I navigate back to the main Admin Panel.
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();
            this.Close();
        }

        // GRID CLICK EVENT: Populates the textboxes
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // When a row is clicked, I fill the textboxes with that doctor's data for easier editing.
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells["UserName"].Value.ToString();
                textBox3.Text = row.Cells["FirstName"].Value.ToString();
                textBox4.Text = row.Cells["LastName"].Value.ToString();
                textBox5.Text = row.Cells["Specialty"].Value.ToString();
            }
        }

        // BUTTON 4: UPDATE DOCTOR
        private void button4_Click(object sender, EventArgs e)
        {
            // I validated that a doctor is selected for the update operation.
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a doctor to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I retrieved the ID of the selected doctor.
            int id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["DoctorID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I prepared the SQL Update statement to modify the doctor's details.
                    string query = @"UPDATE Doctors 
                                     SET UserName=@user, Password=@pass, FirstName=@first, LastName=@last, Specialty=@spec 
                                     WHERE DoctorID=@id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@pass", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@first", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@last", textBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@spec", textBox5.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", id);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Doctor updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // I refreshed the list and cleared the inputs after a successful update.
                        LoadDoctorList();
                        UpdateDoctorCount();

                        textBox1.Clear();
                        textBox2.Clear();
                        textBox3.Clear();
                        textBox4.Clear();
                        textBox5.Clear();
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