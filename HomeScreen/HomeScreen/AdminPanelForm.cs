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
    // My Admin Panel form class. This is the main hub where the administrator accesses all other functions.
    public partial class AdminPanelForm : Form
    {
        // Database Connection
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public AdminPanelForm()
        {
            InitializeComponent();

            // SP IS BEING CALLED UPON HERE.
            // Show statistics as soon as the form opens doctors pharmacist and drugs number shows.
            ShowSystemStats();
        }

        // USING STORED PROCEDURES: RETRIEVING STATISTICS
        private void ShowSystemStats()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // I prepared the command to call the stored procedure (sp_GetSystemStats) in the database.
                    using (SqlCommand cmd = new SqlCommand("sp_GetSystemStats", conn))
                    {
                        // I'm informing the system that the command is a ((Stored Procedure)), ((not an SQL query)).
                        cmd.CommandType = CommandType.StoredProcedure;

                        // I used DataReader to read the results from the database. ((DataReader))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // I start the process if there is a line of data to read.
                            if (reader.Read())
                            {
                                // I retrieved the data from the columns returned by the stored procedure (TotalDoctors, etc.) by converting them to integer variables.
                                int docCount = Convert.ToInt32(reader["TotalDoctors"]);
                                int pharmCount = Convert.ToInt32(reader["TotalPharmacists"]);
                                int medCount = Convert.ToInt32(reader["TotalMedicines"]);

                                // Show Information Message
                                MessageBox.Show($"--- SYSTEM DASHBOARD ---\n\n" +
                                                $"👨‍⚕️ Total Doctors: {docCount}\n" +
                                                $"💊 Total Pharmacists: {pharmCount}\n" +
                                                $"📦 Total Medicines: {medCount}",
                                                "System Statistics",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                // I'm catching the error here so the program doesn't crash if an error occurs while retrieving statistics.
            }
        }

        // BUTTON 1: DOCTOR EDITING SCREEN
        private void button1_Click(object sender, EventArgs e)
        {
            EditDoctorScreen doctorScreen = new EditDoctorScreen();
            doctorScreen.Show();
            this.Hide();
        }

        // BUTTON 2: PHARMACIST EDITING SCREEN
        private void button2_Click(object sender, EventArgs e)
        {
            EditPharmacyScreen pharmacyScreen = new EditPharmacyScreen();
            pharmacyScreen.Show();
            this.Hide();
        }

        // BUTTON 3: EXIT / HOME SCREEN
        private void button3_Click(object sender, EventArgs e)
        {
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }

        // BUTTON 4: ADMIN EDITING SCREEN
        private void button4_Click(object sender, EventArgs e)
        {
            EditAdminScreen adminScreen = new EditAdminScreen();
            adminScreen.Show();
            this.Hide();
        }

        // BUTTON 5: This redirects to the page where I view the drug inventory (stocks).
        private void button5_Click(object sender, EventArgs e)
        {
     
            Pharmacist_SupportForm inventoryPage = new Pharmacist_SupportForm();
            inventoryPage.Show();
            this.Hide();
        }

        // BUTTON 6: I am opening the screen where prescription drugs are listed.
        private void button6_Click(object sender, EventArgs e)
        {
            PrescribedMedications Prescribed = new PrescribedMedications();
            Prescribed.Show();
            this.Hide();
        }

        // BUTTON 7: I am opening the control screen where all system activity (logs) is recorded.
        private void button7_Click(object sender, EventArgs e)
        {
            SystemLogViewer SystemLog = new SystemLogViewer();
            SystemLog.Show();
            this.Hide();
        }

        // BUTTON 8: I am opening the screen showing the total turnover and earnings reports.
        private void button8_Click(object sender, EventArgs e)
        {
            AdminShowTotalRevenue ShowAdmin = new AdminShowTotalRevenue();
            ShowAdmin.Show();
            this.Hide();
        }
    }
}