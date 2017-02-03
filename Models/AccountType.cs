
using LiteDB;

namespace LedgerCore.Models
{
    public class AccountType
    {
        public int AccountTypeId { get; set; }
        [BsonIndex]
        public string Name { get; set; }

    }
}