using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JBA.Exceptions;
using JBA.Model;

namespace JBA
{
    internal class FileProcessor
    {
        //[Years=1991-2000]
        private static readonly Regex regexYear =
            new Regex(@"\[Years=(?<yearStart>(19|20)\d{2})-(?<yearFinish>(19|20)\d{2})\]");

        //Grid-ref=   1, 148
        private static readonly Regex regexGridRef = new Regex(@"^Grid-ref=\D*(?<Xref>\d+),\D*(?<Yref>\d+)");

        private readonly string filename;

        private readonly List<PrecipitationRecord> recordsToAdd = new List<PrecipitationRecord>();
        private int startYear, endYear;

        public FileProcessor(string filename)
        {
            this.filename = filename;
        }

        public List<PrecipitationRecord> Process()
        {
            if (!File.Exists(filename)) throw new FileNotFoundException(filename);

            //Clear database if it already exists
            using (JBADbContext db = new JBADbContext(filename))
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                db.SaveChanges();
            }

            using (StreamReader file =
                new StreamReader(filename))
            {
                if (TryReadHeader(file))
                {
                    ParseRecords(file);
                    return recordsToAdd;
                }

                throw new ParseException("Unable to read file header.");
            }
        }

        private void ParseRecords(StreamReader file)
        {
            string line;
            while ((line = file.ReadLine()) != null)
            {
                //check if we found new record chunk start
                Match match = regexGridRef.Match(line);
                if (match.Success)
                {
                    //parse cord values
                    int Xref = int.Parse(match.Groups["Xref"].Value);
                    int Yref = int.Parse(match.Groups["Yref"].Value);
                    //should contain a line for each year
                    for (int i = 0; i <= endYear - startYear; i++)
                    {
                        line = file.ReadLine();
                        if (line != null)
                        {
                            //originally used regex but due to lines like 7586, 7960 etc. had to switch to splitting string
                            //into fixed sized chunks
                            List<string> substrings = Enumerable.Range(0, line.Length / 5)
                                .Select(s => line.Substring(s * 5, 5)).ToList();
                            //should contain value for each month
                            if (substrings.Count == 12)
                            {
                                int monthCounter = 1;
                                foreach (string valueString in substrings)
                                {
                                    //create records 
                                    recordsToAdd.Add(new PrecipitationRecord
                                    {
                                        Xref = Xref,
                                        Yref = Yref,
                                        Date = new DateTime(startYear + i, monthCounter, 1),
                                        Value = int.Parse(valueString.Trim())
                                    });
                                    monthCounter++;
                                    if (recordsToAdd.Count % 10000 == 0)
                                        Console.WriteLine($"Read {recordsToAdd.Count} records from file {filename}");
                                }
                            }
                            else
                            {
                                throw new ParseException(
                                    $"Grid-ref line \"{line}\" is not followed by the expected amount of data");
                            }
                        }
                        else
                        {
                            throw new ParseException("Unexpected end of file");
                        }
                    }
                }
            }

            Console.WriteLine($"Read {recordsToAdd.Count} records from file {filename}");
        }

        private bool TryReadHeader(StreamReader file)
        {
            //read until we find year header line
            string line;
            while ((line = file.ReadLine()) != null)
            {
                Match match = regexYear.Match(line);
                if (match.Success)
                {
                    startYear = int.Parse(match.Groups["yearStart"].Value);
                    endYear = int.Parse(match.Groups["yearFinish"].Value);
                    //year range is not valid, abort
                    if (startYear > endYear) throw new ParseException("Invalid year range parsed in the file header.");
                    return true;
                }
            }

            return false;
        }
    }
}