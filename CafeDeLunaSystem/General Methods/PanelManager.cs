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
        private readonly Panel AccCreatePanel;
        private readonly Panel EditAccPanel;

        public PanelManagerAMC(Panel accCreatePanel, Panel editAccPanel)
        {
            AccCreatePanel = accCreatePanel;
            EditAccPanel = editAccPanel;
        }
        public void ShowPanel(Panel panelToShow)
        {
            AccCreatePanel.Hide();
            EditAccPanel.Hide();

            panelToShow.Show();
        }
    }
}
