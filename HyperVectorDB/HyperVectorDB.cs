using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyperVectorDB.Embedder;

namespace HyperVectorDB {

    public class HyperVectorDB {

        public readonly string DatabasePath;
        public readonly Dictionary<string, HyperVectorDBIndex> Indexs;

        private readonly IEmbedder _embedder;


        public HyperVectorDB(IEmbedder embedder, string path) {
            _embedder = embedder;
            Indexs = new Dictionary<string, HyperVectorDBIndex>();
            DatabasePath = path;
        }

        public bool CreateIndex(string name) {
            if (Indexs.ContainsKey(name)) {
                return false;
            }
            var index = new HyperVectorDBIndex(name);
            Indexs.Add(name, index);
            return true;
        }

        public bool DeleteIndex(string name) {
            if (!Indexs.ContainsKey(name)) {
                return false;
            }
            Indexs.Remove(name);
            return true;
        }

        public bool IndexDocument(string indexName, string document) {
            if (!Indexs.ContainsKey(indexName)) return false;
            var index = Indexs[indexName];
            var vector = _embedder.GetVector(document);
            var doc = new HVDBDocument(document);
            index.Add(vector, doc);
            return true;
        }
        public void Save() {
            if (!Directory.Exists(DatabasePath)) Directory.CreateDirectory(DatabasePath);
            var indexfile = System.IO.Path.Combine(DatabasePath, "indexs.txt");
            var sw = new System.IO.StreamWriter(indexfile, false);
            foreach (var index in Indexs) {
                sw.WriteLine(index.Value.Name);
                index.Value.Save(DatabasePath);
            }
            sw.Close();
        }

        public void Load() {
            var indexfile = System.IO.Path.Combine(DatabasePath, "indexs.txt");
            var sr = new System.IO.StreamReader(indexfile);
            while (!sr.EndOfStream) {
                var line = sr.ReadLine();
                if (line is null) continue;
                var index = new HyperVectorDBIndex(line);
                index.Load(DatabasePath);
                Indexs.Add(index.Name, index);
            }
        }

        public HVDBQueryResult QueryCosineSimilarity(string query, int topK = 5) {
            var vector = _embedder.GetVector(query);
            var results = new List<HVDBQueryResult>();
            Parallel.ForEach(Indexs, index =>
            {
                var result = index.Value.QueryCosineSimilarity(vector, topK);
                results.Add(result);
            });
            var docs = new List<HVDBDocument>();
            var distances = new List<double>();
            foreach (var result in results) {
                docs.AddRange(result.Documents);
                distances.AddRange(result.Distances);
            }
            var sorted = distances.Select((x, i) => new KeyValuePair<double, HVDBDocument>(x, docs[i])).OrderByDescending(x => x.Key).ToList().Take(topK);
            var newresult = new HVDBQueryResult(sorted.Select(x => x.Value).ToList(), sorted.Select(x => x.Key).ToList());
            return newresult;
        }
    }
}
