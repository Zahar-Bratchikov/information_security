using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CrackPassword
{
    /// <summary>
    /// Менеджер пользователей. Данные сохраняются в JSON‑файл.
    /// Пароли хранятся как SHA256-хэш.
    /// </summary>
    public static class UserManager
    {
        public static List<User> Users { get; set; }
        // Имя файла для хранения пользователей
        private static readonly string dataFile = "users.json";

        static UserManager()
        {
            LoadUsers();
            // Если список пустой, создаём ADMIN с пустым паролем
            if (Users == null || Users.Count == 0)
            {
                Users = new List<User>
                {
                    new User
                    {
                        Name = "ADMIN",
                        Password = ComputeHash(""),
                        PasswordRestrictions = new PasswordRestrictions()
                    }
                };
                SaveUsers();
            }
        }

        // Загрузка данных из JSON-файла
        public static void LoadUsers()
        {
            if (File.Exists(dataFile))
            {
                try
                {
                    string json = File.ReadAllText(dataFile);
                    Users = JsonSerializer.Deserialize<List<User>>(json);
                }
                catch (Exception ex)
                {
                    // Если произошла ошибка при чтении файла, создаём новый список пользователей
                    Console.WriteLine("Ошибка загрузки пользователей: " + ex.Message);
                    Users = new List<User>();
                }
            }
            else
            {
                Users = new List<User>();
            }
        }

        // Сохранение данных в JSON-файл
        public static void SaveUsers()
        {
            try
            {
                string json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сохранения пользователей: " + ex.Message);
            }
        }

        // Получение пользователя по имени
        public static User GetUser(string name)
        {
            return Users.FirstOrDefault(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        // Проверка, соответствует ли введенный пароль сохранённому хэшу
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            return ComputeHash(inputPassword) == storedHash;
        }

        // Метод для хэширования пароля с использованием SHA256
        private static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        // Метод для проверки соответствия нового пароля требованиям
        public static bool ValidatePassword(string password, PasswordRestrictions restrictions)
        {
            if (restrictions == null)
                return true;
            if (restrictions.EnableLengthRestriction && password.Length < restrictions.MinLength)
                return false;
            if (restrictions.RequireUppercase && !password.Any(char.IsUpper))
                return false;
            if (restrictions.RequireDigit && !password.Any(char.IsDigit))
                return false;
            if (restrictions.RequireSpecialChar && !password.Any(c => !char.IsLetterOrDigit(c)))
                return false;
            return true;
        }

        // Смена пароля. Сравнивается введённый старый пароль (с хэшированием) с сохранённым.
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            var user = GetUser(userName);
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"Проверка старого пароля: введено '{oldPassword}', ожидается хэш '{user.Password}'");
                if (VerifyPassword(oldPassword, user.Password))
                {
                    user.Password = ComputeHash(newPassword);
                    SaveUsers();
                    return true;
                }
            }
            return false;
        }

        // Блокировка пользователя
        public static void BlockUser(string userName, bool block)
        {
            var user = GetUser(userName);
            if (user != null)
            {
                user.IsBlocked = block;
                SaveUsers();
            }
        }

        // Добавление нового пользователя. Пароль по умолчанию хэшируется.
        public static void AddUser(string userName, string defaultPassword)
        {
            Users.Add(new User
            {
                Name = userName,
                Password = ComputeHash(defaultPassword),
                PasswordRestrictions = new PasswordRestrictions()
            });
            SaveUsers();
        }
    }
}