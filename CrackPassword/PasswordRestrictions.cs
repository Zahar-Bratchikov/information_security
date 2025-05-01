namespace CrackPassword
{
    public class PasswordRestrictions
    {
        public bool EnableLengthRestriction { get; set; }
        public int MinLength { get; set; }
        public bool RequireUppercase { get; set; }
        public bool RequireDigit { get; set; }
        public bool RequireSpecialChar { get; set; }
    }
}