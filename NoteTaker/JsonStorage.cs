using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;

namespace NoteTaker
{
    /// <summary>
    /// Provides backing storage for notes with a JSON file.
    /// </summary>
    public class JsonStorage : INoteStorage
    {
        readonly string filename;

        /// <summary>
        /// Create a backing storage for notes with a JSON file with the provided name.
        /// </summary>
        /// <param name="filename">The name of the JSON file, including the extension.</param>
        public JsonStorage(string filename)
        {
            this.filename = filename;

            var file = new FileInfo(filename);
            if (!file.Exists || file.Length == 0)
            {
                File.WriteAllText(filename, "{}");
            }
        }

        public void Write(string name, string value)
        {
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                bool append = true;

                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (item.Name == name)
                    {
                        append = false;
                        writer.WriteString(name, value);
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }

                if (append)
                {
                    writer.WriteString(name, value);
                }

                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);
        }

        public void Append(string name, string value)
        {
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                bool newNote = true;

                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (item.Name == name)
                    {
                        newNote = false;
                        writer.WriteString(name, item.Value.ToString() + "\n" + value);
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }

                if (newNote)
                {
                    writer.WriteString(name, value);
                }

                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);
        }

        public void AppendDateTime(string name, string value)
        {
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                bool newNote = true;

                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (item.Name == name)
                    {
                        newNote = false;
                        writer.WriteString(name, item.Value.ToString() + $"\n\n[{DateTime.Now:f}]\n" + value);
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }

                if (newNote)
                {
                    writer.WriteString(name, $"[{DateTime.Now:f}]\n" + value);
                }

                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);
        }

        public bool Delete(string name)
        {
            bool delete = false;
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (item.Name == name)
                    {
                        delete = true;
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }
                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);

            return delete;
        }

        public int Delete(Regex regex)
        {
            int delete = 0;
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (regex.IsMatch(item.Name))
                    {
                        delete++;
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }
                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);

            return delete;
        }

        /// <summary>
        /// Rename a note to a new name. WARNING: Does not provide protection against renaming a note to a name that already exists.
        /// </summary>
        /// <param name="oldName">The old name of the note.</param>
        /// <param name="newName">The new name for the note.</param>
        /// <returns>The result of the rename operation. Cannot return <see cref="RenameResult.NewNameAlreadyExists"/>.</returns>
        /// <returns></returns>
        public RenameResult Rename(string oldName, string newName)
        {
            bool rename = false;
            var temp = Path.GetTempFileName();

            using (var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            using (var output = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var reader = JsonDocument.Parse(input))
            using (var writer = new Utf8JsonWriter(output))
            {
                writer.WriteStartObject();
                foreach (var item in reader.RootElement.EnumerateObject())
                {
                    if (item.Name == oldName)
                    {
                        rename = true;
                        writer.WriteString(newName, item.Value.ToString());
                    }
                    else
                    {
                        writer.WriteString(item.Name, item.Value.ToString());
                    }
                }
                writer.WriteEndObject();
            }

            File.Move(temp, filename, true);

            return rename ? RenameResult.Success : RenameResult.OldNameDoesNotExist;
        }

        public IEnumerable<KeyValuePair<string, string>> Search(Regex regex)
        {
            foreach (var item in Enumerate())
            {
                if (regex.IsMatch(item.Key))
                {
                    yield return new KeyValuePair<string, string>(item.Key, item.Value);
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Enumerate()
        {
            using var input = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
            using var reader = JsonDocument.Parse(input);

            foreach (var item in reader.RootElement.EnumerateObject())
            {
                yield return new KeyValuePair<string, string>(item.Name, item.Value.ToString());
            }
        }

        /// <summary>
        /// Does nothing, as no current optimization are defined for <see cref="JsonStorage"/>.
        /// </summary>
        public void Optimize()
        {
            // For now, does nothing.
        }
    }
}
