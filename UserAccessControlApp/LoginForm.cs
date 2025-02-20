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
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;
            AutoScaleMode = AutoScaleMode.Dpi;

            // Use a TableLayoutPanel for a clean layout.
            TableLayoutPanel panel = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(20),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Label lblName = new Label() { Text = "Name:", AutoSize = true, Anchor = AnchorStyles.Right };
            Label lblPassword = new Label() { Text = "Password:", AutoSize = true, Anchor = AnchorStyles.Right };

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

            MainForm mainForm = new MainForm(user);
            mainForm.FormClosed += (s, args) => this.Show();
            mainForm.Show();
            Hide();
        }
    }
}