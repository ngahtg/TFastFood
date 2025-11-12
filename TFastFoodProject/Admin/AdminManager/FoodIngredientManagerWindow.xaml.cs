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

namespace TFastFoodProject.Admin.AdminManagers.AdminManager
{
    /// <summary>
    /// Interaction logic for FoodIngredientManagerWindow.xaml
    /// </summary>
    public partial class FoodIngredientManagerWindow : Window
    {
        private int foodId;
        private readonly TfastFoodContext _context;

        public FoodIngredientManagerWindow(string name, int foodId)
        {

            InitializeComponent();
            _context = new TfastFoodContext();
            this.foodId = foodId;
            FoodNameTextBlock.Text = $"🍔 Food Name: {name}";
            LoadFoodIn();
            LoadIngredient();

        }

        private void ResetFormAndGrid()
        {
            IngredientComboBox.SelectedIndex = -1;
            QuantityUsedTextBox.Text = "";

            // Đảm bảo DataGrid không giữ bất kỳ lựa chọn nào
            FoodIngredientDataGrid.SelectedItem = null;
            FoodIngredientDataGrid.SelectedIndex = -1;

            LoadFoodIn();
            LoadIngredient();
        }

        private void LoadIngredient()
        {
            IngredientComboBox.ItemsSource = _context.Ingredients.Select(i => i.Name).ToList();
        }

        private void LoadFoodIn()
        {
            FoodIngredientDataGrid.ItemsSource = _context.FoodIngredients.Include(f => f.Ingredient).Where(f => f.FoodId == foodId).ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // VALIDATION
            if (IngredientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an Ingredient.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(QuantityUsedTextBox.Text, out decimal quantityUsed) || quantityUsed <= 0)
            {
                MessageBox.Show("Quantity Used must be a valid number greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // DUPLICATE CHECK

            string selectedIngredientName = IngredientComboBox.SelectedItem.ToString();
            var ingredient = _context.Ingredients.FirstOrDefault(i => i.Name == selectedIngredientName);

            if (ingredient == null)
            {
                MessageBox.Show("Selected ingredient not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            


            // LẤY IngredientId để kiểm tra Khóa chính kép
            int newIngredientId = ingredient.IngredientId;

            // 2. CHECK DUPLICATE PRIMARY KEY (FoodId, IngredientId)
            bool isDuplicate = _context.FoodIngredients
                .Any(fi => fi.FoodId == foodId && fi.IngredientId == newIngredientId);

            if (isDuplicate)
            {
                MessageBox.Show($"The ingredient '{selectedIngredientName}' is already listed for this food. Use Edit to update the quantity.",
                                "Duplicate Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return; 
            }

            // ADD
            try
            {
                FoodIngredient fin = new FoodIngredient
                {
                    FoodId = foodId,
                    IngredientId = newIngredientId, 
                    QuantityUsed = quantityUsed
                };

                _context.FoodIngredients.Add(fin);
                _context.SaveChanges();

                MessageBox.Show("Ingredient added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // RESET
                ResetFormAndGrid();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"An error occurred while adding the ingredient: {errorMessage}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
         
           
            if (!(FoodIngredientDataGrid.SelectedItem is FoodIngredient selectedFin))
            {
                MessageBox.Show("Please select a food ingredient to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // VALIDATION 
            if (IngredientComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an Ingredient.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(QuantityUsedTextBox.Text, out decimal quantityUsed) || quantityUsed <= 0)
            {
                MessageBox.Show("Quantity Used must be a valid number greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check Duplicate (FoodID, IngredientID)
            string newIngredientName = IngredientComboBox.SelectedItem.ToString();
            var newIngredient = _context.Ingredients.FirstOrDefault(i => i.Name == newIngredientName);

            if (newIngredient == null)
            {
                MessageBox.Show("Selected ingredient not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int newIngredientId = newIngredient.IngredientId;

            // Duplicate IngredientId
            if (newIngredientId != selectedFin.IngredientId)
            {
                bool isDuplicateKey = _context.FoodIngredients
                    .Any(fi => fi.FoodId == foodId && fi.IngredientId == newIngredientId);

                if (isDuplicateKey)
                {
                    MessageBox.Show($"Cannot change ingredient to '{newIngredientName}'. This food item already uses that ingredient.",
                                    "Duplicate Key Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Update
            try
            {
                
                var finToUpdate = _context.FoodIngredients
                    .FirstOrDefault(fi => fi.FoodId == selectedFin.FoodId && fi.IngredientId == selectedFin.IngredientId);

                if (finToUpdate != null)
                {
                    // IngredientId Change
                    if (finToUpdate.IngredientId != newIngredientId)
                    {
                        // Delete 
                        _context.FoodIngredients.Remove(finToUpdate);

                        // Create new
                        FoodIngredient newFin = new FoodIngredient
                        {
                            FoodId = foodId,
                            IngredientId = newIngredientId,
                            QuantityUsed = quantityUsed
                        };
                        _context.FoodIngredients.Add(newFin);
                    }
                    else
                    {
                        // QuantityUsed change
                        finToUpdate.QuantityUsed = quantityUsed;
                        
                    }
                }

                _context.SaveChanges();

                MessageBox.Show("Ingredient updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset 
                ResetFormAndGrid();
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"An error occurred while updating the ingredient: {errorMessage}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // VALIDATION SELECTION
            if (!(FoodIngredientDataGrid.SelectedItem is FoodIngredient selectedFin))
            {
                MessageBox.Show("Please select an ingredient to remove from this food.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                return;
            }

            // CONFIRM DELETE
            MessageBoxResult confirmResult = MessageBox.Show(
                $"Are you sure you want to remove '{selectedFin.Ingredient.Name}' from this food?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    _context.FoodIngredients.Remove(selectedFin);
                    _context.SaveChanges();

                    MessageBox.Show("Ingredient successfully removed.",
                                    "Deletion Success",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    // RESET
                    ResetFormAndGrid();
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show($"An error occurred during deletion: {errorMessage}",
                                    "Database Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ResetFormAndGrid();

        }

        private void FoodIngredientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FoodIngredientDataGrid.SelectedItem is FoodIngredient fin)
            {
                var ing = _context.Ingredients.FirstOrDefault(i => i.IngredientId == fin.IngredientId);
                IngredientComboBox.SelectedItem = ing.Name.ToString();
                QuantityUsedTextBox.Text = fin.QuantityUsed.ToString();
            }
            

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            var foodMana_page = new FoodManagerWindow();
            if (foodMana_page != null)
            {
                this.Hide();
                foodMana_page.ShowDialog();
                this.Show();
            }
        }
    }
}
