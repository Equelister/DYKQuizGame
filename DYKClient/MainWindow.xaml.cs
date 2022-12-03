using System.Windows;
using System.Windows.Input;

namespace DYKClient
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            GlobalClass.Server.closeWindowEvent += CloseWindow;
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Mouse.GetPosition(this).Y < 30)
                this.DragMove();
        }

        public void CloseWindow()
        {
            Dispatcher.Invoke(() =>
            {
                App.Current.Shutdown();
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var Result = MessageBox.Show("Did You Know", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (Result == MessageBoxResult.Yes)
            {
                SystemCommands.CloseWindow(this);
            }
        }
    }
}
