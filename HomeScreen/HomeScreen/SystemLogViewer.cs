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
    public partial class SystemLogViewer : Form
    {
        // Database Connection String
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public SystemLogViewer()
        {
            InitializeComponent();

            // Load logs automatically
            LoadLogs();
        }

        // HELPER METHOD: Fetch Data from SystemLogs Table
        private void LoadLogs()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // UPDATED SQL QUERY: Matches your actual table structure
                    // Columns: LogID, LogDate, ActionType, Description
                    string query = "SELECT LogID, LogDate, ActionType, Description FROM SystemLogs ORDER BY LogDate DESC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Bind to DataGridView
                    if (dataGridView1 != null)
                    {
                        dataGridView1.DataSource = dt;

                        // Optional: Adjust column widths
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading logs: " + ex.Message);
            }
        }

        // BUTTON 1: BACK
        private void button1_Click(object sender, EventArgs e)
        {
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();
            this.Close();
        }

        // BUTTON 2: CLEAR LOGS
        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("WARNING: This will delete ALL system history permanently!\n\nAre you sure?",
                                "Critical Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        // TRUNCATE resets the table and ID counter
                        SqlCommand cmd = new SqlCommand("TRUNCATE TABLE SystemLogs", conn);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("System logs have been cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadLogs(); // Refresh
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error clearing logs: " + ex.Message);
                }
            }
        }
    }
}