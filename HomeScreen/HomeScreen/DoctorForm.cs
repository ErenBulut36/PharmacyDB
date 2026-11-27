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
    public partial class DoctorForm : Form
    {
        public DoctorForm()
        {
            InitializeComponent();
            // It sets the character * to * in the password box.
            textBox2.PasswordChar = '*';
        }

        // Event that will run when the Login button is clicked.
        private void button1_Click(object sender, EventArgs e)
        {
            // I removed spaces from the data the user entered into the textboxes and assigned it to variables.
            string userName = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            // I'm checking if the username or password field is left blank.
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username or password cannot be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I'm storing the connection string required to connect to the database here.
            string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

            try
            {
                // I created the connection object using a 'using' block for memory management and connection security.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // I'm opening the link.
                    conn.Open();

                    // SQL query that checks the Doctors table for a record matching the entered username and password.
                    string query = "SELECT COUNT(*) FROM Doctors WHERE UserName=@user AND Password=@pass";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    // I added the parameters securely to prevent security vulnerabilities SQL Injection.
                    {
                        cmd.Parameters.AddWithValue("@user", userName);
                        cmd.Parameters.AddWithValue("@pass", password);

                        // I run the query and get the number of matches returned from the database.
                        int count = (int)cmd.ExecuteScalar();

                        // If the database returns one or more results, the login was successful.
                        if (count > 0)
                        {
                            // If the login is successful, DoctorPanelForm will open.
                            DoctorPanelForm panel = new DoctorPanelForm();
                            panel.Show();
                            this.Hide();
                        }
                        else
                        {
                            // Login is not successful
                            MessageBox.Show("Invalid username or password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If an error occurs during the connection, I catch and display the error so that the program doesn't close.
                MessageBox.Show("Error: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // I'm creating an object and redirecting to return to the main login screen (Form 1).
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }
    }
}