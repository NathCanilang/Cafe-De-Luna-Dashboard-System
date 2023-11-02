using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class PanelManager
    {
        private readonly CreateAndEditAcc createAndEditAcc = new CreateAndEditAcc();

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

            if(panelToShow == StaffPanel)
            {
                CafeDeLunaDashboard.cafeDeLunaInstance.GetData();
                CafeDeLunaDashboard.cafeDeLunaInstance.GetData2();
            }
        }
    }
    internal class PanelManagerAP
    {
        private readonly CreateAndEditAcc createAndEditAcc = new CreateAndEditAcc();

        //Admin Panel
        private readonly MySqlConnection conn;
        private readonly Panel HomePanelAP;
        private readonly Panel AccManagePanelAP;
        private readonly Panel AddMenuPanelAP;

        public PanelManagerAP(Panel homePanelAP, Panel accManagePanelAP, Panel addMenuPanelAP)
        {
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);

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

            if (panelToShow == AddMenuPanelAP) 
            {
                createAndEditAcc.LoadMenuItems();
            }
        }
       
    }
    internal class PanelManagerAMC
    {
        private readonly MySqlConnection conn;
        private readonly CreateAndEditAcc createAndEditAcc = new CreateAndEditAcc();
        private readonly Panel AccCreatePanel;

        public PanelManagerAMC(Panel accCreatePanel)
        {
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);

            AccCreatePanel = accCreatePanel;
        }
        public void ShowPanel(Panel panelToShow)
        {
            AccCreatePanel.Hide();
            

            panelToShow.Show();

            if (panelToShow == AccCreatePanel)
            {
                createAndEditAcc.GenerateAndSetRandomNumber();
                createAndEditAcc.RefreshTbl();
            }
        }
        
       
    }
}
