using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics; // Ensure you add a reference to System.Numerics
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrackPassword
{
    public partial class MainWindow : Window
    {
        private User currentUser;
        // Flag indicating that the cracking process is running
        private volatile bool isCracking = false;
        // Stopwatch for measuring cracking time
        private Stopwatch crackingStopwatch;
        // Attempt counter
        private long attempts = 0;
        // Character set for brute force
        private readonly string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        // Constants for password cracking estimation
        private readonly BigInteger speed = 100000000;            // Attempts per second
        private readonly BigInteger attemptsBeforePause = 100000; // Number of attempts before a pause
        private readonly BigInteger pauseTime = 0;                // Pause time in seconds

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            Title = $"Main Window - Welcome, {currentUser.Name}";

            if (currentUser.Name != "ADMIN")
            {
                tabControl.Items.Remove(adminTab);
            }
            else
            {
                lblAdminAccess.Text = "Admin Actions:";
            }
            LoadUsers();
        }

        // Loads the user list
        private void LoadUsers()
        {
            lstUsers.Items.Clear();
            foreach (var user in UserManager.Users)
                lstUsers.Items.Add(user);
        }

        // Handler for password change
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
                MessageBox.Show("New password does not meet requirements.");
                return;
            }
            if (UserManager.ChangePassword(currentUser.Name, oldPass, newPass))
                MessageBox.Show("Password changed successfully.");
            else
                MessageBox.Show("Incorrect old password.");
        }

        // Handler for logout
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        // Handler for adding a user
        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            InputDialog dlg = new InputDialog("Enter user name:", "Add User");
            dlg.Owner = this;
            if (dlg.ShowDialog() == true)
            {
                string userName = dlg.ResponseText;
                if (!string.IsNullOrWhiteSpace(userName) && UserManager.GetUser(userName) == null)
                {
                    // For demonstration, you may consider prompting for an initial password.
                    UserManager.AddUser(userName, "");
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show("Invalid or duplicate user name.");
                }
            }
        }

        // Handler for blocking a user
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
                MessageBox.Show($"User {selectedUser.Name} has been blocked.");
                LoadUsers();
            }
        }

        // Handler for unblocking a user
        private void BtnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                User selectedUser = lstUsers.SelectedItem as User;
                UserManager.BlockUser(selectedUser.Name, false);
                MessageBox.Show($"User {selectedUser.Name} has been unblocked.");
                LoadUsers();
            }
        }

        // Handler for updating password requirements
        private void BtnUpdateRequirements_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Select a user to update requirements.");
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

        // Handler for selection change in the user list
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

        // Handler for starting a dictionary attack
        private void BtnStartDictionaryAttack_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking)
            {
                MessageBox.Show("Password cracking is already in progress.");
                return;
            }
            Task.Run(() =>
            {
                string[] dictionary = File.Exists("dictionary.txt") ? File.ReadAllLines("dictionary.txt") : new string[0];
                string adminPassword = UserManager.GetUser("ADMIN").PasswordHash;
                foreach (string word in dictionary)
                {
                    string transliteratedWord = Translit(word.Trim().ToLower());
                    // Compare with hash of the dictionary word
                    if (UserManager.ComputeHash(transliteratedWord) == adminPassword)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            txtCrackingStatus.Text = $"Dictionary Attack: Password found: {transliteratedWord}";
                        });
                        return;
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    txtCrackingStatus.Text = "Dictionary Attack: Password not found in dictionary.";
                });
            });
        }

        // Simple transliteration function (example implementation)
        private string Translit(string input)
        {
            var translitMap = new Dictionary<char, char>
            {
                {'а', 'f'}, {'б', ','}, {'в', 'd'}, {'г', 'u'}, {'д', 'l'}, {'е', 't'}, {'ё', '`'}, {'ж', ';'},
                {'з', 'p'}, {'и', 'b'}, {'й', 'q'}, {'к', 'r'}, {'л', 'k'}, {'м', 'v'}, {'н', 'y'}, {'о', 'j'},
                {'п', 'g'}, {'р', 'h'}, {'с', 'c'}, {'т', 'n'}, {'у', 'e'}, {'ф', 'a'}, {'х', '['}, {'ц', 'w'},
                {'ч', 'x'}, {'ш', 'i'}, {'щ', 'o'}, {'ь', 'm'}, {'ы', 's'}, {'ъ', ']'}, {'э', '\''}, {'ю', '.'},
                {'я', 'z'}
            };

            var sb = new StringBuilder(input.Length);
            foreach (char c in input)
                sb.Append(translitMap.ContainsKey(c) ? translitMap[c] : c);
            return sb.ToString();
        }

        // Sequential brute force attack with live updating of current candidate
        private void BtnStartBruteForceAttack_Click(object sender, RoutedEventArgs e)
        {
            if (isCracking)
            {
                MessageBox.Show("Password cracking is already in progress.");
                return;
            }
            if (!int.TryParse(txtMaxLength.Text, out int maxLength) || maxLength <= 0)
            {
                MessageBox.Show("Invalid maximum length value.");
                return;
            }
            // Note: When comparing dictionary values, we compare the computed hash with what is stored.
            string adminPasswordHash = UserManager.GetUser("ADMIN").PasswordHash;
            isCracking = true;
            attempts = 0;
            crackingStopwatch = Stopwatch.StartNew();

            Task.Run(() =>
            {
                for (int length = 1; length <= maxLength && isCracking; length++)
                {
                    foreach (string combination in GetCombinations(charset, length))
                    {
                        if (!isCracking)
                            break;
                        attempts++;

                        // Update the current candidate every 1000 attempts
                        if (attempts % 10000 == 0)
                        {
                            string currentCandidate = combination;
                            Dispatcher.Invoke(() =>
                            {
                                txtCurrentPassword.Text = currentCandidate;
                            });
                        }

                        // Compare hash of current combination with admin password hash
                        if (UserManager.ComputeHash(combination) == adminPasswordHash)
                        {
                            isCracking = false;
                            crackingStopwatch.Stop();
                            double elapsedSeconds = crackingStopwatch.Elapsed.TotalSeconds;
                            double speedAchieved = attempts / elapsedSeconds;
                            Dispatcher.Invoke(() =>
                            {
                                txtCrackingStatus.Text = $"Brute Force: Password found: {combination} in {attempts} attempts, average speed: {speedAchieved:F2} attempts/sec";
                                txtCurrentPassword.Text = combination;
                            });
                            return;
                        }
                    }
                }
                isCracking = false;
                crackingStopwatch.Stop();
                double totalElapsed = crackingStopwatch.Elapsed.TotalSeconds;
                double avgSpeed = attempts / totalElapsed;
                Dispatcher.Invoke(() =>
                {
                    txtCrackingStatus.Text = $"Brute Force: Password not found. Total attempts: {attempts}, average speed: {avgSpeed:F2} attempts/sec";
                    txtCurrentPassword.Text = "";
                });
            });
        }

        // Handler to stop the brute force attack
        private void BtnStopBruteForce_Click(object sender, RoutedEventArgs e)
        {
            isCracking = false;
            if (crackingStopwatch != null)
                crackingStopwatch.Stop();
            Dispatcher.Invoke(() =>
            {
                txtCrackingStatus.Text = "Brute Force: Stopped by user.";
                txtCurrentPassword.Text = "";
            });
        }

        // Recursive generation of combinations for the brute force attack
        private IEnumerable<string> GetCombinations(string charset, int length)
        {
            if (length == 1)
            {
                foreach (char c in charset)
                    yield return c.ToString();
            }
            else
            {
                foreach (string prefix in GetCombinations(charset, length - 1))
                {
                    foreach (char c in charset)
                        yield return prefix + c;
                }
            }
        }

        // Event handler for password changed event of the new password box
        private void TxtNewPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength();
        }

        // Updates the password strength estimation based on cracking time
        private void UpdatePasswordStrength()
        {
            string password = txtNewPassword.Password;
            if (string.IsNullOrEmpty(password))
            {
                txtPasswordEstimate.Text = "";
                return;
            }

            // Determine alphabet size based on characters in password
            int alphabetSize = GetAlphabetSize(password);
            // Calculate total possible combinations
            BigInteger totalCombinations = BigInteger.Pow(alphabetSize, password.Length);
            // Base cracking time without pause
            BigInteger baseTimeSeconds = totalCombinations / speed;
            // Calculate number of pauses required
            BigInteger pauseCount = (totalCombinations - 1) / attemptsBeforePause;
            // Total cracking time considering pauses
            BigInteger totalTimeSeconds = baseTimeSeconds + pauseCount * pauseTime;

            string formattedTime = FormatTime(totalTimeSeconds);

            // Determine password strength based on cracking time estimation
            string strength;
            double totalSecondsDouble = (double)totalTimeSeconds;
            if (totalSecondsDouble < 3600) // less than 1 hour
                strength = "Weak";
            else if (totalSecondsDouble < 31536000) // less than 1 year
                strength = "Moderate";
            else
                strength = "Strong";

            txtPasswordEstimate.Text = $"Estimated cracking time: {formattedTime}\nPassword Strength: {strength}";
        }

        // Determines the alphabet size based on the characters present in the password
        private int GetAlphabetSize(string password)
        {
            bool hasLower = false, hasUpper = false, hasDigits = false, hasSpecial = false;
            int size = 0;
            foreach (char c in password)
            {
                if (char.IsLower(c)) hasLower = true;
                else if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsDigit(c)) hasDigits = true;
                else hasSpecial = true;
            }
            if (hasLower) size += 26;
            if (hasUpper) size += 26;
            if (hasDigits) size += 10;
            if (hasSpecial) size += 33;
            return size;
        }

        // Formats the given total seconds into a human-readable format
        private string FormatTime(BigInteger totalSeconds)
        {
            BigInteger years = totalSeconds / (365 * 24 * 3600);
            totalSeconds %= 365 * 24 * 3600;
            int months = (int)(totalSeconds / (30 * 24 * 3600));
            totalSeconds %= 30 * 24 * 3600;
            int days = (int)(totalSeconds / (24 * 3600));
            totalSeconds %= 24 * 3600;
            int hours = (int)(totalSeconds / 3600);
            totalSeconds %= 3600;
            int minutes = (int)(totalSeconds / 60);
            int seconds = (int)(totalSeconds % 60);
            return $"{years} years, {months} months, {days} days, {hours} hours, {minutes} minutes, {seconds} seconds";
        }
    }
}