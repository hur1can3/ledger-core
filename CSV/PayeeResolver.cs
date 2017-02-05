using System;
using System.Linq;
using System.Collections.Generic;
using LedgerCore.DocumentResolver;

namespace ledger_core.CSV
{
    public class PayeeResolver
    {

        private static Dictionary<string, string> LoadData(string filename)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();

            string[] documents = System.IO.File.ReadAllLines(filename);
            for (int i = 1; i < documents.Length; i++)
            {
                string[] docInfo = documents[i].Split('\t');
                string content = string.Format("{0} {1} {2}", docInfo[1], docInfo[2], docInfo[3].Replace(" | ", " "));
                data.Add(docInfo[0], content);
            }

            return data;
        }
        private static void ResolveList(bool useWordStemmer, string outputfile, string dictionaryfile, string inputfile)
        {
            // Initalize instances of all of the resolvers that will be tested
            DocumentResolver documentResolver = new DocumentResolver();
            BayesResolverEngine bayesResolver = new BayesResolverEngine();
            LevenshteinResolverEngine levenshteinResolver = new LevenshteinResolverEngine();
            TFIDFResolverEngine tfidfResolver = new TFIDFResolverEngine();

            // Load the dictionary into all of the resolvers
            Dictionary<string, string> dictionary = LoadData(dictionaryfile);
            documentResolver.SetDictionary(dictionary);
            bayesResolver.Dictionary = dictionary;
            levenshteinResolver.Dictionary = dictionary;
            tfidfResolver.Dictionary = dictionary;

            // Load the data to be resolved
            Dictionary<string, string> documents = LoadData(inputfile);

            // Process all 100 documents with each resolver, recording the time 
            // each resolver takes to complete the task.
            System.IO.File.AppendAllText(outputfile,
                string.Format("Processing {0} documents against a dictionary with {1} entries.\r\n\r\n",
                documents.Count(),
                dictionary.Count()));

            documentResolver.SetEngine(DocumentResolver.EngineType.BayesTFIDF);
            DateTime startTime = DateTime.Now;
            foreach (KeyValuePair<string, string> document in documents)
            {
                documentResolver.Resolve(document.Value, useWordStemmer);
            }
            DateTime endTime = DateTime.Now;
            System.IO.File.AppendAllText(outputfile, string.Format("Bayes/TFIDF processing complete.  {0} seconds elapsed.\r\n", (endTime - startTime).TotalSeconds.ToString()));

            documentResolver.SetEngine(DocumentResolver.EngineType.BayesLevenshtein);
            startTime = DateTime.Now;
            foreach (KeyValuePair<string, string> document in documents)
            {
                documentResolver.Resolve(document.Value, useWordStemmer);
            }
            endTime = DateTime.Now;
            System.IO.File.AppendAllText(outputfile, string.Format("Bayes/Levenshtein processing complete.  {0} seconds elapsed.\r\n", (endTime - startTime).TotalSeconds.ToString()));

            startTime = DateTime.Now;
            foreach (KeyValuePair<string, string> document in documents)
            {
                bayesResolver.Resolve(document.Value, useWordStemmer);
            }
            endTime = DateTime.Now;
            System.IO.File.AppendAllText(outputfile, string.Format("Bayes processing complete.  {0} seconds elapsed.\r\n", (endTime - startTime).TotalSeconds.ToString()));

            startTime = DateTime.Now;
            foreach (KeyValuePair<string, string> document in documents)
            {
                levenshteinResolver.Resolve(document.Value, useWordStemmer);
            }
            endTime = DateTime.Now;
            System.IO.File.AppendAllText(outputfile, string.Format("Levenshtein processing complete.  {0} seconds elapsed.\r\n", (endTime - startTime).TotalSeconds.ToString()));

            startTime = DateTime.Now;
            foreach (KeyValuePair<string, string> document in documents)
            {
                tfidfResolver.Resolve(document.Value, useWordStemmer);
            }
            endTime = DateTime.Now;
            System.IO.File.AppendAllText(outputfile, string.Format("TFIDF processing complete.  {0} seconds elapsed.\r\n\r\n\r\n", (endTime - startTime).TotalSeconds.ToString()));
        }
    }
}