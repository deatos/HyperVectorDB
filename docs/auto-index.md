---
uid: AutoIndexDoc
---

# Auto Index Documentation

## What is Auto Index?
Auto Index is a smart feature of HyperVectorDB where the database automatically generates a pool of indexes and all entries to the database get sorted across these indexes based on a hash value.

## Pros and Cons

Auto Index isn't ideal for every use case, but can save time and improve performance in others.

### Pros

- Spreading the database across multiple files reduces time spent reading and writing to disk by virtue of parallelized async disk IO.
- Multiple index objects in memory allows for parallel queries with minimal blocking.
- Depending on your hosting environment, multiple small files may be more efficient or more convenient than a single very large file.

### Cons

- Because the sorting across indexes is done by a hash algorithm, the database may not distribute evenly depending on your content.
- Your application may benefit from greater control over which index content is recorded in. With Auto Index active you may still specify which index to store to and create your own indexes, but this may be less convenient.
- The Auto Index feature currently only supports a single pool of indexes, so if you are using indexes to organize content you will be unable to leverage the efficiency of the pools. Support for multiple pools may be added in the future.

## How to Use

To enable Auto Index, specify a number greater than zero in the `HyperVectorDB` constructor. The ideal number will vary based on your application, and there are deeper trade offs on CPU utilization during queries, disk IO cost, and more. 
