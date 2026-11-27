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
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Redirection to the Doctor login screen.
            DoctorForm doctorForm = new DoctorForm();
            doctorForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Redirection to the Pharmacist login screen.
            PharmacistForm pharmacistForm = new PharmacistForm();
            pharmacistForm.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Redirection to the Admin login screen.
            AdminForm adminForm = new AdminForm();
            adminForm.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //We use this button if we want to exit the application.
            DialogResult choose = MessageBox.Show("Are you sure you want to exit the application?",
                                         "Exit",
                                         MessageBoxButtons.YesNo,
                                         MessageBoxIcon.Question);

            
            if (choose == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
