using NUnit.Framework;
using OneHundredFiles;

namespace OneHundredFiles.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        [Test]
        public void GenerateRandomDate_ShouldReturnValidDate()
        {
            string date = Program.GenerateRandomDate();
            Assert.That(DateTime.TryParseExact(date, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out _), Is.True);
        }

        [Test]
        public void GenerateRandomString_ShouldReturnStringLengthEqualTo10()
        {
            char[] latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            string result = Program.GenerateRandomString(latinChars, 10);
            Assert.That(result.Length, Is.EqualTo(10));

        }

        [Test]
        public void GenerateRandomString_ShouldReturnStringWhereEveryCharIsEqualToCharSet()
        {
            char[] latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
            string result = Program.GenerateRandomString(latinChars, 10);

            foreach (char c in result)
            {
                Assert.That(Array.Exists(latinChars, element => element == c), Is.True);
            }
        }

        [Test]
        public void GenerateRandomDate_ShouldBeWithinLast5Years()
        {
            string dateString = Program.GenerateRandomDate();
            DateTime date = DateTime.ParseExact(dateString, "dd.MM.yyyy", null);
            DateTime fiveYearsAgo = new DateTime(DateTime.Now.Year - 5, DateTime.Now.Month, DateTime.Now.Day);
            Assert.That(date, Is.InRange(fiveYearsAgo, DateTime.Now));
        }

        [Test]
        public void GenerateRandomDouble_ShouldHaveExactly8DecimalPlaces()
        {
            double result = Program.GenerateRandomDouble();
            string resultString = result.ToString("F8");
            Assert.That(resultString.Split('.')[1].Length, Is.EqualTo(8));
        }

        [Test]
        public void GenerateRandomEvenNumber_ShouldReturnEvenNumber()
        {
            int result = Program.GenerateRandomEvenNumber();
            Assert.That(result % 2, Is.EqualTo(0));
        }

        [Test]
        public void GenerateRandomEvenNumber_ShouldBeInRange()
        {
            int result = Program.GenerateRandomEvenNumber();
            Assert.That(result, Is.InRange(2, 100000000));
        }

        [Test]
        public void GenerateRandomDouble_ShouldReturnValidDoubleInRangeFrom1To20()
        {
            double result = Program.GenerateRandomDouble();
            Assert.That(result, Is.InRange(1, 20));
        }

        [Test]
        public void MergeFilesAndRemoveLines_ShouldRemoveLinesWithPattern()
        {
            // Создаем временные файлы для теста
            string[] fileNames = { "test_file_1.txt", "test_file_2.txt" };
            string patternToRemove = "abc";
            string outputFileName = "combined_test_file.txt";

            // Записываем данные в временные файлы
            File.WriteAllLines(fileNames[0], new[] { "abc123", "def456", "ghi789" });
            File.WriteAllLines(fileNames[1], new[] { "jkl012", "mno345", "abc678" });

            // Вызываем метод объединения и удаления строк
            int removedLinesCount = Program.MergeFilesAndRemoveLines(outputFileName, patternToRemove);

            // Проверяем количество удаленных строк
            Assert.That(removedLinesCount, Is.EqualTo(2));

            // Проверяем содержимое объединенного файла
            string[] combinedLines = File.ReadAllLines(outputFileName);
            Assert.That(combinedLines, Does.Not.Contain("abc123"));
            Assert.That(combinedLines, Does.Not.Contain("abc678"));
            Assert.That(combinedLines, Does.Contain("def456"));
            Assert.That(combinedLines, Does.Contain("ghi789"));
            Assert.That(combinedLines, Does.Contain("jkl012"));
            Assert.That(combinedLines, Does.Contain("mno345"));

            // Удаляем временные файлы
            File.Delete(fileNames[0]);
            File.Delete(fileNames[1]);
            File.Delete(outputFileName);
        }
    }
}