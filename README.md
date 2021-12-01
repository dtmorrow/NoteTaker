# NoteTaker
A small utility to take notes from the command line.

## Usage
```
USAGE: note [name|search] [command] [contents]

COMMANDS:
  [search]                  --    Display all notes whose name matches [search]
                                  Can use '?'/'*' for wildcard/s
                                  Can use '@' (by itself) to mean the current date.
  [regex] ?                 --    Display all notes whose name matches [regex]
  [name] : [contents]       --    Write [contents] to [name]
  [name] :+ [contents]      --    Append [contents] to [name]
  [name] :@ [contents]      --    Append [contents] to [name] with timestamp
  [name] ::                 --    Write standard input to [name]
  [name] ::+                --    Append standard input to [name]
  [name] ::#                --    Append standard input to [name] with timestamp
                                  If not being redirected, standard input can
                                  be ended by ending line with "::"
  [name] -                  --    Deletes note with [name]
  [search] --               --    Deletes all notes whose name matches [search]
  [regex] --?               --    Deletes all notes whose name matches [regex]
  [old-name] = [new-name]   --    Renames note [old-name] to [new-name]

EXAMPLE:
  note MyNote : Some text.           -- Creates (or overwrites) a note named 'MyNote' with the contents 'Some text'.
  note MyNote :+ Some more text.     -- Appends the text 'Some more text.' to the note named 'MyNote'.
  note MyNote                        -- Displays the contents of the note named 'MyNote'.
  note MyNote*                       -- Displays the contents of any note that starts with 'MyNote'.
  note @ :@ Today's Date and Time.   -- Appends to (or creates) a note with the current date,
                                        prepending the note contents ('Today's Date and Time.') with a timestamp.
  note @                             -- Displays the contents of a note whose name is the current date.
  note *                             -- Displays all notes.
```
