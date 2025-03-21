using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CrackPassword
{
    // Статический класс для управления пользователями
    public static class UserManager
    {
        // Список всех пользователей
        public static List<User> Users { get; private set; } = new List<User>();

        // Путь к файлу с данными пользователей
        private static string filePath = "users.json";

        // Статический конструктор, загружающий пользователей при первом использовании класса
        static UserManager()
        {
            LoadUsers();
            // Если пользователей нет, создаём ADMIN'а"
            if (Users == null || Users.Count == 0)
            {
                AddUser("ADMIN", "");
            }
        }

        // Получение пользователя по имени (без учета регистра)
        public static User GetUser(string username)
        {
            return Users.FirstOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        // Добавление нового пользователя
        public static void AddUser(string username, string password = null)
        {
            string passHash = password != null ? ComputeHash(password) : ComputeHash("");
            Users.Add(new User { Name = username, PasswordHash = passHash, IsBlocked = false });
            SaveUsers();
        }

        // Изменение пароля пользователя (после проверки старого пароля)
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

        // Проверка соответствия пароля заданным ограничениям
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

        // Блокировка или разблокировка пользователя
        public static void BlockUser(string username, bool block)
        {
            var user = GetUser(username);
            if (user != null)
            {
                user.IsBlocked = block;
                SaveUsers();
            }
        }

        // Сохранение пользователей в JSON-файл
        public static void SaveUsers()
        {
            var json = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        // Загрузка пользователей из JSON-файла
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

        // Вычисление SHA256-хэша для заданной строки
        public static string ComputeHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}