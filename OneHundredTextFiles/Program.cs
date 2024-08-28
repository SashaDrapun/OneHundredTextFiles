using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneHundredFiles
{
    public class Program
    {
        private static int totalFiles = 20;
        private static int completedFiles = 0;
        private static int totalLines = 0;
        private static int processedLines = 0;
        private static int removedLinesCount = 0;

        private static string connectionString = "Server=(localdb)\\mssqllocaldb;Database=oneHundredFilesInfo;Trusted_Connection=True;MultipleActiveResultSets=true";

        private static void Main()
        {
            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1. Создать новые файлы");
                Console.WriteLine("2. Объединить все файлы в один");
                Console.WriteLine("3. Импортировать файлы в базу данных");
                Console.WriteLine("4. Вычислить сумму и медиану");
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
                    case "3":
                        ImportFilesToDatabase();
                        break;
                    case "4":
                        CalculateSumAndMedian();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор. Завершение программы.");
                        return;
                }
            }
        }

        public static void CalculateSumAndMedian()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("CalculateSumAndMedian", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                long sumOfIntegers = reader.GetInt64(0);
                                decimal medianOfDoubles = reader.GetDecimal(1);

                                Console.WriteLine($"Сумма всех целых чисел: {sumOfIntegers}");
                                Console.WriteLine($"Медиана всех дробных чисел: {medianOfDoubles}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении хранимой процедуры: {ex.Message}");
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

        public static void ImportFilesToDatabase()
        {
            string directoryPath = Path.Combine(Directory.GetCurrentDirectory());
            string[] fileNames = Directory.GetFiles(directoryPath, "file_*.txt");
            int totalLines = 0;
            int importedLines = 0;

            // Подсчет общего количества строк
            foreach (string fileName in fileNames)
            {
                int linesCount = File.ReadAllLines(fileName).Length;
                Interlocked.Add(ref totalLines, linesCount);
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Подключение к базе данных установлено.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
                    return;
                }

                foreach (string fileName in fileNames)
                {
                    using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] parts = line.Split(new[] { "||" }, StringSplitOptions.None);
                            if (parts.Length == 6)
                            {
                                string date = parts[0];
                                string latinString = parts[1];
                                string russianString = parts[2];
                                int evenNumber = int.Parse(parts[3]);
                                double randomDouble = double.Parse(parts[4]);

                                using (SqlCommand command = new SqlCommand("INSERT INTO ImportedData (Date, LatinString, RussianString, EvenNumber, RandomDouble) VALUES (@Date, @LatinString, @RussianString, @EvenNumber, @RandomDouble)", connection))
                                {
                                    command.Parameters.AddWithValue("@Date", date);
                                    command.Parameters.AddWithValue("@LatinString", latinString);
                                    command.Parameters.AddWithValue("@RussianString", russianString);
                                    command.Parameters.AddWithValue("@EvenNumber", evenNumber);
                                    command.Parameters.AddWithValue("@RandomDouble", randomDouble);

                                    try
                                    {
                                        command.ExecuteNonQuery();
                                        Interlocked.Increment(ref importedLines);
                                        UpdateImportProgress(importedLines, totalLines);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Ошибка при вставке данных: {ex.Message}");
                                    }
                                }



                                Interlocked.Increment(ref importedLines);
                                UpdateImportProgress(importedLines, totalLines);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Импорт завершен.");
        }

        private static void UpdateImportProgress(int importedLines, int totalLines)
        {
            int progressPercentage = (importedLines * 100) / totalLines;
            Console.WriteLine($"Прогресс импорта: {progressPercentage}% ({importedLines}/{totalLines})");
        }

        private static void CombineFiles()
        {
            string outputFileName = "combined_file.txt";
            Console.Write("Введите шаблон для удаления строк: ");
            string patternToRemove = Console.ReadLine();
            int removedLines = MergeFilesAndRemoveLines(outputFileName, patternToRemove).Result;
            Console.WriteLine($"Количество удаленных строк: {removedLines}");
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

        public static async Task<int> MergeFilesAndRemoveLines(string outputFileName, string patternToRemove)
        {
            removedLinesCount = 0;
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
                var tasks = new List<Task>();
                var semaphore = new SemaphoreSlim(1, 1);
                for (int fileIndex = 0; fileIndex < totalFiles; fileIndex++)
                {
                    string fileName = $"file_{fileIndex + 1}.txt";
                    if (File.Exists(fileName))
                    {
                        tasks.Add(ProcessFileAsync(fileName, patternToRemove, writer, semaphore));
                    }
                }
                await Task.WhenAll(tasks);
            }
            return removedLinesCount;
        }

        private static async Task ProcessFileAsync(string fileName, string patternToRemove, StreamWriter writer, SemaphoreSlim semaphore)
        {
            using (StreamReader reader = new StreamReader(fileName, Encoding.UTF8))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (!line.Contains(patternToRemove))
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            await writer.WriteLineAsync(line);
                        }
                        finally
                        {
                            semaphore.Release();
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
        }

        private static void UpdateMergeProgress()
        {
            int progressPercentage = (processedLines * 100) / totalLines;
            Console.WriteLine($"Прогресс объединения: {progressPercentage}% ({processedLines}/{totalLines})");
        }
    }
}
