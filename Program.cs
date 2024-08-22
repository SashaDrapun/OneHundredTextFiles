using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        char[] latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        char[] russianChars = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя".ToCharArray();

        Parallel.For(0, 100, fileIndex =>
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
        });
    }

    static ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

    static string GenerateRandomDate()
    {
        DateTime startDate = new DateTime(DateTime.Now.Year - 5, 1, 1);
        DateTime endDate = DateTime.Now;
        int range = (endDate - startDate).Days;
        DateTime randomDate = startDate.AddDays(random.Value.Next(range));
        return randomDate.ToString("dd.MM.yyyy");
    }

    static string GenerateRandomString(char[] charSet, int length)
    {
        StringBuilder sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            sb.Append(charSet[random.Value.Next(charSet.Length)]);
        }
        return sb.ToString();
    }

    static int GenerateRandomEvenNumber()
    {
        return random.Value.Next(1, 50000000) * 2;
    }

    static double GenerateRandomDouble()
    {
        return random.Value.NextDouble() * 19 + 1;
    }
}