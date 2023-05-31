# HyperVectorDB

HyperVectorDB is a local vector database built in C# that supports various distance/similarity measures. It is designed to store vectors and associated documents and perform high-performance vector queries. This project supports Cosine Similarity, Jaccard Dissimilarity, as well as Euclidean, Manhattan, Chebyshev, and Canberra distances.

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
