using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace HyperVectorDB {
    public class HyperVectorDBIndex {
        public readonly string Name;
        private List<double[]> vectors;
        private List<HVDBDocument> documents;
        private readonly Dictionary<double[], HVDBQueryResult> queryCacheCosineSimilarity;

        public HyperVectorDBIndex(string name) {
            this.vectors = new List<double[]>();
            this.documents = new List<HVDBDocument>();
            this.queryCacheCosineSimilarity = new Dictionary<double[], HVDBQueryResult>();
            Name = name;
        }
        public void Save(string path) {
            //TODO: This could be done better
            var savepath = Path.Combine(path, Name);
            if (!Directory.Exists(savepath)) {
                Directory.CreateDirectory(savepath);
            }
            var js = new JsonSerializer();
            using (var sw = new StreamWriter(Path.Combine(savepath, "vectors.json"),false)) {
                using var jw = new JsonTextWriter(sw);
                js.Serialize(jw, vectors);
            }
            using (var sw = new StreamWriter(Path.Combine(savepath, "documents.json"),false)) {
                using var jw = new JsonTextWriter(sw);
                js.Serialize(jw, documents);
            }

        }
        public void Load(string path) {
            //TODO: This could be done better
            var loadpath = Path.Combine(path, Name);
            if (!Directory.Exists(loadpath)) {
                throw new DirectoryNotFoundException($"Directory {loadpath} not found.");
            }
            var js = new JsonSerializer();
            using (var sr = new StreamReader(Path.Combine(loadpath, "vectors.json"))) {
                using var jr = new JsonTextReader(sr);
                vectors = js.Deserialize<List<double[]>>(jr);
            }
            using (var sr = new StreamReader(Path.Combine(loadpath, "documents.json"))) {
                using var jr = new JsonTextReader(sr);
                documents = js.Deserialize<List<HVDBDocument>>(jr);
            }
        }
        public void Add(double[] vector, HVDBDocument doc) {
            if (vector == null) {
                throw new ArgumentNullException(nameof(vector));
            }
            if (doc == null) {
                throw new ArgumentNullException(nameof(doc));
            }
            if (vector.Length == 0) {
                throw new ArgumentException("Vector length cannot be zero.", nameof(vector));
            }
            vectors.Add(vector);
            documents.Add(doc);
            ResetCaches();
        }
        public void Remove(HVDBDocument doc) {
            if (doc == null) {
                throw new ArgumentNullException(nameof(doc));
            }
            int index = documents.IndexOf(doc);
            if (index == -1) {
                throw new ArgumentException("Document not found.", nameof(doc));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void Remove(double[] vector) {
            if (vector == null) {
                throw new ArgumentNullException(nameof(vector));
            }
            int index = vectors.IndexOf(vector);
            if (index == -1) {
                throw new ArgumentException("Vector not found.", nameof(vector));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void RemoveAt(int index) {
            if (index < 0 || index >= documents.Count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void Clear() {
            vectors.Clear();
            documents.Clear();
            ResetCaches();
        }
        public void ResetCaches() {
            queryCacheCosineSimilarity.Clear();
        }
        private HVDBQueryResult? TryHitCacheCosineSimilarity(double[] queryVector, int topK = 5) {
            if (queryCacheCosineSimilarity.TryGetValue(queryVector, out HVDBQueryResult? result) && result.Documents.Count >= topK) {
                if (result is null) return null; //Sanity check
                if (result.Documents.Count > topK) {
                    result.Documents = result.Documents.Take(topK).ToList();
                    result.Distances = result.Distances.Take(topK).ToList();
                }
                return result;
            }
            return null;
        }
        public HVDBQueryResult QueryCosineSimilarity(double[] queryVector, int topK = 5) {
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            // first check _tryCache
            HVDBQueryResult? cachedResult = TryHitCacheCosineSimilarity(queryVector, topK);
            if (cachedResult != null) {
                return cachedResult;
            }
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                //double similarity = 1 - Distance.Cosine(queryVector, vectors[i]);

                double similarity = 1 - Math.CosineSimilarity(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();

            return new HVDBQueryResult(
                orderedData.Select(pair => pair.Key).ToList(),
                orderedData.Select(pair => pair.Value).ToList()
            );
        }

        public HVDBQueryResult QueryJaccardDissimilarity(double[] queryVector, int topK = 5) {
            //TODO: THIS NEEDS TESTING
            //TODO: This needs caching
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                double similarity = 1 - Math.JaccardDissimilarity(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();
            return new HVDBQueryResult(
                    orderedData.Select(pair => pair.Key).ToList(),
                    orderedData.Select(pair => pair.Value).ToList()
              );
        }

        public HVDBQueryResult QueryEuclideanDistance(double[] queryVector, int topK = 5) {
            //TODO: THIS NEEDS TESTING
            //TODO: This needs caching
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                double similarity = 1 - Math.EuclideanDistance(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();
            return new HVDBQueryResult(
                    orderedData.Select(pair => pair.Key).ToList(),
                    orderedData.Select(pair => pair.Value).ToList()
                );
        }

        public HVDBQueryResult QueryManhattanDistance(double[] queryVector, int topK = 5) {
            //TODO: THIS NEEDS TESTING
            //TODO: This needs caching
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                double similarity = 1 - Math.ManhattanDistance(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();
            return new HVDBQueryResult(
                    orderedData.Select(pair => pair.Key).ToList(),
                    orderedData.Select(pair => pair.Value).ToList()
                );

        }

        public HVDBQueryResult QueryChebyshevDistance(double[] queryVector, int topK = 5) {
            //TODO: THIS NEEDS TESTING
            //TODO: This needs caching
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                double similarity = 1 - Math.ChebyshevDistance(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();
            return new HVDBQueryResult(
                    orderedData.Select(pair => pair.Key).ToList(),
                    orderedData.Select(pair => pair.Value).ToList()
              );
        }

        public HVDBQueryResult QueryCanberraDistance(double[] queryVector, int topK = 5) {
            //TODO: THIS NEEDS TESTING
            //TODO: This needs caching
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            var similarities = new ConcurrentBag<KeyValuePair<HVDBDocument, double>>();
            Parallel.For(0, vectors.Count, i =>
            {
                double similarity = 1 - Math.CanberraDistance(queryVector, vectors[i]);
                similarities.Add(new KeyValuePair<HVDBDocument, double>(documents[i], similarity));
            });
            var orderedData = similarities
                .OrderByDescending(pair => pair.Value)
                .Take(topK)
                .ToList();
            return new HVDBQueryResult(
                    orderedData.Select(pair => pair.Key).ToList(),
                    orderedData.Select(pair => pair.Value).ToList()
                );

        }

    }
}
