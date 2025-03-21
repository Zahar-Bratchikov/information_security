using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics; // Ensure you have referenced System.Numerics
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrackPassword
{
    // Главное окно с вкладками для Change Password, Admin Actions и Password Cracking
    public partial class MainWindow : Window
    {
        private User currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            Title = $"Main Window - Welcome, {currentUser.Name}";

            // Если пользователь не ADMIN, удаляем вкладки Admin Actions и Password Cracking
            if (!currentUser.Name.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                tabControl.Items.Remove(adminTab);
                //tabControl.Items.Remove(passwordCrackingTab);
            }
            else
            {
                lblAdminAccess.Text = "Admin Actions:";
            }
            LoadUsers();
        }

        // Загрузка пользователей в ListBox
        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
                lstUsers.Items.Add(user);
        }

        // Обработка нажатия кнопки Change Password
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = txtOldPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmPassword.Password;
            if (newPass != confirmPass)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }
            if (!UserManager.ValidatePassword(newPass, currentUser.PasswordRestrictions))
            {
                MessageBox.Show("New password does not meet requirements.");
                return;
            }
            if (UserManager.ChangePassword(currentUser.Name, oldPass, newPass))
                MessageBox.Show("Password changed successfully.");
            else
                MessageBox.Show("Incorrect old password.");
        }

        // Обработка нажатия кнопки Logout
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Обработка добавления нового пользователя
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            InputDialog dlg = new InputDialog("Enter user name:", "Add User");
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                string userName = dlg.ResponseText;
                if (!string.IsNullOrWhiteSpace(userName) && UserManager.GetUser(userName) == null)
                {
                    UserManager.AddUser(userName, "defaultPassword");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Invalid or duplicate user name.");
                }
            }
        }

        // Обработка блокировки пользователя
        private void BtnBlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                if (selectedUser.Name.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Cannot block ADMIN.");
                    return;
                }
                UserManager.BlockUser(selectedUser.Name, true);
                MessageBox.Show($"User {selectedUser.Name} has been blocked.");
                LoadUsers();
            }
        }

        // Обработка разблокировки пользователя
        private void BtnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                UserManager.BlockUser(selectedUser.Name, false);
                MessageBox.Show($"User {selectedUser.Name} has been unblocked.");
                LoadUsers();
            }
        }

        // Обновление требований к паролю
        private void BtnUpdateRequirements_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Select a user to update requirements.");
                return;
            }
            User selectedUser = lstUsers.SelectedItem as User;
            var user = UserManager.GetUser(selectedUser.Name);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }
            var restrictions = new PasswordRestrictions
            {
                EnableLengthRestriction = chkEnableLength.IsChecked ?? false,
                MinLength = int.TryParse(txtMinLength.Text, out int min) ? min : 0,
                RequireUppercase = chkRequireUppercase.IsChecked ?? false,
                RequireDigit = chkRequireDigit.IsChecked ?? false,
                RequireSpecialChar = chkRequireSpecial.IsChecked ?? false
            };
            user.PasswordRestrictions = restrictions;
            UserManager.SaveUsers();
            MessageBox.Show("Password requirements updated successfully.");
        }

        // При выборе пользователя в списке загружаем его настройки
        private void LstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                var user = UserManager.GetUser(selectedUser.Name);
                if (user != null)
                {
                    chkEnableLength.IsChecked = user.PasswordRestrictions.EnableLengthRestriction;
                    txtMinLength.Text = user.PasswordRestrictions.MinLength.ToString();
                    chkRequireUppercase.IsChecked = user.PasswordRestrictions.RequireUppercase;
                    chkRequireDigit.IsChecked = user.PasswordRestrictions.RequireDigit;
                    chkRequireSpecial.IsChecked = user.PasswordRestrictions.RequireSpecialChar;
                }
            }
        }

        // Обработчик события PasswordChanged для поля нового пароля
        private void TxtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength();
        }

        // Обновление оценки времени взлома нового пароля
        private void UpdatePasswordStrength()
        {
            string password = txtNewPassword.Password;
            if (string.IsNullOrEmpty(password))
            {
                txtPasswordEstimate.Text = "";
                return;
            }
            int alphabetSize = GetAlphabetSize(password);
            BigInteger totalCombinations = BigInteger.Pow(alphabetSize, password.Length);
            BigInteger speed = 150000; 
            BigInteger timeSec = totalCombinations / speed;
            string formattedTime = FormatTime(timeSec);

            string strength;
            double tSecDouble = (double)timeSec;
            if (tSecDouble < 3600)
                strength = "Weak";
            else if (tSecDouble < 31536000)
                strength = "Moderate";
            else
                strength = "Strong";

            txtPasswordEstimate.Text = $"Estimated cracking time: {formattedTime}\nPassword Strength: {strength}";
        }

        // Метод расчета размера алфавита символов пароля
        private int GetAlphabetSize(string password)
        {
            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
            int size = 0;
            if (hasLower) size += 26;
            if (hasUpper) size += 26;
            if (hasDigit) size += 10;
            if (hasSpecial) size += 33;
            return size;
        }

        // Форматирование времени взлома в читаемый формат
        private string FormatTime(BigInteger totalSeconds)
        {
            BigInteger years = totalSeconds / (365 * 24 * 3600);
            totalSeconds %= (365 * 24 * 3600);
            int months = (int)(totalSeconds / (30 * 24 * 3600));
            totalSeconds %= (30 * 24 * 3600);
            int days = (int)(totalSeconds / (24 * 3600));
            totalSeconds %= (24 * 3600);
            int hours = (int)(totalSeconds / 3600);
            totalSeconds %= 3600;
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return $"{years} years, {months} months, {days} days, {hours} hours, {minutes} minutes, {seconds} seconds";
        }
    }
}