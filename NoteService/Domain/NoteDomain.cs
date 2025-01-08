using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoteService.Domain
{
    public class NoteDomain
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public required int PatientId { get; set; }
        public required string Note { get; set; }
    }
}
