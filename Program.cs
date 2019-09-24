using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MarkdownTableFormatter
{
    class Program
    {
        public class Options
        {
            [Value(0, MetaName = "input file", Required = true, HelpText = "Input file name.")]
            public string FileName { get; set; }

            [Option('b', "backup", Required = false, HelpText = "Backup file name.")]
            public string BackupFileName { get; set; }

            [Option('p', "padding", Required = false, HelpText = "Apply the right padding for each column in the table.")]
            public bool EnablePadding { get; set; }

            [Option('P', "padding1", Required = false, HelpText = "Apply the right padding for 1st column in the table.")]
            public bool EnablePaddingForFirstColumn { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(options =>
                {
                    // 0. Adjust options.
                    if (String.IsNullOrWhiteSpace(options.BackupFileName))
                    {
                        options.BackupFileName = options.FileName + ".bak";
                    }

                    // 1. Create a backup, first.
                    try
                    {
                        FileInfo file = new FileInfo(options.FileName);
                        file.CopyTo(options.BackupFileName, true);

                        FileInfo backupFile = new FileInfo(options.BackupFileName);
                        if (!backupFile.Exists)
                        {
                            throw new FileNotFoundException();
                        }
                    }
                    catch
                    {
                        // Catch any exceptions by design.
                        Console.WriteLine("Error: Failed to create a backup file, {0}.", options.BackupFileName);
                        return;
                    }

                    // 2. Print the markdown file while formatting the tables.
                    try
                    {
                        string line = null;
                        var tableHeaders = new SortedList<int, string>();
                        var sortedTableItems = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
                        var itemWidthMax = new SortedList<int, int>();

                        using (var reader = new StreamReader(options.BackupFileName))
                        using (var write = new StreamWriter(options.FileName, false, Encoding.UTF8))
                        {
                            // A. Determine if a table starts.
                            Func<bool> IsLineTableHeader = () =>
                            {
                                return line.TrimStart(new char[] { ' ', '\t' }).StartsWith("|");
                            };

                            // B. Determine if a table ends.
                            Func<bool> IsLineTableBody = () =>
                            {
                                return !(String.IsNullOrWhiteSpace(line) || line.StartsWith("`"));
                            };

                            // C. Format the table body, stored in sortedTableItems.
                            //
                            // Input
                            //   sortedTableItems
                            //   maxItemLength
                            Func<string> FormatTableBody = () =>
                            {
                                var formattedTable = String.Empty;

                                // Format the body of the table.
                                foreach (var tableItem in sortedTableItems)
                                {
                                    var totalColumnCount = itemWidthMax.Count - 2;
                                    var splittedLine = tableItem.Value.Split(new char[] { '|' });

                                    for (int i = 0; i < itemWidthMax.Count; i++)
                                    {
                                        var item = splittedLine.Length > i ? splittedLine[i] : String.Empty;
                                        var trimedItem = item.Trim();
                                        if (i == 0)
                                        {
                                            // Do not trim. Keep the original indent for the table.
                                            formattedTable += item;
                                        }
                                        else if ((i == 1) && options.EnablePaddingForFirstColumn)
                                        {
                                            formattedTable += "| " + trimedItem.PadRight(itemWidthMax[i]) + " ";
                                        }
                                        else if (i <= totalColumnCount)
                                        {
                                            formattedTable += "| " + (options.EnablePadding ? trimedItem.PadRight(itemWidthMax[i]) : trimedItem) + " ";
                                        }
                                        else
                                        {
                                            formattedTable += "|" + Environment.NewLine;
                                        }
                                    }
                                }

                                return formattedTable;
                            };

                            // D. Find the first item and calculate the padding size of each item.
                            //
                            // Optput
                            //   maxItemLength
                            Func<int, (string firstItem, string line)> FindFirstItemAndCalculatePaddingSize = (tableIndentSize) =>
                            {
                                string firstItem = null;
                                var index = 0;
                                var itemCount = line.Split(new char[] { '|' }).Length;

                                // Unformatted table item was found.
                                // Adjust the line.
                                if (itemCount == 1)
                                {
                                    line = "|" + line + "|";
                                }

                                foreach (var item in line.Split(new char[] { '|' }))
                                {
                                    var trimedItem = item.Trim();
                                    if (itemWidthMax.ContainsKey(index))
                                    {
                                        if (index != 0)
                                        {
                                            itemWidthMax[index] = Math.Max(itemWidthMax[index], trimedItem.Length);
                                        }
                                    }
                                    else
                                    {
                                        if (index != 0)
                                        {
                                            itemWidthMax.Add(index, trimedItem.Length);
                                        }
                                        else
                                        {
                                            itemWidthMax.Add(0, tableIndentSize);
                                        }
                                    }

                                    if (index == 1)
                                    {
                                        firstItem = trimedItem;
                                        // If padding is not enabled, the rest items can be ignored.
                                        if (!options.EnablePadding)
                                        {
                                            break;
                                        }
                                    }

                                    index++;
                                }
                                
                                return (firstItem, line);
                            };

                            // E. Format the table header.
                            Func<(string header, string border, int columnCount, int tableIndentSize)> FormatTableHeader = () =>
                            {
                                // Assume that the @line contains the header line.

                                var itemCount = 0;
                                string header = String.Empty;
                                string border = String.Empty;
                                int totalColumnCount = line.Split(new char[] { '|' }).Length - 2;
                                int tableIndentSize = 0;

                                foreach (var item in line.Split(new char[] { '|' }))
                                {
                                    if (itemCount == 0)
                                    {
                                        // Do not trim the 1st item, which is the indent for the table.
                                        header += item;
                                        border += item;
                                        tableIndentSize = item.Length;
                                    }
                                    else if (itemCount <= totalColumnCount)
                                    {
                                        var trimedItem = item.Trim();

                                        header += "| " + trimedItem + " ";
                                        border += "|-" + "".PadRight(trimedItem.Length, '-') + "-";
                                    }
                                    else
                                    {
                                        // The last item is tailing spaces. Ignore it.
                                        header += "|";
                                        border += "|";
                                    }
                                    itemCount++;
                                }

                                // Assume the first two lines are headers, header and border, and simply skip the next line.
                                reader.ReadLine();

                                return (header, border, totalColumnCount, tableIndentSize);
                            };

                            // F. Adjust the parameters to format the table body.
                            Action<int> AdjustParametersForFormat = (columnCount) =>
                            {
                                int missingColumnCount = columnCount + 2 - itemWidthMax.Count;

                                for (int i = 0; i < missingColumnCount; i++)
                                {
                                    itemWidthMax.Add(itemWidthMax.Count, 0);
                                }
                            };

                            // Parse the markdown file line by line.
                            while (!reader.EndOfStream)
                            {
                                line = reader.ReadLine();
                                // 1. Print the line if not table.
                                if (!IsLineTableHeader())
                                {
                                    write.WriteLine(line);
                                    continue;
                                }
                                else
                                {
                                    // Come here when a table is found.

                                    // 2. Print the formatted table header.
                                    // in this step, figure out the amount of columns.
                                    var formattedHeader = FormatTableHeader();
                                    write.WriteLine(formattedHeader.header);
                                    write.WriteLine(formattedHeader.border);

                                    // Reset variables.
                                    sortedTableItems.Clear();
                                    itemWidthMax.Clear();
                                    while (!reader.EndOfStream)
                                    {
                                        // Read each line until table ends.
                                        line = reader.ReadLine();
                                        if (!IsLineTableBody())
                                        {
                                            // 3. Print the formatted table body.
                                            AdjustParametersForFormat(formattedHeader.columnCount);
                                            write.Write(FormatTableBody());
                                            write.WriteLine(line);
                                            write.Flush();
                                            break;
                                        }
                                        // Calculate the best width of each column.
                                        var parsedLine = FindFirstItemAndCalculatePaddingSize(formattedHeader.tableIndentSize);
                                        if (!sortedTableItems.ContainsKey(parsedLine.firstItem))
                                        {
                                            sortedTableItems.Add(parsedLine.firstItem, parsedLine.line);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Warning: a dup line, {0}", parsedLine.line);
                                        }
                                    }
                                }
                            }
                        }

                        Console.WriteLine("Finished successfully.");
                        Console.WriteLine("The original file is copied to {0}, just in case.", options.BackupFileName);
                    }
                    catch
                    {
                        // Revert back to the original file. Hope it doesn't throw.
                        FileInfo backupFile = new FileInfo(options.BackupFileName);
                        backupFile.CopyTo(options.FileName, true);

                        Console.WriteLine("Failed.");
                    }
                })
                .WithNotParsed<Options>(errors =>
                {
                    Console.WriteLine("Error: Please specify the input file (.md).");
                });

        }

        static bool CreateBackupFile(string fileName, string backupFileName)
        {
            bool succeeded = false;
            return succeeded;
        }
    }
}
