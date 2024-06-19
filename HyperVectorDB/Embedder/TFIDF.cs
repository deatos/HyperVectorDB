using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EnglishStemmer;

namespace HyperVectorDB.Embedder {
    /// <summary>
    /// Copyright (c) 2013 Kory Becker http://www.primaryobjects.com/kory-becker.aspx
    /// 
    /// Permission is hereby granted, free of charge, to any person obtaining
    /// a copy of this software and associated documentation files (the
    /// "Software"), to deal in the Software without restriction, including
    /// without limitation the rights to use, copy, modify, merge, publish,
    /// distribute, sublicense, and/or sell copies of the Software, and to
    /// permit persons to whom the Software is furnished to do so, subject to
    /// the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be
    /// included in all copies or substantial portions of the Software.
    /// 
    /// Description:
    /// Performs a TF*IDF (Term Frequency * Inverse Document Frequency) transformation on an array of documents.
    /// Each document string is transformed into an array of doubles, cooresponding to their associated TF*IDF values.
    /// 
    /// Usage:
    /// string[] documents = LoadYourDocuments();
    ///
    /// double[][] inputs = TFIDF.Transform(documents);
    /// inputs = TFIDF.Normalize(inputs);
    /// 
    /// </summary>
    public static class TFIDF {
        /// <summary>
        /// Document vocabulary, containing each word's IDF value.
        /// </summary>
        private static Dictionary<string, double> _vocabularyIDF = new Dictionary<string, double>();

        /// <summary>
        /// Transforms a list of documents into their associated TF*IDF values.
        /// If a vocabulary does not yet exist, one will be created, based upon the documents' words.
        /// </summary>
        /// <param name="documents">string[]</param>
        /// <param name="vocabularyThreshold">Minimum number of occurences of the term within all documents</param>
        /// <returns>double[][]</returns>
        public static double[][] Transform(string[] documents, int vocabularyThreshold = 3) {
            List<List<string>> stemmedDocs;
            List<string> vocabulary;

            // Get the vocabulary and stem the documents at the same time.
            vocabulary = GetVocabulary(documents, out stemmedDocs, vocabularyThreshold);

            if (_vocabularyIDF.Count == 0) {
                // Calculate the IDF for each vocabulary term.
                foreach (var term in vocabulary) {
                    double numberOfDocsContainingTerm = stemmedDocs.Where(d => d.Contains(term)).Count();
                    _vocabularyIDF[term] = Math.Log((double)stemmedDocs.Count / ((double)1 + numberOfDocsContainingTerm));
                }
            }

            // Transform each document into a vector of tfidf values.
            return TransformToTFIDFVectors(stemmedDocs, _vocabularyIDF);
        }

        /// <summary>
        /// Converts a list of stemmed documents (lists of stemmed words) and their associated vocabulary + idf values, into an array of TF*IDF values.
        /// </summary>
        /// <param name="stemmedDocs">List of List of string</param>
        /// <param name="vocabularyIDF">Dictionary of string, double (term, IDF)</param>
        /// <returns>double[][]</returns>
        private static double[][] TransformToTFIDFVectors(List<List<string>> stemmedDocs, Dictionary<string, double> vocabularyIDF) {
            // Transform each document into a vector of tfidf values.
            List<List<double>> vectors = new List<List<double>>();
            foreach (var doc in stemmedDocs) {
                List<double> vector = new List<double>();

                foreach (var vocab in vocabularyIDF) {
                    // Term frequency = count how many times the term appears in this document.
                    double tf = doc.Where(d => d == vocab.Key).Count();
                    double tfidf = tf * vocab.Value;

                    vector.Add(tfidf);
                }

                vectors.Add(vector);
            }

            return vectors.Select(v => v.ToArray()).ToArray();
        }

        /// <summary>
        /// Normalizes a TF*IDF array of vectors using L2-Norm.
        /// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        /// </summary>
        /// <param name="vectors">double[][]</param>
        /// <returns>double[][]</returns>
        public static double[][] Normalize(double[][] vectors) {
            // Normalize the vectors using L2-Norm.
            List<double[]> normalizedVectors = new List<double[]>();
            foreach (var vector in vectors) {
                var normalized = Normalize(vector);
                normalizedVectors.Add(normalized);
            }

            return normalizedVectors.ToArray();
        }

        /// <summary>
        /// Normalizes a TF*IDF vector using L2-Norm.
        /// Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
        /// </summary>
        /// <param name="vectors">double[][]</param>
        /// <returns>double[][]</returns>
        public static double[] Normalize(double[] vector) {
            List<double> result = new List<double>();

            double sumSquared = 0;
            foreach (var value in vector) {
                sumSquared += value * value;
            }

            double SqrtSumSquared = Math.Sqrt(sumSquared);

            foreach (var value in vector) {
                // L2-norm: Xi = Xi / Sqrt(X0^2 + X1^2 + .. + Xn^2)
                result.Add(value / SqrtSumSquared);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Saves the TFIDF vocabulary to disk.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static void Save(string filePath = "vocabulary.json") {
            // Save result to disk.
            string json = JsonSerializer.Serialize(_vocabularyIDF);
            System.IO.File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads the TFIDF vocabulary from disk.
        /// </summary>
        /// <param name="filePath">File path</param>
        public static void Load(string filePath = "vocabulary.json") {
            // Load from disk.
            Dictionary<string, double>? vocabulary = 
                JsonSerializer.Deserialize<Dictionary<string, double>>(System.IO.File.ReadAllText(filePath));

            if(vocabulary != null)
            {
                _vocabularyIDF = vocabulary;
            }
            else
            {
                throw new Exception();
            }
        }

        #region Private Helpers

        /// <summary>
        /// Parses and tokenizes a list of documents, returning a vocabulary of words.
        /// </summary>
        /// <param name="docs">string[]</param>
        /// <param name="stemmedDocs">List of List of string</param>
        /// <returns>Vocabulary (list of strings)</returns>
        private static List<string> GetVocabulary(string[] docs, out List<List<string>> stemmedDocs, int vocabularyThreshold) {
            List<string> vocabulary = new List<string>();
            Dictionary<string, int> wordCountList = new Dictionary<string, int>();
            stemmedDocs = new List<List<string>>();

            int docIndex = 0;

            foreach (var doc in docs) {
                List<string> stemmedDoc = new List<string>();

                docIndex++;

                if (docIndex % 100 == 0) {
                    Console.WriteLine("Processing " + docIndex + "/" + docs.Length);
                }

                string[] parts2 = Tokenize(doc);

                List<string> words = new List<string>();
                foreach (string part in parts2) {
                    // Strip non-alphanumeric characters.
                    string stripped = Regex.Replace(part, "[^a-zA-Z0-9]", "");

                    if (!StopWords.stopWordsList.Contains(stripped.ToLower())) {
                        try {
                            var english = new EnglishWord(stripped);
                            string stem = english.Stem;
                            words.Add(stem);

                            if (stem.Length > 0) {
                                // Build the word count list.
                                if (wordCountList.ContainsKey(stem)) {
                                    wordCountList[stem]++;
                                } else {
                                    wordCountList.Add(stem, 0);
                                }

                                stemmedDoc.Add(stem);
                            }
                        } catch {
                        }
                    }
                }

                stemmedDocs.Add(stemmedDoc);
            }

            // Get the top words.
            var vocabList = wordCountList.Where(w => w.Value >= vocabularyThreshold);
            foreach (var item in vocabList) {
                vocabulary.Add(item.Key);
            }

            return vocabulary;
        }

        /// <summary>
        /// Tokenizes a string, returning its list of words.
        /// </summary>
        /// <param name="text">string</param>
        /// <returns>string[]</returns>
        private static string[] Tokenize(string text) {
            // Strip all HTML.
            text = Regex.Replace(text, "<[^<>]+>", "");

            // Strip numbers.
            text = Regex.Replace(text, "[0-9]+", "number");

            // Strip urls.
            text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

            // Strip email addresses.
            text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

            // Strip dollar sign.
            text = Regex.Replace(text, "[$]+", "dollar");

            // Strip usernames.
            text = Regex.Replace(text, @"@[^\s]+", "username");

            // Tokenize and also get rid of any punctuation
            return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
        }

        #endregion
    }


    public static class StopWords {
        public static string[] stopWordsList = new string[]
        {
            "a",
            "about",
            "above",
            "across",
            "afore",
            "aforesaid",
            "after",
            "again",
            "against",
            "agin",
            "ago",
            "aint",
            "albeit",
            "all",
            "almost",
            "alone",
            "along",
            "alongside",
            "already",
            "also",
            "although",
            "always",
            "am",
            "american",
            "amid",
            "amidst",
            "among",
            "amongst",
            "an",
            "and",
            "anent",
            "another",
            "any",
            "anybody",
            "anyone",
            "anything",
            "are",
            "aren't",
            "around",
            "as",
            "aslant",
            "astride",
            "at",
            "athwart",
            "away",
            "b",
            "back",
            "bar",
            "barring",
            "be",
            "because",
            "been",
            "before",
            "behind",
            "being",
            "below",
            "beneath",
            "beside",
            "besides",
            "best",
            "better",
            "between",
            "betwixt",
            "beyond",
            "both",
            "but",
            "by",
            "c",
            "can",
            "cannot",
            "can't",
            "certain",
            "circa",
            "close",
            "concerning",
            "considering",
            "cos",
            "could",
            "couldn't",
            "couldst",
            "d",
            "dare",
            "dared",
            "daren't",
            "dares",
            "daring",
            "despite",
            "did",
            "didn't",
            "different",
            "directly",
            "do",
            "does",
            "doesn't",
            "doing",
            "done",
            "don't",
            "dost",
            "doth",
            "down",
            "during",
            "durst",
            "e",
            "each",
            "early",
            "either",
            "em",
            "english",
            "enough",
            "ere",
            "even",
            "ever",
            "every",
            "everybody",
            "everyone",
            "everything",
            "except",
            "excepting",
            "f",
            "failing",
            "far",
            "few",
            "first",
            "five",
            "following",
            "for",
            "four",
            "from",
            "g",
            "gonna",
            "gotta",
            "h",
            "had",
            "hadn't",
            "hard",
            "has",
            "hasn't",
            "hast",
            "hath",
            "have",
            "haven't",
            "having",
            "he",
            "he'd",
            "he'll",
            "her",
            "here",
            "here's",
            "hers",
            "herself",
            "he's",
            "high",
            "him",
            "himself",
            "his",
            "home",
            "how",
            "howbeit",
            "however",
            "how's",
            "i",
            "id",
            "if",
            "ill",
            "i'm",
            "immediately",
            "important",
            "in",
            "inside",
            "instantly",
            "into",
            "is",
            "isn't",
            "it",
            "it'll",
            "it's",
            "its",
            "itself",
            "i've",
            "j",
            "just",
            "k",
            "l",
            "large",
            "last",
            "later",
            "least",
            "left",
            "less",
            "lest",
            "let's",
            "like",
            "likewise",
            "little",
            "living",
            "long",
            "m",
            "many",
            "may",
            "mayn't",
            "me",
            "mid",
            "midst",
            "might",
            "mightn't",
            "mine",
            "minus",
            "more",
            "most",
            "much",
            "must",
            "mustn't",
            "my",
            "myself",
            "n",
            "near",
            "'neath",
            "need",
            "needed",
            "needing",
            "needn't",
            "needs",
            "neither",
            "never",
            "nevertheless",
            "new",
            "next",
            "nigh",
            "nigher",
            "nighest",
            "nisi",
            "no",
            "no-one",
            "nobody",
            "none",
            "nor",
            "not",
            "nothing",
            "notwithstanding",
            "now",
            "o",
            "o'er",
            "of",
            "off",
            "often",
            "on",
            "once",
            "one",
            "oneself",
            "only",
            "onto",
            "open",
            "or",
            "other",
            "otherwise",
            "ought",
            "oughtn't",
            "our",
            "ours",
            "ourselves",
            "out",
            "outside",
            "over",
            "own",
            "p",
            "past",
            "pending",
            "per",
            "perhaps",
            "plus",
            "possible",
            "present",
            "probably",
            "provided",
            "providing",
            "public",
            "q",
            "qua",
            "quite",
            "r",
            "rather",
            "re",
            "real",
            "really",
            "respecting",
            "right",
            "round",
            "s",
            "same",
            "sans",
            "save",
            "saving",
            "second",
            "several",
            "shall",
            "shalt",
            "shan't",
            "she",
            "shed",
            "shell",
            "she's",
            "short",
            "should",
            "shouldn't",
            "since",
            "six",
            "small",
            "so",
            "some",
            "somebody",
            "someone",
            "something",
            "sometimes",
            "soon",
            "special",
            "still",
            "such",
            "summat",
            "supposing",
            "sure",
            "t",
            "than",
            "that",
            "that'd",
            "that'll",
            "that's",
            "the",
            "thee",
            "their",
            "theirs",
            "their's",
            "them",
            "themselves",
            "then",
            "there",
            "there's",
            "these",
            "they",
            "they'd",
            "they'll",
            "they're",
            "they've",
            "thine",
            "this",
            "tho",
            "those",
            "thou",
            "though",
            "three",
            "thro'",
            "through",
            "throughout",
            "thru",
            "thyself",
            "till",
            "to",
            "today",
            "together",
            "too",
            "touching",
            "toward",
            "towards",
            "true",
            "'twas",
            "'tween",
            "'twere",
            "'twill",
            "'twixt",
            "two",
            "'twould",
            "u",
            "under",
            "underneath",
            "unless",
            "unlike",
            "until",
            "unto",
            "up",
            "upon",
            "us",
            "used",
            "usually",
            "v",
            "versus",
            "very",
            "via",
            "vice",
            "vis-a-vis",
            "w",
            "wanna",
            "wanting",
            "was",
            "wasn't",
            "way",
            "we",
            "we'd",
            "well",
            "were",
            "weren't",
            "wert",
            "we've",
            "what",
            "whatever",
            "what'll",
            "what's",
            "when",
            "whencesoever",
            "whenever",
            "when's",
            "whereas",
            "where's",
            "whether",
            "which",
            "whichever",
            "whichsoever",
            "while",
            "whilst",
            "who",
            "who'd",
            "whoever",
            "whole",
            "who'll",
            "whom",
            "whore",
            "who's",
            "whose",
            "whoso",
            "whosoever",
            "will",
            "with",
            "within",
            "without",
            "wont",
            "would",
            "wouldn't",
            "wouldst",
            "x",
            "y",
            "ye",
            "yet",
            "you",
            "you'd",
            "you'll",
            "your",
            "you're",
            "yours",
            "yourself",
            "yourselves",
            "you've",
            "z",
        };
    }
}
