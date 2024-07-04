using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MessagePack;


namespace HyperVectorDB
{
    public class HyperVectorDBIndex
    {
        public readonly string Name;

        public int Count
        {
            get { return documents.Count; }
        }
        private List<double[]> vectors;
        private List<HVDBDocument> documents;
        private readonly Dictionary<double[], HVDBQueryResult> queryCacheCosineSimilarity;

        private bool fileValid = false;

        private MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard
            .WithSecurity(MessagePackSecurity.UntrustedData)
            .WithCompression(MessagePackCompression.Lz4BlockArray);

        public HyperVectorDBIndex(string name)
        {
            this.vectors = new List<double[]>();
            this.documents = new List<HVDBDocument>();
            this.queryCacheCosineSimilarity = new Dictionary<double[], HVDBQueryResult>();
            Name = name;
        }
        public async void Save(string path)
        {
            if (fileValid) { return; }
            var savepath = Path.Combine(path, Name);
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

            byte[] vectorsBytes = MessagePackSerializer.Serialize(vectors);
            var vectorsToken = System.IO.File.WriteAllBytesAsync(Path.Combine(savepath, "vectors.bin"), vectorsBytes);

            byte[] documentsBytes = MessagePackSerializer.Serialize(documents);
            var documentsToken = System.IO.File.WriteAllBytesAsync(Path.Combine(savepath, "documents.bin"), documentsBytes);
            await vectorsToken;
            await documentsToken;

            fileValid = true;
        }
        public async void Load(string path)
        {
            var loadpath = Path.Combine(path, Name);
            if (!Directory.Exists(loadpath))
            {
                throw new DirectoryNotFoundException($"Directory {loadpath} not found.");
            }

            var vectorsToken = System.IO.File.ReadAllBytesAsync(Path.Combine(loadpath, "vectors.bin"));
            var documentsToken = System.IO.File.ReadAllBytesAsync(Path.Combine(loadpath, "documents.bin"));

            await vectorsToken;
            vectors = MessagePackSerializer.Deserialize<List<double[]>>(vectorsToken.Result, options);

            await documentsToken;
            documents = MessagePackSerializer.Deserialize<List<HVDBDocument>>(documentsToken.Result, options);

            fileValid = true;
        }
        public void Add(double[] vector, HVDBDocument doc)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector));
            }
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }
            if (vector.Length == 0)
            {
                throw new ArgumentException("Vector length cannot be zero.", nameof(vector));
            }
            vectors.Add(vector);
            documents.Add(doc);
            ResetCaches();
        }
        public void Remove(HVDBDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(nameof(doc));
            }
            int index = documents.IndexOf(doc);
            if (index == -1)
            {
                throw new ArgumentException("Document not found.", nameof(doc));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void Remove(double[] vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException(nameof(vector));
            }
            int index = vectors.IndexOf(vector);
            if (index == -1)
            {
                throw new ArgumentException("Vector not found.", nameof(vector));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= documents.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            vectors.RemoveAt(index);
            documents.RemoveAt(index);
            ResetCaches();
        }
        public void Clear()
        {
            vectors.Clear();
            documents.Clear();
            ResetCaches();
        }
        public void ResetCaches()
        {
            queryCacheCosineSimilarity.Clear();
            fileValid = false;
        }
        private HVDBQueryResult? TryHitCacheCosineSimilarity(double[] queryVector, int topK = 5)
        {
            if (queryCacheCosineSimilarity.TryGetValue(queryVector, out HVDBQueryResult? result) && result.Documents.Count >= topK)
            {
                if (result is null) return null; //Sanity check
                if (result.Documents.Count > topK)
                {
                    result.Documents = result.Documents.Take(topK).ToList();
                    result.Distances = result.Distances.Take(topK).ToList();
                }
                return result;
            }
            return null;
        }
        public HVDBQueryResult QueryCosineSimilarity(double[] queryVector, int topK = 5)
        {
            if (queryVector == null) throw new ArgumentNullException(nameof(queryVector));
            if (topK <= 0) throw new ArgumentException("Number of results requested (k) must be greater than zero.", nameof(topK));
            // first check _tryCache
            HVDBQueryResult? cachedResult = TryHitCacheCosineSimilarity(queryVector, topK);
            if (cachedResult != null)
            {
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

        public HVDBQueryResult QueryJaccardDissimilarity(double[] queryVector, int topK = 5)
        {
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

        public HVDBQueryResult QueryEuclideanDistance(double[] queryVector, int topK = 5)
        {
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

        public HVDBQueryResult QueryManhattanDistance(double[] queryVector, int topK = 5)
        {
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

        public HVDBQueryResult QueryChebyshevDistance(double[] queryVector, int topK = 5)
        {
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

        public HVDBQueryResult QueryCanberraDistance(double[] queryVector, int topK = 5)
        {
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
