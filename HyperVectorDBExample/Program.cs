using OpenAI.Files;
using System.Diagnostics;

namespace HyperVectorDBExample
{
    internal class Program
    {
        public static HyperVectorDB.HyperVectorDB? DB;


        // Example of a custom preprocessor delegate, used here to filter Markdown
        static bool skippingBlock = false;
        private static string? CustomPreprocessor(string line, string? path)
        {
            if (string.IsNullOrWhiteSpace(line)) { return null; }

            if (path != null && path.ToUpperInvariant().EndsWith(".MD"))
            {
                if (line.Contains("---"))// Skip YAML frontmatter
                {
                    skippingBlock = !skippingBlock;
                    return null;
                }

                if (line.Contains("```"))// Skip code blocks
                {
                    skippingBlock = !skippingBlock;
                    return null;
                }

                if (line.Contains("%%")) { return null; }//Skip annotation lines

                if (line.Trim().StartsWith("[[") && line.Trim().EndsWith("]]")) { return null; }//Skip index links

                if (skippingBlock) { return null; }

            }

            return line.Trim();
        }

        static void Main()
        {
            DB = new HyperVectorDB.HyperVectorDB(new HyperVectorDB.Embedder.LmStudio(), "TestDatabase");
            if (Directory.Exists("TestDatabase"))
            {
                Console.WriteLine("Loading database");
                DB.Load();
            }
            else
            {
                Console.WriteLine("Creating database");
                DB.CreateIndex("TestIndex");
                DB.IndexDocument("TestIndex", "This is a test document about dogs");
                DB.IndexDocument("TestIndex", "This is a test document about cats");
                DB.IndexDocument("TestIndex", "This is a test document about fish");
                DB.IndexDocument("TestIndex", "This is a test document about birds");
                DB.IndexDocument("TestIndex", "This is a test document about dogs and cats");
                DB.IndexDocument("TestIndex", "This is a test document about cats and fish");
                DB.IndexDocument("TestIndex", "This is a test document about fish and birds");
                DB.IndexDocument("TestIndex", "This is a test document about birds and dogs");
                DB.IndexDocument("TestIndex", "This is a test document about dogs and cats and fish");
                DB.IndexDocument("TestIndex", "This is a test document about cats and fish and birds");
                DB.IndexDocument("TestIndex", "This is a test document about fish and birds and dogs");
                DB.IndexDocument("TestIndex", "This is a test document about birds and dogs and cats");
                DB.IndexDocument("TestIndex", "This is a test document about dogs and cats and fish and birds");
                DB.IndexDocument("TestIndex", "This is a test document about cats and fish and birds and dogs");
                DB.IndexDocument("TestIndex", "This is a test document about fish and birds and dogs and cats");
                DB.IndexDocument("TestIndex", "This is a test document about birds and dogs and cats and fish");
                
                string[] files = Directory.GetFiles(@".\TestDocuments", "*.*", SearchOption.AllDirectories);
                Console.WriteLine($"Indexing {files.Length} files.");
                foreach (string file in files)
                {
                    Console.WriteLine(file);
                    DB.IndexDocumentFile("TestIndex", file, CustomPreprocessor);
                }



                DB.Save();
            }
            while (true)
            {
                Console.WriteLine("Enter a search term:");
                var searchterm = Console.ReadLine();
                if (searchterm == "exit") break;
                if (searchterm is null) break;
                if (searchterm is "") continue;
                var sw = new Stopwatch(); sw.Start();
                var result = DB.QueryCosineSimilarity(searchterm, 10);
                sw.Stop();
                Console.WriteLine("Results:");
                for (var i = 0; i < result.Documents.Count; i++) Console.WriteLine(result.Documents[i].DocumentString + " " + result.Distances[i]);
                Console.WriteLine("Time taken: " + sw.ElapsedMilliseconds + "ms");
            }
            Console.WriteLine("Done, press enter to exit");
            Console.ReadLine();
        }
    }
}