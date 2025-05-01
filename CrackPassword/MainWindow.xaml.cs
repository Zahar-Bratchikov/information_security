using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace CrackPassword
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            Title = $"Main Window - Welcome, {currentUser.Name}";

            // Если пользователь не ADMIN, удаляем вкладку Admin Actions
            if (!currentUser.Name.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                tabControl.Items.Remove(adminTab);
            }
            else
            {
                lblAdminAccess.Text = "Admin Actions:";
            }
            LoadUsers();
        }

        // Загрузка пользователей из UserManager
        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
                lstUsers.Items.Add(user);
        }

        // Обработка изменения пароля
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string oldPass = txtOldPassword.Password;
            string newPass = txtNewPassword.Password;
            string confirmPass = txtConfirmPassword.Password;

            // Проверка совпадения нового пароля с подтверждением
            if (newPass != confirmPass)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            // Проверка соответствия нового пароля требованиям
            if (!UserManager.ValidatePassword(newPass, currentUser.PasswordRestrictions))
            {
                MessageBox.Show("New password does not meet requirements.");
                return;
            }

            // Попытка смены пароля (учитывается хэширование)
            if (UserManager.ChangePassword(currentUser.Name, oldPass, newPass))
            {
                MessageBox.Show("Password changed successfully.");
            }
            else
            {
                MessageBox.Show("Incorrect old password.");
            }
        }

        // Выход из системы
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Добавление нового пользователя
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            InputDialog dlg = new InputDialog("Enter user name:", "Add User");
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                string userName = dlg.ResponseText;
                if (!string.IsNullOrWhiteSpace(userName) && UserManager.GetUser(userName) == null)
                {
                    // Новый пользователь создается с дефолтным паролем "defaultPassword"
                    UserManager.AddUser(userName, "defaultPassword");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Invalid or duplicate user name.");
                }
            }
        }

        // Блокировка пользователя
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

        // Разблокировка пользователя
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

        // Обновление требований к паролю для выбранного пользователя
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

        // Событие изменения введённого нового пароля для оценки его прочности
        private void TxtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength();
        }

        // Расчёт предполагаемого времени взлома пароля
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

        // Метод определения размера используемого алфавита
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

        // Форматирование времени взлома в читаемый вид
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