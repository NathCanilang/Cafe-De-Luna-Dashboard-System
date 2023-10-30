using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    public partial class CafeDeLunaDashboard : Form
    {
        public static CafeDeLunaDashboard cafeDeLunaInstance;
        private readonly MySqlConnection conn;
        private readonly PanelManager panelManager;
        private readonly PanelManagerAP panelManagerAP;
        private readonly PanelManagerAMC panelManagerAMC;
        private string employeeFullFilePath;
        private string menuFullFilePath;
        private string varietyFullFilePath;

        //Admin Panel
        private readonly CreateAndEditAcc createAndEditAcc = new CreateAndEditAcc();
        private readonly string[] position = { "Manager", "Cashier" };
        public CafeDeLunaDashboard()
        {
            InitializeComponent();
            cafeDeLunaInstance = this;
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);
            panelManager = new PanelManager(LoginPanel, AdminPanel, ManagerPanel, StaffPanel);
            panelManagerAP = new PanelManagerAP(HomePanelAP, AccManagePanel, AddMenuPanelAP);
            panelManagerAMC = new PanelManagerAMC(AccCreatePanel, EditAccPanel);

            //Placeholders
            TxtPlaceholder.SetPlaceholder(UsernameTxtBLP, "Enter your Username");
            TxtPlaceholder.SetPlaceholder(PasswordTxtBLP, "Enter your Password");
            TxtPlaceholder.SetPlaceholder(LastNTxtB_AP, "Enter last name");
            TxtPlaceholder.SetPlaceholder(FirstNTxtB_AP, "Enter first name");
            TxtPlaceholder.SetPlaceholder(MiddleNTxtB_AP, "Enter middle name");
            TxtPlaceholder.SetPlaceholder(AgeTxtB_AP, "Enter age");
            TxtPlaceholder.SetPlaceholder(EmailTxtB_AP, "Enter e-mail");
            TxtPlaceholder.SetPlaceholder(UsernameTxtB_AP, "Enter username");
            TxtPlaceholder.SetPlaceholder(PasswordTxtB_AP, "Enter password");
            TxtPlaceholder.SetPlaceholder(EmployeeIDTxtB_AP, "Enter ID");
            ComBTxtPlaceholder.SetPlaceholder(PositionComB_AP, "Choose position");

            //Panel Startup
            panelManager.ShowPanel(LoginPanel);
            panelManagerAP.ShowPanel(HomePanelAP);

            //Admin Panel section
            FoodTbl.DataError += new DataGridViewDataErrorEventHandler(createAndEditAcc.FoodTable_DataError);
            FoodTbl.RowPostPaint += new DataGridViewRowPostPaintEventHandler(createAndEditAcc.FoodTable_RowPostPaint);

            MenuSelectComB.DropDownStyle = ComboBoxStyle.DropDownList;
            createAndEditAcc.PopulateMealComboBox();

            PositionComB_AP.Items.AddRange(position);
            PositionComB_AP.DropDownStyle = ComboBoxStyle.DropDownList;

            UserBirthdate.ValueChanged += CalculateAge;
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
        private void CalculateAge(object sender, EventArgs e)
        {
            DateTime selectedDate = UserBirthdate.Value;
            int age = createAndEditAcc.AgeCalculation(selectedDate);
            AgeTxtB_AP.Text = age.ToString();
        }
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

        private void SelectImgBtn_Click(object sender, System.EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFileName = Path.GetFileName(openFileDialog.FileName); // Get just the file name
                    string selectedDirectory = Path.GetDirectoryName(openFileDialog.FileName); // Get the directory
                    employeeFullFilePath = Path.Combine(selectedDirectory, selectedFileName); // Create the full file path

                    ImgTxtB.Text = selectedFileName;

                    try
                    {
                        Image selectedImage = Image.FromFile(employeeFullFilePath);
                        int maxWidth = 160;
                        int maxHeight = 160;

                        Bitmap resizedImage = createAndEditAcc.ResizeImage(selectedImage, maxWidth, maxHeight);

                        UserPicB.Image = resizedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }
        }

        private void CreateBtn_Click(object sender, EventArgs e)
        {
            string adminUsername = "Admin";
            DateTime selectedDate = UserBirthdate.Value;
            string employeeFullName = $"{LastNTxtB_AP.Text}, {FirstNTxtB_AP.Text} {MiddleNTxtB_AP.Text}";
            string userImagePath = ImgTxtB.Text;
            byte[] imageData = null;

            if (!string.IsNullOrEmpty(userImagePath) && File.Exists(employeeFullFilePath))
            {
                using (FileStream fs = new FileStream(employeeFullFilePath, FileMode.Open, FileAccess.Read))
                {
                    imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, (int)fs.Length);
                }
            }
            else
            {
                MessageBox.Show("Invalid image file path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((string.IsNullOrWhiteSpace(LastNTxtB_AP.Text) || LastNTxtB_AP.Text == "Enter last name") ||
                (string.IsNullOrWhiteSpace(FirstNTxtB_AP.Text) || FirstNTxtB_AP.Text == "Enter first name") ||
                (string.IsNullOrWhiteSpace(MiddleNTxtB_AP.Text) || MiddleNTxtB_AP.Text == "Enter middle name") ||
                (string.IsNullOrWhiteSpace(AgeTxtB_AP.Text) || AgeTxtB_AP.Text == "Enter age") ||
                (string.IsNullOrWhiteSpace(UsernameTxtB_AP.Text) || UsernameTxtB_AP.Text == "Enter username") ||
                (string.IsNullOrWhiteSpace(PasswordTxtB_AP.Text) || PasswordTxtB_AP.Text == "Enter password") ||
                PositionComB_AP.SelectedItem == null ||
                string.IsNullOrEmpty(EmployeeIDTxtB_AP.Text) || EmployeeIDTxtB_AP.Text == "Enter ID" ||
                string.IsNullOrEmpty(EmailTxtB_AP.Text) || EmailTxtB_AP.Text == "Enter e-mail")
            {
                MessageBox.Show("Please fill out all the required data", "Missing Informations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (UsernameTxtB_AP.Text == adminUsername)
            {
                MessageBox.Show("The entered username is not allowed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult choices = MessageBox.Show("Are you sure to the information that you have entered?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                string insertQuery = "INSERT INTO employee_acc (Name, Birthday, Age, Email, Username, Password, Position, EmployeeID, EmployeeIMG) " +
                    "values('" + employeeFullName + "', '" + selectedDate + "', '" + AgeTxtB_AP.Text + "', '" + EmailTxtB_AP.Text + "', '" + UsernameTxtB_AP.Text + "', " +
                    "'" + Encryptor.HashPassword(PasswordTxtB_AP.Text) + "', '" + PositionComB_AP.SelectedItem.ToString() + "','" + EmployeeIDTxtB_AP.Text + "','" + imageData + "')";
                MySqlCommand cmdDataBase = new MySqlCommand(insertQuery, conn);

                try
                {
                    conn.Open();
                    cmdDataBase.ExecuteNonQuery();
                    createAndEditAcc.RefreshTbl();
                    MessageBox.Show("Account Created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);


                    /*Name.Clear();
                    UsernameTxtBox.Clear();
                    PasswordTxtBox.Clear();
                    EmailTxtBox.Clear();
                    PositionComBox.SelectedItem = null;*/
                }

                catch (MySqlException a)
                {
                    if (a.Number == 1062)
                    {
                        MessageBox.Show("Username already exists.", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UsernameTxtB_AP.Clear();
                    }
                    else
                    {
                        MessageBox.Show(a.Message, "Registration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                catch (Exception b)
                {
                    MessageBox.Show(b.Message);
                }

                finally
                {
                    conn.Close();
                }
            }
        }

        private void MenuAddImgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFileName = Path.GetFileName(openFileDialog.FileName); // Get just the file name
                    string selectedDirectory = Path.GetDirectoryName(openFileDialog.FileName); // Get the directory
                    menuFullFilePath = Path.Combine(selectedDirectory, selectedFileName); // Create the full file path

                    MenuFilePathTxtB.Text = selectedFileName;

                    try
                    {
                        Image selectedImage = Image.FromFile(menuFullFilePath);

                        // Define maximum width and height for resizing
                        int maxWidth = 120;
                        int maxHeight = 120;

                        Bitmap resizedImage = createAndEditAcc.ResizeImage(selectedImage, maxWidth, maxHeight);

                        MenuPicB.Image = resizedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }
        }
        private void AddMenuBtn_Click(object sender, EventArgs e)
        {
            string connectionString = "server=localhost;user=root;database=dashboarddb;password=";
            string mealName = MenuNTxtB.Text;
            string mealImgPath = MenuFilePathTxtB.Text;

            try
            {
                byte[] imageData = null;
                if (!string.IsNullOrEmpty(mealImgPath) && File.Exists(menuFullFilePath))
                {
                    using (FileStream fs = new FileStream(menuFullFilePath, FileMode.Open, FileAccess.Read))
                    {
                        imageData = new byte[fs.Length];
                        fs.Read(imageData, 0, (int)fs.Length);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid image file path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO meal (MealName, MealImage) VALUES (@mealName, @mealImage)";
                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@mealName", mealName);
                        command.Parameters.AddWithValue("@mealImage", menuFullFilePath);
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("New meal added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    createAndEditAcc.PopulateMealComboBox();
                }
                MenuNTxtB.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VarietyAddImgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select an Image File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFileName = Path.GetFileName(openFileDialog.FileName); // Get just the file name
                    string selectedDirectory = Path.GetDirectoryName(openFileDialog.FileName); // Get the directory
                    varietyFullFilePath = Path.Combine(selectedDirectory, selectedFileName); // Create the full file path

                    VarietyFilePathTxtB.Text = selectedFileName;

                    try
                    {
                        Image selectedImage = Image.FromFile(varietyFullFilePath);

                        int maxWidth = 120;
                        int maxHeight = 120;

                        Bitmap resizedImage = createAndEditAcc.ResizeImage(selectedImage, maxWidth, maxHeight);

                        VariationPicB.Image = resizedImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }
        }

        private void AddVarietyBtn_Click(object sender, EventArgs e)
        {
            string variationName = VariationNmTxtB.Text;
            string variationDescription = VariationDescTxtB.Text;
            decimal variationCost = decimal.Parse(VariationCostTxtB.Text);
            string selectedMenuCategory = MenuSelectComB.SelectedItem.ToString();
            string varietyImagePath = VarietyFilePathTxtB.Text;
            byte[] imageData = null;

            if (!string.IsNullOrEmpty(varietyImagePath) && File.Exists(varietyFullFilePath))
            {
                using (FileStream fs = new FileStream(varietyFullFilePath, FileMode.Open, FileAccess.Read))
                {
                    imageData = new byte[fs.Length];
                    fs.Read(imageData, 0, (int)fs.Length);
                }
            }
            else
            {
                MessageBox.Show("Invalid image file path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int mealID = createAndEditAcc.GetMealIDFromDatabase(selectedMenuCategory);

            if (mealID != -1)
            {
                string connectionString = "server=localhost;user=root;database=dashboarddb;password=";

                try
                {
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO mealvariation (MealImage, MealID, VariationName, VariationDescription, VariationCost) VALUES (@imagePath, @mealID, @variationName, @variationDescription, @variationCost)";
                        using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@mealID", mealID);
                            command.Parameters.AddWithValue("@variationName", variationName);
                            command.Parameters.AddWithValue("@variationDescription", variationDescription);
                            command.Parameters.AddWithValue("@variationCost", variationCost);
                            command.Parameters.AddWithValue("@imagePath", varietyFullFilePath);
                            command.ExecuteNonQuery();
                        }

                        MessageBox.Show("New variation added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    MenuSelectComB.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Invalid menu category. Unable to add variation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            createAndEditAcc.LoadMenuItems();
        }
    }
}
