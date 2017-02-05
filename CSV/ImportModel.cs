using System;
using System.Collections.Generic;
using TinyCsvParser.Mapping;

namespace LedgerCore
{
    public class ImportModel
    {
        public List<string> CategoryList = new List<string>() { "PURCHASE", "MERCHANDISE RET" };

        public DateTime Date { get; set; }
        public string ReferenceNumber { get; set; }
        public string PayeeName { get; set; }
        public string Memo { get; set; }
        public decimal Amount { get; set; }
        public string CategoryName { get; set; }

    }

    public class ImportModelMapping : CsvMapping<ImportModel>
    {
        public ImportModelMapping()
            : base()
        {
            MapProperty(0, x => x.Date);
            MapProperty(1, x => x.ReferenceNumber);
            MapProperty(2, x => x.PayeeName);
            MapProperty(3, x => x.Memo);
            MapProperty(4, x => x.Amount);
            MapProperty(5, x => x.CategoryName);
        }
    }


}