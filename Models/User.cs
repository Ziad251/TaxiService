using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel;

namespace mapsmvcwebapp.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string CollectionName { get; set; } = null!;
    }

    public class Users
    {
        public List<User> results { get; set; }
    }
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public enum Gender { male, female };
        public Gender gender { get; }
        public Name name { get; set; }
        public Location location { get; set; }

        public string email { get; set; }
        public Login login { get; set; }
        public Dob dob { get; set; }
        public Registered registered { get; set; }
        public string phone { get; set; }
        public string cell { get; set; }

        [BsonElement("id")]
        public Identity identity { get; set; }
        public Picture pic { get; set; }
        public string nat { get; set; }

        public List<List<TimeStampedLocation>>? timestamp {get; set;}

    }

    public class Name
    {
        public string title { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }
    public class Location
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]

        public uint? streetNum { get; set; }
        public string streetName { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }

        public object? code;
        public string? postcode { get; set; }
        public Coordinates cordinates { get; set; }
        public Timezone timezone { get; set; }


    }
    public class Coordinates
    {
        public string typeName;

        public string type { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public float longitude { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public float latitude { get; set; }

        public GeoJsonPoint<GeoJson2DCoordinates> geo {get; set;} 
        
    }
    public class Timezone
    {
        public string offset { get; set; }
        public string description { get; set; }
    }
    public class Login
    {
        public Guid? uuid { get; set; }

        public string username { get; set; }
        public string password { get; set; }
        public string salt { get; set; }
        public string md5 { get; set; }
        public string sha1 { get; set; }
        public string sha256 { get; set; }
    }
    public class Dob
    {
        public string date { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint? age { get; set; }
    }
    public class Registered
    {
        public string date { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public uint? age { get; set; }
    }

    public class Identity
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    public class Picture
    {
        public string large { get; set; }
        public string medium { get; set; }
        public string thumbnail { get; set; }
    }


}







