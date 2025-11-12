using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using TFastFoodProject.Models;

namespace TFastFoodProject.Employee.EmployeeViews
{
    /// <summary>
    /// Interaction logic for StaffPageWindow.xaml
    /// </summary>
    public partial class StaffPageWindow : Window
    {
        private string? fullName;
        private readonly TfastFoodContext _context;
        public StaffPageWindow(string? fullName)
        {
            InitializeComponent();
            _context = new TfastFoodContext();
            this.fullName = fullName;
            NameTextBox.Text = $"Welcome back , {fullName}!";
            LoadTotalDayOrder();
            LoadTotalDayRevenue();
        }

        public void LoadTotalDayRevenue()
        {
            // total money today
            DateTime now = DateTime.Now.Date; 
            DateTime startDate = now;
            DateTime endDate = now.AddDays(1);

            decimal totalPrice = _context.Orders
                .Where(o => o.Status == "Completed" &&
                            o.OrderDate >= startDate &&
                            o.OrderDate < endDate) 
                .Sum(o => o.TotalPrice ?? 0m);

            txtRevenue.Text = totalPrice.ToString(); 
        }

        public void LoadTotalDayOrder()
        {
            // total order today
            DateTime now = DateTime.Now.Date; 
            DateTime startDate = now;
            DateTime endDate = now.AddDays(1);

            int TotalOrder = _context.Orders
                .Count(o => o.Status == "Completed" &&
                            o.OrderDate >= startDate &&
                            o.OrderDate < endDate);

            txtOrders.Text = TotalOrder.ToString();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login_page = new Views.LoginWindow();
            if (login_page != null)
            {
                this.Hide();
                login_page.ShowDialog();
                this.Show();
            }
        }

        private void Order_Click(object sender, RoutedEventArgs e)
        {
            var staff = _context.Staff.FirstOrDefault(s => s.FullName == fullName);
            var order_page = new Employee.EmployeeManager.OrderManagerWindow(staff.StaffId);
            if (order_page != null)
            {
                this.Hide();
                order_page.ShowDialog();
                this.Show();
            }
        }
    }
}
