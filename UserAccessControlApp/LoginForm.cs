using System;
using System.Windows.Forms;

namespace UserAccessControl
{
    public class LoginForm : Form
    {
        private TextBox txtName;
        private TextBox txtPassword;
        private Button btnLogin;
        private int loginAttempts = 0;

        public LoginForm()
        {
            Text = "Login";
            Width = 400;
            Height = 250;
            StartPosition = FormStartPosition.CenterScreen;

            TableLayoutPanel panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10),
                AutoSize = true
            };

            Label lblName = new Label() { Text = "Name", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblPassword = new Label() { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Right };

            txtName = new TextBox() { Anchor = AnchorStyles.Left, Width = 200 };
            txtPassword = new TextBox() { Anchor = AnchorStyles.Left, Width = 200, PasswordChar = '*' };

            btnLogin = new Button() { Text = "Login", Anchor = AnchorStyles.None, Width = 100 };
            btnLogin.Click += BtnLogin_Click;

            panel.Controls.Add(lblName, 0, 0);
            panel.Controls.Add(txtName, 1, 0);
            panel.Controls.Add(lblPassword, 0, 1);
            panel.Controls.Add(txtPassword, 1, 1);
            panel.Controls.Add(btnLogin, 1, 2);

            Controls.Add(panel);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string name = txtName.Text;
            string password = txtPassword.Text;

            var user = UserManager.GetUser(name);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("User is blocked.");
                return;
            }

            if (user.Password != password)
            {
                loginAttempts++;
                if (loginAttempts >= 3)
                {
                    MessageBox.Show("Too many incorrect attempts. Exiting.");
                    Application.Exit();
                }
                else
                {
                    MessageBox.Show("Incorrect password.");
                }
                return;
            }

            if (name == "ADMIN")
            {
                new AdminForm().Show();
            }
            else
            {
                new UserForm(user).Show();
            }
            Hide();
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