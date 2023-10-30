using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Syncfusion.Windows.Forms.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml.Linq;

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
        private byte[] imageData;

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
                //panelManagerAMC.ShowPanel(EditAccPanel);
                if (AccDataTbl.SelectedRows.Count == 1)
                {
                    DataGridViewRow selectedRow = AccDataTbl.SelectedRows[0];
                    string nameColumn = selectedRow.Cells["Name"].Value.ToString();
                    string birthdayColumn = selectedRow.Cells["Birthday"].Value.ToString().Trim();
                    string ageColumn = selectedRow.Cells["Age"].Value.ToString();
                    string emailColumn = selectedRow.Cells["Email"].Value.ToString();
                    string usernameColumn = selectedRow.Cells["Username"].Value.ToString();
                    string positionColumn = selectedRow.Cells["Position"].Value.ToString();
                    int employeeIDColumn = Convert.ToInt32(AccDataTbl.SelectedRows[0].Cells["EmployeeID"].Value);
                    string[] nameParts = nameColumn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    //string[] nameParts = nameColumn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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
                        MessageBox.Show("Invalid date format in the 'Birthday' column.");
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
                    MessageBox.Show("Please select a single row for editing.");
                }

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
                    string selectedFilePath = openFileDialog.FileName;
                    ImgTxtB.Text = selectedFilePath;

                    try
                    {
                        // Load the selected image
                        Image selectedImage = Image.FromFile(selectedFilePath);

                        // Check if the image dimensions are 64x64 pixels
                        if (selectedImage.Width == 64 && selectedImage.Height == 64)
                        {
                            UserPicB.Image = selectedImage;
                        }
                        else
                        {
                            MessageBox.Show("Please select an image with dimensions of 64x64 pixels.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exceptions that may occur when loading the image
                        MessageBox.Show("Error loading the image: " + ex.Message);
                    }
                }
            }




            /*
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|.jpg;.jpeg;.png;.gif;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the selected image into the PictureBox
                    Image selectedImage = Image.FromFile(openFileDialog.FileName);

                    // Check if the image dimensions are 64x64 pixels
                    if (selectedImage.Width == 64 && selectedImage.Height == 64)
                    {
                        UserPicB.Image = selectedImage;
                    }
                    else
                    {
                        MessageBox.Show("Please select an image with dimensions of 64x64 pixels.");
                    }
                }
            }*/
        }

        private void CreateBtn_Click(object sender, EventArgs e)
        {
            string adminUsername = "Admin";
            DateTime selectedDate = UserBirthdate.Value;
            string employeeFullName = $"{LastNTxtB_AP.Text}, {FirstNTxtB_AP.Text} {MiddleNTxtB_AP.Text}";
            string userImagePath = ImgTxtB.Text;

            if (string.IsNullOrWhiteSpace(userImagePath) || !File.Exists(userImagePath))
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

        private void MenuAddImgBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|.jpg;.jpeg;.png;.gif;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the selected image into the PictureBox
                    Image selectedImage = Image.FromFile(openFileDialog.FileName);

                    // Check if the image dimensions are 64x64 pixels
                    if (selectedImage.Width == 64 && selectedImage.Height == 64)
                    {
                        MenuPicB.Image = selectedImage;
                    }
                    else
                    {
                        MessageBox.Show("Please select an image with dimensions of 64x64 pixels.");
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
                openFileDialog.Filter = "Image Files|.jpg;.jpeg;.png;.gif;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Load the selected image into the PictureBox
                    Image selectedImage = Image.FromFile(openFileDialog.FileName);

                    // Check if the image dimensions are 64x64 pixels
                    if (selectedImage.Width == 64 && selectedImage.Height == 64)
                    {
                        VariationPicB.Image = selectedImage;
                    }
                    else
                    {
                        MessageBox.Show("Please select an image with dimensions of 64x64 pixels.");
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
                    MessageBox.Show("Error Test: " + ex.Message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
