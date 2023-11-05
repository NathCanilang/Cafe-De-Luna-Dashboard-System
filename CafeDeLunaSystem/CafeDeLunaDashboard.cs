using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
using Image = System.Drawing.Image;
using System.Data;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using static System.Net.Mime.MediaTypeNames;


namespace CafeDeLunaSystem
{
    public partial class CafeDeLunaDashboard : Form
    {
        MySqlConnection cn;
        MySqlCommand cm;
        MySqlDataReader dr;
        private PictureBox pic;
        private PictureBox menupic;
        private Label price;
        private Label mealname;
        public static CafeDeLunaDashboard cafeDeLunaInstance;
        private readonly MySqlConnection conn;
        private readonly PanelManager panelManager;
        private readonly PanelManagerAP panelManagerAP;
        private readonly PanelManagerAMC panelManagerAMC;
        
        private byte[] imageData;
        private decimal totalPrice = 0.00m;
        private bool isSearchTextPlaceholder = true;
        bool isNewImageSelected = false;
        bool isNewFoodImageSelected = false;
        private readonly ManagerPMethods sales;
        //
        private bool IsEditMode = false;
        public int EmployeeIDBeingEdited = -1;
        private bool IsPasswordChanged = false;
        private int GenerateID = orderIDGenerator();
        private int employeeID;

        //Admin Panel
        private readonly CreateAndEditAcc createAndEditAcc = new CreateAndEditAcc();
        private readonly string[] position = { "Manager", "Cashier" };
        List<PictureBox> clickedPictureBoxes = new List<PictureBox>();

        //Manger
        

        //To be deleted
        private string employeeFullFilePath;
        private string menuFullFilePath;
        private string varietyFullFilePath;

        public CafeDeLunaDashboard()
        {
            InitializeComponent();
            cafeDeLunaInstance = this;
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);
            panelManager = new PanelManager(LoginPanel, AdminPanel, ManagerPanel, StaffPanel);
            panelManagerAP = new PanelManagerAP(HomePanelAP, AccManagePanel, AddMenuPanelAP);
            panelManagerAMC = new PanelManagerAMC(AccCreatePanel);

            //Placeholders
            TxtPlaceholder.SetPlaceholder(UsernameTxtBLP, "Enter your Username");
            TxtPlaceholder.SetPlaceholder(PasswordTxtBLP, "Enter your Password");
            TxtPlaceholder.SetPlaceholder(LastNTxtB_AP, "Last name");
            TxtPlaceholder.SetPlaceholder(FirstNTxtB_AP, "First name");
            TxtPlaceholder.SetPlaceholder(MiddleNTxtB_AP, "Middle name");
            TxtPlaceholder.SetPlaceholder(EmployeeIDTxtB_AP, "Enter ID");
            TxtPlaceholder.SetPlaceholder(MenuNTxtB, "Menu Name");
            TxtPlaceholder.SetPlaceholder(VariationNmTxtB, "Food Name");
            TxtPlaceholder.SetPlaceholder(VariationDescTxtB, "Description");
            TxtPlaceholder.SetPlaceholder(VariationCostTxtB, "Price");

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

            //Manager section

        }

        //Login Section
        private void LogBtnLP_Click(object sender, System.EventArgs e)
        {
            string usernameInput = UsernameTxtBLP.Text;
            string passwordInput = PasswordTxtBLP.Text;
            string hsshPasswordInput = Encryptor.HashPassword(PasswordTxtBLP.Text);

            if (usernameInput == "Admin" && passwordInput == "admin123")
            {
                MessageBox.Show("Admin login successful", "Welcome, Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panelManager.ShowPanel(AdminPanel);
            }
            else if (usernameInput == "Staff" && passwordInput == "staff123")
            {
                MessageBox.Show("Staff login successful", "Welcome, Staff", MessageBoxButtons.OK, MessageBoxIcon.Information);
                panelManager.ShowPanel(StaffPanel);
            }
            else
            {
                using (conn)
                {
                    conn.Open();

                    string query = "SELECT Position, EmployeeID FROM employee_acc WHERE Username = @username AND Password = @password";
                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@username", usernameInput);
                        command.Parameters.AddWithValue("@password", hsshPasswordInput);

                        object position = command.ExecuteScalar();

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (position != null)
                                {
                                    string userRole = position.ToString();
                                    employeeID = reader.GetInt32("EmployeeID");
                                    if (userRole == "Manager")
                                    {
                                        MessageBox.Show("Login Successful", "Welcome, Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        panelManager.ShowPanel(StaffPanel);
                                        PositionTxtBox.Text = "Manager";
                                    }
                                    else if (userRole == "Cashier")
                                    {
                                        MessageBox.Show("Login Successful", "Welcome, Staff", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        panelManager.ShowPanel(StaffPanel);
                                        PositionTxtBox.Text = "Staff";
                                        SalesBtn.Hide();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Invalid username or password.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
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
        private void SalesRepLbl_Click(object sender, EventArgs e)
        {
            panelManager.ShowPanel(ManagerPanel);
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
                if (AccDataTbl.SelectedRows.Count == 1)
                {
                    IsEditMode = true;
                    UpdateBtn.Show();
                    CancelBtn.Show();
                    CreateBtn.Hide();
                    EditBtn.Hide();

                    DataGridViewRow selectedRow = AccDataTbl.SelectedRows[0];
                    string nameColumn = selectedRow.Cells["Name"].Value.ToString();
                    string birthdayColumn = selectedRow.Cells["Birthday"].Value.ToString().Trim();
                    string ageColumn = selectedRow.Cells["Age"].Value.ToString();
                    string emailColumn = selectedRow.Cells["Email"].Value.ToString();
                    string usernameColumn = selectedRow.Cells["Username"].Value.ToString();
                    string positionColumn = selectedRow.Cells["Position"].Value.ToString();
                    int employeeIDColumn = Convert.ToInt32(AccDataTbl.SelectedRows[0].Cells["EmployeeID"].Value);
                    string[] nameParts = nameColumn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    EmployeeIDBeingEdited = Convert.ToInt32(AccDataTbl.SelectedRows[0].Cells["EmployeeID"].Value);


                    if (nameParts.Length > 0)
                    {
                        string lastName = nameParts[0].Trim();      // Trim the last name
                        LastNTxtB_AP.Text = lastName;
                    }

                    if (nameParts.Length > 1)
                    {
                        string[] firstMiddleNameParts = nameParts[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (firstMiddleNameParts.Length > 0)
                        {
                            string firstName = firstMiddleNameParts[0].Trim();     // Trim the first name
                            FirstNTxtB_AP.Text = firstName;
                        }

                        if (firstMiddleNameParts.Length > 1)
                        {
                            string middleName = firstMiddleNameParts[1].Trim();    // Trim the middle name
                            MiddleNTxtB_AP.Text = middleName;
                        }
                    }
                    if (DateTime.TryParse(birthdayColumn, out DateTime birthday))
                    {
                        UserBirthdate.Value = birthday;
                    }
                    else
                    {
                        MessageBox.Show("Invalid date format in the 'Birthday' column.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    AgeTxtB_AP.Text = ageColumn;
                    EmailTxtB_AP.Text = emailColumn;
                    UsernameTxtB_AP.Text = usernameColumn;
                    PositionComB_AP.Text = positionColumn;
                    EmployeeIDTxtB_AP.Text = employeeIDColumn.ToString();
                    createAndEditAcc.LoadMenuItemImage(employeeIDColumn);
                }
                else
                {
                    MessageBox.Show("Please select a single row for editing.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

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
                    try
                    {
                        // Load the selected image
                        Image selectedImage = Image.FromFile(openFileDialog.FileName);

                        // Check if the image dimensions are 64x64 pixels
                        if (selectedImage.Width == 64 && selectedImage.Height == 64)
                        {
                            UserPicB.Image = selectedImage;
                            isNewImageSelected = true; // Set the flag to true
                        }
                        else
                        {
                            MessageBox.Show("Please select an image with dimensions of 64x64 pixels.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
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
            //string userImagePath = ImgTxtB.Text;

            if (UserPicB.Image == null)
            {
                MessageBox.Show("Please select an image.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            DialogResult choices = MessageBox.Show("Are you sure the information you have entered is correct?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        UserPicB.Image.Save(ms, ImageFormat.Jpeg); // You can choose the format you want
                        imageData = ms.ToArray();
                    }
                    string insertQuery = "INSERT INTO employee_acc(Name, Birthday, Age, Email, Username, Password, Position, EmployeeID, EmployeeIMG) " +
                        "VALUES (@Name, @Birthday, @Age, @Email, @Username, @Password, @Position, @EmployeeID, @EmployeeIMG)";

                    MySqlCommand cmdDataBase = new MySqlCommand(insertQuery, conn); cmdDataBase.Parameters.AddWithValue("@Name", employeeFullName);
                    cmdDataBase.Parameters.AddWithValue("@Birthday", selectedDate);
                    cmdDataBase.Parameters.AddWithValue("@Age", AgeTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@Email", EmailTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@Username", UsernameTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@Password", Encryptor.HashPassword(PasswordTxtB_AP.Text));
                    cmdDataBase.Parameters.AddWithValue("@Position", PositionComB_AP.SelectedItem.ToString());
                    cmdDataBase.Parameters.AddWithValue("@EmployeeID", EmployeeIDTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@EmployeeIMG", imageData);
                    cmdDataBase.ExecuteNonQuery();

                    createAndEditAcc.RefreshTbl();
                    MessageBox.Show("Account Created!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void UpdateBtn_Click(object sender, EventArgs e)
        {
            string adminUsername = "Admin";
            DateTime selectedDate = UserBirthdate.Value;
            string employeeFullName = $"{LastNTxtB_AP.Text}, {FirstNTxtB_AP.Text} {MiddleNTxtB_AP.Text}";
            //string userImagePath = ImgTxtB.Text;

            if (UserPicB.Image == null)
            {
                MessageBox.Show("Please select an image.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((string.IsNullOrWhiteSpace(LastNTxtB_AP.Text) || LastNTxtB_AP.Text == "Enter last name") ||
                (string.IsNullOrWhiteSpace(FirstNTxtB_AP.Text) || FirstNTxtB_AP.Text == "Enter first name") ||
                (string.IsNullOrWhiteSpace(MiddleNTxtB_AP.Text) || MiddleNTxtB_AP.Text == "Enter middle name") ||
                (string.IsNullOrWhiteSpace(AgeTxtB_AP.Text) ||
                (string.IsNullOrWhiteSpace(UsernameTxtB_AP.Text) ||
                PositionComB_AP.SelectedItem == null ||
                string.IsNullOrEmpty(EmployeeIDTxtB_AP.Text) ||
                string.IsNullOrEmpty(EmailTxtB_AP.Text))))
            {
                MessageBox.Show("Please fill out all the required data", "Missing Informations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (UsernameTxtB_AP.Text == adminUsername)
            {
                MessageBox.Show("The entered username is not allowed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (UsernameTxtB_AP.Text == adminUsername)
            {
                MessageBox.Show("The entered username is not allowed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult choices = MessageBox.Show("Are you sure the information you have entered is correct?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    string updateQuery;
                    if (string.IsNullOrEmpty(PasswordTxtB_AP.Text))
                    {
                        // If password field is empty, don't update the password
                        updateQuery = "UPDATE employee_acc " +
                            "SET Name = @Name, Birthday = @Birthday, Age = @Age, Email = @Email, " +
                            "Username = @Username, Position = @Position";

                        if (isNewImageSelected)
                        {
                            updateQuery += ", EmployeeIMG = @EmployeeIMG";
                        }

                        updateQuery += " WHERE EmployeeID = @EmployeeID";
                    }
                    else
                    {
                        // If password field is not empty, update the password
                        updateQuery = "UPDATE employee_acc " +
                            "SET Name = @Name, Birthday = @Birthday, Age = @Age, Email = @Email, " +
                            "Username = @Username, Password = @Password, Position = @Position";

                        if (isNewImageSelected)
                        {
                            updateQuery += ", EmployeeIMG = @EmployeeIMG";
                        }

                        updateQuery += " WHERE EmployeeID = @EmployeeID";
                    }

                    MySqlCommand cmdDataBase = new MySqlCommand(updateQuery, conn);
                    cmdDataBase.Parameters.AddWithValue("@Name", employeeFullName);
                    cmdDataBase.Parameters.AddWithValue("@Birthday", selectedDate);
                    cmdDataBase.Parameters.AddWithValue("@Age", AgeTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@Email", EmailTxtB_AP.Text);
                    cmdDataBase.Parameters.AddWithValue("@Username", UsernameTxtB_AP.Text);

                    if (!string.IsNullOrEmpty(PasswordTxtB_AP.Text))
                    {
                        cmdDataBase.Parameters.AddWithValue("@Password", Encryptor.HashPassword(PasswordTxtB_AP.Text));
                    }

                    cmdDataBase.Parameters.AddWithValue("@Position", PositionComB_AP.SelectedItem.ToString());
                    cmdDataBase.Parameters.AddWithValue("@EmployeeID", EmployeeIDTxtB_AP.Text);

                    if (isNewImageSelected)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            UserPicB.Image.Save(ms, ImageFormat.Jpeg); // You can choose the format you want
                            byte[] imageData = ms.ToArray();
                            cmdDataBase.Parameters.AddWithValue("@EmployeeIMG", imageData);
                        }
                    }

                    cmdDataBase.ExecuteNonQuery();

                    createAndEditAcc.RefreshTbl();
                    MessageBox.Show("Account Updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            UpdateBtn.Hide();
            CancelBtn.Hide();
            CreateBtn.Show();
            EditBtn.Show();

            TxtPlaceholder.SetPlaceholder(LastNTxtB_AP, "Last name");
            TxtPlaceholder.SetPlaceholder(FirstNTxtB_AP, "First name");
            TxtPlaceholder.SetPlaceholder(MiddleNTxtB_AP, "Middle name");
            UserBirthdate.Value = DateTime.Today;
            AgeTxtB_AP.Text = "";
            UsernameTxtB_AP.Text = "";
            PasswordTxtB_AP.Text = "";
            EmailTxtB_AP.Text = "";
            PositionComB_AP.SelectedIndex = -1;
            UserPicB.Image = null;
            ImgTxtB.Text = "";

            panelManagerAP.ShowPanel(AccManagePanel);
            panelManagerAMC.ShowPanel(AccCreatePanel);
        }

        private void CancelBtn_Click_1(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to cancel the operation?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                UpdateBtn.Hide();
                CancelBtn.Hide();
                CreateBtn.Show();
                EditBtn.Show();

                TxtPlaceholder.SetPlaceholder(LastNTxtB_AP, "Last name");
                TxtPlaceholder.SetPlaceholder(FirstNTxtB_AP, "First name");
                TxtPlaceholder.SetPlaceholder(MiddleNTxtB_AP, "Middle name");
                UserBirthdate.Value = DateTime.Today;
                AgeTxtB_AP.Text = "";
                UsernameTxtB_AP.Text = "";
                PasswordTxtB_AP.Text = "";
                EmailTxtB_AP.Text = "";
                PositionComB_AP.SelectedIndex = -1;
                UserPicB.Image = null;
                ImgTxtB.Text = "";

                panelManagerAP.ShowPanel(AccManagePanel);
                panelManagerAMC.ShowPanel(AccCreatePanel);
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
                    string selectedFilePath = openFileDialog.FileName;
                    MenuFilePathTxtB.Text = selectedFilePath;

                    try
                    {
                        Image originalImage = Image.FromFile(selectedFilePath);
                        Image resizedImage = createAndEditAcc.ResizeImages(originalImage, 64, 64);

                        // Check if the image dimensions are 64x64 pixels
                        if (resizedImage.Width == 64 && resizedImage.Height == 64)
                        {
                            MenuPicB.Image = resizedImage;
                        }
                        else
                        {
                            MessageBox.Show("Error resizing the image to 64x64 pixels.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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
            string mealName = MenuNTxtB.Text;
            string mealImgPath = MenuFilePathTxtB.Text;

            if (string.IsNullOrWhiteSpace(mealImgPath) || !File.Exists(mealImgPath))
            {
                MessageBox.Show("Invalid image file path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((string.IsNullOrWhiteSpace(MenuNTxtB.Text) || MenuNTxtB.Text == "Menu Name") || string.IsNullOrEmpty(mealImgPath))
            {
                MessageBox.Show("Please fill out all the required data", "Missing Informations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult choices = MessageBox.Show("Are you sure the information you have entered is correct?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        MenuPicB.Image.Save(ms, ImageFormat.Jpeg);
                        imageData = ms.ToArray();
                    }
                    string insertQuery = "INSERT INTO meal (MealName, MealImage) VALUES (@mealName, @mealImage)";
                    MySqlCommand command = new MySqlCommand(insertQuery, conn);
                    command.Parameters.AddWithValue("@mealName", mealName);
                    command.Parameters.AddWithValue("@mealImage", imageData);
                    command.ExecuteNonQuery();

                    createAndEditAcc.PopulateMealComboBox();
                    MessageBox.Show("New meal added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    TxtPlaceholder.SetPlaceholder(MenuNTxtB, "Menu Name");
                    MenuFilePathTxtB.Text = null;
                    MenuPicB.Image = null;

                }
                catch (MySqlException a)
                {
                    if (a.Number == 1062)
                    {
                        MessageBox.Show("Menu already exists", "Food Creattion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
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
                    try
                    {
                        // Load the selected image
                        Image selectedImage = Image.FromFile(openFileDialog.FileName);

                        // Check if the image dimensions are 64x64 pixels
                        if (selectedImage.Width == 64 && selectedImage.Height == 64)
                        {
                            VariationPicB.Image = selectedImage;
                            isNewFoodImageSelected = true; // Set the flag to true
                        }
                        else
                        {
                            MessageBox.Show("Please select an image with dimensions of 64x64 pixels.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }
        }

        private void UpdateMealBtn_Click(object sender, EventArgs e)
        {
            string variationName = VariationNmTxtB.Text;
            string variationDescription = VariationDescTxtB.Text;
            decimal variationCost = decimal.Parse(VariationCostTxtB.Text);
            string variationCostText = VariationCostTxtB.Text;
            string selectedMenuCategory = MenuSelectComB.SelectedItem.ToString();
            string variationImgPath = VarietyFilePathTxtB.Text;
            int variationID = Convert.ToInt32(FoodTbl.SelectedRows[0].Cells["VariationID"].Value);
            //string variationID = VariationIDTxtBox.Text;
            int mealID = createAndEditAcc.GetMealIDFromDatabase(selectedMenuCategory);

            if (string.IsNullOrWhiteSpace(variationCostText) || !decimal.TryParse(variationCostText, out variationCost))
            {
                MessageBox.Show("Invalid variation cost.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((string.IsNullOrWhiteSpace(variationName) || variationName == "Food Name") ||
                string.IsNullOrEmpty(variationDescription) || variationDescription == "Description")
            {
                MessageBox.Show("Please fill out all the required data", "Missing Informations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult choices = MessageBox.Show("Are you sure the information you have entered is correct?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    string updateQuery = "UPDATE mealvariation " +
                "SET VariationName = @variationName, VariationDescription = @variationDescription, VariationCost = @variationCost, MealID = @mealID";

                    if (isNewFoodImageSelected)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            VariationPicB.Image.Save(ms, ImageFormat.Jpeg); // You can choose the format you want
                            byte[] imageData = ms.ToArray();
                            updateQuery += ", MealImage = @mealImage";
                        }
                    }

                    updateQuery += " WHERE VariationID = @variationID";
                    MySqlCommand cmdDataBase = new MySqlCommand(updateQuery, conn);
                    cmdDataBase.Parameters.AddWithValue("@variationName", variationName);
                    cmdDataBase.Parameters.AddWithValue("@variationDescription", variationDescription);
                    cmdDataBase.Parameters.AddWithValue("@variationCost", variationCost);
                    cmdDataBase.Parameters.AddWithValue("@mealID", mealID);
                    cmdDataBase.Parameters.AddWithValue("@variationID", variationID);

                    if (isNewFoodImageSelected)
                    {
                        cmdDataBase.Parameters.AddWithValue("@MealImage", imageData);
                    }

                    cmdDataBase.ExecuteNonQuery();

                    createAndEditAcc.LoadMenuItems();
                    MessageBox.Show("Account Updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                catch (MySqlException a)
                {
                    if (a.Number == 1062)
                    {
                        MessageBox.Show("Variation name already exist.", "Add variation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UsernameTxtB_AP.Clear();
                    }
                    else
                    {
                        MessageBox.Show(a.Message, "Add variation", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            UpdateMealBtn.Hide();
            CancelMealBtn.Hide();
            DeleteFoodlBtn.Show();
            EditMealBtn.Show();

            TxtPlaceholder.SetPlaceholder(VariationNmTxtB, "Food Name");
            TxtPlaceholder.SetPlaceholder(VariationDescTxtB, "Description");
            TxtPlaceholder.SetPlaceholder(VariationCostTxtB, "Price");
            VariationPicB.Image = null;
            VarietyFilePathTxtB.Text = "";
            MenuSelectComB.SelectedIndex = -1;
            VariationIDTxtBox.Clear();

            panelManagerAP.ShowPanel(AddMenuPanelAP);

        }

        private void AddVarietyBtn_Click(object sender, EventArgs e)
        {
            string variationName = VariationNmTxtB.Text;
            string variationDescription = VariationDescTxtB.Text;
            decimal variationCost = decimal.Parse(VariationCostTxtB.Text);
            string variationCostText = VariationCostTxtB.Text;
            string selectedMenuCategory = MenuSelectComB.SelectedItem.ToString();
            string variationImgPath = VarietyFilePathTxtB.Text;
            int mealID = createAndEditAcc.GetMealIDFromDatabase(selectedMenuCategory);

            if (string.IsNullOrWhiteSpace(variationCostText) || !decimal.TryParse(variationCostText, out variationCost))
            {
                MessageBox.Show("Invalid variation cost.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(variationImgPath) || !File.Exists(variationImgPath))
            {
                MessageBox.Show("Invalid image file path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if ((string.IsNullOrWhiteSpace(variationName) || variationName == "Food Name") ||
                string.IsNullOrEmpty(variationDescription) || variationDescription == "Description" ||

                string.IsNullOrEmpty(variationImgPath))
            {
                MessageBox.Show("Please fill out all the required data", "Missing Informations", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult choices = MessageBox.Show("Are you sure the information you have entered is correct?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (choices == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    using (MemoryStream ms = new MemoryStream())
                    {
                        VariationPicB.Image.Save(ms, ImageFormat.Jpeg);
                        imageData = ms.ToArray();
                    }
                    string insertQuery = "INSERT INTO mealvariation (MealImage, MealID, VariationName, VariationDescription, VariationCost ) " +
                        "VALUES (@variationImage, @mealID, @variationName, @variationDescription, @variationCost)";

                    MySqlCommand command = new MySqlCommand(insertQuery, conn);
                    command.Parameters.AddWithValue("@variationImage", imageData);
                    command.Parameters.AddWithValue("@mealID", mealID);
                    command.Parameters.AddWithValue("@variationName", variationName);
                    command.Parameters.AddWithValue("@variationDescription", variationDescription);
                    command.Parameters.AddWithValue("@variationCost", variationCost);

                    command.ExecuteNonQuery();

                    MessageBox.Show("New variation added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    TxtPlaceholder.SetPlaceholder(VariationNmTxtB, "Food Name");
                    TxtPlaceholder.SetPlaceholder(VariationDescTxtB, "Description");
                    TxtPlaceholder.SetPlaceholder(VariationCostTxtB, "Price");
                    VarietyFilePathTxtB.Text = "";
                    VariationPicB.Image = null;
                }
                catch (MySqlException a)
                {
                    if (a.Number == 1062)
                    {
                        MessageBox.Show("Menu already exists", "Food Creattion", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
                createAndEditAcc.LoadMenuItems();
            }
        }
        private void EditMealBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to edit accounts?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (FoodTbl.SelectedRows.Count == 1)
                {
                    DataGridViewRow selectedRow = FoodTbl.SelectedRows[0];

                    string variationName = selectedRow.Cells["VariationName"].Value.ToString();
                    string variationDesc = selectedRow.Cells["VariationDescription"].Value.ToString().Trim();
                    string variationCost = selectedRow.Cells["VariationCost"].Value.ToString();
                    string mealID = selectedRow.Cells["MealID"].Value.ToString();
                    string variationID = selectedRow.Cells["VariationID"].Value.ToString();
                    int variationIDColumn = Convert.ToInt32(FoodTbl.SelectedRows[0].Cells["VariationID"].Value);

                    VariationNmTxtB.Text = variationName;
                    VariationDescTxtB.Text = variationDesc;
                    VariationCostTxtB.Text = variationCost;
                    VariationIDTxtBox.Text = variationID;
                    createAndEditAcc.LoadMenuItemImageFood(variationIDColumn);

                    try
                    {
                        conn.Open();
                        string sqlQuery = "SELECT MealName FROM meal WHERE mealID = @mealID";
                        MySqlCommand cmdDataBase = new MySqlCommand(sqlQuery, conn);
                        cmdDataBase.Parameters.AddWithValue("@mealID", mealID); // Replace 'yourMealID' with the actual mealID
                        MySqlDataReader reader = cmdDataBase.ExecuteReader();

                        // Loop through the results and add them to the ComboBox

                        if (reader.Read())
                        {
                            string mealName = reader.GetString(0);
                            MenuSelectComB.SelectedItem = mealName; // Set the selected item in the ComboBox
                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }

                }
                else
                {
                    MessageBox.Show("Please select a single row for editing.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
            UpdateMealBtn.Show();
            CancelMealBtn.Show();
            DeleteFoodlBtn.Hide();
            EditMealBtn.Hide();
        }
        private void DeleteFoodlBtn_Click(object sender, EventArgs e)
        {
            if (FoodTbl.SelectedRows.Count == 1)
            {
                DataGridViewRow selectedRow = FoodTbl.SelectedRows[0];
                int variationIDColumn = Convert.ToInt32(FoodTbl.SelectedRows[0].Cells["VariationID"].Value);

                DialogResult result = MessageBox.Show("Are you sure you want to delete this row?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        conn.Open();
                        string deleteQuery = "DELETE FROM mealvariation WHERE VariationID = @VariationID";
                        MySqlCommand cmdDataBase = new MySqlCommand(deleteQuery, conn);
                        cmdDataBase.Parameters.AddWithValue("@VariationID", variationIDColumn);
                        cmdDataBase.ExecuteNonQuery();

                        createAndEditAcc.LoadMenuItems();
                        MessageBox.Show("Row Deleted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a single row for deletion.", "Try again", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CancelMealBtn_Click(object sender, EventArgs e)
        {
            UpdateMealBtn.Hide();
            CancelMealBtn.Hide();
            DeleteFoodlBtn.Show();
            EditMealBtn.Show();

            TxtPlaceholder.SetPlaceholder(VariationNmTxtB, "Food Name");
            TxtPlaceholder.SetPlaceholder(VariationDescTxtB, "Description");
            TxtPlaceholder.SetPlaceholder(VariationCostTxtB, "Price");
            VariationPicB.Image = null;
            VarietyFilePathTxtB.Text = "";
            MenuSelectComB.SelectedIndex = -1;
            VariationIDTxtBox.Clear();

            panelManagerAP.ShowPanel(AddMenuPanelAP);
        }

        //Staff panel
        public static int orderIDGenerator()
        {
            Random random = new Random();
            return random.Next(1000, 1000000);
        }

        private void SalesBtn_Click(object sender, EventArgs e)
        {
            panelManager.ShowPanel(ManagerPanel);
        }

        public void GetData()
        {
            conn.Close();
            flowLayoutPanel1.Controls.Clear();
            conn.Open();
            cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation", conn);
            dr = cm.ExecuteReader();

            while (dr.Read())
            {
                byte[] imageBytes = (byte[])dr["MealImage"];

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image mealImage = Image.FromStream(ms);
                    pic = new PictureBox
                    {
                        Width = 150,
                        Height = 150,
                        BackgroundImage = mealImage,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = dr["VariationID"].ToString(),
                    };

                    price = new Label
                    {
                        Text = "Php. " + dr["VariationCost"].ToString(),
                        Width = 25,
                        Height = 15,
                        TextAlign = ContentAlignment.TopLeft,
                        Dock = DockStyle.Top,
                        BackColor = Color.White,
                    };

                    mealname = new Label
                    {
                        Text = dr["VariationName"].ToString(),
                        Width = 25,
                        Height = 15,
                        TextAlign = ContentAlignment.BottomCenter,
                        Dock = DockStyle.Bottom,
                        BackColor = Color.White,
                    };

                    pic.Controls.Add(mealname);
                    pic.Controls.Add(price);
                    flowLayoutPanel1.Controls.Add(pic);
                    pic.Click += OnFLP1Click;
                }
            }
            dr.Close();
            conn.Close();
        }

        public void GetData2()
        {
            conn.Close();
            flowLayoutPanel2.Controls.Clear();
            conn.Open();
            cm = new MySqlCommand("SELECT MealImage, MealID FROM meal WHERE MealID>=24", conn);
            dr = cm.ExecuteReader();

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,  // One column for one picture per row
            };

            while (dr.Read())
            {
                int mealID = (int)dr["MealID"];
                byte[] imageBytes = (byte[])dr["MealImage"];

                using (MemoryStream ms = new MemoryStream(imageBytes))
                {
                    Image mealImage = Image.FromStream(ms);
                    menupic = new PictureBox
                    {
                        Width = 140,
                        Height = 125,
                        BackgroundImage = mealImage,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = mealID.ToString(),
                    };
                    table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    table.Controls.Add(menupic);
                    flowLayoutPanel2.Controls.Add(table);
                    menupic.Click += OnFLP2Click;
                }
            }
            dr.Close();
            conn.Close();
        }

        private void OnFLP2Click(object sender, EventArgs e)
        {
            if (sender is PictureBox clickedPic)
            {
                string mealID = clickedPic.Tag.ToString();
                DisplayVariationNamesByMealID(mealID);
            }
        }
        private void DisplayVariationNamesByMealID(string mealID)
        {
            flowLayoutPanel1.Controls.Clear();
            conn.Open();
            cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation WHERE MealID = @mealID", conn);
            cm.Parameters.AddWithValue("@mealID", mealID);
            dr = cm.ExecuteReader();

            while (dr.Read())
            {
                string mealName = dr["VariationName"].ToString();

                if (!dr.IsDBNull(dr.GetOrdinal("MealImage")))
                {
                    byte[] imageBytes = (byte[])dr["MealImage"];

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image mealImage = Image.FromStream(ms);
                        pic = new PictureBox
                        {
                            Width = 150,
                            Height = 150,
                            BackgroundImage = mealImage,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Tag = dr["VariationID"].ToString(),
                        };

                        price = new Label
                        {
                            Text = "Php. " + dr["VariationCost"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.TopLeft,
                            Dock = DockStyle.Top,
                            BackColor = Color.White,
                        };

                        mealname = new Label
                        {
                            Text = dr["VariationName"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.BottomCenter,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.White,

                        };

                        pic.Controls.Add(mealname);
                        pic.Controls.Add(price);
                        flowLayoutPanel1.Controls.Add(pic);
                        pic.Click += OnFLP1Click;
                    }
                }
            }
            dr.Close();
            conn.Close();
        }

        private void allBtn_Click(object sender, EventArgs e)
        {
            GetData();
        }

        private void coffBtn_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            conn.Open();
            int Mealid = 23 + 1;
            cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation WHERE MealID ='" + Mealid + "'", conn);
            dr = cm.ExecuteReader();

            while (dr.Read())
            {
                string mealName = dr["VariationName"].ToString();

                if (!dr.IsDBNull(dr.GetOrdinal("MealImage")))
                {
                    byte[] imageBytes = (byte[])dr["MealImage"];

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image mealImage = Image.FromStream(ms);
                        pic = new PictureBox
                        {
                            Width = 150,
                            Height = 150,
                            BackgroundImage = mealImage,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Tag = dr["VariationID"].ToString(),
                        };

                        price = new Label
                        {
                            Text = "Php. " + dr["VariationCost"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.TopLeft,
                            Dock = DockStyle.Top,
                            BackColor = Color.White,

                        };

                        mealname = new Label
                        {
                            Text = dr["VariationName"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.BottomCenter,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.White,
                        };

                        pic.Controls.Add(mealname);
                        pic.Controls.Add(price);
                        flowLayoutPanel1.Controls.Add(pic);
                        pic.Click += OnFLP1Click;
                    }
                }
            }
            dr.Close();
            conn.Close();
        }

        private void breakBtn_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            conn.Open();
            cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation WHERE MealID = '25'", conn);
            dr = cm.ExecuteReader();

            while (dr.Read())
            {
                string mealName = dr["VariationName"].ToString();

                if (!dr.IsDBNull(dr.GetOrdinal("MealImage")))
                {
                    byte[] imageBytes = (byte[])dr["MealImage"];

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image mealImage = Image.FromStream(ms);
                        pic = new PictureBox
                        {
                            Width = 150,
                            Height = 150,
                            BackgroundImage = mealImage,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Tag = dr["VariationID"].ToString(),
                        };

                        price = new Label
                        {
                            Text = "Php. " + dr["VariationCost"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.TopLeft,
                            Dock = DockStyle.Top,
                            BackColor = Color.White,

                        };

                        mealname = new Label
                        {
                            Text = dr["VariationName"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.BottomCenter,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.White,
                        };

                        pic.Controls.Add(mealname);
                        pic.Controls.Add(price);
                        flowLayoutPanel1.Controls.Add(pic);
                        pic.Click += OnFLP1Click;
                    }
                }
            }
            dr.Close();
            conn.Close();
        }

        private void snackBtn_Click(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            conn.Open();
            cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation WHERE MealID = '26'", conn);
            dr = cm.ExecuteReader();

            while (dr.Read())
            {
                string mealName = dr["VariationName"].ToString();

                if (!dr.IsDBNull(dr.GetOrdinal("MealImage")))
                {
                    byte[] imageBytes = (byte[])dr["MealImage"];

                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        Image mealImage = Image.FromStream(ms);
                        pic = new PictureBox
                        {
                            Width = 150,
                            Height = 150,
                            BackgroundImage = mealImage,
                            BackgroundImageLayout = ImageLayout.Stretch,
                            Tag = dr["VariationID"].ToString(),
                        };

                        price = new Label
                        {
                            Text = "Php. " + dr["VariationCost"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.TopLeft,
                            Dock = DockStyle.Top,
                            BackColor = Color.White,

                        };

                        mealname = new Label
                        {
                            Text = dr["VariationName"].ToString(),
                            Width = 25,
                            Height = 15,
                            TextAlign = ContentAlignment.BottomCenter,
                            Dock = DockStyle.Bottom,
                            BackColor = Color.White,
                        };

                        pic.Controls.Add(mealname);
                        pic.Controls.Add(price);
                        flowLayoutPanel1.Controls.Add(pic);
                        pic.Click += OnFLP1Click;
                    }
                }
            }
            dr.Close();
            conn.Close();
        }

        private void OnFLP1Click(object sender, EventArgs e)
        {
            PictureBox clickedPic = (PictureBox)sender;
            string tag = clickedPic.Tag.ToString();
            conn.Open();
            cm = new MySqlCommand("Select * from mealvariation where VariationID like'" + tag + "'", conn);
            dr = cm.ExecuteReader();
            dr.Read();

            if (dr.HasRows)
            {
                string variationName = dr["VariationName"].ToString();
                string variationCost = dr["VariationCost"].ToString();
                string quantity = dr["qty"].ToString();

                // Check if a variation with the same VariationName already exists in the DataGridView
                bool exists = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == variationName)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    DialogResult result = MessageBox.Show("Do you want to add this variation?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        dataGridView1.Rows.Add(variationName, "-", quantity, "+", variationCost, "X");
                        // Update the total price
                        UpdateTotalPrice();
                    }
                }
                else
                {
                    MessageBox.Show("This variation is already in the cart.", "Try another one", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            dr.Close();
            conn.Close();

        }

        private void SearchTxtbx_TextChanged(object sender, EventArgs e)
        {
            string searchQuery = SearchTxtbx.Text;
            flowLayoutPanel1.Controls.Clear();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                conn.Open();
                cm = new MySqlCommand("SELECT VariationName, VariationCost, MealImage, VariationID FROM mealvariation WHERE VariationName LIKE @searchQuery", conn);
                cm.Parameters.AddWithValue("@searchQuery", "%" + searchQuery + "%");

                dr = cm.ExecuteReader();

                while (dr.Read())
                {
                    if (!dr.IsDBNull(dr.GetOrdinal("MealImage")))
                    {
                        byte[] imageBytes = (byte[])dr["MealImage"];

                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            Image mealImage = Image.FromStream(ms);
                            pic = new PictureBox
                            {
                                Width = 150,
                                Height = 150,
                                BackgroundImage = mealImage,
                                BackgroundImageLayout = ImageLayout.Stretch,
                                Tag = dr["VariationID"].ToString(),
                            };

                            price = new Label
                            {
                                Text = "Php. " + dr["VariationCost"].ToString(),
                                Width = 25,
                                Height = 15,
                                TextAlign = ContentAlignment.TopLeft,
                                Dock = DockStyle.Top,
                                BackColor = Color.White,
                            };

                            mealname = new Label
                            {
                                Text = dr["VariationName"].ToString(),
                                Width = 25,
                                Height = 15,
                                TextAlign = ContentAlignment.BottomCenter,
                                Dock = DockStyle.Bottom,
                                BackColor = Color.White,
                            };

                            pic.Controls.Add(mealname);
                            pic.Controls.Add(price);
                            flowLayoutPanel1.Controls.Add(pic);
                            pic.Click += OnFLP1Click;
                        }
                    }
                }
                dr.Close();
                conn.Close();
            }
            else
            {
                GetData();
            }
        }

        private void UpdateTotalPrice()
        {
            totalPrice = 0.00m;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[4].Value != null)
                {
                    decimal rowTotal = decimal.Parse(row.Cells[4].Value.ToString());
                    totalPrice += rowTotal;
                }
            }
            sbLbl.Text = "Php. " + totalPrice.ToString("0.00");
            ttlLbl.Text = sbLbl.Text;
            if (discChckBx.Checked)
            {
                decimal totalPrice = decimal.Parse(sbLbl.Text.Replace("Php. ", ""));
                decimal discount = totalPrice * 0.20m; // 20% discount
                decimal discountedTotal = totalPrice - discount;
                dscLbl.Text = "Php. " + discount.ToString("0.00");
                ttlLbl.Text = "Php. " + discountedTotal.ToString("0.00");
            }
        }

        private void AddTotalPrice(int rowIndex)
        {
            int currentQty = int.Parse(dataGridView1.Rows[rowIndex].Cells[2].Value.ToString());
            string foodName = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString(); // Get the food name from DataGridView
            decimal unitPrice = GetUnitPriceForFood(foodName); // Retrieve unit price based on VariationName
            decimal totalPrice = currentQty * unitPrice;
            dataGridView1.Rows[rowIndex].Cells[4].Value = totalPrice.ToString();

            UpdateTotalPrice();
        }

        private void SubtractTotalPrice(int rowIndex)
        {
            int currentQty = int.Parse(dataGridView1.Rows[rowIndex].Cells[2].Value.ToString());
            string foodName = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            decimal unitPrice = GetUnitPriceForFood(foodName);

            if (currentQty > 1)
            {
                currentQty--;
                dataGridView1.Rows[rowIndex].Cells[2].Value = currentQty; // Update the quantity in the DataGridView

                decimal totalPrice = currentQty * unitPrice;
                dataGridView1.Rows[rowIndex].Cells[4].Value = totalPrice.ToString();
                UpdateTotalPrice();
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                SubtractTotalPrice(e.RowIndex);
            }

            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                int currentQty = int.Parse(dataGridView1.Rows[e.RowIndex].Cells[2].Value.ToString());
                currentQty++;
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = currentQty;
                AddTotalPrice(e.RowIndex);
            }

            if (e.ColumnIndex == 5 && e.RowIndex >= 0)
            {
                string userPosition = PositionTxtBox.Text; // Replace this with the logic to get the user's position

                DialogResult result;

                if (userPosition == "Staff")
                {
                    // If the user is a staff member, prompt for manager's password
                    string enteredPassword = Encryptor.HashPassword(Microsoft.VisualBasic.Interaction.InputBox("Enter manager password:", "Password Required", ""));

                    string connectionString = "server=localhost;user=root;database=dashboarddb;password=";

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = "SELECT Position FROM employee_acc WHERE Password = @Password";

                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Password", enteredPassword);

                            // Execute the query
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string position = reader["Position"].ToString();

                                    if (position == "Manager")
                                    {
                                        result = MessageBox.Show("Do you want to remove this item?", "Remove Item", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                        if (result == DialogResult.Yes)
                                        {
                                            if (e.RowIndex < dataGridView1.Rows.Count)
                                            {
                                                // Calculate the price of the removed item
                                                decimal removedItemPrice = decimal.Parse(dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString());

                                                // Remove the selected row from the DataGridView
                                                dataGridView1.Rows.RemoveAt(e.RowIndex);

                                                // Update the total price by subtracting the removed item's price
                                                totalPrice -= removedItemPrice;
                                                sbLbl.Text = "Php. " + totalPrice.ToString("0.00");
                                                ttlLbl.Text = sbLbl.Text;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Invalid password. You need manager permission to remove an item.", "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Invalid password. You need manager permission to remove an item.", "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }
                else // For Managers and Admins, no password is required
                {
                    result = MessageBox.Show("Do you want to remove this item?", "Remove Item", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (e.RowIndex < dataGridView1.Rows.Count)
                        {
                            // Calculate the price of the removed item
                            decimal removedItemPrice = decimal.Parse(dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString());

                            // Remove the selected row from the DataGridView
                            dataGridView1.Rows.RemoveAt(e.RowIndex);

                            // Update the total price by subtracting the removed item's price
                            totalPrice -= removedItemPrice;
                            sbLbl.Text = "Php. " + totalPrice.ToString("0.00");
                            ttlLbl.Text = sbLbl.Text;
                        }
                    }
                }
            }
        }

        private decimal GetUnitPriceForFood(string foodName)
        {
            decimal unitPrice = 0;
            conn.Open();
            cm = new MySqlCommand("SELECT VariationCost FROM mealvariation WHERE VariationName = @foodName", conn);
            cm.Parameters.AddWithValue("@foodName", foodName);
            dr = cm.ExecuteReader();

            if (dr.Read())
            {
                unitPrice = decimal.Parse(dr["VariationCost"].ToString());
            }
            dr.Close();
            conn.Close();
            return unitPrice;
        }

        private void discChckBx_CheckedChanged(object sender, EventArgs e)
        {
            if (discChckBx.Checked)
            {
                decimal totalPrice = decimal.Parse(sbLbl.Text.Replace("Php. ", ""));
                decimal discount = totalPrice * 0.20m;
                decimal discountedTotal = totalPrice - discount;

                dscLbl.Text = "Php. " + discount.ToString("0.00");
                ttlLbl.Text = "Php. " + discountedTotal.ToString("0.00");
            }
            else
            {
                dscLbl.Text = "Php. 0.00";
                UpdateTotalPrice();
            }
        }


        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    dataGridView1.SelectedRows[0].Selected = false;
                }
            }
        }

        private void logLbl_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to log-out?", "information", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                panelManager.ShowPanel(LoginPanel);
            }
        }

        private void SearchTxtbx_Enter(object sender, EventArgs e)
        {
            if (SearchTxtbx.Text == "Type here to search")
            {
                SearchTxtbx.Text = "";
                SearchTxtbx.ForeColor = Color.Black;
            }
        }

        private void SearchTxtbx_Leave(object sender, EventArgs e)
        {
            if (SearchTxtbx.Text == "")
            {
                SearchTxtbx.Text = "Type here to search";
                SearchTxtbx.ForeColor = Color.LightGray;
                GetData();
            }
        }

        private void cashtxtBx_Enter(object sender, EventArgs e)
        {
            if (cashtxtBx.Text == "0.00")
            {
                cashtxtBx.Text = "";
                cashtxtBx.ForeColor = Color.Black;
            }
        }

        private void cashtxtBx_Leave(object sender, EventArgs e)
        {
            if (cashtxtBx.Text == "")
            {
                cashtxtBx.Text = "0.00";
                cashtxtBx.ForeColor = Color.LightGray;

            }
        }

        private void CafeDeLunaDashboard_Load(object sender, EventArgs e)
        {
            SearchTxtbx.Text = "Type here to search";
            SearchTxtbx.ForeColor = Color.LightGray;
            cashtxtBx.Text = "0.00";
            cashtxtBx.ForeColor = Color.LightGray;
        }

        private void placeBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to generate the receipt?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                //insert to table order id, userid, 
                GeneratePDFReceipt();
            }
        }

        private void GeneratePDFReceipt()
        {
            decimal subtotal = decimal.Parse(sbLbl.Text.Replace("Php. ", ""));
            decimal discount = decimal.Parse(dscLbl.Text.Replace("Php. ", ""));
            decimal totalAmount = decimal.Parse(ttlLbl.Text.Replace("Php. ", ""));
            decimal cashEntered;

            int totalQuantity = 0;

            if (!decimal.TryParse(cashtxtBx.Text, out cashEntered))
            {
                MessageBox.Show("Please enter a valid amount for payment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cashEntered < totalAmount)
            {
                MessageBox.Show("Please enter a valid amount for payment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "PDF Files|*.pdf";
                saveFileDialog1.Title = "Save PDF File";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string pdfFilePath = saveFileDialog1.FileName;

                    using (PdfWriter writer = new PdfWriter(new FileStream(pdfFilePath, FileMode.Create)))
                    using (PdfDocument pdf = new PdfDocument(writer))
                    using (Document doc = new Document(pdf))
                    {
                        doc.SetProperty(Property.TEXT_ALIGNMENT, TextAlignment.JUSTIFIED_ALL);

                        doc.Add(new Paragraph("Café De Luna").SetTextAlignment(TextAlignment.CENTER));
                        doc.Add(new Paragraph("Order Confirmation Receipt").SetTextAlignment(TextAlignment.CENTER));
                        doc.Add(new Paragraph("Date: " + DateTime.Now.ToString("MM/dd/yyyy   hh:mm tt")).SetTextAlignment(TextAlignment.LEFT));
                        doc.Add(new Paragraph("--------------------------------------------------------------------------------------------------"));
                        doc.Add(new Paragraph($"QUANTITY                         MEAL                    PRICE"));

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            string food = row.Cells[0].Value.ToString();
                            string quantity = row.Cells[2].Value.ToString();
                            string price = row.Cells[4].Value.ToString();
                            if (int.TryParse(quantity, out int quantityValue))
                            {
                                totalQuantity += quantityValue;
                            }
                            doc.Add(new Paragraph($"{quantity}                                   {food}                    {price}"));
                        }

                        doc.Add(new Paragraph($"---------------------------------------{totalQuantity} Item(s)-----------------------------------------"));
                        doc.Add(new Paragraph($"SUBTOTAL:                         Php. {subtotal.ToString("0.00")}"));
                        doc.Add(new Paragraph($"DISCOUNT:                         Php. {discount.ToString("0.00")}"));
                        doc.Add(new Paragraph($"TOTAL:                         Php. {totalAmount.ToString("0.00")}"));
                        doc.Add(new Paragraph($"CASH:                         Php. {cashEntered.ToString("0.00")}"));
                        decimal change = cashEntered - totalAmount;
                        doc.Add(new Paragraph($"CHANGE:                         Php. {change.ToString("0.00")}"));

                        doc.Add(new Paragraph("--------------------------------------------------------------------------------------------------"));
                        doc.Add(new Paragraph("This Receipt Serves as Your Proof of Purchase").SetTextAlignment(TextAlignment.CENTER));
                    }

                    MessageBox.Show("Receipt generated successfully and saved to:\n" + pdfFilePath, "Enjoy your meal!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InsertOrderData(GenerateID, false);
                    InsertOrderItemsData(GenerateID, dataGridView1, false);
                    InsertSalesData(GenerateID);
                    GenerateID = orderIDGenerator();
                    dataGridView1.Rows.Clear();
                    sbLbl.Text = "Php. 0.00";
                    ttlLbl.Text = "Php. 0.00";
                    dscLbl.Text = "Php. 0.00";
                    cashtxtBx.Text = "0.00";
                    cashtxtBx.ForeColor = Color.LightGray;
                    System.Diagnostics.Process.Start(pdfFilePath);
                }
            }
        }

        private void voidBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("There are no items in your cart.", "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string userPosition = PositionTxtBox.Text; // Replace this with the logic to get the user's position

            DialogResult result;

            if (userPosition == "Staff")
            {
                // If the user is a staff member, prompt for manager's password
                string enteredPassword = Encryptor.HashPassword(Microsoft.VisualBasic.Interaction.InputBox("Enter manager password:", "Password Required", ""));

                string connectionString = "server=localhost;user=root;database=dashboarddb;password=";

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT Position FROM employee_acc WHERE Password = @Password";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Password", enteredPassword);

                        // Execute the query
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string position = reader["Position"].ToString();

                                if (position == "Manager")
                                {
                                    result = MessageBox.Show("Do you want to void these items?", "Void Items", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                }
                                else
                                {
                                    MessageBox.Show("Invalid password. You need manager permission to void items.", "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid password. You need manager permission to void items.", "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }
                }

            }
            else // For Managers and Admins, no password is required
            {
                result = MessageBox.Show("Do you want to void these items?", "Void Items", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (result == DialogResult.Yes)
            {
                GenerateID = orderIDGenerator();
                InsertOrderData(GenerateID, true);
                InsertOrderItemsData(GenerateID, dataGridView1, true);

                // Clear all rows from the DataGridView
                dataGridView1.Rows.Clear();
                sbLbl.Text = "Php. 0.00";
                ttlLbl.Text = "Php. 0.00";
                dscLbl.Text = "Php. 0.00";
                cashtxtBx.Text = "0.00";
                cashtxtBx.ForeColor = Color.LightGray;
            }
            GenerateID = orderIDGenerator();
        }

        //Methods for sending place order to database

        string connectionString = "server=localhost;user=root;database=dashboarddb;password=";
        private void InsertOrderData(int generatedOrderID, bool isVoided)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string orderQuery;

                if (isVoided)
                {
                    orderQuery = "INSERT INTO orders (OrderID, UserID, IsVoided) VALUES (@OrderID, @UserID, @Voided)";
                }
                else
                {
                    orderQuery = "INSERT INTO orders (OrderID, UserID) VALUES (@OrderID, @UserID)";
                }

                using (MySqlCommand orderCmd = new MySqlCommand(orderQuery, connection))
                {
                    orderCmd.Parameters.AddWithValue("@OrderID", generatedOrderID);
                    orderCmd.Parameters.AddWithValue("@UserID", employeeID);

                    if (isVoided)
                    {
                        orderCmd.Parameters.AddWithValue("@Voided", "voided");
                    }

                    orderCmd.ExecuteNonQuery();
                }
            }

            string voidedStatus = isVoided ? "Voided" : "Placed";
            MessageBox.Show($"{voidedStatus} order successfully. OrderID={generatedOrderID}, UserID={employeeID}, Amount={ttlLbl.Text}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void InsertSalesData(int generatedOrderID)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string salesQuery = "INSERT INTO sales (OrderID, Amount) VALUES (@OrderID, @Amount)";

                using (MySqlCommand salesCmd = new MySqlCommand(salesQuery, connection))
                {
                    string totalText = ttlLbl.Text;
                    string numericValue = totalText.Replace("Php.", "").Trim();
                    decimal.TryParse(numericValue, out decimal amount);

                    // Insert data into the sales table with the correct total value
                    salesCmd.Parameters.AddWithValue("@OrderID", generatedOrderID);
                    salesCmd.Parameters.AddWithValue("@Amount", amount);
                    salesCmd.ExecuteNonQuery();
                }
            }
        }

        private Tuple<int, int> GetVariationInfo(string itemName)
        {
            int variationID = -1;
            int mealID = -1;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT VariationID, MealID FROM mealvariation WHERE VariationName = @ItemName";
                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ItemName", itemName);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            variationID = reader.GetInt32("VariationID");
                            mealID = reader.GetInt32("MealID");
                        }
                    }
                }
            }
            return Tuple.Create(variationID, mealID);
        }

        private void InsertOrderItemsData(int generatedOrderID, DataGridView dataGridView, bool isVoided)
        {
            bool itemNameFound = false;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    string itemName;
                    if (row.Cells["Column1"].Value != null)
                    {
                        itemName = row.Cells["Column1"].Value.ToString();
                        itemNameFound = true;
                    }
                    else
                    {
                        continue;
                    }

                    int qty = Convert.ToInt32(row.Cells["Column3"].Value);
                    Tuple<int, int> variationInfo = GetVariationInfo(itemName);
                    int variationID = variationInfo.Item1;
                    int mealID = variationInfo.Item2;

                    string query;
                    if (isVoided)
                    {
                        query = "INSERT INTO orderitems (OrderID, MealID, VariationID, Quantity, IsVoided) VALUES (@OrderID, @MealID, @VariationID, @Qty, @voided)";
                    }
                    else
                    {
                        query = "INSERT INTO orderitems (OrderID, MealID, VariationID, Quantity) VALUES (@OrderID, @MealID, @VariationID, @Qty)";
                    }
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", generatedOrderID);
                        cmd.Parameters.AddWithValue("@MealID", mealID);
                        cmd.Parameters.AddWithValue("@VariationID", variationID);
                        cmd.Parameters.AddWithValue("@Qty", qty);

                        if (isVoided)
                        {
                            cmd.Parameters.AddWithValue("@voided", "voided");
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
                if (!itemNameFound)
                {
                    MessageBox.Show("ItemName is null. IDK why.");
                }
            }
        }

        private void GenerateBtn_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = DateReportSelector.Value;
            CalculateAndDisplaySalesReportDaily(DailyDGV, ComputedSalesDailyTbl, selectedDate);

            DataTable mostSoldItem = GetMostSoldItemForDay(selectedDate);
            MostSalesDailyTbl.DataSource = mostSoldItem;
        }
        private void WeeklyRepBtn_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = DateReportSelector.Value;
            CalculateAndDisplaySalesReportWeekly(WeeklyDGV, ComputedSalesWeeklyTbl, selectedDate);

            DataTable mostSoldItem = GetMostSoldItemForWeek(selectedDate);
            MostSalesWeeklyTbl.DataSource = mostSoldItem;
        }

        private void MonthlyRepBtn_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = DateReportSelector.Value;
            CalculateAndDisplaySalesReportMonthly(MonthlyDGV, ComputedSalesMonthlyTbl, selectedDate);

            DataTable mostSoldItem = GetMostSoldItemForMonth(selectedDate);
            MostSalesMonthlyTbl.DataSource = mostSoldItem;
        }
        public void CalculateAndDisplaySalesReportDaily(DataGridView dailyDGV, DataGridView computedSalesDailyTbl, DateTime selectedDate)
        {
            conn.Close();
            conn.Open();

            // Get sales data for the selected date
            string query = "SELECT * FROM Sales WHERE DATE(SaleDate) = @Date";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = selectedDate.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable salesData = new DataTable();
                    adapter.Fill(salesData);
                    dailyDGV.DataSource = salesData;
                }
            }

            // Calculate and display daily sales
            decimal dailySales = CalculateSalesForDay(selectedDate);
            computedSalesDailyTbl.Rows.Add(selectedDate.ToString("d"), dailySales);
        }

        public void CalculateAndDisplaySalesReportWeekly(DataGridView weeklyDGV, DataGridView computedSalesWeeklyTbl, DateTime selectedDate)
        {
            conn.Close();
            conn.Open();

            // Get sales data for the selected week
            string query = "SELECT * FROM Sales WHERE YEARWEEK(SaleDate) = YEARWEEK(@Date)";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = selectedDate.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable salesData = new DataTable();
                    adapter.Fill(salesData);
                    weeklyDGV.DataSource = salesData;
                }
            }

            // Calculate and display weekly sales
            decimal weeklySales = CalculateSalesForWeek(selectedDate);
            computedSalesWeeklyTbl.Rows.Add(selectedDate.AddDays(-7).ToString("d") + " - " + selectedDate.ToString("d"), weeklySales);
        }

        public void CalculateAndDisplaySalesReportMonthly(DataGridView monthlyDGV, DataGridView computedSalesMonthlyTbl, DateTime selectedDate)
        {
            conn.Close();
            conn.Open();

            // Get sales data for the selected month
            string query = "SELECT * FROM Sales WHERE YEAR(SaleDate) = YEAR(@Date) AND MONTH(SaleDate) = MONTH(@Date)";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = selectedDate.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable salesData = new DataTable();
                    adapter.Fill(salesData);
                    monthlyDGV.DataSource = salesData;
                }
            }

            // Calculate and display monthly sales
            decimal monthlySales = CalculateSalesForMonth(selectedDate);
            computedSalesMonthlyTbl.Rows.Add(new DateTime(selectedDate.Year, selectedDate.Month, 1).ToString("d") + " - " + selectedDate.ToString("d"), monthlySales);
        }

        public decimal CalculateSalesForDay(DateTime date)
        {
            string query = "SELECT SUM(Amount) AS TotalSales FROM Sales WHERE DATE(SaleDate) = @Date";

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date.Date });
                object result = command.ExecuteScalar();
                return (result == DBNull.Value) ? 0 : Convert.ToDecimal(result);
            }
        }

        public decimal CalculateSalesForWeek(DateTime date)
        {
            string query = "SELECT SUM(Amount) AS TotalSales FROM Sales WHERE YEARWEEK(SaleDate) = YEARWEEK(@Date)";

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date });
                object result = command.ExecuteScalar();
                return (result == DBNull.Value) ? 0 : Convert.ToDecimal(result);
            }
        }

        public decimal CalculateSalesForMonth(DateTime date)
        {
            string query = "SELECT SUM(Amount) AS TotalSales FROM Sales WHERE YEAR(SaleDate) = YEAR(@Date) AND MONTH(SaleDate) = MONTH(@Date)";

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date });
                object result = command.ExecuteScalar();
                return (result == DBNull.Value) ? 0 : Convert.ToDecimal(result);
            }
        }

        private void cashtxtBx_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cashtxtBx.Text) && cashtxtBx.Text != "0.00")
            {
                placeBtn.Enabled = true;
            }
            else
            {
                placeBtn.Enabled = false;
            }
        }

        public DataTable GetMostSoldItemForDay(DateTime date)
        {

            string query = @"
        SELECT DATE(s.SaleDate) as SaleDate, mv.VariationName, SUM(oi.Quantity) as TotalQuantity, COUNT(*) as TotalSales
        FROM sales s
        INNER JOIN orderitems oi ON s.OrderID = oi.OrderID
        INNER JOIN mealvariation mv ON oi.VariationID = mv.VariationID
        WHERE DATE(s.SaleDate) = @Date
        GROUP BY DATE(s.SaleDate), mv.VariationName
        ORDER BY TotalQuantity DESC";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable dailySummary = new DataTable();
                    adapter.Fill(dailySummary);
                    return dailySummary;
                }
            }
        }

        public DataTable GetMostSoldItemForWeek(DateTime date)
        {
            string query = @"
        SELECT DATE(s.SaleDate) as SaleDate, mv.VariationName, SUM(oi.Quantity) as TotalQuantity, COUNT(*) as TotalSales
        FROM sales s
        INNER JOIN orderitems oi ON s.OrderID = oi.OrderID
        INNER JOIN mealvariation mv ON oi.VariationID = mv.VariationID
        WHERE DATE(s.SaleDate) >= DATE_SUB(@Date, INTERVAL 7 DAY)
        GROUP BY DATE(s.SaleDate), mv.VariationName
        ORDER BY TotalQuantity DESC";

            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable weeklySummary = new DataTable();
                    adapter.Fill(weeklySummary);
                    return weeklySummary;
                }
            }
        }

        public DataTable GetMostSoldItemForMonth(DateTime date)
        {
            string query = @"
        SELECT DATE(s.SaleDate) as SaleDate, mv.VariationName, SUM(oi.Quantity) as TotalQuantity, COUNT(*) as TotalSales
        FROM sales s
        INNER JOIN orderitems oi ON s.OrderID = oi.OrderID
        INNER JOIN mealvariation mv ON oi.VariationID = mv.VariationID
        WHERE DATE(s.SaleDate) >= DATE_SUB(@Date, INTERVAL 30 DAY)
        GROUP BY DATE(s.SaleDate), mv.VariationName
        ORDER BY TotalQuantity DESC";
            using (MySqlCommand command = new MySqlCommand(query, conn))
            {
                command.Parameters.Add(new MySqlParameter("@Date", MySqlDbType.Date) { Value = date.Date });
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                {
                    DataTable monthlySummary = new DataTable();
                    adapter.Fill(monthlySummary);
                    return monthlySummary;
                }
            }
        }
    }
}
