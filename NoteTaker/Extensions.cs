using Microsoft.Data.Sqlite;

namespace NoteTaker
{
    /// <summary>
    /// Extensions used for this application.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates a new command associated with the connection with the provided SQL.
        /// </summary>
        /// <param name="connection">The connection used by the command.</param>
        /// <param name="commandText">The SQL to execute against the database.</param>
        /// <returns>The new command.</returns>
        public static SqliteCommand CreateCommand(this SqliteConnection connection, string commandText)
        {
            connection.CreateCommand();
            return new SqliteCommand(commandText, connection);
        }
    }
}
