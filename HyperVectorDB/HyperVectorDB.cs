using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HyperVectorDB.Embedder;

namespace HyperVectorDB
{

    public class HyperVectorDB
    {

        public readonly string DatabasePath;

        public delegate string? DocumentPreprocessor(string line, string? path = null, int? lineNumber = null);

        public delegate string? DocumentPostprocessor(string line, string? path = null, int? lineNumber = null);

        public readonly Dictionary<string, HyperVectorDBIndex> Indexes;

        private readonly IEmbedder _embedder;

        private ulong AutoIndexCount = 0;


        public HyperVectorDB(IEmbedder embedder, string path, int autoIndexCount = 0)
        {
            _embedder = embedder;
            Indexes = new Dictionary<string, HyperVectorDBIndex>();
            DatabasePath = path;

            if (autoIndexCount > 0)
            {
                AutoIndexCount = (ulong)autoIndexCount;
                for (ulong i = 0; i < AutoIndexCount; i++)
                {
                    CreateIndex($"AutoIndex_{i}");
                }
            }
            else
            {
                CreateIndex("Default");
            }
        }

        public bool CreateIndex(string name)
        {
            if (Indexes.ContainsKey(name))
            {
                return false;
            }
            var index = new HyperVectorDBIndex(name);
            Indexes.Add(name, index);
            return true;
        }

        public bool DeleteIndex(string name)
        {
            if (!Indexes.ContainsKey(name))
            {
                return false;
            }
            Indexes.Remove(name);
            return true;
        }

        public bool IndexDocument(string indexName, string document, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null)
        {
            //Passthrough overload to avoid breaking changes to API
            return IndexDocument(document, preprocessor, postprocessor, indexName);
        }

        public bool IndexDocument(string document, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null, string? indexName = null)
        {
            string IndexName;
            if (indexName != null)
            {
                if (!Indexes.ContainsKey(indexName)) return false;
                IndexName = indexName;
            }
            else if (indexName == null && AutoIndexCount == 0)
            {
                IndexName = Indexes.First().Key;
            }
            else
            {
                ulong hash = StringHash(document);
                ulong bucket = hash % AutoIndexCount;
                IndexName = $"AutoIndex_{bucket}";
            }
            var index = Indexes[IndexName];


            string? line = document;
            if (preprocessor != null)
            {
                line = preprocessor(document);
                if (line == null) { return false; }
            }



            if (postprocessor != null)
            {
                string? postDoc = postprocessor(document);
                if (postDoc == null) { return false; }
                var doc = new HVDBDocument(postDoc);
                var vector = _embedder.GetVector(line);
                index.Add(vector, doc);
                return true;
            }
            else
            {
                var doc = new HVDBDocument(line);
                var vector = _embedder.GetVector(line);
                index.Add(vector, doc);
                return true;
            }
        }

        public bool IndexDocumentFile(string indexName, string documentPath, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null)
        {
            return IndexDocumentFile(documentPath, preprocessor, postprocessor, indexName);
        }

        public bool IndexDocumentFile(string documentPath, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null, string? indexName = null)
        {
            if (!System.IO.File.Exists(documentPath)) return false;

            string[] lines = System.IO.File.ReadAllLines(documentPath);
            for (int i = 0; i < lines.Length; i++)
            {
                string IndexName;
                if (indexName != null)
                {
                    if (!Indexes.ContainsKey(indexName)) return false;
                    IndexName = indexName;

                }
                else if (indexName == null && AutoIndexCount == 0)
                {
                    IndexName = Indexes.First().Key;
                }
                else
                {
                    ulong hash = StringHash(lines[i]);
                    ulong bucket = hash % AutoIndexCount;
                    IndexName = $"AutoIndex_{bucket}";
                }

                var index = Indexes[IndexName];

                string? line = lines[i];

                if (preprocessor != null)
                {
                    line = preprocessor(lines[i], documentPath, i);
                    if (line == null) { continue; }
                }

                if (postprocessor != null)
                {
                    string? postDoc = postprocessor(lines[i], documentPath, i);
                    if (postDoc == null) { return false; }
                    var doc = new HVDBDocument(postDoc);
                    var vector = _embedder.GetVector(line);
                    index.Add(vector, doc);
                }
                else
                {
                    var doc = new HVDBDocument(line);
                    var vector = _embedder.GetVector(line);
                    index.Add(vector, doc);
                }
            }

            return true;
        }

        public void Save()
        {
            if (!Directory.Exists(DatabasePath)) Directory.CreateDirectory(DatabasePath);
            var indexfile = System.IO.Path.Combine(DatabasePath, "indexs.txt");
            var sw = new System.IO.StreamWriter(indexfile, false);
            foreach (var index in Indexes)
            {
                sw.WriteLine(index.Value.Name);
                index.Value.Save(DatabasePath);
            }
            sw.Close();

            Console.WriteLine("Index Usage:");
            foreach(string key in Indexes.Keys)
            {
                Console.Write($"{Indexes[key].Count,4}");
            }
            Console.WriteLine();
        }

        public void Load()
        {
            var indexfile = System.IO.Path.Combine(DatabasePath, "indexs.txt");
            var sr = new System.IO.StreamReader(indexfile);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line is null) continue;
                var index = new HyperVectorDBIndex(line);
                index.Load(DatabasePath);
                if (!Indexes.ContainsKey(index.Name))
                {
                    Indexes.Add(index.Name, index);
                }
                else
                {
                    Indexes[index.Name] = index;
                }
            }
            sr.Close();
        }

        public HVDBQueryResult QueryCosineSimilarity(string query, int topK = 5)
        {
            var vector = _embedder.GetVector(query);
            var results = new List<HVDBQueryResult>();
            Parallel.ForEach(Indexes, index =>
            {
                var result = index.Value.QueryCosineSimilarity(vector, topK);
                results.Add(result);
            });
            var docs = new List<HVDBDocument>();
            var distances = new List<double>();
            foreach (var result in results)
            {
                docs.AddRange(result.Documents);
                distances.AddRange(result.Distances);
            }
            var sorted = distances.Select((x, i) => new KeyValuePair<double, HVDBDocument>(x, docs[i])).OrderByDescending(x => x.Key).ToList().Take(topK);
            var newresult = new HVDBQueryResult(sorted.Select(x => x.Value).ToList(), sorted.Select(x => x.Key).ToList());
            return newresult;
        }



        private static UInt64 StringHash(string text)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < text.Length; i++)
            {
                hashedValue = (hashedValue + text[i]) * 3074457345618258799ul;
            }
            return hashedValue;
        }

    }
}
