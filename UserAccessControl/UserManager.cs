using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UserAccessControl
{
    public static class UserManager
    {
        private static readonly string userFile = "users.txt";
        public static List<User> Users = new List<User>();

        static UserManager()
        {
            if (!File.Exists(userFile))
            {
                // Create default ADMIN user with no restrictions.
                Users.Add(new User("ADMIN", "", false, new PasswordRestrictions()));
                SaveUsers();
            }
            else
            {
                LoadUsers();
            }
        }

        public static void SaveUsers()
        {
            using (StreamWriter writer = new StreamWriter(userFile))
            {
                foreach (var user in Users)
                {
                    writer.WriteLine($"{user.Name},{user.Password},{user.IsBlocked}," +
                                     $"{user.PasswordRestrictions.EnableLengthRestriction}," +
                                     $"{user.PasswordRestrictions.MinLength}," +
                                     $"{user.PasswordRestrictions.RequireUppercase}," +
                                     $"{user.PasswordRestrictions.RequireDigit}," +
                                     $"{user.PasswordRestrictions.RequireSpecialChar}");
                }
            }
        }

        public static void LoadUsers()
        {
            using (StreamReader reader = new StreamReader(userFile))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(',');
                    Users.Add(new User(
                        parts[0],
                        parts[1],
                        bool.Parse(parts[2]),
                        new PasswordRestrictions
                        {
                            EnableLengthRestriction = bool.Parse(parts[3]),
                            MinLength = int.Parse(parts[4]),
                            RequireUppercase = bool.Parse(parts[5]),
                            RequireDigit = bool.Parse(parts[6]),
                            RequireSpecialChar = bool.Parse(parts[7])
                        }
                    ));
                }
            }
        }

        public static User GetUser(string name)
        {
            return Users.FirstOrDefault(u => u.Name == name);
        }

        public static void AddUser(string name)
        {
            Users.Add(new User(name, "", false, new PasswordRestrictions()));
            SaveUsers();
        }

        public static void BlockUser(string name, bool block)
        {
            var user = GetUser(name);
            if (user != null)
            {
                user.IsBlocked = block;
                SaveUsers();
            }
        }

        public static bool ValidatePassword(string password, PasswordRestrictions restrictions)
        {
            if (restrictions.EnableLengthRestriction && password.Length < restrictions.MinLength)
                return false;

            if (restrictions.RequireUppercase && !password.Any(char.IsUpper))
                return false;

            if (restrictions.RequireDigit && !password.Any(char.IsDigit))
                return false;

            if (restrictions.RequireSpecialChar && !password.Any(ch => !char.IsLetterOrDigit(ch)))
                return false;

            return true;
        }

        public static bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            var user = GetUser(name);
            if (user != null && user.Password == oldPassword)
            {
                user.Password = newPassword;
                SaveUsers();
                return true;
            }
            return false;
        }
    }
}