using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NoteService.Domain
{
    public class NoteDomain
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public required int PatientId { get; set; }
        public required string Note { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Date { get; set; } = DateTime.UtcNow;

    }
}
