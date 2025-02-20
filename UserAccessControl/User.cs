namespace UserAccessControl
{
    public class PasswordRestrictions
    {
        public bool EnableLengthRestriction { get; set; }
        public int MinLength { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecialChar { get; set; }
    }

    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public bool IsBlocked { get; set; }
        public PasswordRestrictions PasswordRestrictions { get; set; }

        public User(string name, string password, bool isBlocked, PasswordRestrictions restrictions)
        {
            Name = name;
            Password = password;
            IsBlocked = isBlocked;
            PasswordRestrictions = restrictions;
        }
    }
}