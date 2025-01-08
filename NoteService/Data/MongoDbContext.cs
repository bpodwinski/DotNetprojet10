using MongoDB.Driver;
using NoteService.Domain;

namespace NoteService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<NoteDomain> Notes => _database.GetCollection<NoteDomain>("notes");
    }
}
