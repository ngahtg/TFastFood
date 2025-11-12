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
    /// <summary>
    /// Interaction logic for FoodManagerWindow.xaml
    /// </summary>
    public partial class IngredientManagerWindow : Window
    {
        readonly private TfastFoodContext _context;
        public IngredientManagerWindow()
        {
            _context = new TfastFoodContext();
            InitializeComponent();
            LoadIngredient();
        }

        private void ResetIngredientFormAndGrid()
        {
          
            IngredientNameTextBox.Text = string.Empty;
            UnitTextBox.Text = string.Empty;
            StockQuantityTextBox.Text = string.Empty;


            IngredientDataGrid.SelectedItem = null;

            LoadIngredient();
        }

        private void LoadIngredient()
        {
            IngredientDataGrid.ItemsSource = _context.Ingredients.ToList();
        }

        private void IngredientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IngredientDataGrid.SelectedItem is Ingredient ing) {
                IngredientNameTextBox.Text = ing.Name;
                UnitTextBox.Text = ing.Unit;
                if (ing.StockQuantity.HasValue)
                {
                
                    StockQuantityTextBox.Text = ing.StockQuantity.Value.ToString("G29");
                }
                else
                {
                    StockQuantityTextBox.Text = string.Empty;
                }

            }
            
        }

        private void Refesh_Click(object sender, RoutedEventArgs e)
        {
            ResetIngredientFormAndGrid();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string name = IngredientNameTextBox.Text;
            string unit = UnitTextBox.Text;
            string stockQtyText = StockQuantityTextBox.Text;

            // 2. REQUIRED FIELDS VALIDATION
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(unit) ||
                string.IsNullOrWhiteSpace(stockQtyText))
            {
                MessageBox.Show("Please fill in all fields (Name, Unit, Stock Quantity).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. NUMBER FORMAT VALIDATION for StockQuantity
            if (!decimal.TryParse(stockQtyText, out decimal stockQuantity))
            {
                MessageBox.Show("Stock Quantity must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if the number is non-negative
            if (stockQuantity < 0)
            {
                MessageBox.Show("Stock Quantity cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. DATABASE LOGIC (Unique Name and Addition)
            try
            {
                // Check for duplicate ingredient name (assuming Name must be unique)
                if (_context.Ingredients.Any(i => i.Name.Equals(name)))
                {
                    MessageBox.Show($"Ingredient '{name}' already exists.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create new Ingredient object
                Ingredient ing = new Ingredient
                {
                    Name = name,
                    Unit = unit,
                    StockQuantity = stockQuantity
                };

                _context.Ingredients.Add(ing);
                _context.SaveChanges();

           
                ResetIngredientFormAndGrid();
                MessageBox.Show($"Ingredient '{name}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while adding ingredient: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
          
            if (IngredientDataGrid.SelectedItem is Ingredient selectedIngredient)
            {
                // 2. Retrieve data from controls
                string name = IngredientNameTextBox.Text;
                string unit = UnitTextBox.Text;
                string stockQtyText = StockQuantityTextBox.Text;

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(unit) ||
                    string.IsNullOrWhiteSpace(stockQtyText))
                {
                    MessageBox.Show("Please fill in all fields (Name, Unit, Stock Quantity).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Number format validation for StockQuantity
                if (!decimal.TryParse(stockQtyText, out decimal stockQuantity))
                {
                    MessageBox.Show("Stock Quantity must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (stockQuantity < 0)
                {
                    MessageBox.Show("Stock Quantity cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // 4. DATABASE UPDATE LOGIC
                try
                {
                    // Find the original entity in the database
                    var ingredientToUpdate = _context.Ingredients.FirstOrDefault(i => i.IngredientId == selectedIngredient.IngredientId);

                    if (ingredientToUpdate != null)
                    {
                        // Check for duplicate name (only check other ingredients)
                        if (_context.Ingredients.Any(i => i.Name.Equals(name) && i.IngredientId != selectedIngredient.IngredientId))
                        {
                            MessageBox.Show($"Ingredient '{name}' already exists for another item.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Update properties
                        ingredientToUpdate.Name = name;
                        ingredientToUpdate.Unit = unit;
                        ingredientToUpdate.StockQuantity = stockQuantity;

                        _context.Ingredients.Update(ingredientToUpdate);
                        _context.SaveChanges();

                       
                        ResetIngredientFormAndGrid();

                        MessageBox.Show($"Ingredient '{name}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        // TODO: Refresh the DataGrid
                        // LoadIngredients(); 
                    }
                    else
                    {
                        MessageBox.Show("Ingredient not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred during update: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select an Ingredient to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if(IngredientDataGrid.SelectedItem is Ingredient selectedIngredient)
            {
                MessageBoxResult result = MessageBox.Show(
                            $"Are you sure you want to delete staff member: {selectedIngredient.Name} ? " +
                            $"This will delete all associated orders and order details.",
                            "Confirm Deletion",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    var foodin = _context.FoodIngredients.Where(fi => fi.IngredientId == selectedIngredient.IngredientId).ToList();
                    if (foodin.Any())
                    {
                        _context.FoodIngredients.RemoveRange(foodin);
                    }
                    var ingre_se = _context.Ingredients.FirstOrDefault(ig => ig.IngredientId == selectedIngredient.IngredientId);
                    if (ingre_se != null)
                    {
                        _context.Ingredients.Remove(ingre_se);
                    }
                    _context.SaveChanges();
                    ResetIngredientFormAndGrid();

                    MessageBox.Show($"Ingredient '{selectedIngredient.Name}' Deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }

            } else
            {
                MessageBox.Show("Please select an Ingredient to Delete", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var dashboard_page = new TFastFoodProject.Admin.AdminViews.AdminPage();
            if (dashboard_page != null)
            {
                this.Hide();
                dashboard_page.ShowDialog();
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
    }
}
