namespace CrackPassword
{
    public class User
    {
        public string Name { get; set; }
        // Пароль хранится в виде SHA256-хэша
        public string Password { get; set; }
        public PasswordRestrictions PasswordRestrictions { get; set; }
        public bool IsBlocked { get; set; }
        public override string ToString() => Name;
    }
}