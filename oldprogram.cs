// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using LedgerCore.Data;
// using LedgerCore.Models;
// using LiteDB;
// using Sprache;

// namespace LedgerCore
// {
//     public class Program
//     {


//         private static void seed()
//         {
//             using (var db = new LiteDatabase(@"test.db"))
//             {
//                 // Get a collection (or create, if doesn't exist)
//                 var col = db.GetCollection<Ledger>("ledgers");

//                 Ledger l = new Ledger();
//                 l.Journal = new List<Transaction>();

//                 Transaction t = new Transaction();
//                 t.TransactionDetails = new List<TransactionDetail>();
//                 Account a = new Account();
//                 a.AccountType = new AccountType() { Name = "Assets" };
//                 a.Name = "Huntington:Checking";
//                 Account a2 = new Account();
//                 a2.AccountType = new AccountType() { Name = "Equity" };
//                 a2.Name = "OpeningBalances";

//                 TransactionDetail td = new TransactionDetail();
//                 td.Accounts = new List<Account>();
//                 td.Accounts.Add(a);
//                 td.Amount = -2.85M;
//                 TransactionDetail td2 = new TransactionDetail();
//                 td2.Accounts = new List<Account>();
//                 td2.Accounts.Add(a2);
//                 td2.Amount = 2.85M;

//                 t.TransactionDetails.Add(td);
//                 t.TransactionDetails.Add(td2);
//                 l.Journal.Add(t);

//                 // Insert new customer document (Id will be auto-incremented)
//                 col.Insert(l);
//             }
//         }

//         public static void read()
//         {
//             var context = new LiteDBContext<Ledger>("test.db", "ledgers");

//             var data = context.db.FindAll();

//             foreach (var l in data)
//             {
//                 Console.WriteLine(l);
//             }


//         }

//         public static void Main(string[] args)
//         {

//             string testfile = @"/home/hur1can3/src/finance/ledger.dat";
//             var lines = System.IO.File.ReadLines(testfile);

//             int skip = 0;
//             bool notdone = true;
//             while (notdone)
//             {

//                 var newLines = lines.Skip(skip).Take(3);
//                 Transaction t = new Transaction();
//                 t.TransactionDetails = new List<TransactionDetail>();
//                 int i = 0;
//                 foreach (var l in newLines)
//                 {
                    

//                     if (i == 0)
//                     {
//                         Parser<string> dateid =
//                                                          from year in Parse.Digit.Many()
//                                                          from sep in Parse.Char('/').Once()
//                                                          from month in Parse.Digit.Many()
//                                                          from sep2 in Parse.Char('/').Once()
//                                                          from day in Parse.Digit.Many()
//                                                          select new string(year.Concat(sep).Concat(month).Concat(sep2).Concat(day).ToArray());
//                         var date = dateid.Parse(l);
                      
//                         t.Date = DateTime.Parse(date);

//                         Parser<string> payeeid =
//                                             from first in dateid
//                                             from space in Parse.WhiteSpace.Many()
//                                             from payee in Parse.AnyChar.Many().Text()
//                                             select new string(payee.ToArray());

//                         t.PayeeName = payeeid.Parse(l);
//                     } else {
//                         TransactionDetail td = new TransactionDetail();
//                         Parser<string> accountid =    from sp in Parse.WhiteSpace.Many()
//                                                          from year in Parse.Letter.Many()
//                                                          select new string(year.ToArray());
                       
//                         td.Accounts = new List<Account>();
//                         Account a = new Account();
//                         a.Name = accountid.Parse(l);

//                         Parser<string> amountid = from ac in accountid
//                                                    from sp in Parse.WhiteSpace.Many()
//                                                    from dollar in Parse.Digit.Many()
//                                                    from dec in Parse.Char('.').Once()
//                                                    from cents in Parse.Digit.Many()
//                                                    select new string(dollar.Concat(dec).Concat(cents).ToArray());
//                         decimal amount = Decimal.Parse(amountid.Parse(l));
//                         td.Amount = amount;

//                         t.TransactionDetails.Add(td);

//                     }
                    
//                     i++;
                   

//                     skip = skip + 3;
//                 }
//                 notdone = false;
//             }

//             //seed();



//             Console.WriteLine("Hello World!");
//         }
//     }
// }
