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
            Width = 300;
            Height = 200;

            Label lblName = new Label() { Text = "Name", Top = 20, Left = 20 };
            Label lblPassword = new Label() { Text = "Password", Top = 60, Left = 20 };

            txtName = new TextBox() { Top = 20, Left = 100, Width = 150 };
            txtPassword = new TextBox() { Top = 60, Left = 100, Width = 150, PasswordChar = '*' };

            btnLogin = new Button() { Text = "Login", Top = 100, Left = 100 };
            btnLogin.Click += BtnLogin_Click;

            Controls.Add(lblName);
            Controls.Add(lblPassword);
            Controls.Add(txtName);
            Controls.Add(txtPassword);
            Controls.Add(btnLogin);
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