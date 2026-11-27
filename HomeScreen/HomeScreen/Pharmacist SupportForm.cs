using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient; // Added for Database connectivity
using System.Drawing;        // Added to control colors in the DataGridView
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class Pharmacist_SupportForm : Form
    {
        // I keep the database connection string here for easy access throughout the class.
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public Pharmacist_SupportForm()
        {
            InitializeComponent();

            // When the form opens, I immediately load the categories and the inventory list.
            LoadCategories();
            LoadInventory();

            // I manually attach the CellClick and CellFormatting events here to ensure 
            // the visual logic and selection logic work correctly, even if the designer missed them.
            this.dataGridView1.CellClick += new DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
        }

        // HELPER METHOD: Fetch inventory data
        private void LoadInventory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I use a LIKE query here so the user can search by either Medicine Name or Barcode.
                    string query = @"SELECT * FROM PharmacyData 
                                     WHERE MedicineName LIKE @search OR Barcode LIKE @search
                                     ORDER BY MedicineName";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    // I pass the text from textBox1 as the search parameter.
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + textBox1.Text + "%");

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

        // HELPER METHOD: Populate Categories
        private void LoadCategories()
        {
            try
            {
                comboBox1.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I fetch distinct categories to populate the dropdown filter.
                    SqlCommand cmd = new SqlCommand("SELECT DISTINCT Category FROM PharmacyData WHERE Category IS NOT NULL ORDER BY Category", conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        comboBox1.Items.Add(reader["Category"].ToString());
                    }
                }
            }
            catch { }
        }

        // HELPER METHOD: Clear Input Fields
        private void ClearFields()
        {
            // I reset all input controls to their default values to prepare for a new entry.
            textBox2.Clear(); // Barcode
            textBox3.Clear(); // Medicine Name
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            comboBox1.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Now;

            // Clearing the extra detail fields
            textBox4.Clear(); // ATC
            textBox5.Clear(); // Active Ingredient
            textBox6.Clear(); // Manufacturer
            textBox7.Clear(); // Dosage
            numericUpDown3.Value = 0; // Usage Days
        }

        // BUTTON 1: REFRESH
        private void button1_Click(object sender, EventArgs e)
        {
            // I clear the search box and reload the full inventory.
            textBox1.Clear();
            LoadInventory();
        }

        // BUTTON 8: SEARCH
        private void button8_Click(object sender, EventArgs e)
        {
            // Calls LoadInventory which already handles the search logic via textBox1.
            LoadInventory();
        }

        // BUTTON 7: SHOW CRITICAL STOCK (Uses SQL View)
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I query the 'vw_CriticalItems' VIEW to quickly show items running low on stock.
                    string query = "SELECT * FROM vw_CriticalItems ORDER BY StockQuantity ASC";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;

                    MessageBox.Show($"Found {dt.Rows.Count} critical items!", "Critical Stock Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading view: " + ex.Message);
            }
        }

        // BUTTON 9: REMOVE EXPIRED MEDICINES (Uses Stored Procedure)
        private void button9_Click(object sender, EventArgs e)
        {
            // I warn the user that this action resets stock to 0 rather than deleting the record.
            if (MessageBox.Show("Expired medicines will have their stock set to 0.\nThey will NOT be deleted from the database to preserve sales history.\nAre you sure?",
                                "Confirm Stock Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I execute the 'sp_RemoveExpiredMedicines' stored procedure to handle the logic on the server side.
                    SqlCommand cmd = new SqlCommand("sp_RemoveExpiredMedicines", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    object result = cmd.ExecuteScalar();
                    int affectedCount = (result != null) ? Convert.ToInt32(result) : 0;

                    if (affectedCount > 0)
                    {
                        MessageBox.Show($"{affectedCount} expired medicines have been set to 0 stock.\nSales for these items are now blocked.",
                                        "Operation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadInventory(); // Refresh to see the changes.
                    }
                    else
                    {
                        MessageBox.Show("No expired medicines with active stock found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during operation: " + ex.Message);
            }
        }

        // BUTTON 2: ADD MEDICINE
        private void button2_Click(object sender, EventArgs e)
        {
            // Validation check.
            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Barcode and Medicine Name are required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I prepare a standard INSERT statement to add a new medicine record.
                    string query = @"INSERT INTO PharmacyData 
                                     (Barcode, MedicineName, Category, StockQuantity, Price, ExpiryDate, 
                                      ATCCode, ActiveIngredient, Manufacturer, DosageInfo, UsageDays) 
                                     VALUES 
                                     (@barcode, @name, @cat, @stock, @price, @date, 
                                      @atc, @active, @manuf, @dosage, @days)";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    // I map the UI fields to the SQL parameters.
                    cmd.Parameters.AddWithValue("@barcode", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@cat", comboBox1.Text);
                    cmd.Parameters.AddWithValue("@stock", (int)numericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@price", numericUpDown2.Value);
                    cmd.Parameters.AddWithValue("@date", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@atc", textBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@active", textBox5.Text.Trim());
                    cmd.Parameters.AddWithValue("@manuf", textBox6.Text.Trim());
                    cmd.Parameters.AddWithValue("@dosage", textBox7.Text.Trim());
                    cmd.Parameters.AddWithValue("@days", (int)numericUpDown3.Value);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Medicine added successfully.");
                    LoadInventory();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding medicine: " + ex.Message);
            }
        }

        // BUTTON 3: UPDATE MEDICINE
        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please select a medicine to update (Barcode required).");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I use the Barcode as the key to update the rest of the information.
                    string query = @"UPDATE PharmacyData 
                                     SET MedicineName=@name, 
                                         Category=@cat, 
                                         StockQuantity=@stock, 
                                         Price=@price, 
                                         ExpiryDate=@date,
                                         ATCCode=@atc,
                                         ActiveIngredient=@active,
                                         Manufacturer=@manuf,
                                         DosageInfo=@dosage,
                                         UsageDays=@days
                                     WHERE Barcode=@barcode";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@barcode", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@name", textBox3.Text.Trim());
                    cmd.Parameters.AddWithValue("@cat", comboBox1.Text);
                    cmd.Parameters.AddWithValue("@stock", (int)numericUpDown1.Value);
                    cmd.Parameters.AddWithValue("@price", numericUpDown2.Value);
                    cmd.Parameters.AddWithValue("@date", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@atc", textBox4.Text.Trim());
                    cmd.Parameters.AddWithValue("@active", textBox5.Text.Trim());
                    cmd.Parameters.AddWithValue("@manuf", textBox6.Text.Trim());
                    cmd.Parameters.AddWithValue("@dosage", textBox7.Text.Trim());
                    cmd.Parameters.AddWithValue("@days", (int)numericUpDown3.Value);

                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Record updated.");
                        LoadInventory();
                        ClearFields();
                    }
                    else
                    {
                        MessageBox.Show("Medicine with this barcode not found!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Error: " + ex.Message);
            }
        }

        // BUTTON 4: DELETE MEDICINE
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please select a medicine to delete.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this medicine?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        // I delete the record based on the Barcode.
                        SqlCommand cmd = new SqlCommand("DELETE FROM PharmacyData WHERE Barcode=@barcode", conn);
                        cmd.Parameters.AddWithValue("@barcode", textBox2.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Medicine deleted.");
                    LoadInventory();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Delete Error: " + ex.Message);
                }
            }
        }

        // BUTTON 5: CLEAR
        private void button5_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        // BUTTON 6: BACK
        private void button6_Click(object sender, EventArgs e)
        {
            // I navigate back to the Admin Panel.
            AdminPanelForm adminForm = new AdminPanelForm();
            adminForm.Show();

            // Close the current form.
            this.Close();
        }

        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }

        // DATAGRIDVIEW: CELL CLICK (Populates TextBoxes)
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    // When a user clicks a row, I take the data and fill the textboxes for easy editing.
                    DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                    textBox2.Text = row.Cells["Barcode"].Value?.ToString();
                    textBox3.Text = row.Cells["MedicineName"].Value?.ToString();

                    if (row.Cells["Category"].Value != DBNull.Value)
                        comboBox1.Text = row.Cells["Category"].Value.ToString();

                    if (row.Cells["StockQuantity"].Value != DBNull.Value)
                        numericUpDown1.Value = Convert.ToDecimal(row.Cells["StockQuantity"].Value);

                    if (row.Cells["Price"].Value != DBNull.Value)
                        numericUpDown2.Value = Convert.ToDecimal(row.Cells["Price"].Value);

                    if (row.Cells["ExpiryDate"].Value != DBNull.Value)
                        dateTimePicker1.Value = Convert.ToDateTime(row.Cells["ExpiryDate"].Value);

                    // Filling the new fields
                    textBox4.Text = row.Cells["ATCCode"].Value?.ToString();
                    textBox5.Text = row.Cells["ActiveIngredient"].Value?.ToString();
                    textBox6.Text = row.Cells["Manufacturer"].Value?.ToString();
                    textBox7.Text = row.Cells["DosageInfo"].Value?.ToString();

                    if (row.Cells["UsageDays"].Value != DBNull.Value)
                        numericUpDown3.Value = Convert.ToDecimal(row.Cells["UsageDays"].Value);
                }
                catch { }
            }
        }

        // DATAGRIDVIEW: CONDITIONAL FORMATTING
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.RowIndex < 0) return;

            DataGridViewColumn col = dataGridView1.Columns[e.ColumnIndex];

            // RULE 1: If Stock < 10, highlight in RED.
            if (col.DataPropertyName == "StockQuantity" || col.Name == "StockQuantity")
            {
                decimal stock = 0;
                if (decimal.TryParse(e.Value.ToString(), out stock))
                {
                    if (stock < 10)
                    {
                        e.CellStyle.BackColor = Color.Red;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.SelectionBackColor = Color.DarkRed;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }
                }
            }

            // RULE 2: If Expired (Black) or Near Expiry (Yellow).
            if (col.DataPropertyName == "ExpiryDate" || col.Name == "ExpiryDate")
            {
                DateTime expiryDate;
                if (DateTime.TryParse(e.Value.ToString(), out expiryDate))
                {
                    if (expiryDate < DateTime.Now) // Expired
                    {
                        e.CellStyle.BackColor = Color.Black;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout);
                    }
                    else if (expiryDate < DateTime.Now.AddMonths(6)) // Less than 6 months left
                    {
                        e.CellStyle.BackColor = Color.Yellow;
                        e.CellStyle.ForeColor = Color.Black;
                        e.CellStyle.SelectionBackColor = Color.Gold;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }
                }
            }
        }
    }
}