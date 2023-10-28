using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
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
        private readonly string[] position = { "Manager", "Cashier" };
        public CafeDeLunaDashboard()
        {
            InitializeComponent();
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);
            panelManager = new PanelManager(LoginPanel, AdminPanel, ManagerPanel, StaffPanel);
            panelManagerAP = new PanelManagerAP(HomePanelAP, AccManagePanel, AddMenuPanelAP);
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

            TxtPlaceholder.PlaceholderHandler username_APPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter username");
            UsernameTxtB_AP.Enter += username_APPlaceholder.Enter;
            UsernameTxtB_AP.Leave += username_APPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler password_APPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter password");
            PasswordTxtB_AP.Enter += password_APPlaceholder.Enter;
            PasswordTxtB_AP.Leave += password_APPlaceholder.Leave;

            TxtPlaceholder.PlaceholderHandler employeeIDPlaceholder = new TxtPlaceholder.PlaceholderHandler("Enter ID");
            EmployeeIDTxtB_AP.Enter += employeeIDPlaceholder.Enter;
            EmployeeIDTxtB_AP.Leave += employeeIDPlaceholder.Leave;

            ComBTxtPlaceholder.PlaceholderHandler positionPlaceholder = new ComBTxtPlaceholder.PlaceholderHandler("Choose position");
            PositionComB_AP.Enter += positionPlaceholder.Enter;
            PositionComB_AP.Leave += positionPlaceholder.Leave;

            //Panel Startup
            panelManager.ShowPanel(LoginPanel);
            panelManagerAP.ShowPanel(HomePanelAP);

            //Admin Panel section
            PositionComB_AP.Items.AddRange(position);
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
        private void EditBtn_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to edit accounts?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                panelManagerAMC.ShowPanel(EditAccPanel);
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

        private void LogoutLbl_Click(object sender, System.EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log-out?", "information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                panelManager.ShowPanel(LoginPanel);
            }
        }

        public Bitmap ResizeImage(Image sourceImage, int maxWidth, int maxHeight)
        {
            double aspectRatio = (double)sourceImage.Width / sourceImage.Height;
            int newWidth, newHeight;

            if (sourceImage.Width > sourceImage.Height)
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
            }
            else
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
            }

            using (Bitmap resizedImage = new Bitmap(newWidth, newHeight))
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(sourceImage, 0, 0, newWidth, newHeight);

                return new Bitmap(resizedImage);
            }
        }

        private void SelectImgBtn_Click(object sender, System.EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    ImgTxtB.Text = selectedFilePath;

                    try
                    {
                        Image selectedImage = Image.FromFile(selectedFilePath);

                        // Define maximum width and height for resizing
                        int maxWidth = 800;
                        int maxHeight = 600;

                        Bitmap resizedImage = ResizeImage(selectedImage, maxWidth, maxHeight);

                        UserPicB.Image = resizedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }
        }
    }
}
