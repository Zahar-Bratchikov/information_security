using System;
using System.Numerics;

class PasswordCrackerEstimator
{
    static void Main()
    {
        Console.Write("Введите пароль: ");
        string password = Console.ReadLine();

        Console.Write("Введите скорость перебора (паролей в секунду): ");
        if (!BigInteger.TryParse(Console.ReadLine(), out BigInteger speed) || speed <= 0)
        {
            Console.WriteLine("Ошибка: неверная скорость перебора.");
            return;
        }

        Console.Write("Введите количество неудачных попыток до паузы: ");
        if (!BigInteger.TryParse(Console.ReadLine(), out BigInteger attemptsBeforePause) || attemptsBeforePause <= 0)
        {
            Console.WriteLine("Ошибка: неверное количество попыток.");
            return;
        }

        Console.Write("Введите время паузы (в секундах): ");
        if (!BigInteger.TryParse(Console.ReadLine(), out BigInteger pauseTime) || pauseTime < 0)
        {
            Console.WriteLine("Ошибка: неверное время паузы.");
            return;
        }

        int alphabetSize = GetAlphabetSize(password);
        Console.WriteLine($"Мощность алфавита: {alphabetSize}");

        BigInteger totalCombinations = BigInteger.Pow(alphabetSize, password.Length);
        Console.WriteLine($"Общее количество возможных паролей: {totalCombinations}");

        // Реальный подсчет времени перебора
        BigInteger totalAttempts = totalCombinations;
        BigInteger baseTimeSeconds = totalAttempts / speed;

        // Корректный учёт пауз (только если кол-во попыток больше попыток до паузы)
        BigInteger pauseCount = totalAttempts >= attemptsBeforePause
            ? totalAttempts / attemptsBeforePause
            : 0;
        BigInteger totalTimeSeconds = baseTimeSeconds + pauseCount * pauseTime;

        // Конвертация времени в удобный формат
        BigInteger years = totalTimeSeconds / (365 * 24 * 3600);
        BigInteger remainingSeconds = totalTimeSeconds % (365 * 24 * 3600);
        int months = (int)(remainingSeconds / (30 * 24 * 3600));
        remainingSeconds %= 30 * 24 * 3600;
        int days = (int)(remainingSeconds / (24 * 3600));
        remainingSeconds %= 24 * 3600;
        int hours = (int)(remainingSeconds / 3600);
        remainingSeconds %= 3600;
        int minutes = (int)(remainingSeconds / 60);
        int seconds = (int)(remainingSeconds % 60);

        Console.WriteLine($"Время подбора: {years} лет, {months} месяцев, {days} дней, " +
                          $"{hours} часов, {minutes} минут, {seconds} секунд");
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
        if (hasSpecial) size += 32;

        return size;
    }
}
