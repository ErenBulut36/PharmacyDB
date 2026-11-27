using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeScreen
{
    public partial class PharmacistMainForm : Form
    {
        public PharmacistMainForm()
        {
            InitializeComponent();
        }

        // BUTTON 1: GO TO PRESCRIPTION AND SALES SCREEN
        private void button1_Click(object sender, EventArgs e)
        {
            // I am redirecting users to the 'Sales Panel' where prescription viewing and drug sales transactions are performed via PharmacistPanelForm.
            PharmacistPanelForm saleForm = new PharmacistPanelForm();
            saleForm.Show();
            this.Hide(); // Hide main menu
        }

        // BUTTON 2: GO TO WAREHOUSE AND STOCK TRACKING
        private void button2_Click(object sender, EventArgs e)
        {
            // I'm opening 'InventoryForm' to manage drug stocks, add new drugs, or check for expired ones.
            InventoryForm saleForm1 = new InventoryForm();
            saleForm1.Show();
            this.Hide();
        }

        // BUTTON 3: EXIT
        private void button3_Click(object sender, EventArgs e)
        {
            // By securely logging out, I'm sending the user back to the main login screen, Form1.
            Form1 home = new Form1();
            home.Show();
            this.Hide();
        }

        // BUTTON 4: SALES STATISTICS AND REPORTS
        private void button4_Click(object sender, EventArgs e)
        {
            // I'm opening the 'Sales Statistics' form so the pharmacist can review their daily sales and turnover.
            SalesStatsForm salesForm = new SalesStatsForm();
            salesForm.Show();
            this.Hide();
        }
    }
}