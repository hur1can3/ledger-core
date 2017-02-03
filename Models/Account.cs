
using LiteDB;

namespace LedgerCore.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        [BsonIndex]
        public string Name { get; set; }
        public AccountType AccountType { get; set; }

    }
}