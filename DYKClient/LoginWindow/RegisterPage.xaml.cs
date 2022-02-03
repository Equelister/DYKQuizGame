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
    /// Interaction logic for RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private void InitializeConnectionToServer()
        {
            GlobalClass.Server = new Server();
            GlobalClass.Server.unlockRegisterButtonEvent += UnlockRegisterButton;
        }

        private void UserRegister(object sender, RoutedEventArgs e)
        {
            //registerButton.IsEnabled = false;
            InitializeConnectionToServer();
            string emailTe = emailTextBox.Text.ToString();

            if (string.IsNullOrEmpty(emailTextBox.Text) == false &&
                string.IsNullOrEmpty(userNameTextBox.Text) == false &&
                string.IsNullOrEmpty(passwordPasswordBox.Password.ToString()) == false &&
                string.IsNullOrEmpty(retypePasswordPasswordBox.Password.ToString()) == false)
            {
                if (retypePasswordPasswordBox.Password.ToString().Equals(passwordPasswordBox.Password.ToString()))
                {
                    GlobalClass.Server.SendRegisterCredentialsToServer(
                        emailTextBox.Text,
                        HashPassword(passwordPasswordBox.Password),
                        userNameTextBox.Text);

                    emailTextBox.Text = "";
                    userNameTextBox.Text = "";
                    passwordPasswordBox.Password = "";
                    retypePasswordPasswordBox.Password = "";
                }
            }
            else
            {
                UnlockRegisterButton();
            }
        }

        private void UnlockRegisterButton()
        {
            if (registerButton.IsEnabled == false)
            {
                registerButton.IsEnabled = true;
            }
        }
        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            /*            using (var rngCsp = new RNGCryptoServiceProvider())
                        {
                            rngCsp.GetNonZeroBytes(salt);
                        }*/
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
