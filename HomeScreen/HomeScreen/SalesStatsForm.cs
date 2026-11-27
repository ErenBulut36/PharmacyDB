using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient; // Required for SQL Connection
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class SalesStatsForm : Form
    {
        // I defined the connection string here to communicate with the database.
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public SalesStatsForm()
        {
            InitializeComponent();
            this.Load += new EventHandler(SalesStatsForm_Load);
        }

        private void SalesStatsForm_Load(object sender, EventArgs e)
        {
            // When the form loads, I immediately fetch the sales data so the screen isn't empty.
            LoadSalesData();
        }

        // EVENT: CheckBox Changed
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // Whenever the user toggles the checkbox (Daily vs Total), I reload the data to reflect the change.
            LoadSalesData();
        }

        // CORE METHOD: Fetch Data and Calculate Revenue
        private void LoadSalesData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    // SCENARIO 1: DAILY REPORT If CheckBox is Checked
                    if (checkBox1.Checked)
                    {
                        // I wrote a raw SQL query here to filter sales strictly for TODAY.
                        // It groups sales by Medicine Name and sums up the revenue for the current date only.

                        // *** FIX APPLIED HERE: Using PD.SalePrice instead of Ph.Price ***
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
                        cmd.CommandType = CommandType.Text; // Using raw text query here.

                        // I changed the label color to Red to give visual feedback that we are looking at specific daily stats.
                        label2.ForeColor = Color.Red;
                    }
                    // SCENARIO 2: GENERAL REPORT If CheckBox is Unchecked
                    else
                    {
                        // For the general report, I used my Stored Procedure 'sp_GetMedicineSalesStats' 
                        // which calculates sales from the beginning of time.
                        cmd.CommandText = "sp_GetMedicineSalesStats";
                        cmd.CommandType = CommandType.StoredProcedure;

                        // I reverted the color to Black for the standard view.
                        label2.ForeColor = Color.Black;
                    }

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // 2. Bind Data to Grid
                    dataGridView1.DataSource = dt;

                    // 3. Calculate Total Revenue Manually
                    // Since the database returns rows, I loop through them to calculate the Grand Total.
                    decimal grandTotal = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Total Revenue"] != DBNull.Value)
                        {
                            grandTotal += Convert.ToDecimal(row["Total Revenue"]);
                        }
                    }

                    // 4. Display Result
                    // I formatted the total as currency C2 for better readability.
                    label2.Text = grandTotal.ToString("C2");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        // BUTTON: GO BACK
        private void button4_Click(object sender, EventArgs e)
        {
            // I navigate back to the Pharmacist Main Menu.
            PharmacistMainForm mainForm = new PharmacistMainForm();
            mainForm.Show();
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}