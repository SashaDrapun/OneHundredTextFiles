using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OneHundredFiles.Tests")]

namespace OneHundredFiles
{
    public class Program
    {
        private static int totalFiles = 100;
        private static int completedFiles = 0;
        private static int totalLines = 0;
        private static int processedLines = 0;

        private static void Main()
        {
            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Создать новые файлы");
                Console.WriteLine("2. Объединить все файлы в один");
                Console.Write("Введите номер действия: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        CreateNewFiles();
                        break;
                    case "2":
                        CombineFiles();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Завершение программы.");
                        return;
                }
            }
        }

        private static void CreateNewFiles()
        {
            // Удаляем старые файлы
            for (int fileIndex = 0; fileIndex < totalFiles; fileIndex++)
            {
                string fileName = $"file_{fileIndex + 1}.txt";
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }

            char[] latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            char[] russianChars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();

            Parallel.For(0, totalFiles, fileIndex =>
            {
                string fileName = $"file_{fileIndex + 1}.txt";
                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    for (int lineIndex = 0; lineIndex < 100000; lineIndex++)
                    {
                        string date = GenerateRandomDate();
                        string latinString = GenerateRandomString(latinChars, 10);
                        string russianString = GenerateRandomString(russianChars, 10);
                        int evenNumber = GenerateRandomEvenNumber();
                        double randomDouble = GenerateRandomDouble();

                        string line = $"{date}||{latinString}||{russianString}||{evenNumber}||{randomDouble:F8}||";
                        writer.WriteLine(line);
                    }
                }

                Interlocked.Increment(ref completedFiles);
                UpdateProgress();
            });

            Console.WriteLine("Все файлы созданы.");
        }

        private static void CombineFiles()
        {
            string outputFileName = "combined_file.txt";
            string patternToRemove = "abc";
            int removedLinesCount = MergeFilesAndRemoveLines(outputFileName, patternToRemove);
            Console.WriteLine($"Количество удаленных строк: {removedLinesCount}");
        }

        private static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

        public static string GenerateRandomDate()
        {
            DateTime startDate = new DateTime(DateTime.Now.Year - 5, 1, 1);
            DateTime endDate = DateTime.Now;
            int range = (endDate - startDate).Days;
            DateTime randomDate = startDate.AddDays(random.Value.Next(range));
            return randomDate.ToString("dd.MM.yyyy");
        }

        public static string GenerateRandomString(char[] charSet, int length)
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(charSet[random.Value.Next(charSet.Length)]);
            }
            return sb.ToString();
        }

        public static int GenerateRandomEvenNumber()
        {
            return random.Value.Next(1, 50000000) * 2;
        }

        public static double GenerateRandomDouble()
        {
            return random.Value.NextDouble() * 19 + 1;
        }

        private static void UpdateProgress()
        {
            int progressPercentage = (completedFiles * 100) / totalFiles;
            Console.WriteLine($"Прогресс: {progressPercentage}% ({completedFiles}/{totalFiles})");
        }

        public static int MergeFilesAndRemoveLines(string outputFileName, string patternToRemove)
        {
            int removedLinesCount = 0;
            totalLines = 0;
            processedLines = 0;

            // Подсчет общего количества строк
            Parallel.For(0, totalFiles, fileIndex =>
            {
                string fileName = $"file_{fileIndex + 1}.txt";
                if (File.Exists(fileName))
                {
                    int linesCount = File.ReadAllLines(fileName).Length;
                    Interlocked.Add(ref totalLines, linesCount);
                }
            });

            using (StreamWriter writer = new StreamWriter(outputFileName, false, Encoding.UTF8))
            {
                Parallel.For(0, totalFiles, fileIndex =>
                {
                    string fileName = $"file_{fileIndex + 1}.txt";
                    if (File.Exists(fileName))
                    {
                        string[] lines = File.ReadAllLines(fileName);
                        foreach (string line in lines)
                        {
                            if (!line.Contains(patternToRemove))
                            {
                                lock (writer)
                                {
                                    writer.WriteLine(line);
                                }
                            }
                            else
                            {
                                Interlocked.Increment(ref removedLinesCount);
                            }
                            Interlocked.Increment(ref processedLines);
                            UpdateMergeProgress();
                        }
                    }
                });
            }
            return removedLinesCount;
        }

        private static void UpdateMergeProgress()
        {
            int progressPercentage = (processedLines * 100) / totalLines;
            Console.WriteLine($"Прогресс объединения: {progressPercentage}% ({processedLines}/{totalLines})");
        }
    }
}