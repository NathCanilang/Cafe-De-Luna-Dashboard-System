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

        public void LoadMenuItems()
        {

            string query = "SELECT VariationID, MealID, VariationName, VariationDescription, VariationCost, MealImage FROM mealvariation";
            DataTable dt = new DataTable();

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
            {
                adapter.Fill(dt);
            }

            CafeDeLunaDashboard.cafeDeLunaInstance.FoodTbl.DataSource = dt;
        }
    }
}
