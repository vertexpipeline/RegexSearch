using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static System.Console;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;


namespace FileScaner
{
    class FileMatch
    {
        public MatchCollection Matches { get; set; }
        public string FileName { get; set; } = "";
    }

    class Program
    {
        //results data
        static int ReadErrors = 0;
        static int FileRead = 0;
        static int Entries = 0;
        static List<FileMatch> FilesMatches = new List<FileMatch>();

        //options data
        static List<DirectoryInfo> dirsToSearch = new List<DirectoryInfo>();
        static Regex regex = null;

        static CancellationTokenSource cancSource = new CancellationTokenSource();

        static void CheckFile(FileInfo file)
        {
            try
            {
                var matches = regex.Matches(File.ReadAllText(file.FullName));
                FilesMatches.Add(new FileMatch { Matches = matches, FileName = file.Name });
                Entries += matches.Count;
                FileRead++;
            }
            catch (IOException ex)
            {
                ReadErrors++;
            }
        }

        static void CheckFolder(DirectoryInfo dir, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                foreach (var file in dir.GetFiles())
                {
                    CheckFile(file);
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    CheckFolder(subDir, token);
                }
            }
            catch(UnauthorizedAccessException ex)
            {
                ReadErrors++;
            }
        }


        static void ShowResultProgram(int indent)
        {
            WriteLine("[Results program]");
            ForegroundColor = ConsoleColor.Green;
            WriteLine($"{FileRead} files read.");
            ForegroundColor = ConsoleColor.Red;
            WriteLine($"{ReadErrors} file corrupted \\ read error");
            ForegroundColor = ConsoleColor.White;
            foreach (var file in FilesMatches)
            {
                ForegroundColor = ConsoleColor.Magenta;
                WriteLine($"{file.FileName}:");
                foreach (Match match in file.Matches)
                {
                    foreach (Group group in match.Groups)
                    {
                        ForegroundColor = ConsoleColor.Gray;
                        WriteLine($"{new string(' ', indent)}Group: {group.Name}\n{new string(' ', indent * 2)}{group.Value}");
                    }
                }
            }
            WriteLine("Press any key...");
            ReadKey();
            ForegroundColor = ConsoleColor.Gray;
        }

        static void SearchProgram()
        {
            Clear();
            WriteLine("[Search program]");
            if (regex == null)
            {
                WriteLine("Regex is empty, set dat with \"set regex\" command.\nPress any key...");
                ReadKey();
            }
            else if (dirsToSearch.Count == 0)
            {
                WriteLine("Pathes is empty, set dat with \"add path\" command.\nPress any key...");
                ReadKey();
            }
            else
            {
                cancSource = new CancellationTokenSource(new TimeSpan(0,0,10));
                foreach (var path in dirsToSearch)
                {
                    Task.Run(() =>
                    {
                        CheckFolder(path, cancSource.Token);
                    }, cancSource.Token).Wait();

                    if (cancSource.IsCancellationRequested)
                    {
                        WriteLine("Search time exceed, press any key...");
                        ReadKey();
                    }
                }
                WriteLine($"Search completed.\nPress any key...");
                ReadKey();
                GC.Collect();
            }
        }

        static void ShowPathes()
        {
            int counter = 0;
            foreach (var path in dirsToSearch)
            {
                WriteLine($"{counter}:{path.FullName}");
                counter++;
            }
        }

        static bool ParseCommand()
        {
            var inp = ReadLine();

            switch (inp.Trim().ToLower())
            {
                case "exit":
                    {
                        return false;
                    }
                case "search":
                    {
                        SearchProgram();
                        return true;
                    }
                case "results count":
                    {
                        WriteLine($">{FilesMatches.Sum(p => p.Matches.Count)} matches in {FilesMatches.Count} files");
                        return true;
                    }
                case "set regex":
                    {
                        WriteLine($"Current regex:{regex?.ToString() ?? " EMPTY"}");
                        Write($"New>");
                        regex = new Regex(ReadLine());
                        return true;
                    }
                case "add path":
                    {
                        bool validWaiting = true;
                        while (validWaiting)
                        {
                            Write("Enter path>");
                            string path = ReadLine();
                            if (Directory.Exists(path))
                            {
                                dirsToSearch.Add(new DirectoryInfo(path));
                                validWaiting = false;
                            }
                            else
                            {
                                WriteLine("Invalid directory...\n Press <R> to retry\n or some other to exit in main menu...");
                                var ch = ReadLine();
                                if(ch[0].ToString().ToUpper() != "R")
                                    validWaiting = false;
                            }
                        }
                        return true;
                    }
                case "remove path":
                    {
                        
                        ShowPathes();
                        Write("Enter id to exclude>");
                        int id = 0;
                        while (int.TryParse(ReadLine(), out id) == false)
                        {
                            WriteLine("Invalid path...");
                            Write(">");
                        }
                        if(id >= 0 && id < dirsToSearch.Count)
                        {
                            dirsToSearch.RemoveAt(id);
                        }

                        return true;
                    }
                case "show pathes":
                    {
                        ShowPathes();
                        WriteLine("Press any key...");
                        ReadKey();
                        return true;
                    }
                case "show regex":
                    {
                        WriteLine(regex.ToString());
                        WriteLine("Press any key...");
                        ReadKey();
                        return true;
                    }
                case "clean results":
                    {
                        FilesMatches.Clear();
                        Entries = 0;
                        return true;
                    }
                case "results":
                    {
                        ShowResultProgram(4);
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
results - запустить подпрогамму вывода результатов
clean results - очистить список результатов
exit - выход
------------------------------------------------------------------
");
        }

        static void ShowMainInfo()
        {
            var posLeft = CursorLeft;
            var posTop = CursorTop;
            SetCursorPosition(0, WindowHeight - 1);
            Write($"Search folders:{dirsToSearch.Count}|Results entries:{Entries}");
            SetCursorPosition(posLeft, posTop);
        }

        static void Main(string[] args)
        {
            OutputEncoding = System.Text.Encoding.UTF8;

            do
            {
                ShowLegend();
                ShowMainInfo();
                Write(">");
                if(!ParseCommand())
                {
                    break;
                }
                Clear();

            } while(1 == 1);
        }
    }
}