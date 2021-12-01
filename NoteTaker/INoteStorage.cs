using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NoteTaker
{
    /// <summary>
    /// The result of a rename operation: <see cref="RenameResult.Success"/> if the operation is successful; <see cref="RenameResult.OldNameDoesNotExist"/> if the note with the provided old name could not be found; and <see cref="RenameResult.NewNameAlreadyExists"/> if a note with the provided new name already exists.
    /// </summary>
    public enum RenameResult
    {
        /// <summary>
        /// The rename operation completed successfully.
        /// </summary>
        Success,
        /// <summary>
        /// A note with the provided old name does not exist.
        /// </summary>
        OldNameDoesNotExist,
        /// <summary>
        /// A note with the provided new name already exists.
        /// </summary>
        NewNameAlreadyExists
    }

    /// <summary>
    /// Provides an interface for storing and retrieving notes as <see cref="KeyValuePair">KeyValuePairs</see>
    /// </summary>
    public interface INoteStorage
    {
        /// <summary>
        /// Write a new note or overwrite an old note if one exists.
        /// </summary>
        /// <param name="name">The name of the note.</param>
        /// <param name="value">The note contents.</param>
        public void Write(string name, string value);

        /// <summary>
        /// Write a new note or append to an old note if one exists.
        /// </summary>
        /// <param name="name">The name of the note.</param>
        /// <param name="value">The note contents to be appended.</param>
        public void Append(string name, string value);


        /// <summary>
        /// Write a new note or append to an old note if one exists, that precedes the note contents with the current <see cref="DateTime.Now"/>.
        /// </summary>
        /// <param name="name">The name of the note.</param>
        /// <param name="value">The note contents to be appended.</param>
        public void AppendDateTime(string name, string value);

        /// <summary>
        /// Delete a note with the provided name.
        /// </summary>
        /// <param name="name">The name of the note.</param>
        /// <returns>Returns <see langword="true"/> if the note was deleted, otherwise returns <see langword="false"/>.</returns>
        public bool Delete(string name);

        /// <summary>
        /// Delete all notes whose names match the provided <see cref="Regex"/>.
        /// </summary>
        /// <param name="regex">The <see cref="Regex"/> to be matched against the note name.</param>
        /// <returns>The number of notes deleted.</returns>
        public int Delete(Regex regex);

        /// <summary>
        /// Perform optimizations on size or performance, if any, on the backing store.
        /// </summary>
        public void Optimize();

        /// <summary>
        /// Rename a note to a new name.
        /// </summary>
        /// <param name="oldName">The old name of the note.</param>
        /// <param name="newName">The new name for the note.</param>
        /// <returns>The result of the rename operation.</returns>
        public RenameResult Rename(string oldName, string newName);

        /// <summary>
        /// Search for notes whose names match the provided <see cref="Regex"/>.
        /// </summary>
        /// <param name="regex">The <see cref="Regex"/> to be matched against the note name.</param>
        /// <returns>An <see cref="IEnumerable{KeyValuePair}"/>of notes.</returns>
        public IEnumerable<KeyValuePair<string, string>> Search(Regex regex);

        /// <summary>
        /// Enumerates all notes in the backing store.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{KeyValuePair}"/>of notes.</returns>
        public IEnumerable<KeyValuePair<string, string>> Enumerate();
    }
}
