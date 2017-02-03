using LiteDB;

namespace LedgerCore.Data
{
    public class LiteDBContext<T>
    {
        private LiteDatabase _context { get; set; }

        public LiteCollection<T> db {get;set;}

        public LiteDBContext(string connstring, string tablename) {
            this._context = new LiteDatabase(connstring);
            this.db = _context.GetCollection<T>(tablename);
        }   
    }
}