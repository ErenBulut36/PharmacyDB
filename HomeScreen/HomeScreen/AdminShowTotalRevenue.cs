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
    public partial class AdminShowTotalRevenue : Form
    {
        // SQL Connection String
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public AdminShowTotalRevenue()
        {
            InitializeComponent();
            // I added this line to load the data when the form opens.
            this.Load += new EventHandler(AdminShowTotalRevenue_Load);
        }

        // Event to run while form is loading
        private void AdminShowTotalRevenue_Load(object sender, EventArgs e)
        {
            LoadSalesData();
        }

        // Event to run when the CheckBox (End of Day) is clicked
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Re-fetch data when the box changes
            LoadSalesData();
        }

        // Function That Pulls Data From the Database
        private void LoadSalesData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // If CheckBox is CHECKED (End of Day Report)
                    if (checkBox1.Checked)
                    {
                        // I only pulled sales that are dated TODAY. Not use sp
                        // *** FIX APPLIED: Changed Ph.Price to PD.SalePrice for consistency ***
                        string dailyQuery = @"
                            SELECT 
                                PD.MedicineName AS [Medicine Name],
                                COUNT(*) AS [Total Quantity Sold],
                                SUM(ISNULL(PD.SalePrice, 0)) AS [Total Revenue]
                            FROM PrescriptionDetails PD
                            INNER JOIN Prescriptions P ON PD.PrescriptionID = P.PrescriptionID
                            LEFT JOIN PharmacyData Ph ON PD.MedicineName = Ph.MedicineName
                            WHERE P.IsSold = 1 
                            AND CAST(P.PrescriptionDate AS DATE) = CAST(GETDATE() AS DATE) 
                            GROUP BY PD.MedicineName
                            ORDER BY [Total Quantity Sold] DESC";

                        cmd.CommandText = dailyQuery;
                        cmd.CommandType = CommandType.Text;

                        // Visual Tip: I made the daily turnover red.
                        label2.ForeColor = Color.Red;
                    }
                    // If CheckBox is NOT CHECKED (Grand Total)
                    else
                    {
                        // If the total turnover is required, we obtain this using the store of origin (SP).
                        // Note: Since we updated the SP in SQL, this line now automatically uses the correct logic.
                        cmd.CommandText = "sp_GetMedicineSalesStats";
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Visual Hint: I made the overall turnover Black
                        label2.ForeColor = Color.Black;
                    }


                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Fill in the table
                    dataGridView1.DataSource = dt;

                    // Calculate Total Revenue
                    decimal grandTotal = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Total Revenue"] != DBNull.Value)
                        {
                            grandTotal += Convert.ToDecimal(row["Total Revenue"]);
                        }
                    }

                    // Print Result
                    label2.Text = grandTotal.ToString("C2");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Veri yükleme hatası: " + ex.Message);
                }
            }
        }

        // RETURN BUTTON (Goes to AdminPanelForm)
        private void button4_Click(object sender, EventArgs e)
        {
            AdminPanelForm adminPanel = new AdminPanelForm();
            adminPanel.Show();
            this.Close();
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}