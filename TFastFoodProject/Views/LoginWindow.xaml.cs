using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.IdentityModel.Tokens;
using TFastFoodProject.Models;

namespace TFastFoodProject.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly TfastFoodContext _context;
        public LoginWindow()
        {
            _context = new TfastFoodContext();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = pwdPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all information.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            var user = _context.Staff.FirstOrDefault(u =>
                u.Username.Equals(username) &&
                u.Password.Equals(password)
            );


            if (user == null)
            {

                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user.Active==false)
            {
                MessageBox.Show("This account was ban by admin !", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user.Role.Equals("Manager"))
            {
                var admin_page = new Admin.AdminViews.AdminPage();

                if (admin_page != null)
                {
                    this.Hide();
                    admin_page.ShowDialog();
                    this.Close();
                }
            }
            else
            {
                var staff = _context.Staff.FirstOrDefault(s => s.Username == username);

                    var staff_page = new Employee.EmployeeViews.StaffPageWindow(staff.FullName);
                    if (staff_page != null)
                    {
                        this.Hide();
                        staff_page.ShowDialog();
                        this.Close();
                    }
               
                
            }
        }
    }
}