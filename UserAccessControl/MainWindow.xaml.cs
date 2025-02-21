using System.Windows;
using System.Windows.Controls;

namespace UserAccessControl
{
    public partial class MainWindow : Window
    {
        private User currentUser;
        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            Title = $"Main Window - Welcome {currentUser.Name}";

            if (currentUser.Name != "ADMIN")
            {
                adminTab.Visibility = Visibility.Hidden;
            }
            else
            {
                lblAdminAccess.Text = "Admin Actions:";
            }
            LoadUsers();
        }

        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
                lstUsers.Items.Add(user);
        }

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
                MessageBox.Show("Password does not meet the requirements.");
                return;
            }
            if (UserManager.ChangePassword(currentUser.Name, oldPass, newPass))
                MessageBox.Show("Password changed successfully.");
            else
                MessageBox.Show("Incorrect old password.");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            InputDialog dlg = new InputDialog("Enter new user name:", "Add User");
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                string userName = dlg.ResponseText;
                if (!string.IsNullOrWhiteSpace(userName) && UserManager.GetUser(userName) == null)
                {
                    UserManager.AddUser(userName);
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Invalid or duplicate user name.");
                }
            }
        }

        private void BtnBlockUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                if (selectedUser.Name == "ADMIN")
                {
                    MessageBox.Show("Cannot block ADMIN.");
                    return;
                }
                UserManager.BlockUser(selectedUser.Name, true);
                MessageBox.Show($"User {selectedUser.Name} is blocked.");
                LoadUsers();
            }
        }

        private void BtnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                UserManager.BlockUser(selectedUser.Name, false);
                MessageBox.Show($"User {selectedUser.Name} is unblocked.");
                LoadUsers();
            }
        }

        private void BtnUpdateRequirements_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to update requirements.");
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
    }
}