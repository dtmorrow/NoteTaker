using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace NoteTaker
{
    /// <summary>
    /// Provides backing storage for notes with an Sqlite file.
    /// </summary>
    public class SqliteStorage : INoteStorage
    {
        readonly SqliteConnection db;

        /// <summary>
        /// Create a backing storage for notes with an Sqlite file with the provided name.
        /// </summary>
        /// <param name="filename">The name of the Sqlite file, including the extension.</param>
        public SqliteStorage(string filename)
        {
            var connection = new SqliteConnectionStringBuilder()
            {
                DataSource = filename,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            db = new SqliteConnection(connection.ConnectionString);
            db.Open();

            db.CreateFunction("REGEXP", (string pattern, string input) => Regex.IsMatch(input, pattern));

            using var create = db.CreateCommand("CREATE TABLE IF NOT EXISTS `Notes` (`Name` TEXT UNIQUE, `Value` TEXT)");
            create.ExecuteNonQuery();
        }

        private string GetValue(string name)
        {
            using var select = db.CreateCommand("SELECT `Value` FROM `Notes` WHERE `Name` == $name;");
            select.Parameters.AddWithValue("$name", name);
            var reader = select.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                return reader.GetString(0);
            }
            else
            {
                return null;
            }
        }

        public void Write(string name, string value)
        {
            using var upsert = db.CreateCommand("INSERT OR REPLACE INTO `Notes` VALUES ( $name, $value );");
            upsert.Parameters.AddWithValue("$name", name);
            upsert.Parameters.AddWithValue("$value", value);
            upsert.ExecuteNonQuery();
        }

        public void Append(string name, string value)
        {
            var old = GetValue(name);

            if (old == null)
            {
                Write(name, value);
            }
            else
            {
                Write(name, $"{old}\n{value}");
            }
            
        }

        public void AppendDateTime(string name, string value)
        {
            var old = GetValue(name);

            if (old == null)
            {
                Write(name, $"[{DateTime.Now:f}]\n{value}");
            }
            else
            {
                Write(name, $"{GetValue(name)}\n\n[{DateTime.Now:f}]\n{value}");
            }
        }

        public bool Delete(string name)
        {
            using var delete = db.CreateCommand("DELETE FROM `Notes` WHERE `Name` == $name;");
            delete.Parameters.AddWithValue("$name", name);
            var rows = delete.ExecuteNonQuery();

            return rows > 0;
            
        }

        public int Delete(Regex regex)
        {
            using var delete = db.CreateCommand("DELETE FROM `Notes` WHERE `Name` REGEXP $name;");
            delete.Parameters.AddWithValue("$name", regex.ToString());
            var rows = delete.ExecuteNonQuery();

            return rows;
        }

        public RenameResult Rename(string oldName, string newName)
        {
            using var rename = db.CreateCommand("UPDATE `Notes` SET `Name` = $newName WHERE `Name` == $oldName;");
            rename.Parameters.AddWithValue("$oldName", oldName);
            rename.Parameters.AddWithValue("$newName", newName);

            try
            {
                var rows = rename.ExecuteNonQuery();

                if (rows == 0)
                {
                    return RenameResult.OldNameDoesNotExist;
                }
                else
                {
                    return RenameResult.Success;
                }
            }
            catch (SqliteException ex)
            {
                if (ex.SqliteErrorCode == 19)
                {
                    return RenameResult.NewNameAlreadyExists;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Search(Regex regex)
        {
            using var regexp = db.CreateCommand("SELECT `Name`, `Value` FROM `Notes` WHERE `Name` REGEXP $name ORDER BY ROWID DESC;");
            regexp.Parameters.AddWithValue("$name", regex.ToString());
            var reader = regexp.ExecuteReader();

            while (reader.Read())
            {
                var name = reader.GetString(0);
                var value = reader.GetString(1);

                yield return new KeyValuePair<string, string>(name, value);
            }
        }

        public IEnumerable<KeyValuePair<string, string>> Enumerate()
        {
            using var all = db.CreateCommand("SELECT `Name`, `Value` FROM `Notes` ORDER BY ROWID DESC;");
            var reader = all.ExecuteReader();

            while (reader.Read())
            {
                var name = reader.GetString(0);
                var value = reader.GetString(1);

                yield return new KeyValuePair<string, string>(name, value);
            }
        }

        /// <summary>
        /// Runs a "VACUUM" operation on the Sqlite file.
        /// </summary>
        public void Optimize()
        {
            using var vacuum = db.CreateCommand("VACUUM;");
            vacuum.ExecuteNonQuery();
        }
    }
}
