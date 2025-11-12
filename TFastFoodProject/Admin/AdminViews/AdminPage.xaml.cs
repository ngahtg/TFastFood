using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TFastFoodProject.Models;

namespace TFastFoodProject.Admin.AdminViews
{
    /// <summary>
    /// Interaction logic for AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Window
    {
        private readonly TfastFoodContext _context;
       
        public AdminPage()
        {
            _context = new TfastFoodContext();
            InitializeComponent();
            LoadTotalOrder();
            LoadTotalMonth();
        }

        private void LoadTotalMonth()
        {
            // total money month
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            decimal totalPrice = _context.Orders.Where(o => o.Status == "Completed" &&
            o.OrderDate >= startDate &&
            o.OrderDate < endDate).Sum(o => o.TotalPrice ?? 0m);
            txtRevenue.Text = totalPrice.ToString();
        }

        private void LoadTotalOrder()
        {
            // total order month
            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year, now.Month, 1);
            DateTime endDate = startDate.AddMonths(1);

            int TotalOrder = _context.Orders
        .Where(o => o.Status == "Completed" &&
                    o.OrderDate >= startDate &&
                    o.OrderDate < endDate)
        .Count(); 
            txtOrders.Text = TotalOrder.ToString();
        }

        private void Employee_Click(object sender, RoutedEventArgs e)
        {
            var employee_page = new TFastFoodProject.Admin.AdminManagers.AdminManager.EmployeeManagerWindow();
            if (employee_page != null)
            {
                this.Hide();
                employee_page.ShowDialog();
                this.Show();
            }
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

        private void Ingredient_Click(object sender, RoutedEventArgs e)
        {
            var ingredient_page = new Admin.AdminManagers.AdminManager.IngredientManagerWindow();
            if (ingredient_page != null)
            {
                this.Hide();
                ingredient_page.ShowDialog();
                this.Show();
            }
        }

        private void FoodManager_Click(object sender, RoutedEventArgs e)
        {
            var foodManager_page = new Admin.AdminManagers.AdminManager.FoodManagerWindow();
            if (foodManager_page != null)
            {
                this.Hide();
                foodManager_page.ShowDialog();
                this.Show();
            }

        }
    }
}
