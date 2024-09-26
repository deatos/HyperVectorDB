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
    /// <summary>
    /// The top level object of the vector database system, and the center point of the API.
    /// </summary>
    public class HyperVectorDB
    {

        /// <summary>
        /// Path to the directory where the database will be stored on disk.
        /// </summary>
        public readonly string DatabasePath;

        /// <summary>
        /// Optional delegate for processing text prior to vectorization. If this delegate returns `null` the vectorization and storage will be skipped.
        /// </summary>
        /// <param name="line">Text to process</param>
        /// <param name="path">Path to file text originated from, if applicable</param>
        /// <param name="lineNumber">Line number of file text originated from, if applicable</param>
        /// <returns></returns>
        public delegate string? DocumentPreprocessor(string line, string? path = null, int? lineNumber = null);

        /// <summary>
        /// Optional delegate for processing text prior to storage in the database. If this delegate returns `null` the vectorization and storage will be skipped.
        /// </summary>
        /// <param name="line">Text to process</param>
        /// <param name="path">Path to file text originated from, if applicable</param>
        /// <param name="lineNumber">Line number of file text originated from, if applicable</param>
        /// <returns></returns>
        public delegate string? DocumentPostprocessor(string line, string? path = null, int? lineNumber = null);

		/// <summary>
		/// Provides a List of all index names in the current `HyperVectorDB`
		/// </summary>
		public List<string> IndexNames { get { return Indexes.Keys.ToList() ?? new List<string>(); } }

        /// <summary>
        /// Collection of `HyperVectorDBIndex` organized by names
        /// </summary>
        private readonly Dictionary<string, HyperVectorDBIndex> Indexes;

        private readonly IEmbedder _embedder;

        private ulong AutoIndexCount = 0;


        /// <summary>
        /// If `autoIndexCount` is ommited or less than 1, the Auto Index functionality will be disabled. See the [Auto Index documentation](xref:AutoIndexDoc) for more information.
        /// </summary>
        /// <param name="embedder">The `IEmbedder` class to provide text embedding functionality</param>
        /// <param name="path">Path to the directory where the database will be stored on disk</param>
        /// <param name="autoIndexCount">Optional. Number of automatic indexes to generate and use</param>
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

        /// <summary>
        /// Creates a named index. Note that every index must have a name that is unique within the current `HyperVectorDB`.
        /// </summary>
        /// <param name="name">Unique name for the index</param>
        /// <returns>`True` if creation was successful, `False` if an error was encountered1</returns>
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

        /// <summary>
        /// Deletes an index with the given name, if it exists.
        /// </summary>
        /// <param name="name">Name of the index to be deleted</param>
        /// <returns>`True` if an index with a matching name was found and removed. `False` if the `HyperVectorDB` does not contain an index with the specified name</returns>
        public bool DeleteIndex(string name)
        {
            if (!Indexes.ContainsKey(name))
            {
                return false;
            }
            Indexes.Remove(name);
            return true;
        }

        /// <summary>
        /// Indexes a single string.
        /// Note: This overload is a pass-through for backwards compatibility with previous versions.
        /// </summary>
        /// <param name="indexName">Name of the index to store in</param>
        /// <param name="document">Text to be vectorized and indexed</param>
        /// <param name="preprocessor">Optional preprocessor delegate to process text prior to vectorization</param>
        /// <param name="postprocessor">Optional postrpocessor delegate to process text prior to storage in database</param>
        /// <returns>`True` if the text was vectorized and stored without issue. `False` if an error was encountered or if the Preprocessor or Postprocessor delegates returned `null`</returns>
        public bool IndexDocument(string indexName, string document, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null)
        {
            //Passthrough overload to avoid breaking changes to API
            return IndexDocument(document, preprocessor, postprocessor, indexName);
        }

        /// <summary>
        /// Indexes a single string.
        /// </summary>
        /// <param name="document">Text to be vectorized and indexed</param>
        /// <param name="preprocessor">Optional preprocessor delegate to process text prior to vectorization</param>
        /// <param name="postprocessor">Optional postrpocessor delegate to process text prior to storage in database</param>
        /// <param name="indexName">Optional. Name of the index to store in. If ommited, an index will be chosen automatically</param>
        /// <returns>`True` if the text was vectorized and stored without issue. `False` if an error was encountered or if the Preprocessor or Postprocessor delegates returned `null`</returns>
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

        /// <summary>
        /// Indexes every line in a file, sequentially
        /// Note: This overload is a pass-through for backwards compatibility with previous versions.
        /// </summary>
        /// <param name="indexName">Name of the index to store in</param>
        /// <param name="documentPath">Path to the file to be indexed</param>
        /// <param name="preprocessor">Optional preprocessor delegate to process text prior to vectorization</param>
        /// <param name="postprocessor">Optional postrpocessor delegate to process text prior to storage in database</param>
        /// <returns></returns>
        public bool IndexDocumentFile(string indexName, string documentPath, DocumentPreprocessor? preprocessor = null, DocumentPostprocessor? postprocessor = null)
        {
            return IndexDocumentFile(documentPath, preprocessor, postprocessor, indexName);
        }

        /// <summary>
        /// Indexes every line in a file, sequentially
        /// </summary>
        /// <param name="documentPath">Path to the file to be indexed</param>
        /// <param name="preprocessor">Optional preprocessor delegate to process text prior to vectorization</param>
        /// <param name="postprocessor">Optional postrpocessor delegate to process text prior to storage in database</param>
        /// <param name="indexName">Optional. Name of the index to store in. If ommited, an index will be chosen automatically</param>
        /// <returns></returns>
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

        /// <summary>
        /// Saves the entire database to disk. Every index is stored in its own files.
        /// The files will not be overwritten if the index hasn't changed since a `Load()` or `Save()`.
        /// </summary>
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
            foreach (string key in Indexes.Keys)
            {
                Console.Write($"{Indexes[key].Count,4}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Loads the entire database from disk. Existing indexes with duplicate names are overwritten from disk.
        /// </summary>
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

        /// <summary>
        /// Queries all indexes of the current `HyperVectorDB` in parallel to find similar entries.
        /// </summary>
        /// <param name="query">Text to be used as query</param>
        /// <param name="topK">Optional, indicates how many results to return</param>
        /// <returns></returns>
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
