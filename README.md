# Mongo Repository

C# strongly typed repository for mongoDB

Now with full async support!
Also all the async method are throttled to a limit of concurrent commands, so we'll not starve the connection pool.

to solve this error:
MongoDB.Driver.MongoWaitQueueFullException: The wait queue for acquiring a connection to server xyz.mongo.com:54128 is full.


