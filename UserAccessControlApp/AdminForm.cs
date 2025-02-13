using System;
using System.Windows.Forms;

namespace UserAccessControl
{
    public class AdminForm : Form
    {
        private ListBox lstUsers;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtOldPassword;
        private Button btnChangePassword;
        private Button btnAddUser;
        private Button btnBlockUser;
        private Button btnSetPasswordRestrictions;
        private Button btnClose;

        public AdminForm()
        {
            Text = "Admin Panel";
            Width = 500;
            Height = 400;

            lstUsers = new ListBox() { Top = 20, Left = 20, Width = 200, Height = 300 };
            LoadUsers();

            txtOldPassword = new TextBox() { Top = 20, Left = 240, Width = 200, PasswordChar = '*' };
            txtNewPassword = new TextBox() { Top = 60, Left = 240, Width = 200, PasswordChar = '*' };
            txtConfirmPassword = new TextBox() { Top = 100, Left = 240, Width = 200, PasswordChar = '*' };

            btnChangePassword = new Button() { Text = "Change Password", Top = 140, Left = 240 };
            btnChangePassword.Click += BtnChangePassword_Click;

            btnAddUser = new Button() { Text = "Add User", Top = 180, Left = 240 };
            btnAddUser.Click += BtnAddUser_Click;

            btnBlockUser = new Button() { Text = "Block User", Top = 220, Left = 240 };
            btnBlockUser.Click += BtnBlockUser_Click;

            btnSetPasswordRestrictions = new Button() { Text = "Set Password Restrictions", Top = 260, Left = 240 };
            btnSetPasswordRestrictions.Click += BtnSetPasswordRestrictions_Click;

            btnClose = new Button() { Text = "Close", Top = 300, Left = 240 };
            btnClose.Click += (s, e) => { Application.Exit(); };

            Controls.Add(lstUsers);
            Controls.Add(txtOldPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(btnChangePassword);
            Controls.Add(btnAddUser);
            Controls.Add(btnBlockUser);
            Controls.Add(btnSetPasswordRestrictions);
            Controls.Add(btnClose);
        }

        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
            {
                lstUsers.Items.Add(user.Name);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            if (!UserManager.ValidatePassword(newPassword, false))
            {
                MessageBox.Show("Password does not meet the requirements.");
                return;
            }

            if (UserManager.ChangePassword("ADMIN", oldPassword, newPassword))
            {
                MessageBox.Show("Password changed successfully.");
            }
            else
            {
                MessageBox.Show("Incorrect old password.");
            }
        }

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            string name = Microsoft.VisualBasic.Interaction.InputBox("Enter new user name:", "Add User", "");
            if (!string.IsNullOrEmpty(name) && UserManager.GetUser(name) == null)
            {
                UserManager.AddUser(name);
                LoadUsers();
            }
            else
            {
                MessageBox.Show("Invalid or duplicate user name.");
            }
        }

        private void BtnBlockUser_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string name = lstUsers.SelectedItem.ToString();
                UserManager.BlockUser(name, true);
                MessageBox.Show($"User {name} is blocked.");
            }
        }

        private void BtnSetPasswordRestrictions_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string name = lstUsers.SelectedItem.ToString();
                UserManager.SetPasswordRestrictions(name, true);
                MessageBox.Show($"Password restrictions are set for user {name}.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources here.
            }
            base.Dispose(disposing);
        }
    }
}