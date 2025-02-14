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
        private Button btnLogout;
        private User currentUser;

        public UserForm(User user)
        {
            currentUser = user;
            Text = "User Panel";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10),
                AutoSize = true
            };

            Label lblOldPassword = new Label() { Text = "Old Password", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblNewPassword = new Label() { Text = "New Password", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblConfirmPassword = new Label() { Text = "Confirm Password", AutoSize = true, Anchor = AnchorStyles.Right };

            txtOldPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtNewPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtConfirmPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };

            btnChangePassword = new Button() { Text = "Change Password", Anchor = AnchorStyles.None, Width = 150 };
            btnChangePassword.Click += BtnChangePassword_Click;

            btnLogout = new Button() { Text = "Log Out", Anchor = AnchorStyles.None, Width = 150 };
            btnLogout.Click += (s, e) => { Close(); };

            btnClose = new Button() { Text = "Close", Anchor = AnchorStyles.None, Width = 150 };
            btnClose.Click += (s, e) => { Application.Exit(); };

            panel.Controls.Add(lblOldPassword, 0, 0);
            panel.Controls.Add(txtOldPassword, 1, 0);
            panel.Controls.Add(lblNewPassword, 0, 1);
            panel.Controls.Add(txtNewPassword, 1, 1);
            panel.Controls.Add(lblConfirmPassword, 0, 2);
            panel.Controls.Add(txtConfirmPassword, 1, 2);
            panel.Controls.Add(btnChangePassword, 1, 3);
            panel.Controls.Add(btnLogout, 1, 4);
            panel.Controls.Add(btnClose, 1, 5);

            Controls.Add(panel);
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