using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using JBA.Model;
using Microsoft.EntityFrameworkCore;

namespace JBA
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //can process multiple files in parallel
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => Parallel.ForEach(opts.InputFiles, filename =>
                {
                    try
                    {
                        FileProcessor fileProcessor = new FileProcessor(filename);
                        List<PrecipitationRecord> records = fileProcessor.Process();
                        WriteRecords(records, opts.Raw, filename);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }));
        }

        private static void WriteRecords(List<PrecipitationRecord> records, bool rawSql, string filename)
        {
            if (rawSql)
            {
                //need to manually increment ids since EF is not used
                int idCounter = 0;
                //batch inserts in chunks to avoid massive query size
                using (JBADbContext context = new JBADbContext(filename))
                {
                    context.ChangeTracker.AutoDetectChangesEnabled = false;
                    for (int i = 0; i < Math.Ceiling((double) records.Count / 10000); i++)
                    {
                        StringBuilder builder = new StringBuilder("INSERT INTO \"PrecipitationRecords\" VALUES ");
                        //safe to append values directly as the were parsed into primitives previously
                        builder.AppendJoin(",", records.Skip(10000 * i).Take(10000).Select(
                            x =>
                                $"({idCounter++}, {x.Xref}, {x.Yref}, \"{x.Date:yyyy-MM-dd HH:mm:ss}\", {x.Value})"));
                        context.Database.ExecuteSqlCommand(builder.ToString());
                        Console.WriteLine(
                            $"Inserted {idCounter} records into database for {filename}");
                    }

                    context.SaveChanges();
                }
            }

            else
            {
                //insert using EFCore, slower as each record is inserted individually so requires a round trip
                using (JBADbContext context = new JBADbContext(filename))
                {
                    //batch adding to provide status updates
                    for (int i = 0; i < Math.Ceiling((double) records.Count / 1000); i++)
                    {
                        context.ChangeTracker.AutoDetectChangesEnabled = false;
                        List<PrecipitationRecord> recordsToInsert = records.Skip(1000 * i).Take(1000).ToList();
                        context.AddRange(recordsToInsert);
                        context.SaveChanges();
                        Console.WriteLine(
                            $"Inserted {i * 1000 + recordsToInsert.Count} records into database for {filename}");
                    }
                }
            }
        }
    }
}