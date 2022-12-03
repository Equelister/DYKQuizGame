using System.Windows;
using System.Windows.Input;

namespace DYKClient.LoginWindow
{
    public partial class LoginWindow : Window
    {
        LoginPage loginPage = new LoginPage();
        RegisterPage registerPage = new RegisterPage();

        public LoginWindow()
        {
            InitializeComponent();
            LoginWindowFrame.Content = loginPage;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LoginPageButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindowFrame.Content = loginPage;
        }

        private void RegisterPageButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindowFrame.Content = registerPage;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Mouse.GetPosition(this).Y < 30)
                this.DragMove();
        }

        private void LoginWindowFrameTopBar_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}
