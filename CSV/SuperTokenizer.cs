using System;
using System.Collections.Generic;
using System.Linq;
using Superpower;
using TinyCsvParser.Tokenizer;

namespace ledger_core.CSV
{
    public class SuperTokenizer : ITokenizer
    {
        public class ColumnDefinition
        {
            public readonly TextParser<string> Parser;


            public ColumnDefinition(TextParser<string> parser)
            {
                Parser = parser;
            }

            public override string ToString()
            {
                return string.Format("ColumnDefinition (Parse = {0})", Parser);
            }
        }

        public readonly ColumnDefinition[] Columns;

        public SuperTokenizer(ColumnDefinition[] columns)
        {
            Columns = columns;
        }

        public SuperTokenizer(IList<ColumnDefinition> columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException("columns");
            }
            Columns = columns.ToArray();
        }


        public string[] Tokenize(string input)
        {
            string[] tokenizedLine = new string[Columns.Length];

            TextParser<string> lastParser = null;
            for (int columnIndex = 0; columnIndex < Columns.Length; columnIndex++)
            {
                ColumnDefinition columnDefinition = Columns[columnIndex];


                if (columnIndex > 0)
                {
                    lastParser = from top in lastParser
                                 from parser in columnDefinition.Parser
                                 select parser;
                }
                else
                {
                    lastParser = columnDefinition.Parser;
                }

                var columnData = lastParser.Parse(input);

                tokenizedLine[columnIndex] = columnData;
            }

            return tokenizedLine;
        }
    }
}