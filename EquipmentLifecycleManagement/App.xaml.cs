using System.Windows;

namespace EquipmentLifecycleManager
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            
        }
    }
}