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
using TFastFoodProject.Models;


namespace TFastFoodProject.Admin.AdminManagers.AdminManager
{
    public partial class FoodManagerWindow : Window
    {
        private readonly TfastFoodContext _context;
        

        // Hằng số cho thư mục ảnh (Dùng cho thông báo lỗi)
        private const string FriendlyImageSubFolder = "Images/Foods";

        // Tên Assembly (Tên Project)
        private const string AssemblyName = "TFastFoodProject";

        public FoodManagerWindow()
        {
            _context = new TfastFoodContext();
            InitializeComponent();
            LoadFood();
        }

        private static string ResolveImagePath(string basePath)
        {
            // Dùng \ cho thư viện System.IO
            const string InternalImageSubFolder = "Images\\Foods";
            string[] extensions = { ".png", ".jpg", ".jpeg" };
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            foreach (var ext in extensions)
            {
                // 1. TẠO ĐƯỜNG DẪN TUYỆT ĐỐI ĐẾN FILE VẬT LÝ ĐỂ CHECK
                string fullPath = System.IO.Path.Combine(baseDirectory, InternalImageSubFolder, basePath + ext);

                if (System.IO.File.Exists(fullPath))
                {
                    // 2. TẠO VÀ TRẢ VỀ ĐƯỜNG DẪN PACK URI ĐẦY ĐỦ (DÙNG CHO BINDING TRONG DATAGRID)
                    // Format: /AssemblyName;component/Path/To/Resource.ext

                    string resourcePath = InternalImageSubFolder + @"\" + basePath + ext;

                    // SỬA ĐỔI QUAN TRỌNG NHẤT: Trả về URI hoàn chỉnh:
                    // Ví dụ: /TFastFoodProject;component/Images/Foods/pepsi.jpg
                    return $"/{AssemblyName};component/{resourcePath.Replace('\\', '/')}";
                }
            }

            System.Diagnostics.Debug.WriteLine($"Error: Image file not found for base path: {basePath}");
            return null;
        }

        private void ResetFoodFormAndGrid()
        {
            FoodNameTextBox.Text = PriceTextBox.Text = ImagePathTextBox.Text = CategoryTextBox.Text = "";
            FoodDataGrid.SelectedItem = null;
            LoadFood();
        }

        private void LoadFood()
        {
            FoodDataGrid.ItemsSource = _context.Foods.ToList();
        }

        private void DashBoard_click(object sender, RoutedEventArgs e)
        {
            var dashboard_page = new TFastFoodProject.Admin.AdminViews.AdminPage();
            if (dashboard_page != null)
            {
                this.Hide();
                dashboard_page.ShowDialog();
                this.Show();
            }
        }

        private void Employee_Click(object sender, RoutedEventArgs e)
        {
            var employee_page = new EmployeeManagerWindow();
            if (employee_page != null)
            {
                this.Hide();
                employee_page.ShowDialog();
                this.Show();
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login_page = new TFastFoodProject.Views.LoginWindow();
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

        private void FoodDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FoodDataGrid.SelectedItem is Food selected)
            {
                FoodNameTextBox.Text = selected.Name;
                PriceTextBox.Text = selected.Price.ToString();
                CategoryTextBox.Text = selected.Category;

                string fullPath = selected.ImagePath;
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullPath);
                ImagePathTextBox.Text = fileNameWithoutExtension;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ResetFoodFormAndGrid();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FoodNameTextBox.Text))
            {
                MessageBox.Show("Food Name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(CategoryTextBox.Text))
            {
                MessageBox.Show("Category cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                MessageBox.Show("Price must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            bool isDuplicate = _context.Foods
                                       .Any(food => food.Name.ToLower() == FoodNameTextBox.Text.Trim().ToLower());

            if (isDuplicate)
            {
                MessageBox.Show("A food item with this name already exists. Please choose a different name.",
                                "Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (price <= 0)
            {
                MessageBox.Show("Price must be greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Food f = new Food();
            f.Name = FoodNameTextBox.Text;
            f.Price = price;
            f.Category = CategoryTextBox.Text;

            string inputPath = ImagePathTextBox.Text.Trim();
            string path = ResolveImagePath(inputPath);

            if (path == null)
            {
                MessageBox.Show($"Image file '{inputPath}' not found in the '{FriendlyImageSubFolder}' folder. Please check the image name and extension.",
                                "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var food = _context.Foods.FirstOrDefault(f => f.ImagePath == path);
            if (food != null)
            {
                MessageBox.Show("This image path is already used by another food item. Please use a unique image name.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            f.ImagePath = path;

            try
            {
                _context.Foods.Add(f);
                _context.SaveChanges();

                ResetFoodFormAndGrid();

                MessageBox.Show("Food added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!(FoodDataGrid.SelectedItem is Food selected))
            {
                MessageBox.Show("Please select a food item to edit!", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newFoodName = FoodNameTextBox.Text.Trim();

            // VALIDATION
            if (string.IsNullOrWhiteSpace(newFoodName) || string.IsNullOrWhiteSpace(CategoryTextBox.Text))
            {
                MessageBox.Show("Food Name and Category cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Price must be a valid number greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // DUPLICATE NAME CHECK
            bool isDuplicateName = _context.Foods
                .Any(food => food.FoodId != selected.FoodId && food.Name.ToLower() == newFoodName.ToLower());

            if (isDuplicateName)
            {
                MessageBox.Show("A food item with this name already exists. Please choose a different name.",
                                "Duplicate Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // IMAGE PATH CHECK
            string inputPath = ImagePathTextBox.Text.Trim();
            string newPath = ResolveImagePath(inputPath);

            if (newPath == null)
            {
                MessageBox.Show($"Image file '{inputPath}' not found in the '{FriendlyImageSubFolder}' folder. Please check the image name and extension.",
                                "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // CHECK DUPLICATE IMAGE PATH
            bool isDuplicatePath = _context.Foods
                .Any(food => food.FoodId != selected.FoodId && food.ImagePath == newPath);

            if (isDuplicatePath)
            {
                MessageBox.Show("This image is already used by another food item. Please use a unique image name.",
                                "Duplicate Image Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // UPDATE
            try
            {
                var foodToUpdate = _context.Foods.FirstOrDefault(f => f.FoodId == selected.FoodId);

                if (foodToUpdate != null)
                {
                    foodToUpdate.Name = newFoodName;
                    foodToUpdate.Price = price;
                    foodToUpdate.Category = CategoryTextBox.Text;
                    foodToUpdate.ImagePath = newPath;
                }

                _context.Foods.Update(foodToUpdate);
                _context.SaveChanges();

                ResetFoodFormAndGrid();

                MessageBox.Show($"Food '{newFoodName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (FoodDataGrid.SelectedItem is Food food)
            {
                MessageBoxResult confirmResult = MessageBox.Show(
                    $"Are you sure you want to delete the food item: {food.Name}?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 1. Delete foodin
                        var foodIngredients = _context.FoodIngredients
                                                    .Where(fi => fi.FoodId == food.FoodId)
                                                    .ToList();

                        if (foodIngredients.Any())
                        {
                            _context.FoodIngredients.RemoveRange(foodIngredients);
                        }

                        // 2. Delete orderdetail
                        var orderDetails = _context.OrderDetails
                                               .Where(od => od.FoodId == food.FoodId)
                                               .ToList();
                        if (orderDetails.Any())
                        {
                            _context.OrderDetails.RemoveRange(orderDetails);
                        }

                        // 3. Delete Food
                        _context.Foods.Remove(food);
                        _context.SaveChanges();

                        ResetFoodFormAndGrid();

                        MessageBox.Show($"Food '{food.Name}' has been successfully deleted.",
                                        "Deletion Success",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred during deletion: {ex.Message}",
                                        "Database Error",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a food item to delete.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void FoodIn_Click(object sender, RoutedEventArgs e)
        {
            if (FoodDataGrid.SelectedItem is Food f)
            {
                // FoodIngredientManagerWindow cũng phải nằm trong namespace Views.Managers
                var foodIn_Page = new FoodIngredientManagerWindow(f.Name, f.FoodId);
                if (foodIn_Page != null)
                {
                    this.Hide();
                    foodIn_Page.ShowDialog();
                    this.Show();
                }
            }
            else
            {
                MessageBox.Show("Please select a food.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}