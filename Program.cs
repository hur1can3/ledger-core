using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LedgerCore.Data;
using LedgerCore.Models;
using LiteDB;
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

        private static void write(Ledger led) {
                        using (var db = new LiteDatabase(@"test.db"))
            {
                // Get a collection (or create, if doesn't exist)
                var col = db.GetCollection<Ledger>("ledgers");

                col.Insert(led);
            }

        }

        public static void Main(string[] args)
        {
            string testfile = @"/home/hur1can3/src/finance/ledger.dat";

            Ledger ledger = parseLedger(testfile);
            //ledger.PrintRegister();
            ledger.PrintBalance();
        }
    }
}
