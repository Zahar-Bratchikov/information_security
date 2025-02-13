using System;
using System.Windows.Forms;

namespace UserAccessControl
{
    public class UserForm : Form
    {
        private TextBox txtOldPassword;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private Button btnChangePassword;
        private Button btnClose;
        private User currentUser;

        public UserForm(User user)
        {
            currentUser = user;
            Text = "User Panel";
            Width = 300;
            Height = 200;

            Label lblOldPassword = new Label() { Text = "Old Password", Top = 20, Left = 20 };
            Label lblNewPassword = new Label() { Text = "New Password", Top = 60, Left = 20 };
            Label lblConfirmPassword = new Label() { Text = "Confirm Password", Top = 100, Left = 20 };

            txtOldPassword = new TextBox() { Top = 20, Left = 120, Width = 150, PasswordChar = '*' };
            txtNewPassword = new TextBox() { Top = 60, Left = 120, Width = 150, PasswordChar = '*' };
            txtConfirmPassword = new TextBox() { Top = 100, Left = 120, Width = 150, PasswordChar = '*' };

            btnChangePassword = new Button() { Text = "Change Password", Top = 140, Left = 50 };
            btnChangePassword.Click += BtnChangePassword_Click;

            btnClose = new Button() { Text = "Close", Top = 140, Left = 170 };
            btnClose.Click += (s, e) => { Application.Exit(); };

            Controls.Add(lblOldPassword);
            Controls.Add(lblNewPassword);
            Controls.Add(lblConfirmPassword);
            Controls.Add(txtOldPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(txtConfirmPassword);
            Controls.Add(btnChangePassword);
            Controls.Add(btnClose);
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

            if (!UserManager.ValidatePassword(newPassword, currentUser.PasswordRestrictions))
            {
                MessageBox.Show("Password does not meet the requirements.");
                return;
            }

            if (UserManager.ChangePassword(currentUser.Name, oldPassword, newPassword))
            {
                MessageBox.Show("Password changed successfully.");
            }
            else
            {
                MessageBox.Show("Incorrect old password.");
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