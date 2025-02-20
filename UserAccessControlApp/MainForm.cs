using System;
using System.Drawing;
using System.Windows.Forms;

namespace UserAccessControl
{
    public class MainForm : Form
    {
        private User currentUser;
        private TabControl tabControl;

        // Change Password tab controls
        private TextBox txtOldPassword, txtNewPassword, txtConfirmPassword;
        private Button btnChangePassword, btnLogoutCP;

        // Admin Actions tab controls
        private Panel adminPanel;
        private Label lblAdminAccess;
        private ListBox lstUsers;
        private Button btnAddUser, btnBlockUser, btnUnblockUser, btnLogoutAdmin;

        // Password Requirements controls within Admin Actions
        private GroupBox gbPasswordRequirements;
        private CheckBox chkEnableLength;
        private TextBox txtMinLength;
        private CheckBox chkRequireUppercase, chkRequireDigit, chkRequireSpecial;
        private Button btnUpdateRequirements;

        public MainForm(User user)
        {
            currentUser = user;
            Text = "Main Form - Welcome " + currentUser.Name;
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill };

            // Create two tabs
            TabPage tabChangePassword = new TabPage("Change Password");
            TabPage tabAdminActions = new TabPage("Admin Actions");

            InitializeChangePasswordTab(tabChangePassword);
            InitializeAdminActionsTab(tabAdminActions);

            tabControl.TabPages.Add(tabChangePassword);
            tabControl.TabPages.Add(tabAdminActions);

            // Disable Admin tab controls if user is not ADMIN.
            if (currentUser.Name != "ADMIN")
            {
                adminPanel.Enabled = false;
                lblAdminAccess.Text = "Access Denied. Admin actions are disabled for your account.";
                lblAdminAccess.ForeColor = Color.Red;
                lblAdminAccess.Font = new Font(lblAdminAccess.Font, FontStyle.Bold);
            }
            else
            {
                lblAdminAccess.Text = "Admin Actions:";
            }

            Controls.Add(tabControl);
        }

        private void InitializeChangePasswordTab(TabPage tabPage)
        {
            // Create a scrollable panel in case the window is small.
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Set up the TableLayoutPanel for inputs.
            TableLayoutPanel cpPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(20)
            };

            cpPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            cpPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

            // Set row styles for three input rows.
            cpPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            cpPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            cpPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));

            Label lblOld = new Label { Text = "Old Password:", Anchor = AnchorStyles.Right, AutoSize = true };
            Label lblNew = new Label { Text = "New Password:", Anchor = AnchorStyles.Right, AutoSize = true };
            Label lblConfirm = new Label { Text = "Confirm Password:", Anchor = AnchorStyles.Right, AutoSize = true };

            txtOldPassword = new TextBox { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtNewPassword = new TextBox { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };
            txtConfirmPassword = new TextBox { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };

            cpPanel.Controls.Add(lblOld, 0, 0);
            cpPanel.Controls.Add(txtOldPassword, 1, 0);
            cpPanel.Controls.Add(lblNew, 0, 1);
            cpPanel.Controls.Add(txtNewPassword, 1, 1);
            cpPanel.Controls.Add(lblConfirm, 0, 2);
            cpPanel.Controls.Add(txtConfirmPassword, 1, 2);

            // Create a FlowLayoutPanel for the two buttons and center them below the inputs.
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            btnChangePassword = new Button { Text = "Change Password", Width = 150 };
            btnChangePassword.Click += BtnChangePassword_Click;
            btnLogoutCP = new Button { Text = "Log Out", Width = 150 };
            btnLogoutCP.Click += BtnLogout_Click;

            buttonPanel.Controls.Add(btnChangePassword);
            buttonPanel.Controls.Add(btnLogoutCP);

            // Create a parent panel to center the button panel.
            Panel centerPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };
            centerPanel.Controls.Add(buttonPanel);
            // Center the buttonPanel within centerPanel.
            buttonPanel.Location = new Point((centerPanel.ClientSize.Width - buttonPanel.Width) / 2, 0);
            buttonPanel.Anchor = AnchorStyles.Top;

            // Enclose both input and button panels in a main vertical layout.
            FlowLayoutPanel mainLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false
            };
            mainLayout.Controls.Add(cpPanel);
            mainLayout.Controls.Add(centerPanel);

            scrollPanel.Controls.Add(mainLayout);
            tabPage.Controls.Add(scrollPanel);
        }

        private void InitializeAdminActionsTab(TabPage tabPage)
        {
            // Create a scrollable panel for admin actions.
            Panel scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            adminPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(20)
            };

            TableLayoutPanel adminLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 3,
                AutoSize = true
            };

            // Set equal width for three columns.
            for (int i = 0; i < 3; i++)
                adminLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            // Header label spanning all columns.
            lblAdminAccess = new Label { Text = "Admin Actions:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Arial", 12, FontStyle.Bold) };
            adminLayout.Controls.Add(lblAdminAccess, 0, 0);
            adminLayout.SetColumnSpan(lblAdminAccess, 3);

            // ListBox for users.
            lstUsers = new ListBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = 150 };
            lstUsers.SelectedIndexChanged += LstUsers_SelectedIndexChanged;
            adminLayout.Controls.Add(lstUsers, 0, 1);
            adminLayout.SetColumnSpan(lstUsers, 3);
            LoadUsers();

            // User management buttons.
            btnAddUser = new Button { Text = "Add User", Anchor = AnchorStyles.None, Width = 120 };
            btnAddUser.Click += BtnAddUser_Click;
            btnBlockUser = new Button { Text = "Block User", Anchor = AnchorStyles.None, Width = 120 };
            btnBlockUser.Click += BtnBlockUser_Click;
            btnUnblockUser = new Button { Text = "Unblock User", Anchor = AnchorStyles.None, Width = 120 };
            btnUnblockUser.Click += BtnUnblockUser_Click;
            adminLayout.Controls.Add(btnAddUser, 0, 2);
            adminLayout.Controls.Add(btnBlockUser, 1, 2);
            adminLayout.Controls.Add(btnUnblockUser, 2, 2);

            // GroupBox for password requirements.
            gbPasswordRequirements = new GroupBox { Text = "Password Requirements", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(10) };
            TableLayoutPanel reqLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                AutoSize = true
            };

            for (int i = 0; i < 5; i++)
                reqLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            // First row: Enable Length Restriction checkbox spanning all columns.
            chkEnableLength = new CheckBox { Text = "Enable Length Restriction", AutoSize = true, Anchor = AnchorStyles.Left };
            reqLayout.Controls.Add(chkEnableLength, 0, 0);
            reqLayout.SetColumnSpan(chkEnableLength, 5);

            // Second row: Label "Min Length:", TextBox, and three checkboxes.
            Label lblMin = new Label { Text = "Min Length:", AutoSize = true, Anchor = AnchorStyles.Right };
            txtMinLength = new TextBox { Width = 50, Anchor = AnchorStyles.Left };
            chkRequireUppercase = new CheckBox { Text = "Require Uppercase", AutoSize = true, Anchor = AnchorStyles.Left };
            chkRequireDigit = new CheckBox { Text = "Require Digit", AutoSize = true, Anchor = AnchorStyles.Left };
            chkRequireSpecial = new CheckBox { Text = "Require Special", AutoSize = true, Anchor = AnchorStyles.Left };

            reqLayout.Controls.Add(lblMin, 0, 1);
            reqLayout.Controls.Add(txtMinLength, 1, 1);
            reqLayout.Controls.Add(chkRequireUppercase, 2, 1);
            reqLayout.Controls.Add(chkRequireDigit, 3, 1);
            reqLayout.Controls.Add(chkRequireSpecial, 4, 1);

            // Third row: Update button spanning all columns.
            btnUpdateRequirements = new Button { Text = "Update Requirements", Anchor = AnchorStyles.None, Width = 150 };
            btnUpdateRequirements.Click += BtnUpdateRequirements_Click;
            reqLayout.Controls.Add(btnUpdateRequirements, 0, 2);
            reqLayout.SetColumnSpan(btnUpdateRequirements, 5);

            gbPasswordRequirements.Controls.Add(reqLayout);
            adminLayout.Controls.Add(gbPasswordRequirements, 0, 3);
            adminLayout.SetColumnSpan(gbPasswordRequirements, 3);

            // Logout button.
            btnLogoutAdmin = new Button { Text = "Log Out", Anchor = AnchorStyles.None, Width = 150 };
            btnLogoutAdmin.Click += BtnLogout_Click;
            adminLayout.Controls.Add(btnLogoutAdmin, 0, 4);
            adminLayout.SetColumnSpan(btnLogoutAdmin, 3);

            adminPanel.Controls.Add(adminLayout);
            scrollPanel.Controls.Add(adminPanel);
            tabPage.Controls.Add(scrollPanel);
        }

        private void LstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string userName = lstUsers.SelectedItem.ToString();
                var user = UserManager.GetUser(userName);
                if (user != null)
                {
                    chkEnableLength.Checked = user.PasswordRestrictions.EnableLengthRestriction;
                    txtMinLength.Text = user.PasswordRestrictions.MinLength.ToString();
                    chkRequireUppercase.Checked = user.PasswordRestrictions.RequireUppercase;
                    chkRequireDigit.Checked = user.PasswordRestrictions.RequireDigit;
                    chkRequireSpecial.Checked = user.PasswordRestrictions.RequireSpecialChar;
                }
            }
        }

        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
                lstUsers.Items.Add(user.Name);
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            string oldPass = txtOldPassword.Text;
            string newPass = txtNewPassword.Text;
            string confirmPass = txtConfirmPassword.Text;

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

        private void BtnAddUser_Click(object sender, EventArgs e)
        {
            string userName = Microsoft.VisualBasic.Interaction.InputBox("Enter new user name:", "Add User", "");
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

        private void BtnBlockUser_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string userName = lstUsers.SelectedItem.ToString();
                if (userName == "ADMIN")
                {
                    MessageBox.Show("Cannot block ADMIN.");
                    return;
                }
                UserManager.BlockUser(userName, true);
                MessageBox.Show($"User {userName} is blocked.");
                LoadUsers();
            }
        }

        private void BtnUnblockUser_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                string userName = lstUsers.SelectedItem.ToString();
                UserManager.BlockUser(userName, false);
                MessageBox.Show($"User {userName} is unblocked.");
                LoadUsers();
            }
        }

        private void BtnUpdateRequirements_Click(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to update requirements.");
                return;
            }
            string userName = lstUsers.SelectedItem.ToString();
            var user = UserManager.GetUser(userName);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }
            var restrictions = new PasswordRestrictions
            {
                EnableLengthRestriction = chkEnableLength.Checked,
                MinLength = int.TryParse(txtMinLength.Text, out int min) ? min : 0,
                RequireUppercase = chkRequireUppercase.Checked,
                RequireDigit = chkRequireDigit.Checked,
                RequireSpecialChar = chkRequireSpecial.Checked
            };
            user.PasswordRestrictions = restrictions;
            UserManager.SaveUsers();
            MessageBox.Show("Password requirements updated successfully.");
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
