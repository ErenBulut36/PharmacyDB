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
    // The form class I use for administrator login processes.
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
            // Hides password box
            textBox2.PasswordChar = '*';
        }

        // Event that will run when the Login button is clicked.
        private void button1_Click(object sender, EventArgs e)
        {
            // I assigned the username and password entered by the user to variables, removing any spaces. ((trim))
            string userName = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            // Here I check if the input fields have been left blank.
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username or password cannot be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Here I defined the connection address required to connect to the database.
            string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

            
            try
            {
                // I'm creating my database connection object and using a 'using' block for memory management.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //I've prepared an SQL query that checks if a record with this username and password exists in the admin table.
                    string query = "SELECT COUNT(*) FROM Admins WHERE UserName=@user AND Password=@pass";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // To prevent SQL injection attacks, I used AddWithValue instead of adding parameters directly to the query.
                        cmd.Parameters.AddWithValue("@user", userName);
                        cmd.Parameters.AddWithValue("@pass", password);

                        // I run the query and get the number of records returned from the database as an integer (int).
                        int count = (int)cmd.ExecuteScalar();

                        // If the database returns a value greater than 0, it means the user information is correct.
                        if (count > 0)
                        {
                            // Login successful. AdminPanelForm is valid.
                            AdminPanelForm panel = new AdminPanelForm();
                            panel.Show();
                            this.Hide();
                        }
                        else
                        {
                            // Login not successful
                            MessageBox.Show("Invalid username or password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // I catch potential database connection errors and inform the user.
                MessageBox.Show("Connection Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // To return to the home screen, I'm creating and opening the Form1 object.
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }
    }
}