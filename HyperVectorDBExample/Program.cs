using OpenAI.Files;

namespace HyperVectorDBExample {
    internal class Program {
        public static HyperVectorDB.HyperVectorDB ?DB;

        static void Main() {
            var openaiapikey = "";
            var openaiapikey2 = System.Environment.GetEnvironmentVariable("openaiapikey");
            if (openaiapikey2 != null) openaiapikey = openaiapikey2;
            var filepath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "oak.txt");
            if ((openaiapikey == "") && (System.IO.File.Exists(filepath)))  openaiapikey = System.IO.File.ReadAllText(filepath).Trim();
            while(openaiapikey == "") {
                Console.WriteLine("Openapi key not found in environment variable or file,  please enter it now:");
                openaiapikey = Console.ReadLine();
            }
            if (openaiapikey is null) return;
            var currentdir = System.IO.Directory.GetCurrentDirectory();
            DB = new HyperVectorDB.HyperVectorDB(new HyperVectorDB.Embedder.EmbedderOpenAI_ADA_002(openaiapikey), "TestDatabase");
            if(Directory.Exists("TestDatabase")) {
                Console.WriteLine("Loading database");
                DB.Load();
            } else {
                Console.WriteLine("Creating database");
                DB.CreateIndex("TestIndex");
                DB.IndexDocument("TestIndex", "This is a test document");
                DB.IndexDocument("TestIndex", "This is a test file");
                DB.IndexDocument("TestIndex", "This is a test image");
                DB.Save();
            }
            while(true) {
                Console.WriteLine("Enter a search term:");
                var searchterm = Console.ReadLine();
                if (searchterm is null) break;
                var result = DB.QueryCosineSimilarity(searchterm);
                Console.WriteLine("Results:");
                for (var i = 0; i < result.Documents.Count; i++) Console.WriteLine(result.Documents[i].DocumentString + " " + result.Distances[i]);
            }
            Console.WriteLine("Done, press enter to exit");
            Console.ReadLine();
        }
    }
}