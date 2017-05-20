using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static System.Console;
using System.Linq;


namespace FileScaner
{
    class FileMatch
    {
        public MatchCollection Matches { get; set; }
        public string FileName { get; set; } = "";
    }

    class Program
    {
        static int ReadErrors = 0;
        static int FileRead = 0;

        static List<FileMatch> FilesMatches = new List<FileMatch>();
        
        static void CheckFile(FileInfo file, string regex)
        {
            try
            {
                var matches = Regex.Matches(File.ReadAllText(file.FullName), regex);

                FilesMatches.Add(new FileMatch { Matches = matches, FileName = file.Name });
                FileRead++;
            }
            catch(IOException ex)
            {
                ReadErrors++;
            }
        }

        static void ShowResult(int indent)
        {
            WriteLine("Results...");
            ForegroundColor = ConsoleColor.Green;
            WriteLine($"{FileRead} files read.");
            ForegroundColor = ConsoleColor.Red;
            WriteLine($"{ReadErrors} file corrupted \\ read error");
            ForegroundColor = ConsoleColor.White;
            foreach(var file in FilesMatches)
            {
                ForegroundColor = ConsoleColor.Magenta;
                WriteLine($"{file.FileName}:");
                foreach(Match match in file.Matches)
                {
                    foreach(Group group in match.Groups)
                    {
                        ForegroundColor = ConsoleColor.Blue;
                        WriteLine($"{new string(' ', indent)}Group: {group.Name}\n{new string(' ', indent*2)}{group.Value}");
                    }
                }
            }
        }

        static void CheckFolder(DirectoryInfo dir, string regex)
        {
            foreach(var file in dir.GetFiles())
            {
                CheckFile(file, regex);
            }
            foreach(var subDir in dir.GetDirectories())
            {
                CheckFolder(subDir, regex);
            }
        }

        static void Search()
        {
            Write("Regex>");
            var regex = ReadLine();
            Write("Path>");
            var path = ReadLine();

            if(!Directory.Exists(path))
            {
                WriteLine("Invalid path");
                return;
            }

            CheckFolder(new DirectoryInfo(path), regex);

            WriteLine($"Search succeed ({FilesMatches.Count})");
        }

        static bool ParseCommand()
        {
            var inp = ReadLine();

            switch(inp.Trim().ToLower())
            {
                case "exit":
                {
                    return false;
                }
                case "search":
                {
                    Search();
                    return true;
                }
                case "results count":
                {
                    WriteLine($">{FilesMatches.Sum(p => p.Matches.Count)} matches in {FilesMatches.Count} files");
                    return true;
                }
                case "results":
                {
                    ShowResult(4);
                    return true;
                }
                default:
                   return true;
            }
        }

        static void ShowLegend()
        {
            WriteLine(@"Основные команды:
search - запустить подпрограмму поиска
set regex - устанавливает искомое регулярное выражение
show regex - показать регулярное выражение
add path - добавить директорию поиска
show pathes - показать директории поиска
remove path [index] - удалить директорию по ее идентификатору
results - вывод результатов поиска
results count - вывод количества результатов
exit - выход
");
        }

        static void Main(string[] args)
        {
            OutputEncoding = System.Text.Encoding.UTF8;

            ShowLegend();
            do
            {
                Write(">");
                if(!ParseCommand())
                {
                    break;
                }

            } while(1 == 1);
        }
    }
}