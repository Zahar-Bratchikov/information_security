using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CrackPassword
{
    public static class UserManager
    {
        // List of all users
        public static List<User> Users { get; private set; } = new List<User>();

        private static string filePath = "users.json";

        static UserManager()
        {
            LoadUsers();
            // If no users exist, add a default admin user.
            if (Users == null || Users.Count == 0)
            {
                // Default admin user with password "admin123"
                AddUser("ADMIN", "");
            }
        }

        // Returns the user object with the specified username
        public static User GetUser(string username)
        {
            return Users.FirstOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // Adds a new user with the given username and (plain-text) password.
        // The password will be saved as a SHA256 hash.
        // If no password is provided, an empty password is hashed.
        public static void AddUser(string username, string password = null)
        {
            string passHash = password != null ? ComputeHash(password) : ComputeHash("");
            Users.Add(new User { Name = username, PasswordHash = passHash, IsBlocked = false });
            SaveUsers();
        }

        // Changes the password for the specified user after verifying that the old password is correct.
        public static bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = GetUser(username);
            if (user == null)
                return false;
            string oldHash = ComputeHash(oldPassword);
            if (user.PasswordHash != oldHash)
                return false;
            user.PasswordHash = ComputeHash(newPassword);
            SaveUsers();
            return true;
        }

        // Validates a password against the given set of restrictions.
        public static bool ValidatePassword(string password, PasswordRestrictions restrictions)
        {
            if (restrictions.EnableLengthRestriction && password.Length < restrictions.MinLength)
                return false;
            if (restrictions.RequireUppercase && !password.Any(char.IsUpper))
                return false;
            if (restrictions.RequireDigit && !password.Any(char.IsDigit))
                return false;
            if (restrictions.RequireSpecialChar && password.All(char.IsLetterOrDigit))
                return false;
            return true;
        }

        // Blocks or unblocks the specified user.
        public static void BlockUser(string username, bool block)
        {
            var user = GetUser(username);
            if (user != null)
            {
                user.IsBlocked = block;
                SaveUsers();
            }
        }

        // Saves all user data to a JSON file.
        public static void SaveUsers()
        {
            var json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Loads user data from the JSON file.
        public static void LoadUsers()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                    Users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                else
                    Users = new List<User>();
            }
            else
            {
                Users = new List<User>();
                SaveUsers();
            }
        }

        // Computes the SHA256 hash of the input string.
        public static string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}