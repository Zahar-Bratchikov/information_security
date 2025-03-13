using System.Text.Json.Serialization;

namespace CrackPassword
{
    public class User
    {
        public string Name { get; set; }
        // The password is stored as a SHA256 hash
        public string PasswordHash { get; set; }
        public bool IsBlocked { get; set; } = false;
        public PasswordRestrictions PasswordRestrictions { get; set; } = new PasswordRestrictions();
    }
}