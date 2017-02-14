
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using MWL.DocumentResolver;
using Superpower;
using Superpower.Parsers;

namespace LedgerCore.Commands
{
    public class ImportCSVCommand : ICommand
    {

        public static void Configure(CommandLineApplication command, CommandLineOptions options)
        {

            command.Description = "Parse a bank csv into ledger format";
            command.HelpOption("--help|-h|-?");

            var fileArgument = command.Argument("file",
                                   "File I should parse");

            command.OnExecute(() =>
                {
                    options.Command = new ImportCSVCommand(fileArgument.Value, options);

                    return 0;
                });

        }

        private readonly string _file;
        private readonly CommandLineOptions _options;

        public ImportCSVCommand(string file, CommandLineOptions options)
        {
            _file = file;
            _options = options;
        }


         private  void parseCSV(string csv)
        {
            var lines = System.IO.File.ReadAllLines(csv);
            Dictionary<string, string> dict = new Dictionary<string, string>();



            List<ImportModel> models = new List<ImportModel>();
            int index = 0;
            foreach (var line in lines.Skip(1))
            {
                string[] split = line.Split(',');

                TextParser<string> dateid =
                                             from month in Character.Digit.Many()
                                             from sep2 in Character.EqualTo('/')
                                             from day in Character.Digit.Many()
                                             from sep in Character.EqualTo('/')
                                             from year in Character.Digit.Many()
                                             select new string(year.Append(sep).Concat(month).Append(sep2).Concat(day).ToArray());

                TextParser<string> refid = from digits in Character.Digit.Many()
                                           select new string(digits);

                TextParser<string> payeeid = from letters in Character.AnyChar.Many().AtEnd()
                                             select new string(letters);

                TextParser<string> memo1id = from l in Character.AnyChar.Many()
                                             from tref in Character.Digit.Many().AtEnd()
                                             select new string(tref);

                TextParser<string> memo2id =
                                            from tref in Character.AnyChar.Many().AtEnd()
                                            select new string(tref);

                TextParser<string> amountid = from dollar in Character.AnyChar.Many().AtEnd()
                                              select new string(dollar.ToArray());

                var date = dateid.Parse(split[0]);
                var refnum = refid.Parse(split[1]);
                var payeename = payeeid.Parse(split[2]);
                var memonames = memo2id.TryParse(split[3]);
                var memorefs = memo1id.TryParse(split[3]);
                var amount = amountid.Parse(split[4]);

                if (refnum == "0")
                {
                    if (memorefs.HasValue)
                    {
                        refnum = memorefs.Value;
                    }
                }
                ImportModel im = new ImportModel() { Date = DateTime.Parse(date), ReferenceNumber = refnum, PayeeName = payeename, Memo = (memonames.HasValue ? memonames.Value : string.Empty), Amount = decimal.Parse(amount) };

                if (memonames.HasValue)
                {
                    foreach (var c in im.CategoryList)
                    {
                        if (memonames.Value.Contains(c))
                        {
                            im.CategoryName = c;
                        }
                    }

                }

                if(string.IsNullOrEmpty(im.PayeeName)) {
                    im.PayeeName = im.Memo;
                }

                if (index == 0)
                {
                    dict.Add(index.ToString(), im.PayeeName);
                }
                else
                {
                    // Initialize an instance of the resolver
                    DocumentResolver resolver = new DocumentResolver(dict);
                    resolver.SetEngine(DocumentResolver.EngineType.BayesTFIDF);
                    List<ResolutionResult> resolutionResults = resolver.Resolve(im.PayeeName, true);
                    foreach (ResolutionResult resolutionResult in resolutionResults)
                    {
                        Console.WriteLine(string.Format("{0} {1} {2}\r\n", resolutionResult.Score.ToString(), resolutionResult.Key, resolutionResult.Document));

                        //System.IO.File.AppendAllText(outputfile, string.Format("{0} {1} {2}\r\n", resolutionResult.Score.ToString(), resolutionResult.Key, resolutionResult.Document));
                    }
                    dict.Add(index.ToString(), im.PayeeName);
                }

                models.Add(im);
                index++;
            }
        }

        public void Run()
        {
            Console.WriteLine("Hello "
                + (_file != null ? _file : "NO FILE GIVEN")
                + (_options.IsVerbose ? "!!!" : "."));

                parseCSV(_file);
        }

    }

}