# HyperVectorDB

HyperVectorDB is a local vector database built in C# that supports various distance/similarity measures. It is designed to store vectors and associated documents and perform high-performance vector queries. This project supports Cosine Similarity, Jaccard Dissimilarity, as well as Euclidean, Manhattan, Chebyshev, and Canberra distances.
If you are looking for a python library to do the same thing check out John Dagdelen https://github.com/jdagdelen/hyperDB

## Installation

```
dotnet add package HyperVectorDB
```


## Features(To be updated)

- **Query/Response Caching**: Currently only supported for cosine similarity queries. This feature allows the database to cache the results of a query for a given vector, so that the next time the same vector is queried, the results are returned immediately. This feature is useful for applications that require frequent queries on the same vector.
- **Cache invalidation**: Cache invalidation is supported for cosine similarity queries. The cache is invalidated when a new vector is added to the database, or when a vector is removed from the database.
- **Query Functions**: The database supports several types of queries for similarity and distance measures:
  - **Cosine Similarity**: This function performs a Cosine Similarity query on the database.
  - **Jaccard Dissimilarity**: This function performs a Jaccard Dissimilarity query on the database.
  - **Euclidean Distance**: This function performs a Euclidean Distance query on the database.
  - **Manhattan Distance**: This function performs a Manhattan Distance query on the database.
  - **Chebyshev Distance**: This function performs a Chebyshev Distance query on the database.
  - **Canberra Distance**: This function performs a Canberra Distance query on the database.
- **Automatic Parallelization**: When configured, the database will automatically split across multiple files and memory regions to take full advantage of async IO on store and multithreading on query.
- **Data Compression**: When saved to disk, the database uses LZ4 compression

Each query function returns the top `k` documents and their corresponding similarity or distance values. The value of `k` is configurable and defaults to 5.

## Usage

Usage is very straight-forward and is illustrated well by the example program in HyperVectorDBExample. A quick summary of the core elements:

```csharp
var db = new HyperVectorDB(new Embedder.LmStudio(), "MyDatabase");
```
The HyperVectorDB object is the core element of the library and the two things it needs to be provided are an `Embedder` object and a folder name. The folder name is treated as a path, which can be relative or absolute.

```csharp
db.CreateIndex("MyIndex");
```
A `HyperVectorDB` contains one or more named indices. Support for multiple indices allows for seperation of indexed content if needed, or a single index can be used for everything.

```csharp
db.IndexDocument("This is a test document about dogs");
```

Indexing of documents can be done in several ways, the most trivial being individual strings. When indexed this way, the whole string is vectorized and the vector stored in the index along with the string.

```csharp
db.IndexDocumentFile(filepath, CustomPreprocessor, CustomPostprocessor);
```

Whole text files can also be indexed, in which case the files are split into lines (delimited be newline characters) and each line vectorized and the vectors stored in the index with the full path of the file and the line number where the line was found.

This approach also provides the option to pass custom preprocessor and postprocessor methods that can be used to filter documents to eliminate spurious or uninteresting content. These methods are run for every line of the file and can return a processed string or `null` to indicate the line should be ignored completely. The preprocessor allows for modifying the text prior to vectorization, while the postprocessor allows for customizing how the data is represented in the index.


```csharp
db.Save();
db.Load();
```

As documents are indexed the database is written to disk periodically. The `Save()` method allows you to force a write to disk, while the `Load()` method forces a reload from disk, intuitively.


```csharp
var results = db.QueryCosineSimilarity("dogs and cats", 10);
```

Ultimately, the point of building the database is to query it. There are multiple query methods available and all of them are expecting some string and a number indicating the maximum number of results desired.

## Contributing

Contributions are welcome. Please feel free to fork the project, make changes, and open a pull request. Please make sure to test all changes thoroughly.

## License

This project is open-source. Released under the MIT license. Please see the license file for more information.

Please note that some of the code in this project(Math.cs) is based on Acord.Math library which is released under the GNU Lesser General Public License v2.1 license.
TFIDF is from Kory Becker's project located at https://github.com/primaryobjects/TFIDF

## About this project and its author and why it came to be

It started out with me getting back into artificial intellegence and wanting to do so using c#.  I was unable to find anything that would suite my needs for a vector database.  Then John Dagdelen put together this vector store in python https://github.com/jdagdelen/hyperDB,  it was faily basic at the time posted without that many lines of code so I decided to try and use gpt to port it to c#.  This was somewhat successful but it did not quite work as needed so this project was born.
