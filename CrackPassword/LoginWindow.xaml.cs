using System.Windows;

namespace CrackPassword
{
    public partial class LoginWindow : Window
    {
        private int loginAttempts = 0;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string name = txtName.Text;
            string password = txtPassword.Password;

            var user = UserManager.GetUser(name);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }
            if (user.IsBlocked)
            {
                MessageBox.Show("This user is blocked.");
                return;
            }
            // Compare the stored password hash with the computed hash of the input password.
            string hashedPassword = UserManager.ComputeHash(password);
            if (user.PasswordHash != hashedPassword)
            {
                loginAttempts++;
                if (loginAttempts >= 3)
                {
                    MessageBox.Show("Too many incorrect attempts. Exiting application.");
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("Incorrect password.");
                }
                return;
            }

            MainWindow mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }
    }
}