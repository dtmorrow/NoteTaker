using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NoteTaker
{
    class Program
    {
        static void Main(string[] args)
        {
            //INoteStorage notes = new JsonStorage("notes.json");
            INoteStorage notes = new SqliteStorage("notes.db");

            switch (args.Length)
            {
                case 0:
                    ShowUsage();
                    break;
                case 1:
                    {
                        var name = (args[0] == "@") ? $"{DateTime.Now:yyyy-MM-dd}" : args[0];
                        PrintSearch(notes, name, ConvertGlobToRegex(name));
                    }
                    break;
                case 2:
                    {
                        var name = (args[0] == "@") ? $"{DateTime.Now:yyyy-MM-dd}" : args[0];
                        var command = args[1];
                        Process(notes, name, command);
                    }
                    break;
                default:
                    {
                        var name = (args[0] == "@") ? $"{DateTime.Now:yyyy-MM-dd}" : args[0];
                        var command = args[1];
                        var value = string.Join(' ', args.Skip(2));
                        Process(notes, name, command, value);
                    }
                    break;
            }

            notes.Optimize();
        }

        static void ShowUsage(bool exit = true)
        {
            Console.WriteLine("USAGE: note [name|search] [command] [contents]");
            Console.WriteLine("\nCOMMANDS:");
            Console.WriteLine
            (
                "  [search]                  --    Display all notes whose name matches [search]\n" +
                "                                  Can use '?'/'*' for wildcard/s\n" +
                "                                  Can use '@' (by itself) to mean the current date.\n" +
                "  [regex] ?                 --    Display all notes whose name matches [regex]\n" +
                "  [name] : [contents]       --    Write [contents] to [name]\n" +
                "  [name] :+ [contents]      --    Append [contents] to [name]\n" +
                "  [name] :@ [contents]      --    Append [contents] to [name] with timestamp\n" +
                "  [name] ::                 --    Write standard input to [name]\n" +
                "  [name] ::+                --    Append standard input to [name]\n" +
                "  [name] ::#                --    Append standard input to [name] with timestamp\n" +
                "                                  If not being redirected, standard input can\n" +
                "                                  be ended by ending line with \"::\"\n" +
                "  [name] -                  --    Deletes note with [name]\n" +
                "  [search] --               --    Deletes all notes whose name matches [search]\n" +
                "  [regex] --?               --    Deletes all notes whose name matches [regex]\n" +
                "  [old-name] = [new-name]   --    Renames note [old-name] to [new-name]"
            );
            Console.WriteLine("\nEXAMPLE:");
            Console.WriteLine
            (
                "  note MyNote : Some text.           -- Creates (or overwrites) a note named 'MyNote' with the contents 'Some text'.\n" +
                "  note MyNote :+ Some more text.     -- Appends the text 'Some more text.' to the note named 'MyNote'.\n" +
                "  note MyNote                        -- Displays the contents of the note named 'MyNote'.\n" +
                "  note MyNote*                       -- Displays the contents of any note that starts with 'MyNote'.\n" +
                "  note @ :@ Today's Date and Time.   -- Appends to (or creates) a note with the current date,\n" +
                "                                        prepending the note contents ('Today's Date and Time.') with a timestamp.\n" +
                "  note @                             -- Displays the contents of a note whose name is the current date.\n" +
                "  note *                             -- Displays all notes."
            );

            if (exit)
            {
                Environment.Exit(0);
            }
        }

        static void PrintSearch(INoteStorage notes, string searchString, Regex searchRegex)
        {
            bool none = true;
            foreach (var item in notes.Search(searchRegex))
            {
                none = false;
                Console.Error.WriteLine($"--{item.Key}--");
                Console.WriteLine(item.Value);
                Console.WriteLine();
            }

            if (none)
            {
                Console.Error.WriteLine($"Could not find any notes with search pattern \"{searchString}\".");
            }
        }

        static void Process(INoteStorage notes, string name, string command)
        {
            switch (command)
            {
                case "::":
                    notes.Write(name, StandardInputMode());
                    break;
                case "::+":
                    notes.Append(name, StandardInputMode());
                    break;
                case "::@":
                    notes.AppendDateTime(name, StandardInputMode());
                    break;
                case "-":
                    if (notes.Delete(name))
                    {
                        Console.Error.WriteLine("Note successfully deleted.");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Could not find note with name \"{name}\".");
                    }
                    break;
                case "--":
                    {
                        int delete = notes.Delete(ConvertGlobToRegex(name));
                        Console.Error.WriteLine($"Deleted {delete} notes.");
                    }
                    break;
                case "--?":
                    {
                        int delete = notes.Delete(new Regex(name));
                        Console.Error.WriteLine($"Deleted {delete} notes.");
                    }
                    break;
                case "?":
                    PrintSearch(notes, name, new Regex(name));
                    break;
                default:
                    Console.Error.WriteLine($"Error: Unknown Command \"{command}\".");
                    ShowUsage();
                    break;
            }
        }

        static void Process(INoteStorage notes, string name, string command, string value)
        {
            switch (command)
            {
                case ":":
                    notes.Write(name, value);
                    break;
                case ":+":
                    notes.Append(name, value);
                    break;
                case ":@":
                    notes.AppendDateTime(name, value);
                    break;
                case "=":
                    var result = notes.Rename(name, value);
                    if (result == RenameResult.Success)
                    {
                        Console.Error.WriteLine("Note successfully renamed,");
                    }
                    else if (result == RenameResult.OldNameDoesNotExist)
                    {
                        Console.Error.WriteLine($"Could not find note with name \"{name}\".");
                    }
                    else
                    {
                        Console.Error.WriteLine($"Could note with name \"{value}\" already exists.");
                    }
                    break;
                default:
                    Console.Error.WriteLine($"Error: Unknown Command \"{command}\".");
                    ShowUsage();
                    break;
            }
        }

        static string StandardInputMode()
        {
            if (Console.IsInputRedirected)
            {
                using var reader = new StreamReader(Console.OpenStandardInput());
                return reader.ReadToEnd();
            }

            var sb = new StringBuilder();

            string read;
            do
            {
                read = Console.ReadLine();
                sb.Append(read);
                sb.Append('\n');
            }
            while (!read.EndsWith("::"));

            sb.Length -= 3; // Remove "::\n"

            return sb.ToString();
        }

        static Regex ConvertGlobToRegex(string search)
        {
            var sb = new StringBuilder(search.Length);

            sb.Append('^');
            foreach (var c in search)
            {
                if (c == '*')
                {
                    sb.Append(".*?");
                }
                else if (c == '?')
                {
                    sb.Append('.');
                }
                else
                {
                    sb.Append(Regex.Escape(c.ToString()));
                }
            }
            sb.Append('$');

            return new Regex(sb.ToString());
        }
        
    }
}
