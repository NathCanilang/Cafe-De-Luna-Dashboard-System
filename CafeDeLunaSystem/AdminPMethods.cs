using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeDeLunaSystem
{
    internal class CreateAndEditAcc
    {
        private readonly MySqlConnection conn;

        public CreateAndEditAcc()
        {
            string mysqlcon = "server=localhost;user=root;database=dashboarddb;password=";
            conn = new MySqlConnection(mysqlcon);
        }

        public int AgeCalculation(DateTime employeeBirth)
        {
            int years = DateTime.Now.Year - employeeBirth.Year;

            if (employeeBirth.AddYears(years) > DateTime.Now) years--;
            return years;
        }

        public void PopulateMealComboBox()
        {
            CafeDeLunaDashboard.cafeDeLunaInstance.MenuSelectComB.Items.Clear();

            string connectionString = "server=localhost;user=root;database=dashboarddb;password=";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT MealName FROM meal";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string mealName = reader["MealName"].ToString();
                                    CafeDeLunaDashboard.cafeDeLunaInstance.MenuSelectComB.Items.Add(mealName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public int GetMealIDFromDatabase(string mealName)
        {
            string connectionString = "server=localhost;user=root;database=dashboarddb;password=";
            int mealID = -1;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT MealID FROM meal WHERE MealName = @mealName";

                    using (MySqlCommand command = new MySqlCommand(query, conn))
                    {
                        command.Parameters.AddWithValue("@mealName", mealName);

                        object result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out mealID))
                        {
                            return mealID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            return mealID;
        }

        public void GenerateAndSetRandomNumber()
        {
            Random random = new Random();
            int random6Digit = random.Next(100000, 1000000);
            CafeDeLunaDashboard.cafeDeLunaInstance.EmployeeIDTxtB_AP.Text = random6Digit.ToString();
        }

        public void RefreshTbl()
        {
            string query = "SELECT Name, Birthday, Age, Email, Username, Password, Position, EmployeeID, EmployeeIMG FROM employee_acc";
            DataTable dt = new DataTable();

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }

            CafeDeLunaDashboard.cafeDeLunaInstance.AccDataTbl.DataSource = dt;

        }

        public void LoadMenuItems()
        {
            try
            {
                using (conn)
                {
                    conn.Open();
                    string query = "SELECT MealImage, VariationID, MealID, VariationName, VariationDescription, VariationCost FROM mealvariation";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    System.Data.DataTable dataTable = new System.Data.DataTable();

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);

                        // Create the "Item Picture" column with the specified settings
                        DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
                        imageColumn.HeaderText = "Item Picture";
                        imageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;

                        // Clear any existing columns to remove the extra "Item Picture" column
                        CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.Columns.Clear();

                        // Add the image column to the DataGridView
                        CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.Columns.Add(imageColumn);
                        CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.Columns[0].Visible = false; // Assuming this hides the MealImage column
                        CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.DataSource = dataTable;

                        CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occurred: " + e.Message);
            }
            finally
            {
                // Make sure to close the connection (if it's open)
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public void FoodTable_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 0) // Assuming column index for "AccountPfp" is 1
            {
                // Set the cell value to null to display an empty cell
                e.ThrowException = false;
                CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl[e.ColumnIndex, e.RowIndex].Value = null;
            }
        }

        public void FoodTable_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.AutoResizeRow(e.RowIndex, DataGridViewAutoSizeRowMode.AllCells);
        }

        public void LoadMenuItemImage(int variationID)
        {
            byte[] imageData = GetImageDataFromDatabase(variationID); // Call a new method to get image data

            try
            {
                if (imageData != null && imageData.Length > 0)
                {
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        Image image = Image.FromStream(ms);

                        // Set the PictureBox image only if the conversion succeeds
                        CafeDeLunaDashboard.cafeDeLunaInstance.UserPicB.Image = image;
                    }
                }
                else
                {
                    // Set PictureBox image to a default image or null if there's no image data
                    CafeDeLunaDashboard.cafeDeLunaInstance.UserPicB.Image = null;
                }
            }
            catch (ArgumentException ex)
            {
                // Handle the exception if the byte array does not represent a valid image format
                MessageBox.Show("Error loading image: Invalid image data format.");
                MessageBox.Show("Exception Details: " + ex.Message);

                // Set PictureBox image to a default image or show an error image
                CafeDeLunaDashboard.cafeDeLunaInstance.UserPicB.Image = null; // Set pictureBox image to default or show an error image
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                MessageBox.Show("Error loading image: " + ex.Message);

                // Set PictureBox image to a default image or show an error image
                CafeDeLunaDashboard.cafeDeLunaInstance.UserPicB.Image = null; // Set pictureBox image to default or show an error image
            }
        }

        public byte[] GetImageDataFromDatabase(int employeeID)
        {
            string connectionString = "server=localhost;user=root;database=dashboarddb;password=";

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT EmployeeIMG FROM employee_acc WHERE EmployeeID = @employeeID";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@employeeID", employeeID);

                        object result = command.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return (byte[])result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }
    }
}
