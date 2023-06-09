﻿using OpenAI.Files;
using System.Diagnostics;

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
            DB = new HyperVectorDB.HyperVectorDB(new HyperVectorDB.Embedder.EmbedderOpenAI_ADA_002(openaiapikey), "TestDatabase");
            if(Directory.Exists("TestDatabase")) {
                Console.WriteLine("Loading database");
                DB.Load();
            } else {
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
                DB.Save();
            }
            while(true) {
                Console.WriteLine("Enter a search term:");
                var searchterm = Console.ReadLine();
                if(searchterm == "exit") break;
                if (searchterm is null) break;
                if (searchterm is "") continue;
                var sw = new Stopwatch();sw.Start();
                var result = DB.QueryCosineSimilarity(searchterm);
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