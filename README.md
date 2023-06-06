# HyperVectorDB

HyperVectorDB is a local vector database built in C# that supports various distance/similarity measures. It is designed to store vectors and associated documents and perform high-performance vector queries. This project supports Cosine Similarity, Jaccard Dissimilarity, as well as Euclidean, Manhattan, Chebyshev, and Canberra distances.
If you are looking for a python library to do the same thing check out John Dagdelen https://github.com/jdagdelen/hyperDB

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

Each query function returns the top `k` documents and their corresponding similarity or distance values. The value of `k` is configurable and defaults to 5.

## Usage

Please note that this project is currently in its development phase. Some functions still need to be tested, and caching for some query types is yet to be implemented.

Example usage comming soon

## Contributing

Contributions are welcome. Please feel free to fork the project, make changes, and open a pull request. Please make sure to test all changes thoroughly.

## License

This project is open-source. Released under the MIT license. Please see the license file for more information.

Please note that some of the code in this project(GalaxyBrainedMathsLOL.cs) is based on Acord.Math library which is released under the GNU Lesser General Public License v2.1 license.
TFIDF is from Kory Becker's project located at https://github.com/primaryobjects/TFIDF

## About this project and its author and why it came to be

It started out with me getting back into artificial intellegence and wanting to do so using c#.  I was unable to find anything that would suite my needs for a vector database.  Then John Dagdelen put together this vector store in python https://github.com/jdagdelen/hyperDB,  it was faily basic at the time posted without that many lines of code so I decided to try and use gpt to port it to c#.  This was somewhat successful but it did not quite work as needed so this project was born.