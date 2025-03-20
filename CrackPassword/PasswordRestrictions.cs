namespace CrackPassword
{
    // Класс, представляющий ограничения для пароля
    public class PasswordRestrictions
    {
        // Флаг: включено ли ограничение на длину пароля
        public bool EnableLengthRestriction { get; set; }
        // Минимальная длина пароля
        public int MinLength { get; set; }
        // Требуется ли наличие заглавной буквы
        public bool RequireUppercase { get; set; }
        // Требуется ли наличие цифры
        public bool RequireDigit { get; set; }
        // Требуются ли специальные символы
        public bool RequireSpecialChar { get; set; }
    }
}