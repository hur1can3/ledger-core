using System;
using System.Collections.Generic;
using LiteDB;

namespace LedgerCore.Models
{
    public class TransactionDetail 
    {

        public int TransactionDetailId { get; set; }
        public List<Account> Accounts { get; set; }

        public decimal Amount {get;set;}

    }
}