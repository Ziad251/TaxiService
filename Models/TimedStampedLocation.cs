using MongoDB.Bson.Serialization.Attributes;

namespace mapsmvcwebapp.Models
{

    public class TimeStampedLocation
    {
        public string username {get; set;}
        public float latitude { get; set; }
        public float longitude { get; set; }
        public DateTime timestamp { get; set; }

         [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime serverTime { get; set; } = DateTime.UtcNow;

    }
}