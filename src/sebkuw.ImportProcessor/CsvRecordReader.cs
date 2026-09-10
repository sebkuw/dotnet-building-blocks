using System.Runtime.CompilerServices;
using System.Text;

namespace sebkuw.ImportProcessor;

internal static class CsvRecordReader
{
    public static async IAsyncEnumerable<IReadOnlyList<string>> ReadAsync(
        TextReader reader,
        char delimiter,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var buffer = new char[4096];
        var inQuotes = false;
        var quotePending = false;
        var skipLineFeed = false;
        var anyCharacters = false;

        while (true)
        {
            var count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (count == 0)
            {
                break;
            }

            for (var index = 0; index < count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var character = buffer[index];
                anyCharacters = true;

                if (skipLineFeed)
                {
                    skipLineFeed = false;
                    if (character == '\n')
                    {
                        continue;
                    }
                }

                if (inQuotes)
                {
                    if (quotePending)
                    {
                        if (character == '"')
                        {
                            field.Append('"');
                            quotePending = false;
                            continue;
                        }

                        inQuotes = false;
                        quotePending = false;
                    }
                    else if (character == '"')
                    {
                        quotePending = true;
                        continue;
                    }
                    else
                    {
                        field.Append(character);
                        continue;
                    }
                }

                if (character == '"' && field.Length == 0)
                {
                    inQuotes = true;
                }
                else if (character == delimiter)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else if (character is '\r' or '\n')
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    yield return fields.ToArray();
                    fields.Clear();
                    skipLineFeed = character == '\r';
                    anyCharacters = false;
                }
                else
                {
                    field.Append(character);
                }
            }
        }

        if (inQuotes && !quotePending)
        {
            throw new FormatException("The CSV document ends inside a quoted field.");
        }

        if (anyCharacters || field.Length > 0 || fields.Count > 0)
        {
            fields.Add(field.ToString());
            yield return fields.ToArray();
        }
    }
}
