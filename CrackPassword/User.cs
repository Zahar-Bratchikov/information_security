using System.Text.Json.Serialization;

namespace CrackPassword
{
    // Класс, представляющий пользователя
    public class User
    {
        // Имя пользователя
        public string Name { get; set; }
        // SHA256-хэш пароля пользователя
        public string PasswordHash { get; set; }
        // Флаг, указывающий, заблокирован ли пользователь
        public bool IsBlocked { get; set; } = false;
        // Ограничения на пароль пользователя
        public PasswordRestrictions PasswordRestrictions { get; set; } = new PasswordRestrictions();

        // Переопределение ToString для корректного отображения имени в списках
        public override string ToString() => Name;
    }
}