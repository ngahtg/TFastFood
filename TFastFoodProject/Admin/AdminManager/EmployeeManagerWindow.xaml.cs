using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
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
using TFastFoodProject.Models;

namespace TFastFoodProject.Admin.AdminManagers.AdminManager
{
    /// <summary>
    /// Interaction logic for EmployeeManagerWindow.xaml
    /// </summary>
    /// 


    public partial class EmployeeManagerWindow : Window
    {
        private readonly TfastFoodContext _context;
        //Theo dõi xem cửa sổ đã sẵn sàng chưa
        private bool _isInitialized = false;
        public EmployeeManagerWindow()
        {
            _context = new TfastFoodContext();
            InitializeComponent();
            
        }


        //Chỉ gọi LoadStaff() sau khi cửa sổ đã tải xong và đặt cờ
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitialized = true;
            LoadStaff();
        }

        private void ResetStaffFormAndGrid()
        {

            FullNameTextBox.Text = string.Empty;
            PasswordTextBox.Text = string.Empty;
            UsernameTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;

        
            StaffDataGrid.SelectedItem = null;

            if (StatusFilterComboBox != null)
            {
                StatusFilterComboBox.SelectedIndex = 0;
            }

            LoadStaff();
        }

        private void LoadStaff()
        {
            // Nếu context chưa khởi tạo thì thoát
            if (_context == null) return;

            // Nếu combobox hoặc datagrid chưa sẵn sàng thì thoát
            if (StatusFilterComboBox == null || StaffDataGrid == null)
                return;

            // Nếu chưa khởi tạo xong cửa sổ, chỉ tải tất cả nhân viên
            if (!_isInitialized)
            {
                StaffDataGrid.ItemsSource = _context.Staff.ToList();
                return;
            }

            // Xác định trạng thái lọc
            string filterStatus = "All";
            int selectedIndex = StatusFilterComboBox.SelectedIndex;

            if (selectedIndex == 1) filterStatus = "Active";
            else if (selectedIndex == 2) filterStatus = "Inactive";

            // Lọc dữ liệu
            switch (filterStatus)
            {
                case "Active":
                    StaffDataGrid.ItemsSource = _context.Staff.Where(s => s.Active == true).ToList();
                    break;
                case "Inactive":
                    StaffDataGrid.ItemsSource = _context.Staff.Where(s => s.Active == false).ToList();
                    break;
                default:
                    StaffDataGrid.ItemsSource = _context.Staff.ToList();
                    break;
            }
        }


        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var dashboard_page = new Admin.AdminViews.AdminPage();
            if (dashboard_page != null)
            {
                this.Hide();
                dashboard_page.ShowDialog();
                this.Show();
            }
        }

        private void StaffDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem is Staff st)
            {

                FullNameTextBox.Text = st.FullName.ToString();
                PasswordTextBox.Text = st.Password.ToString();
                UsernameTextBox.Text = st.Username.ToString();
                PhoneTextBox.Text = st.Phone.ToString();
            }
        }

        private void Refesh_Click(object sender, RoutedEventArgs e)
        {

            ResetStaffFormAndGrid();

        }


        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve data from controls
            string fullName = FullNameTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;
            string phone = PhoneTextBox.Text;


            string role = "Staff";


            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Please fill in all mandatory information (Full Name, Username, Password, Phone).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$"))
            {
                MessageBox.Show("Phone number must contain only digits (0-9).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check for reasonable phone length (e.g., 10 or 11 digits)
            if (phone.Length < 10 || phone.Length > 11)
            {
                MessageBox.Show("Invalid phone number length (must be 10 or 11 digits).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            try
            {
                // check duplicate username
                if (_context.Staff.Any(s => s.Username == username))
                {
                    MessageBox.Show("The username already exists. Please choose a different one.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Staff staff = new Staff
                {
                    FullName = fullName,
                    Password = password,
                    Username = username,
                    Phone = phone,
                    Role = role
                };

                _context.Staff.Add(staff);
                _context.SaveChanges();

                ResetStaffFormAndGrid();

                MessageBox.Show("Staff added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: Implement logic to clear the form fields and refresh the DataGrid
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding staff: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {

            if (StaffDataGrid.SelectedItem is Staff st)
            {
                // Get input values from the detail form controls
                string fullName = FullNameTextBox.Text;
                string username = UsernameTextBox.Text;
                string password = PasswordTextBox.Text;
                string phone = PhoneTextBox.Text.ToString();
                string role = "Staff";


                if (string.IsNullOrWhiteSpace(fullName) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(role))
                {
                    MessageBox.Show("Please fill in all mandatory fields (Full Name, Username, Phone, Role).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                if (!string.IsNullOrWhiteSpace(password) && password.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long if changed.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d+$") || phone.Length < 10 || phone.Length > 11)
                {
                    MessageBox.Show("Invalid phone number format or length (10-11 digits).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                try
                {

                    var staffToUpdate = _context.Staff.FirstOrDefault(s => s.StaffId == st.StaffId);

                    if (staffToUpdate != null)
                    {

                        staffToUpdate.FullName = fullName;
                        staffToUpdate.Username = username;
                        staffToUpdate.Phone = phone;
                        staffToUpdate.Role = role;

                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            staffToUpdate.Password = password;
                        }


                        var existingUser = _context.Staff.FirstOrDefault(u => u.Username == username && u.StaffId != st.StaffId);
                        if (existingUser != null)
                        {
                            MessageBox.Show("This Username is already taken by another staff member.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        _context.Staff.Update(staffToUpdate);
                        _context.SaveChanges();
                        ResetStaffFormAndGrid();

                        MessageBox.Show($"Staff '{staffToUpdate.FullName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                       

                    }
                    else
                    {
                        MessageBox.Show("Staff member not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during update: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a Staff member to edit!", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Inactive_Click(object sender, RoutedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem is Staff st)
                if (st.Role != "Manager")
                {
                    {

                        MessageBoxResult result = MessageBox.Show(
                            $"Are you sure you want to delete staff member: {st.FullName} ({st.Username})? " +
                            $"This will delete all associated orders and order details.",
                            "Confirm Deletion",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            var selected_St = _context.Staff.FirstOrDefault(s => s.StaffId == st.StaffId);
                            if (selected_St != null)
                            {
                                selected_St.Active = false;
                            }
                            else
                            {
                                MessageBox.Show("Don't have that Staff in Database", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            _context.Staff.Update(selected_St);
                            _context.SaveChanges();

                            ResetStaffFormAndGrid();
                            MessageBox.Show($"Inactive successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Can't Inactive Manager Account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            else
            {
                MessageBox.Show("Please select a Staff member to inactive account.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            var ingredient_page = new IngredientManagerWindow();
            if (ingredient_page != null)
            {
                this.Hide();
                ingredient_page.ShowDialog();
                this.Show();
            }
        }

        private void FoodManager_Click(object sender, RoutedEventArgs e)
        {
            var foodManager_page = new FoodManagerWindow();
            if (foodManager_page != null)
            {
                this.Hide();
                foodManager_page.ShowDialog();
                this.Show();
            }

        }

        private void Deactive_Click(object sender, RoutedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem is Staff st)
            {
                if (st.Active == false)
                {
                    st.Active = true;
                    _context.Staff.Update(st);
                    _context.SaveChanges();

                    ResetStaffFormAndGrid();
                    MessageBox.Show($"Deactive successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                } else
                {
                    MessageBox.Show("Can't Deactive with the active account", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }else
            {
                MessageBox.Show("Please selected a account to deactive", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Chặn khi combobox còn đang khởi tạo hoặc grid chưa sẵn sàng
            if (!_isInitialized || StatusFilterComboBox == null || StaffDataGrid == null)
                return;

            LoadStaff();
        }

    }
}
