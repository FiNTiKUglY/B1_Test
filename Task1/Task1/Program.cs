using System;
using System.Numerics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Task1
{
    internal class Program
    {
        static string GenDate()
        {
            Random rnd = new Random();
            DateTime subTime = DateTime.MinValue;
            DateTime currentTime = DateTime.Now;
            subTime = subTime.AddDays(rnd.Next(1825));
            var result = currentTime.Ticks - subTime.Ticks;
            return new DateTime(result).ToString("dd.MM.yyyy");
        }

        static string GenLatinicString()
        {
            StringBuilder outputString = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < 9; i++)
            {
                while (true)
                {
                    var character = rnd.Next(58);
                    if (character >= 26 && character <= 31)
                    {
                        continue;
                    }
                    else
                    {
                        outputString.Append((char)(character + 65));
                        break;
                    }
                }
            }
            return outputString.ToString();
        }

        static string GenCyrillicString()
        {
            StringBuilder outputString = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < 9; i++)
            {
                var character = rnd.Next(64);
                outputString.Append((char)(character + 1040));
            }
            return outputString.ToString();
        }

        static string GenInteger()
        {
            Random rnd = new Random();
            return (rnd.Next(100000000) + 1).ToString();
        }

        static string GenReal()
        {
            Random rnd = new Random();
            return ((rnd.Next(1900000000) + 100000000) / 100000000.0).ToString("0.00000000");
        }

        static async Task WriteFile(int index)
        {
            FileStream outputStream = new FileStream($"{index + 1}.txt", FileMode.Create);
            for (int i = 0; i < 100000; i++)
            {
                var outputString = $"{GenDate()}||{GenLatinicString()}||{GenCyrillicString()}||{GenInteger()}||{GenReal()}\n";
                await outputStream.WriteAsync(Encoding.UTF8.GetBytes(outputString), 0, Encoding.UTF8.GetBytes(outputString).Length);
            }
            outputStream.Dispose();
        }

        static async Task ConcatFiles(int fileCount, string template)
        {
            FileStream outputStream = new FileStream($"final.txt", FileMode.Create);
            for (var i = 0; i < fileCount; i++)
            {
                StreamReader inputStream = new StreamReader($"{i + 1}.txt", Encoding.UTF8);
                string? inputString;
                while ((inputString = await inputStream.ReadLineAsync()) != null)
                {
                    if (inputString == "" || inputString.Contains(template))
                    {
                        continue;
                    }
                    await outputStream.WriteAsync(Encoding.UTF8.GetBytes(inputString + "\n"), 0, Encoding.UTF8.GetBytes(inputString).Length + 1);
                }
                inputStream.Dispose();
            }
            outputStream.Dispose();
        }

        static async Task Main(string[] args)
        {
            int fileCount = 100;
            Console.CursorVisible = false;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < fileCount; i++)
            {
                tasks.Add(WriteFile(i));
            }
            Console.WriteLine("Генерация файлов...");
            await Task.WhenAll(tasks);
            Console.WriteLine("Объединение файлов...");
            var template = "abc";
            await ConcatFiles(fileCount, template);

            var fileForImport = "1.txt";
            Console.WriteLine("Импорт строк...");
            StreamReader inputStream = new StreamReader(fileForImport, Encoding.UTF8);
            using (ApplicationContext db = new ApplicationContext())
            {
                string? inputString;
                var current = 0;
                var totalLines = new List<string>();
                while ((inputString = await inputStream.ReadLineAsync()) != null)
                {
                    if (inputString == "")
                    {
                        continue;
                    }
                    totalLines.Add(inputString);
                }
                foreach(var line in totalLines)
                {
                    var splitString = line.Split("||");
                    int imported = await db.Database.ExecuteSqlRawAsync($"INSERT INTO entities (\"Date\", \"Latinic\", \"Cyrillic\", \"Integer\", \"Real\") " +
                       $"VALUES ('{splitString[0]}', '{splitString[1]}', '{splitString[2]}', '{splitString[3]}', '{splitString[4].Replace(',', '.')}')");
                    current += imported;
                    Console.SetCursorPosition(0, 3);
                    Console.WriteLine($"{current} из {totalLines.Count}");
                }

            var values = db.Database.SqlQueryRaw<SupportForQuery>($"SELECT SUM(\"Integer\"), AVG(\"Real\") FROM entities").ToList();
                Console.WriteLine($"Сумма целых чисел: {values[0].Sum}\nМедиана дробных чисел: {values[0].Avg}");
            }
            inputStream.Dispose();
        }
    }
}
