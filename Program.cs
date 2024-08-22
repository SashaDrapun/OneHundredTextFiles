using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        Random random = new Random();
        char[] latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] russianChars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();

        for (int fileIndex = 0; fileIndex < 100; fileIndex++)
        {
            string fileName = $"file_{fileIndex + 1}.txt";
            using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                for (int lineIndex = 0; lineIndex < 100000; lineIndex++)
                {
                    string date = GenerateRandomDate(random);
                    string latinString = GenerateRandomString(random, latinChars, 10);
                    string russianString = GenerateRandomString(random, russianChars, 10);
                    int evenNumber = GenerateRandomEvenNumber(random);
                    double randomDouble = GenerateRandomDouble(random);

                    string line = $"{date}||{latinString}||{russianString}||{evenNumber}||{randomDouble:F8}||";
                    writer.WriteLine(line);
                }
            }
        }
    }

    static string GenerateRandomDate(Random random)
    {
        DateTime startDate = new DateTime(DateTime.Now.Year - 5, DateTime.Now.Month, DateTime.Now.Day);
        DateTime endDate = DateTime.Now;
        int range = (endDate - startDate).Days;
        DateTime randomDate = startDate.AddDays(random.Next(range));
        return randomDate.ToString("dd.MM.yyyy");
    }

    static string GenerateRandomString(Random random, char[] charSet, int length)
    {
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(charSet[random.Next(charSet.Length)]);
        }
        return sb.ToString();
    }

    static int GenerateRandomEvenNumber(Random random)
    {
        return random.Next(1, 50000000) * 2;
    }

    static double GenerateRandomDouble(Random random)
    {
        return random.NextDouble() * 19 + 1;
    }
}