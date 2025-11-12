using System.Configuration;
using System.Data;
using System.Windows;

namespace TFastFoodProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Đặt chế độ tắt ứng dụng khi cửa sổ chính (MainWindow - thường là LoginWindow) bị đóng.
            // Điều này đảm bảo tất cả các process nền đều dừng lại.
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

    }

}
