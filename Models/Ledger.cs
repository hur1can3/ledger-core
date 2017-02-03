using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace LedgerCore.Models
{
    public class Ledger
    {

        public int LedgerId { get; set; }

        public string LedgerName { get; set; }

        public List<Transaction> Journal { get; set; }

        public decimal GetAccountBalance(Account account)
        {
            var z = Journal
                .SelectMany(t => t.TransactionDetails)
                .Where(t => t.Accounts.Contains(account))
                .Sum(t => t.Amount);

            return z;
        }

        public decimal GetBalanceForPayee(string payee)
        {
            var z = Journal
                .Where(j => j.PayeeName.Contains(payee))
                .SelectMany(t => t.TransactionDetails)
                .Sum(t => t.Amount);

            return z;
        }

        public decimal GetBalanceForAccount(string account)
        {
            var z = Journal
                .SelectMany(t => t.TransactionDetails)
                .Where(t => t.Accounts.Any(a => a.Name.Contains(account)))
                .Sum(t => t.Amount);

            return z;
        }

        public decimal GetLedgerBalance()
        {
            var z = Journal
                .SelectMany(t => t.TransactionDetails)
                .Sum(t => t.Amount);

            return z;
        }

        public void PrintRegister()
        {
            foreach (var t in Journal)
            {
                Console.WriteLine(t);
            }
            Console.WriteLine(this.ToString());

        }



        public void PrintBalance()
        {
            var acts = Journal.SelectMany(t => t.TransactionDetails).SelectMany(a => a.Accounts.First().Name).ToList();

            foreach(var act in acts) {
                
            }
        }

        public override string ToString()
        {
            decimal totalIncome = GetBalanceForAccount("Income") * -1;
            decimal totalExpenses = GetBalanceForAccount("Expense") * -1;
            decimal total = totalIncome + totalExpenses;
            decimal checking = GetBalanceForAccount("Assets") * -1;
            return $"{LedgerName} - {Journal.Count} Transactions - OpeningBalance: {GetLedgerBalance()}\n" +
                   $"Income: {totalIncome:c} Expenses: {totalExpenses:c} Assets: {checking:c}  Balance: {total:c}";
        }
    }
}