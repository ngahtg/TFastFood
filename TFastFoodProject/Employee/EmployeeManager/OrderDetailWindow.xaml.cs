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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens;
using TFastFoodProject.Models;

namespace TFastFoodProject.Employee.EmployeeManager
{
    public partial class OrderDetailWindow : Window
    {
        private readonly TfastFoodContext _context;
        private Order _currentOrder; // Đơn hàng đang chỉnh sửa
        private ObservableCollection<OrderDetail> _detailsList; // Giỏ hàng (bind với UI)

        public OrderDetailWindow(int orderId)
        {
            InitializeComponent();
            _context = new TfastFoodContext();

            // 1. Tải Order VÀ các chi tiết (OrderDetail) VÀ Food liên quan
            _currentOrder = _context.Orders
                                    .Include(o => o.OrderDetails) // Tải các chi tiết
                                    .ThenInclude(od => od.Food)   // Tải luôn Food của chi tiết
                                    .FirstOrDefault(o => o.OrderId == orderId);
            
            if (_currentOrder == null)
            {
                MessageBox.Show("Lỗi: Không tìm thấy đơn hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            txtOrderId.Text = _currentOrder.OrderId.ToString();

            // 2. Khởi tạo "giỏ hàng" từ các chi tiết đã có (nếu có)
            _detailsList = new ObservableCollection<OrderDetail>(_currentOrder.OrderDetails);
            dgOrderDetail.ItemsSource = _detailsList;

            // 3. Tải các phần khác
            LoadedCategory();
            LoadFoodList();
            UpdateTotalAmount(); // Cập nhật tổng tiền (có thể là 0)
        }

        #region (1) Tải Danh sách Món ăn & Lọc
        private void LoadFoodList()
        {
            IQueryable<Food> query = _context.Foods;
            query = query.Where(f => f.FoodIngredients.Any(fi => fi.QuantityUsed.HasValue && fi.QuantityUsed > 0));
            string selectedCategory = cbCategory.SelectedItem as string;
            if (selectedCategory != null && selectedCategory != "All")
            {
                query = query.Where(f => f.Category == selectedCategory );
                
            }

            string searchText = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(f => f.Name.ToLower().Contains(searchText));
            }

            dgFoodList.ItemsSource = query.ToList();
        }

        private void LoadedCategory()
        {
            var categories = _context.Foods
                                     .Select(f => f.Category)
                                     .Distinct()
                                     .OrderBy(c => c)
                                     .ToList();
            categories.Insert(0, "All");
            cbCategory.ItemsSource = categories;
            cbCategory.SelectedIndex = 0;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadFoodList();
        }

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_context != null)
            {
                LoadFoodList();
            }
        }
        #endregion

        #region (2) Logic Nút Thêm, Xóa, Lưu

        // KIỂM TRA TỒN KHO 
        private bool CheckStock(Food food, int quantityToOrder)
        {
            // 1. Lấy công thức (các nguyên liệu cần)
            var requiredIngredients = _context.FoodIngredients
                                              .Include(fi => fi.Ingredient) // Join với bảng Ingredient
                                              .Where(fi => fi.FoodId == food.FoodId)
                                              .ToList();

            // 3. Xử lý món có công thức (như Burger, Fries)
            foreach (var item in requiredIngredients)
            {
                // Đảm bảo dữ liệu không bị null
                if (!item.QuantityUsed.HasValue || item.QuantityUsed.Value == 0) continue;
                if (!item.Ingredient.StockQuantity.HasValue) continue;

                // Tính toán (giống như cũ)
                decimal neededQuantity = (decimal)quantityToOrder * item.QuantityUsed.Value;

                if (item.Ingredient.StockQuantity.Value < neededQuantity)
                {
                    // --- SỬA THÔNG BÁO Ở ĐÂY ---

                    // Tính số lượng tối đa có thể làm từ nguyên liệu này
                    int maxCanMake = (int)Math.Floor(item.Ingredient.StockQuantity.Value / item.QuantityUsed.Value);

                    MessageBox.Show($"Không đủ tồn kho cho món: {food.Name}." +
                                    $"\n(Do hết nguyên liệu: {item.Ingredient.Name})" +
                                    $"\n\nBạn muốn đặt: {quantityToOrder} phần." +
                                    $"\nKho chỉ đủ làm tối đa: {maxCanMake} phần.", 
                                    "Lỗi Tồn Kho", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true; 
        }

        // THÊM MÓN VÀO ĐƠN HÀNG
        private void btnAddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgFoodList.SelectedItem is not Food selectedFood)
            {
                MessageBox.Show("Vui lòng chọn một món ăn.", "Chưa chọn món", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là một số nguyên dương.", "Số lượng không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Kiểm tra tồn kho
            if (!CheckStock(selectedFood, quantity))
            {
                return; 
            }

            //Thêm 
            var existingDetail = _detailsList.FirstOrDefault(d => d.FoodId == selectedFood.FoodId);

            // Đã có 
            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
                dgOrderDetail.Items.Refresh(); 
            }
            else
            {
                // Chưa có
                var newDetail = new OrderDetail
                {
                    OrderId = _currentOrder.OrderId,
                    FoodId = selectedFood.FoodId,
                    Quantity = quantity,
                    Food = selectedFood, 
                    Order = _currentOrder 
                };

                _detailsList.Add(newDetail); 
                _context.OrderDetails.Add(newDetail); 
            }

            UpdateTotalAmount();
        }

        // XÓA MÓN KHỎI ĐƠN HÀNG
        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrderDetail.SelectedItem is not OrderDetail selectedDetail)
            {
                MessageBox.Show("Vui lòng chọn một món trong giỏ hàng để xóa.", "Chưa chọn món", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _detailsList.Remove(selectedDetail); // Xóa khỏi UI
            _context.OrderDetails.Remove(selectedDetail); // Báo cho EF theo dõi

            UpdateTotalAmount();
        }

        // LƯU TẤT CẢ THAY ĐỔI (Trừ kho & Phục hồi kho)
        private void btnSaveOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Cập nhật tổng tiền cuối
                _currentOrder.TotalPrice = _detailsList.Sum(d => d.Food.Price * d.Quantity);
                

                // --- 1. Xử lý Thêm/Sửa (Trừ kho) ---
                foreach (var detail in _detailsList)
                {
                    //"hỏi" Entity Framework (EF) về trạng thái (State) của một đối tượng cụ thể
                    var entry = _context.Entry(detail);

                    var requiredIngredients = _context.FoodIngredients
                                                      .Include(fi => fi.Ingredient)
                                                      .Where(fi => fi.FoodId == detail.FoodId)
                                                      .ToList();

                    int quantityToDeduct = 0;

                    // bi add
                    if (entry.State == EntityState.Added)
                    {
                        quantityToDeduct = detail.Quantity;
                    }
                    
                    // bi sua
                    else if (entry.State == EntityState.Modified)
                    {
                        // luu so quantity ma bi sua
                        var originalQuantity = (int)entry.OriginalValues["Quantity"];
                        quantityToDeduct = detail.Quantity - originalQuantity;
                    }

                    // tru di so stock
                    foreach (var item in requiredIngredients)
                    {
                        item.Ingredient.StockQuantity -= (quantityToDeduct * (item.QuantityUsed ?? 0));
                    }
                }

                // --- 2. Xử lý Xóa (Phục hồi kho) ---

                // lấy danh sách vừa bị xóa 
                var deletedDetails = _context.ChangeTracker.Entries<OrderDetail>()
                                           .Where(entry => entry.State == EntityState.Deleted)
                                           .ToList();

                foreach (var entry in deletedDetails)
                {
                    // lay cac thong tin truoc khi xoa boi entry.orginalValue 
                    var foodId = (int)entry.OriginalValues["FoodId"];
                    var quantity = (int)entry.OriginalValues["Quantity"];

                    // asnotracking la lenh toi uu hoa , chi xem va ko thay doi
                    var requiredIngredients = _context.FoodIngredients
                                                      .Include(fi => fi.Ingredient)
                                                      .Where(fi => fi.FoodId == foodId)
                                                      .AsNoTracking()
                                                      .ToList();

                    foreach (var item in requiredIngredients)
                    {
                        var ingredientInDb = _context.Ingredients.Find(item.IngredientId);
                        if (ingredientInDb != null)
                        {
                            ingredientInDb.StockQuantity += (quantity * (item.QuantityUsed ?? 0));
                        }
                    }
                }

                // --- 3. Lưu tất cả thay đổi ---
                
                _context.SaveChanges();

                MessageBox.Show("Đã lưu đơn hàng và cập nhật kho thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi lưu: {ex.Message}\n\n{ex.InnerException?.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region (3) Logic Cập nhật UI

        private void dgFoodList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgFoodList.SelectedItem is Food food)
            {
               
                FoodNameTextBlock.Text = food.Name;
                txtQuantity.Text = "1"; 
            }
        }

        private void UpdateTotalAmount()
        {
            // Tính tổng tiền từ giỏ hàng (_detailsList)
            decimal total = 0;
            foreach (var detail in _detailsList)
            {
                if (detail.Food != null) 
                {
                    total += (detail.Food.Price * detail.Quantity);
                }
            }
            txtTotalAmount.Text = total.ToString(); 
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra thay đổi chưa lưu bang changetracker tiep
            if (_context.ChangeTracker.HasChanges())
            {
                var result = MessageBox.Show("Bạn có thay đổi chưa lưu. Bạn có chắc muốn thoát?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    return; 
                }
            }

           
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == _currentOrder.OrderId);
            if (order?.StaffId != null)
            {
                var orderMana_page = new OrderManagerWindow(order.StaffId.Value);
                this.Hide();
                orderMana_page.ShowDialog();
                this.Show();
            }
            
        }
        #endregion
    }
}