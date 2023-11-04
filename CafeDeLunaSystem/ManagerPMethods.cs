using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace CafeDeLunaSystem
{
    internal class ManagerPMethods
    {
        private readonly MySqlConnection conn;
        public static ManagerPMethods managerPMethodsInstance;
        public ManagerPMethods(string connectionString)
        {
            conn = new MySqlConnection(connectionString);
            managerPMethodsInstance = this;
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
    }
}
