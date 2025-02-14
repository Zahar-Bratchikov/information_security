using System;
using System.Drawing;
using System.Windows.Forms;

namespace UserAccessControl
{
    public class AdminForm : Form
    {
        private ListBox lstUsers;
        private TextBox txtNewPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtOldPassword;
        private TextBox txtMinLength;
        private Button btnChangePassword;
        private Button btnAddUser;
        private Button btnBlockUser;
        private Button btnUnblockUser;
        private Button btnClose;
        private Button btnLogout;
        private CheckBox chkLengthRestriction;
        private CheckBox chkRequireUppercase;
        private CheckBox chkRequireDigit;
        private CheckBox chkRequireSpecialChar;

        public AdminForm()
        {
            Text = "Admin Panel";
            Width = 600;
            Height = 500;
            StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10),
                AutoSize = true
            };

            lstUsers = new ListBox() { Dock = DockStyle.Fill };
            lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;
            LoadUsers();

            GroupBox grpActions = new GroupBox() { Text = "Actions", Dock = DockStyle.Fill, Padding = new Padding(10) };
            TableLayoutPanel actionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 12,
                AutoSize = true
            };

            Label lblOldPassword = new Label() { Text = "Old Password", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblNewPassword = new Label() { Text = "New Password", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblConfirmPassword = new Label() { Text = "Confirm Password", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblMinLength = new Label() { Text = "Min Length", AutoSize = true, Anchor = AnchorStyles.Right };

            txtOldPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtNewPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtConfirmPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtMinLength = new TextBox() { Anchor = AnchorStyles.Left, Width = 50 };

            chkLengthRestriction = new CheckBox() { Text = "Enable Length Restriction", Anchor = AnchorStyles.Left, AutoSize = true };
            chkRequireUppercase = new CheckBox() { Text = "Require Uppercase", Anchor = AnchorStyles.Left, AutoSize = true };
            chkRequireDigit = new CheckBox() { Text = "Require Digit", Anchor = AnchorStyles.Left, AutoSize = true };
            chkRequireSpecialChar = new CheckBox() { Text = "Require Special Character", Anchor = AnchorStyles.Left, AutoSize = true };

            btnChangePassword = new Button() { Text = "Change Password", Anchor = AnchorStyles.None, Width = 150 };
            btnChangePassword.Click += BtnChangePassword_Click;

            btnAddUser = new Button() { Text = "Add User", Anchor = AnchorStyles.None, Width = 150 };
            btnAddUser.Click += BtnAddUser_Click;

            btnBlockUser = new Button() { Text = "Block User", Anchor = AnchorStyles.None, Width = 150 };
            btnBlockUser.Click += BtnBlockUser_Click;

            btnUnblockUser = new Button() { Text = "Unblock User", Anchor = AnchorStyles.None, Width = 150 };
            btnUnblockUser.Click += BtnUnblockUser_Click;

            btnLogout = new Button() { Text = "Log Out", Anchor = AnchorStyles.None, Width = 150 };
            btnLogout.Click += (s, e) => { Close(); };

            btnClose = new Button() { Text = "Close", Anchor = AnchorStyles.None, Width = 150 };
            btnClose.Click += (s, e) => { Application.Exit(); };

            actionPanel.Controls.Add(lblOldPassword, 0, 0);
            actionPanel.Controls.Add(txtOldPassword, 1, 0);
            actionPanel.Controls.Add(lblNewPassword, 0, 1);
            actionPanel.Controls.Add(txtNewPassword, 1, 1);
            actionPanel.Controls.Add(lblConfirmPassword, 0, 2);
            actionPanel.Controls.Add(txtConfirmPassword, 1, 2);
            actionPanel.Controls.Add(lblMinLength, 0, 3);
            actionPanel.Controls.Add(txtMinLength, 1, 3);
            actionPanel.Controls.Add(chkLengthRestriction, 1, 4);
            actionPanel.Controls.Add(chkRequireUppercase, 1, 5);
            actionPanel.Controls.Add(chkRequireDigit, 1, 6);
            actionPanel.Controls.Add(chkRequireSpecialChar, 1, 7);
            actionPanel.Controls.Add(btnChangePassword, 1, 8);
            actionPanel.Controls.Add(btnAddUser, 1, 9);
            actionPanel.Controls.Add(btnBlockUser, 1, 10);
            actionPanel.Controls.Add(btnUnblockUser, 1, 11);
            actionPanel.Controls.Add(btnLogout, 1, 12);
            actionPanel.Controls.Add(btnClose, 1, 13);

            grpActions.Controls.Add(actionPanel);

            mainPanel.Controls.Add(lstUsers, 0, 0);
            mainPanel.Controls.Add(grpActions, 1, 0);

            Controls.Add(mainPanel);
        }

        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
            {
                lstUsers.Items.Add(user.Name);
            }
        }

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string name = lstUsers.SelectedItem.ToString();
                var user = UserManager.GetUser(name);
                if (user != null)
                {
                    chkLengthRestriction.CheckedChanged -= ChkPasswordRestrictions_CheckedChanged;
                    chkRequireUppercase.CheckedChanged -= ChkPasswordRestrictions_CheckedChanged;
                    chkRequireDigit.CheckedChanged -= ChkPasswordRestrictions_CheckedChanged;
                    chkRequireSpecialChar.CheckedChanged -= ChkPasswordRestrictions_CheckedChanged;

                    chkLengthRestriction.Checked = user.PasswordRestrictions.EnableLengthRestriction;
                    txtMinLength.Text = user.PasswordRestrictions.MinLength.ToString();
                    chkRequireUppercase.Checked = user.PasswordRestrictions.RequireUppercase;
                    chkRequireDigit.Checked = user.PasswordRestrictions.RequireDigit;
                    chkRequireSpecialChar.Checked = user.PasswordRestrictions.RequireSpecialChar;

                    chkLengthRestriction.CheckedChanged += ChkPasswordRestrictions_CheckedChanged;
                    chkRequireUppercase.CheckedChanged += ChkPasswordRestrictions_CheckedChanged;
                    chkRequireDigit.CheckedChanged += ChkPasswordRestrictions_CheckedChanged;
                    chkRequireSpecialChar.CheckedChanged += ChkPasswordRestrictions_CheckedChanged;
                }
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

            if (!UserManager.ValidatePassword(newPassword, new PasswordRestrictions()))
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

        private void BtnUnblockUser_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string name = lstUsers.SelectedItem.ToString();
                UserManager.BlockUser(name, false);
                MessageBox.Show($"User {name} is unblocked.");
            }
        }

        private void ChkPasswordRestrictions_CheckedChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string name = lstUsers.SelectedItem.ToString();
                var user = UserManager.GetUser(name);
                if (user != null)
                {
                    user.PasswordRestrictions.EnableLengthRestriction = chkLengthRestriction.Checked;
                    user.PasswordRestrictions.MinLength = int.Parse(txtMinLength.Text);
                    user.PasswordRestrictions.RequireUppercase = chkRequireUppercase.Checked;
                    user.PasswordRestrictions.RequireDigit = chkRequireDigit.Checked;
                    user.PasswordRestrictions.RequireSpecialChar = chkRequireSpecialChar.Checked;
                    UserManager.SaveUsers();
                }
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