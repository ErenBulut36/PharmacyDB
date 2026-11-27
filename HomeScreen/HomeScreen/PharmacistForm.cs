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
    public partial class PharmacistForm : Form
    {
        public PharmacistForm()
        {
            InitializeComponent();
            // We added this line to hide the password box.
            textBox2.PasswordChar = '*';
        }

        // Login button
        private void button1_Click(object sender, EventArgs e)
        {
            // I retrieved the data from the textboxes and removed the margins.
            string userName = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            //I checked if the username and password fields were empty.
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username or password cannot be empty!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // I defined my database connection string here.
            string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

            // I've set up a Try-Catch block to catch potential connection errors.
            try
            {
                // I opened the connection using a ((using)) block for memory management.
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //I prepared an SQL query that checks if there is a record with this username and password in the Pharmacists table.
                    string query = "SELECT COUNT(*) FROM Pharmacists WHERE UserName=@user AND Password=@pass";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // I used parameterized queries to prevent SQL injection attacks.
                        cmd.Parameters.AddWithValue("@user", userName);
                        cmd.Parameters.AddWithValue("@pass", password);

                        //I ran the query and got the number of matching records.
                        int count = (int)cmd.ExecuteScalar();

                        // If the database returns 1 (or more) results, the login is successful.
                        if (count > 0)
                        {

                            // Login successful. PharmacistMainForm (Main Menu) opens.
                            PharmacistMainForm menu = new PharmacistMainForm();
                            menu.Show();
                            this.Hide();
                        }
                        else
                        {
                            // I displayed an incorrect login message if there was no match.
                            MessageBox.Show("Invalid username or password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // In case of an error, I prevent the program from crashing and display the error to the user.
                MessageBox.Show("Connection Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Back button
        private void button2_Click(object sender, EventArgs e)
        {
            // I redirected the user to return to Form1, the main login screen.
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }
    }
}