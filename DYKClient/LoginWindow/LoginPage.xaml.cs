using DYKClient.Core;
using DYKClient.Net;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DYKClient.LoginWindow
{
    public partial class LoginPage : Page
    {
        private RelayCommand ReceivedLoginResultCommand { get; set; }

        public LoginPage()
        {
            InitializeComponent();
        }

        private void InitializeConnectionToServer()
        {
            GlobalClass.Server = new Server();
            GlobalClass.Server.receivedLoginResultEvent += ReceivedLoginResult;
            GlobalClass.Server.unlockLoginButtonEvent += UnlockLoginButton;
        }

        private void UserLogin(object sender, RoutedEventArgs e)
        {
            loginButton.IsEnabled = false;
            InitializeConnectionToServer();

            if (string.IsNullOrEmpty(emailTextBox.Text) == false && string.IsNullOrEmpty(passwordPasswordBox.Password.ToString()) == false)
            {
                GlobalClass.Server.SendLoginCredentialsToServer(emailTextBox.Text, HashPassword(passwordPasswordBox.Password));

                System.Threading.Thread.Sleep(3000);
                UnlockLoginButton();
            }
            else
            {
                UnlockLoginButton();
            }
        }

        private void UnlockLoginButton()
        {
            if (loginButton.IsEnabled == false)
            {
                loginButton.IsEnabled = true;
            }
        }

        private bool ReceivedLoginResult()
        {
            var msg = GlobalClass.Server.PacketReader.ReadMessage();
            if (msg.Equals("credsLegit"))
            {
                Dispatcher.Invoke(() =>
                {
                    App.Current.MainWindow.Hide();
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    App.Current.MainWindow.Close();
                });
                return true;
            }
            else
            {
                GlobalClass.Server.DisconnectFromServer();
                GlobalClass.Server = null;
                Dispatcher.Invoke(() =>
                {
                    UnlockLoginButton();
                });
                return false;
            }
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];

            for (int i = 0; i < salt.Length; i++)
            {
                salt[i] = 0;
            }
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}
