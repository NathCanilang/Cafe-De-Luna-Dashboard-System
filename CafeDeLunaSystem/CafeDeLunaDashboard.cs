using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System.Drawing;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    public partial class CafeDeLunaDashboard : Form
    {
        private readonly MySqlConnection conn;
        private readonly PanelManager panelManager;
        private readonly PanelManagerAP panelManagerAP;
        private readonly PanelManagerAMC panelManagerAMC;

        //Admin Panel
        private string[] positiion = { "Manager", "Cashier" };
        public CafeDeLunaDashboard()
        {
            InitializeComponent();
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);
            panelManager = new PanelManager(LoginPanel, AdminPanel, ManagerPanel, StaffPanel);
            panelManagerAP = new PanelManagerAP(HomePanelAP, AccManagePanel, SettingPanelAP, AddMenuPanelAP);
            panelManagerAMC = new PanelManagerAMC(AccCreatePanel, EditAccPanel);

            //Placeholders
            TxtPlaceholder.PlaceholderHandler usernamePlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter your Username");
            UsernameTxtBLP.Enter += usernamePlaceholder.Enter;
            UsernameTxtBLP.Leave += usernamePlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler passwordPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter your Password");
            PasswordTxtBLP.Enter += passwordPlaceholder.Enter;
            PasswordTxtBLP.Leave += passwordPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler lastNamePlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter last name");
            LastNTxtB_AP.Enter += lastNamePlaceholder.Enter;
            LastNTxtB_AP.Leave += lastNamePlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler firstNamePlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter first name");
            FirstNTxtB_AP.Enter += firstNamePlaceholder.Enter;
            FirstNTxtB_AP.Leave += firstNamePlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler middleNamePlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter middle name");
            MiddleNTxtB_AP.Enter += middleNamePlaceholder.Enter;
            MiddleNTxtB_AP.Leave += middleNamePlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler agePlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter age");
            AgeTxtB_AP.Enter += agePlaceholder.Enter;
            AgeTxtB_AP.Leave += agePlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler emailPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter e-mail");
            EmailTxtB_AP.Enter += emailPlaceholder.Enter;
            EmailTxtB_AP.Leave += emailPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler username_APPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter Username");
            UsernameTxtB_AP.Enter += username_APPlaceholder.Enter;
            UsernameTxtB_AP.Leave += username_APPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler password_APPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter password");
            PasswordTxtB_AP.Enter += password_APPlaceholder.Enter;
            PasswordTxtB_AP.Leave += password_APPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler PositionPlaceholder = new TxtPlaceholder.PlaceholderHandler("Choose Position");
            PositionComB_AP.Enter += PositionPlaceholder.Enter;
            PositionComB_AP.Leave += PositionPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler employeeIDPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter ID");
            EmployeeIDTxtB_AP.Enter += employeeIDPlaceholder.Enter;
            EmployeeIDTxtB_AP.Leave += employeeIDPlaceholder.Leave;

            //Panel Startup
            panelManager.ShowPanel(LoginPanel);
            panelManagerAP.ShowPanel(HomePanelAP);

            //Admin Panel section
        }

        //Login Section
        private void LogBtnLP_Click(object sender, System.EventArgs e)
        {
            string usernameInput = UsernameTxtBLP.Text;
            string passwordInput = PasswordTxtBLP.Text;
            string hsshPasswordInput = Encryptor.HashPassword(PasswordTxtBLP.Text);

            if (usernameInput == "Admin" && passwordInput == "admin123")
            {
                MessageBox.Show("Admin login successful");
                panelManager.ShowPanel(AdminPanel);
            }
            else
            {
                using (conn)
                {
                    conn.Open();

                    string query = "SELECT Position FROM employee_acc WHERE Username = @username AND Password = @password";
                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", usernameInput);
                        command.Parameters.AddWithValue("@password", hsshPasswordInput);

                        object position = command.ExecuteScalar();

                        if (position != null)
                        {
                            string userRole = position.ToString();
                            if (userRole == "Manager")
                            {
                                MessageBox.Show("Login Successful");
                                panelManager.ShowPanel(ManagerPanel);
                            }
                            else if (userRole == "Cashier")
                            {
                                MessageBox.Show("Login Successful");
                                panelManager.ShowPanel(StaffPanel);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                }
            }
        }
        //Admin Panel

        private void AccManageLbl_Click(object sender, System.EventArgs e)
        {
            panelManagerAP.ShowPanel(AccManagePanel);
            panelManagerAMC.ShowPanel(AccCreatePanel);
        }

        private void AddMenuLbl_Click(object sender, System.EventArgs e)
        {
            panelManagerAP.ShowPanel(AddMenuPanelAP);
        }

        private void SettingsLbl_Click(object sender, System.EventArgs e)
        {
            panelManagerAP.ShowPanel(SettingPanelAP);
        }

        private void EditBtn_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to edit accounts?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                panelManagerAMC.ShowPanel(EditAccPanel);
            }
        }

        private void logoutBtn_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log-out?", "information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                panelManager.ShowPanel(LoginPanel);
            }
        }

        private void CancelBtn_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel the operation?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                panelManagerAMC.ShowPanel(AccCreatePanel);
            }
        }

        private void AccDataTbl_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
