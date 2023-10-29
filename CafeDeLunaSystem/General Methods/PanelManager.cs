using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class PanelManager
    {
        // Main Panel
        private readonly Panel LoginPanel;
        private readonly Panel AdminPanel;
        private readonly Panel ManagerPanel;
        private readonly Panel StaffPanel;

        public PanelManager(Panel loginPanel, Panel adminPanel, Panel managerPanel, Panel staffPanel)
        {
            LoginPanel = loginPanel;
            AdminPanel = adminPanel;
            ManagerPanel = managerPanel;
            StaffPanel = staffPanel;
        }
        public void ShowPanel(Panel panelToShow)
        {
            // Main Panel
            LoginPanel.Hide();
            AdminPanel.Hide();
            ManagerPanel.Hide();
            StaffPanel.Hide();

            panelToShow.Show();
        }
    }
    internal class PanelManagerAP
    {
        //Admin Panel
        private readonly Panel HomePanelAP;
        private readonly Panel AccManagePanelAP;
        private readonly Panel AddMenuPanelAP;

        public PanelManagerAP(Panel homePanelAP, Panel accManagePanelAP, Panel addMenuPanelAP)
        {
            HomePanelAP = homePanelAP;
            AccManagePanelAP = accManagePanelAP;
            AddMenuPanelAP = addMenuPanelAP;
        }

        public void ShowPanel(Panel panelToShow)
        {
            //Admin Panel
            HomePanelAP.Hide();
            AccManagePanelAP.Hide();
            AddMenuPanelAP.Hide();

            panelToShow.Show();
        }
    }
    internal class PanelManagerAMC
    {
        private readonly MySqlConnection conn;
        private readonly Panel AccCreatePanel;
        private readonly Panel EditAccPanel;

        public PanelManagerAMC(Panel accCreatePanel, Panel editAccPanel)
        {
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);

            AccCreatePanel = accCreatePanel;
            EditAccPanel = editAccPanel;
        }
        public void ShowPanel(Panel panelToShow)
        {
            AccCreatePanel.Hide();
            EditAccPanel.Hide();

            panelToShow.Show();

            if (panelToShow == AccCreatePanel)
            {
                GenerateAndSetRandomNumber();
                RefreshTbl();


            }
        }
        private void GenerateAndSetRandomNumber()
        {
            Random random = new Random();
            int random6Digit = random.Next(100000, 1000000);
            CafeDeLunaDashboard.cafeDeLunaInstance.EmployeeIDTxtB_AP.Text = random6Digit.ToString();
        }

        public void RefreshTbl()
        {
            string query = "SELECT * FROM employee_acc";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            CafeDeLunaDashboard.cafeDeLunaInstance.AccDataTbl.DataSource = dataTable;
        }
    }
}
