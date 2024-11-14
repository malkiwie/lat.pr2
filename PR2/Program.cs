using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

class Program
{
    // Массив с хэшами, которые нужно найти
    private static string[] targetHashes = {
        "1115dd800feaacefdf481f1f9070374a2a81e27880f187396db67958b207cbad", // MD5
        "3a7bd3e2360a3d29eea436fcfb7e44c735d117c42d1c1835420b6b9942dd4f1b", // SHA-256
        "74e1bb62f8dabb8125a58852b63bdf6eaef667cb56ac7f7cdba6d7305c50a22f", // SHA-256
        "7a68f09bd992671bb3b19a5e70b7827e"  // MD5
    };

    // Алфавит строчных букв
    private static string alphabet = "abcdefghijklmnopqrstuvwxyz";

    // Метод для хеширования пароля с использованием MD5
    private static string GetMD5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    // Метод для хеширования пароля с использованием SHA-256
    private static string GetSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.ASCII.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    // Метод для поиска пароля методом грубой силы (однопоточный)
    private static void BruteForceSingleThreaded(string[] targetHashes)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        foreach (string password in GeneratePasswords())
        {
            foreach (string hash in targetHashes)
            {
                if (GetMD5Hash(password) == hash || GetSHA256Hash(password) == hash)
                {
                    Console.WriteLine($"Найден пароль: {password} для хэша: {hash}");
                }
            }
        }

        stopwatch.Stop();
        Console.WriteLine($"Время однопоточного поиска: {stopwatch.Elapsed.TotalSeconds} секунд");
    }

    // Метод для поиска пароля методом грубой силы (многопоточный)
    private static void BruteForceMultiThreaded(string[] targetHashes, int threads)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        // Разбиваем работу на несколько потоков
        List<Thread> threadList = new List<Thread>();
        int totalPasswords = 26 * 26 * 26 * 26 * 26; // 26^5 паролей
        int passwordsPerThread = totalPasswords / threads;

        for (int i = 0; i < threads; i++)
        {
            int startIndex = i * passwordsPerThread;
            int endIndex = (i == threads - 1) ? totalPasswords : (i + 1) * passwordsPerThread;

            Thread thread = new Thread(() => SearchInRange(startIndex, endIndex, targetHashes));
            threadList.Add(thread);
            thread.Start();
        }

        // Ожидаем завершения всех потоков
        foreach (Thread thread in threadList)
        {
            thread.Join();
        }

        stopwatch.Stop();
        Console.WriteLine($"Время многопоточного поиска: {stopwatch.Elapsed.TotalSeconds} секунд");
    }

    // Метод для поиска пароля в определённом диапазоне
    private static void SearchInRange(int startIndex, int endIndex, string[] targetHashes)
    {
        int index = 0;
        foreach (string password in GeneratePasswords())
        {
            if (index >= startIndex && index < endIndex)
            {
                foreach (string hash in targetHashes)
                {
                    if (GetMD5Hash(password) == hash || GetSHA256Hash(password) == hash)
                    {
                        Console.WriteLine($"Найден пароль: {password} для хэша: {hash}");
                    }
                }
            }
            index++;
            if (index >= endIndex) break;
        }
    }

    // Генератор всех пятибуквенных паролей
    private static IEnumerable<string> GeneratePasswords()
    {
        char[] password = new char[5];
        for (int i = 0; i < 26; i++)
        {
            for (int j = 0; j < 26; j++)
            {
                for (int k = 0; k < 26; k++)
                {
                    for (int l = 0; l < 26; l++)
                    {
                        for (int m = 0; m < 26; m++)
                        {
                            password[0] = alphabet[i];
                            password[1] = alphabet[j];
                            password[2] = alphabet[k];
                            password[3] = alphabet[l];
                            password[4] = alphabet[m];
                            yield return new string(password);
                        }
                    }
                }
            }
        }
    }

    // Основной метод
    static void Main(string[] args)
    {
        while (true) // Цикл, который позволяет выбирать и выполнять операции снова
        {
            
            Console.WriteLine("Выберите режим:");
            Console.WriteLine("1. Однопоточный поиск");
            Console.WriteLine("2. Многопоточный поиск");
            Console.WriteLine("3. Выход");

            int mode;
            bool validInput = int.TryParse(Console.ReadLine(), out mode);

            if (!validInput || mode < 1 || mode > 3)
            {
                Console.WriteLine("Некорректный выбор. Пожалуйста, выберите 1, 2 или 3.");
                continue;
            }

            if (mode == 3)
            {
                Console.WriteLine("Выход из программы...");
                break; // Прерывает цикл и завершает программу
            }

            int threads = 1; // Если выбран многопоточный режим, задаем количество потоков
            if (mode == 2)
            {
                Console.WriteLine("Введите количество потоков для многопоточного поиска:");
                bool isValidThreads = int.TryParse(Console.ReadLine(), out threads);
                if (!isValidThreads || threads <= 0)
                {
                    Console.WriteLine("Некорректное количество потоков. Будет использован один поток.");
                    threads = 1;
                }
            }

            // Запуск в зависимости от выбранного режима
            switch (mode)
            {
                case 1:
                    BruteForceSingleThreaded(targetHashes);
                    break;
                case 2:
                    BruteForceMultiThreaded(targetHashes, threads);
                    break;
            }

            Console.WriteLine("Нажмите 'Enter' для нового выбора...");
            Console.ReadLine(); // Ожидаем нажатия клавиши Enter, чтобы начать заново
        }
    }
}
