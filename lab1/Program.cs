using System;
using System.Numerics;

class PasswordCrackerEstimator
{
    static void Main()
    {
        string password = Prompt("Введите пароль: ");
        BigInteger speed = PromptBigInteger("Введите скорость перебора (паролей в секунду): ");
        BigInteger attemptsBeforePause = PromptBigInteger("Введите количество неудачных попыток до паузы: ");
        BigInteger pauseTime = PromptBigInteger("Введите время паузы (в секундах): ");

        int alphabetSize = GetAlphabetSize(password);
        BigInteger totalCombinations = BigInteger.Pow(alphabetSize, password.Length);
        BigInteger baseTimeSeconds = totalCombinations / speed;
        BigInteger pauseCount = (totalCombinations - 1) / attemptsBeforePause;
        BigInteger totalTimeSeconds = baseTimeSeconds + pauseCount * pauseTime;

        Console.WriteLine($"\nМощность алфавита: {alphabetSize}");
        Console.WriteLine($"Общее количество возможных паролей: {totalCombinations}");
        Console.WriteLine($"Примерное время подбора: {FormatTime(totalTimeSeconds)}");
    }

    static string Prompt(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? string.Empty;
    }

    static BigInteger PromptBigInteger(string message)
    {
        while (true)
        {
            Console.Write(message);
            if (BigInteger.TryParse(Console.ReadLine(), out var value) && value >= 0)
                return value;
            Console.WriteLine("Ошибка ввода. Введите положительное число.");
        }
    }

    static int GetAlphabetSize(string password)
    {
        bool hasLower = false, hasUpper = false, hasDigits = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsLower(c)) hasLower = true;
            else if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsDigit(c)) hasDigits = true;
            else hasSpecial = true;
        }
        int size = 0;
        if (hasLower) size += 26;
        if (hasUpper) size += 26;
        if (hasDigits) size += 10;
        if (hasSpecial) size += 33;
        return size;
    }

    static string FormatTime(BigInteger totalSeconds)
    {
        BigInteger years = totalSeconds / (365 * 24 * 3600);
        totalSeconds %= 365 * 24 * 3600;
        int months = (int)(totalSeconds / (30 * 24 * 3600));
        totalSeconds %= 30 * 24 * 3600;
        int days = (int)(totalSeconds / (24 * 3600));
        totalSeconds %= 24 * 3600;
        int hours = (int)(totalSeconds / 3600);
        totalSeconds %= 3600;
        int minutes = (int)(totalSeconds / 60);
        int seconds = (int)(totalSeconds % 60);
        return $"{years} лет, {months} месяцев, {days} дней, {hours} часов, {minutes} минут, {seconds} секунд";
    }
}
