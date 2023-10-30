using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
        
            string query = "SELECT * FROM employee_acc";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            CafeDeLunaDashboard.cafeDeLunaInstance.AccDataTbl.DataSource = dataTable;
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
    }
}
