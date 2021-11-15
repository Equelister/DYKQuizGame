using DYKClient.Core;
using DYKClient.MVVM.Model;
using DYKClient.Net;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DYKClient.LoginWindow
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        private RelayCommand ReceivedLoginResultCommand { get; set; }

        public LoginPage()
        {
            InitializeComponent();
            
            //InitializeConnectionToServer();
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
            string emailTe = emailTextBox.Text.ToString();

            if (string.IsNullOrEmpty(emailTextBox.Text) == false && string.IsNullOrEmpty(passwordPasswordBox.Password.ToString()) == false)
            {
                GlobalClass.Server.SendLoginCredentialsToServer(emailTextBox.Text, HashPassword(passwordPasswordBox.Password));

                /*if (_gc.Server.GetLoginCredentialsResult())
                {
                    App.Current.MainWindow.Hide();
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    App.Current.MainWindow.Close();
                }else
                {*/
                   // _gc.Server.DisconnectFromServer();
                   // _gc.Server = null;
                //}
            }else
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
            //Application.Current.Dispatcher.Invoke(() => Messages.Add(msg));
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

            /*        private bool LoginEvent()
                    {
                        string userEmail = emailTextBox.Text;
                        string userPassword = passwordPasswordBox.Password;

                        Net.Server server = new Net.Server();
                        server.SendLoginCredentialsToServer(userEmail, userPassword);

                        server.DisconnectFromServer();

                        return true;
                    }*/
        


        private string HashPassword(string password)
        {/*
                        byte[] salt = new byte[128 / 8];
                        using (var rngCsp = new RNGCryptoServiceProvider())
                        {
                            rngCsp.GetNonZeroBytes(salt);
                        }
                        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: password,
                            salt: salt,
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 100000,
                            numBytesRequested: 256 / 8));
                        return hashed;
*/

            return password;
        }
    }
}
