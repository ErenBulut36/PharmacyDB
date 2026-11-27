using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing; // I added this to color the grid rows.
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class InventoryForm : Form
    {
        //Database Connection
        string connectionString = "Server=DESKTOP-TV9F6PO\\SQLEXPRESS;Database=PharmacyDB;Trusted_Connection=True;";

        public InventoryForm()
        {
            InitializeComponent();

            // I'm loading the categories and drug list when the form opens.
            LoadCategories();
            LoadInventory();

            //IMPORTANT
            // Sometimes events can get lost in automatically generated code.
            // I'm manually connecting the Click and Color events here.
            this.dataGridView1.CellClick += new DataGridViewCellEventHandler(this.dataGridView1_CellClick);
            this.dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
        }

        // METHOD FOR RETRIEVING THE LIST
        private void LoadInventory()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    //I have prepared a search query that can search by drug name or barcode.
                    conn.Open();
                    string query = @"SELECT * FROM PharmacyData 
                                     WHERE MedicineName LIKE @search OR Barcode LIKE @search
                                     ORDER BY MedicineName";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    // I added the text in the search box as a parameter.
                    da.SelectCommand.Parameters.AddWithValue("@search", "%" + textBox1.Text + "%");

                    DataTable dt = new DataTable();
                    // I connected the data to the table DataGridView.
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading list: " + ex.Message);
            }
        }

        // FILL IN THE CATEGORIES
        private void LoadCategories()
        {
            try
            {
                comboBox1.Items.Clear();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I'm retrieving the existing categories from the database and populating the ComboBox with them.
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

        // BUTON 1: Refresh in list
        private void button1_Click(object sender, EventArgs e)
        {
            //I'm clearing the search box and reloading the list.
            textBox1.Clear();
            LoadInventory();
        }

        // BUTON 2: Adding new drugs
        private void button2_Click(object sender, EventArgs e)
        {
            // I'm checking if the Barcode and Name fields are empty.
            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("Barcode and Medicine Name are required!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    //I prepared the INSERT query that saves all drug information to the database.
                    conn.Open();
                    string query = @"INSERT INTO PharmacyData 
                                     (Barcode, MedicineName, Category, StockQuantity, Price, ExpiryDate, 
                                      ATCCode, ActiveIngredient, Manufacturer, DosageInfo, UsageDays) 
                                     VALUES 
                                     (@barcode, @name, @cat, @stock, @price, @date, 
                                      @atc, @active, @manuf, @dosage, @days)";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    // I'm assigning parameters by retrieving them from the controls on the form.
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

                    // After adding the files, I refresh the list and clear the fields.
                    LoadInventory();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding medicine: " + ex.Message);
            }
        }

        // BUTON 3: Update drugs
        private void button3_Click(object sender, EventArgs e)
        {
            // The barcode must be filled in for the update.
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please select a medicine to update (Barcode required).");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // UPDATE query that updates the information of the drug matching the barcode.
                    conn.Open();
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

                    // I'm assigning parameters by retrieving them from the controls on the form.
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

        // BUTON 4: Delete drugs
        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Please select a medicine to delete.");
                return;
            }

            // I'm getting final confirmation from the user.
            if (MessageBox.Show("Are you sure you want to delete this medicine?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        // I am performing a deletion operation based on the barcode.
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

        // BUTON 5: Clear
        private void button5_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        // BUTON 6: Return PharmacistMainForm
        private void button6_Click(object sender, EventArgs e)
        {
            PharmacistMainForm mainForm = new PharmacistMainForm();
            mainForm.Show();
            this.Close();
        }

        // HELPER METHOD: Resets all input boxes.
        private void ClearFields()
        {
            textBox2.Clear();
            textBox3.Clear();
            numericUpDown1.Value = 0;
            numericUpDown2.Value = 0;
            comboBox1.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Now;

            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox7.Clear();
            numericUpDown3.Value = 0;
        }

        // SELECT FROM LIST (((Clicking fills the boxes)))
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // I don't take any action if the title line is clicked.
            if (e.RowIndex < 0) return;

            try
            {
                // Get the clicked row
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // I'm securely transferring data to boxes by performing a null check.
                textBox2.Text = row.Cells["Barcode"].Value?.ToString();
                textBox3.Text = row.Cells["MedicineName"].Value?.ToString();

                if (row.Cells["Category"].Value != DBNull.Value)
                    comboBox1.Text = row.Cells["Category"].Value.ToString();
                else
                    comboBox1.SelectedIndex = -1;

                if (row.Cells["StockQuantity"].Value != DBNull.Value)
                    numericUpDown1.Value = Convert.ToDecimal(row.Cells["StockQuantity"].Value);
                else
                    numericUpDown1.Value = 0;

                if (row.Cells["Price"].Value != DBNull.Value)
                    numericUpDown2.Value = Convert.ToDecimal(row.Cells["Price"].Value);
                else
                    numericUpDown2.Value = 0;

                if (row.Cells["ExpiryDate"].Value != DBNull.Value)
                    dateTimePicker1.Value = Convert.ToDateTime(row.Cells["ExpiryDate"].Value);

                // I'm also filling in the extra detail fields.
                textBox4.Text = row.Cells["ATCCode"].Value?.ToString();
                textBox5.Text = row.Cells["ActiveIngredient"].Value?.ToString();
                textBox6.Text = row.Cells["Manufacturer"].Value?.ToString();
                textBox7.Text = row.Cells["DosageInfo"].Value?.ToString();

                if (row.Cells["UsageDays"].Value != DBNull.Value)
                    numericUpDown3.Value = Convert.ToDecimal(row.Cells["UsageDays"].Value);
                else
                    numericUpDown3.Value = 0;
            }
            catch (Exception ex)
            {
                
            }
        }

        // This method runs each time a row is created in the table and applies color matching based on the conditions.
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.RowIndex < 0) return;

            DataGridViewColumn col = dataGridView1.Columns[e.ColumnIndex];

            // RULE 1: I turn it RED if the stock level drops below 10.
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

            // RULE 2: I color the products according to their expiration date.
            if (col.DataPropertyName == "ExpiryDate" || col.Name == "ExpiryDate")
            {
                DateTime expiryDate;
                if (DateTime.TryParse(e.Value.ToString(), out expiryDate))
                {
                    if (expiryDate < DateTime.Now) // Historical Background BLACK
                    {
                        e.CellStyle.BackColor = Color.Black;
                        e.CellStyle.ForeColor = Color.White;
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout);
                    }
                    else if (expiryDate < DateTime.Now.AddMonths(6)) //Less than 6 months remaining YELLOW
                    {
                        e.CellStyle.BackColor = Color.Yellow;
                        e.CellStyle.ForeColor = Color.Black;
                        e.CellStyle.SelectionBackColor = Color.Gold;
                        e.CellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }
                }
            }
        }

        // BUTTON 7: CRITICAL LEVEL REPORT View Usage
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // I'm using the 'vw_CriticalItems' view in the database to retrieve items with low stock or expired dates.
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

        // Searching
        private void button8_Click(object sender, EventArgs e)
        {
            // Since the loading method already looks in the search box, I just need to call it.
            LoadInventory();
        }

        // BUTTON 9: RESET STOCK OF EXPIRED ITEMS Stored Procedure Usage
        private void button9_Click(object sender, EventArgs e)
        {
            // I'm warning the user: "It won't be deleted, only the stock will be reset."
            if (MessageBox.Show("Expired medicines will have their stock set to 0. They will NOT be deleted from the database.\nAre you sure?",
                                "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //I'm resetting the stock of expired medications by running the 'sp Remove Expired Medicines' procedure.
                    SqlCommand cmd = new SqlCommand("sp_RemoveExpiredMedicines", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    object result = cmd.ExecuteScalar();
                    int affectedCount = (result != null) ? Convert.ToInt32(result) : 0;

                    if (affectedCount > 0)
                    {
                       
                        MessageBox.Show($"{affectedCount} expired medicines stock set to 0.\nSales for these items are now blocked.",
                                        "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadInventory(); // I'm refreshing the list to see the change.
                    }
                    else
                    {
                        MessageBox.Show("No expired medicines with active stock found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during update: " + ex.Message);
            }
        }

        // Other events
        private void InventoryForm_Load(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void textBox5_TextChanged(object sender, EventArgs e) { }
        private void textBox6_TextChanged(object sender, EventArgs e) { }
        private void textBox7_TextChanged(object sender, EventArgs e) { }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e) { }
    }
}