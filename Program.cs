using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LedgerCore.Data;
using LedgerCore.Models;
using LiteDB;
using MWL.DocumentResolver;
//using Sprache;
using Superpower;
using Superpower.Parsers;

namespace LedgerCore
{

    public enum LedgerToken
    {
        None,
        Keyword,
        Colon,
        Amount
    }

    public class Program
    {


        private static void seed()
        {
            using (var db = new LiteDatabase(@"test.db"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<Ledger>("ledgers");

                Ledger l = new Ledger();
                l.Journal = new List<Transaction>();

                Transaction t = new Transaction();
                t.TransactionDetails = new List<TransactionDetail>();
                Account a = new Account();
                a.AccountType = new AccountType() { Name = "Assets" };
                a.Name = "Huntington:Checking";
                Account a2 = new Account();
                a2.AccountType = new AccountType() { Name = "Equity" };
                a2.Name = "OpeningBalances";

                TransactionDetail td = new TransactionDetail();
                td.Accounts = new List<Account>();
                td.Accounts.Add(a);
                td.Amount = -2.85M;
                TransactionDetail td2 = new TransactionDetail();
                td2.Accounts = new List<Account>();
                td2.Accounts.Add(a2);
                td2.Amount = 2.85M;

                t.TransactionDetails.Add(td);
                t.TransactionDetails.Add(td2);
                l.Journal.Add(t);

                // Insert new customer document (Id will be auto-incremented)
                col.Insert(l);
            }
        }

        private static void read()
        {
            var context = new LiteDBContext<Ledger>("test.db", "ledgers");

            var data = context.db.FindAll();
        }


        private static Ledger parseLedger(string file)
        {
            var lines = System.IO.File.ReadLines(file);

            int lineCount = lines.Count();
            int transactionLines = 4;
            int transactionCount = (lineCount - 1) / transactionLines + 1;
            List<Transaction> transactions = new List<Transaction>();

            for (int index = 0; index < transactionCount; index++)
            {


                var newLines = lines.Skip(index == 0 ? 0 : index * transactionLines).Take(transactionLines);
                Transaction t = new Transaction();
                t.TransactionDetails = new List<TransactionDetail>();
                int i = 0;
                foreach (var line in newLines)
                {
                    if (String.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }


                    if (i == 0)
                    {
                        TextParser<string> dateid =
                                                from year in Character.Digit.Many()
                                                from sep in Character.EqualTo('/')
                                                from month in Character.Digit.Many()
                                                from sep2 in Character.EqualTo('/')
                                                from day in Character.Digit.Many()
                                                select new string(year.Append(sep).Concat(month).Append(sep2).Concat(day).ToArray());


                        var date = dateid.Parse(line);

                        t.Date = DateTime.Parse(date);

                        TextParser<string> payeeid =
                                        from first in dateid
                                        from space in Character.WhiteSpace.Many()
                                        from payee in Character.AnyChar.Many().AtEnd()
                                        select new string(payee.ToArray());

                        t.PayeeName = payeeid.Parse(line);
                    }
                    else
                    {
                        TransactionDetail td = new TransactionDetail();

                        TextParser<string> accountid = from sp in Character.WhiteSpace.Many()
                                                       from at in Character.Letter.Many().ManyDelimitedBy(Character.EqualTo(':'))
                                                       select new string(at.SelectMany(inner => inner.Append(':')).ToArray());

                        td.Accounts = new List<Account>();
                        Account a = new Account();
                        a.Name = accountid.Parse(line);
                        a.Name = a.Name.Substring(0, a.Name.Length - 1);
                        td.Accounts.Add(a);


                        TextParser<string> amountid = from ac in accountid
                                                      from sp in Character.WhiteSpace.Many()
                                                      from dollar in Character.AnyChar.Many().AtEnd()
                                                      select new string(dollar.ToArray());

                        string amount = amountid.Parse(line);
                        if (String.IsNullOrEmpty(amount))
                        {
                            td.Amount = t.TransactionDetails.FirstOrDefault().Amount;
                        }
                        else
                        {
                            td.Amount = Decimal.Parse(amount, NumberStyles.AllowLeadingSign | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint);
                        }


                        t.TransactionDetails.Add(td);

                    }

                    i++;


                }
                transactions.Add(t);
            }

            Ledger l = new Ledger();
            l.Journal = transactions;
            return l;
        }

        private static void write(Ledger led)
        {
            using (var db = new LiteDatabase(@"test.db"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<Ledger>("ledgers");

                col.Insert(led);
            }

        }

        public static void parseCSV(string csv)
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

        public static void Main(string[] args)
        {
            //string testfile = @"ledger.dat";

            //Ledger ledger = parseLedger(testfile);
            //ledger.PrintRegister();
            //ledger.PrintBalance();

            string file1 = @"C:\src\finance\dat\20160712-0811.csv";

            // CsvParserOptions csvParserOptions = new CsvParserOptions(true, new[] { ',' });

            // ImportModelMapping csvMapper = new ImportModelMapping();
            // CsvParser<ImportModel> csvParser = new CsvParser<ImportModel>(csvParserOptions, csvMapper);

            // var result = csvParser
            //     .ReadFromFile(file1, Encoding.ASCII)
            //     .ToList();


            parseCSV(file1);
            Console.Write("done");


        }
    }
}
