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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using TFastFoodProject.Employee.EmployeeViews;
using TFastFoodProject.Models;

namespace TFastFoodProject.Employee.EmployeeManager
{
    /// <summary>
    /// Interaction logic for OrderManagerWindow.xaml
    /// </summary>
    public partial class OrderManagerWindow : Window
    {
        private int staffId;
        private readonly TfastFoodContext _context;
      

        public OrderManagerWindow(int staffId)
        {
            InitializeComponent();
            _context = new TfastFoodContext();
            this.staffId = staffId;
            var staff = _context.Staff.FirstOrDefault(s => s.StaffId == staffId);
            NameTextBlock.Text = $"Welcome back, { staff.FullName} !";
           
            LoadOrder();
        }

        private void Reset()
        {
            OrderDataGrid.SelectedItem = null;
            LoadOrder();
        }

        private void LoadOrder()
        {

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Lọc các đơn hàng:
            // 1. Lớn hơn hoặc bằng 00:00:00 hôm nay
            // 2. VÀ nhỏ hơn 00:00:00 ngày mai
            OrderDataGrid.ItemsSource = _context.Orders
                                                .Include(o => o.Staff)
                                                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow && o.StaffId == staffId)
                                                .ToList();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Order od = new Order();
            od.OrderDate = DateTime.Now;
            od.Status = "Pending";
            od.StaffId = staffId;
            od.TotalPrice = 0;


            _context.Orders.Add(od);
            _context.SaveChanges();
            
            Reset();
        }

        private void Detail_Click(object sender, RoutedEventArgs e)
        {
            if (OrderDataGrid.SelectedItem is Order od)
            {
                if (od.Status.Equals("Completed"))
                {
                    MessageBox.Show("Can't detail ordder with the completed order !",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    return;
                }
                if (od.Status.Equals("Cancelled"))
                {
                    MessageBox.Show("Can't detail ordder with the cancelled order !",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    return;
                }
                var orderD_page = new OrderDetailWindow(od.OrderId);
                if(orderD_page != null)
                {
                    this.Hide();
                    orderD_page.ShowDialog();
                    this.Show();
                }
            } else
            {
                MessageBox.Show("Please select a order to write of it.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (OrderDataGrid.SelectedItem is Order selectedOrder) // Đổi tên biến để tránh bug
            {
                // Kiểm tra trạng thái
                if (selectedOrder.Status.Equals("Completed"))
                {
                    MessageBox.Show("Không thể hủy đơn hàng đã hoàn thành!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (selectedOrder.Status.Equals("Cancelled"))
                {
                    MessageBox.Show("Đơn hàng này đã được hủy từ trước!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Xác nhận / không
                var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy Đơn hàng #{selectedOrder.OrderId}?" +
                                             "\nTất cả nguyên liệu sẽ được phục hồi về kho.",
                                             "Xác nhận Hủy",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    return; 
                }

                // PHỤC HỒI KHO 
                try
                {


                    // Lấy tất cả chi tiết của đơn hàng này
                    var detailsToRestore = _context.OrderDetails
                                                   .Where(d => d.OrderId == selectedOrder.OrderId)
                                                   .ToList();

                    foreach (var detail in detailsToRestore)
                    {
                        // Lấy công thức (requiredIngredients) của món ăn
                        var requiredIngredients = _context.FoodIngredients
                                                          .Where(fi => fi.FoodId == detail.FoodId)
                                                          .ToList(); 

                        foreach (var item in requiredIngredients)
                        {
                            // Tìm nguyên liệu trong kho
                            var ingredientInDb = _context.Ingredients.Find(item.IngredientId);

                            if (ingredientInDb != null && item.QuantityUsed.HasValue)
                            {
                                // CỘNG TRẢ LẠI KHO 
                                ingredientInDb.StockQuantity += (detail.Quantity * item.QuantityUsed.Value);
                            }
                        }
                    }

         

          
                    selectedOrder.Status = "Cancelled";
                    _context.Orders.Update(selectedOrder);

                    
                    _context.SaveChanges();

                    Reset(); 
                    MessageBox.Show("Đã hủy đơn hàng thành công! Kho đã được phục hồi.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi nghiêm trọng khi hủy: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a order to cancel.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Completed_Click(object sender, RoutedEventArgs e)
        {
            if (OrderDataGrid.SelectedItem is Order od)
            {
                if (od.Status.Equals("Cancelled"))
                {
                    MessageBox.Show("Can't completed with the cancelled order !",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    return;
                }
                if (od.Status.Equals("Completed"))
                {
                    MessageBox.Show("Can't completed with the completed order !",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    return;
                }
                od.Status = "Completed";
                _context.Orders.Update(od);
                _context.SaveChanges();
                Reset();
                MessageBox.Show("Order completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            else
            {
                MessageBox.Show("Please select a order to completed.",
                                "Selection Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

       

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            var staff = _context.Staff.FirstOrDefault(s => s.StaffId == staffId);
            var staff_page = new Employee.EmployeeViews.StaffPageWindow(staff.FullName);
            if (staff_page != null)
            {
                this.Hide();
                staff_page.ShowDialog();
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
    }
}
