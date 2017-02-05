using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LedgerCore.Models
{
    public class Transaction
    {

        public int TransactionId { get; set; }

        public DateTime Date { get; set; }

        public string RefId { get; set; }

        public string PayeeName { get; set; }

        public string Memo { get; set; }
        public List<TransactionDetail> TransactionDetails { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string shortdate = this.Date.ToString("d");
            sb.AppendLine($"{shortdate,-10} {PayeeName}");
            foreach (var td in TransactionDetails)
            {
                string accounts = string.Join(":", td.Accounts.Select(a => a.Name));
                decimal amount = td.Amount;
                sb.AppendLine(String.Format($"{accounts,-50}{amount,12}"));
            }
            return sb.ToString();
        }
    }
}