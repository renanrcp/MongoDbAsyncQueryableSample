namespace MongoDbAsyncQueryableSample.Database
{
    public interface INoSqlDbFactory
    {
        INoSqlDbContext Create(string databaseName);
    }
}