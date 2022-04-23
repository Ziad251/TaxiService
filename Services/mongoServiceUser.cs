using mapsmvcwebapp.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using mapsmvcwebapp.Services;
using mapsmvcwebapp.Utils;
using MongoDB.Driver.GeoJsonObjectModel;
using Serilog;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.IO;

namespace mapsmvcwebapp.Services
{
    public class mongoServiceUser
    {
        private IMongoCollection<User> _mapsCollection;
        private IGetClaimsProvider _userClaims;
        private Mulberry32 _m32 = new Mulberry32((int)DateTime.UtcNow.Second);


        private User user;
        private User nearestUser;

        public mongoServiceUser(IOptions<DatabaseSettings> DatabaseSettings, IGetClaimsProvider userClaims)
        {
            var mongoClient = new MongoClient(
                DatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                DatabaseSettings.Value.DatabaseName);

            _mapsCollection = mongoDatabase.GetCollection<User>(
               DatabaseSettings.Value.CollectionName);
               
               _userClaims = userClaims;
        }

        public async Task<List<User>> GetAsync() =>
                await _mapsCollection.Find(_ => true).ToListAsync();

        public async void AddCoordinates(string username, float latitude, float longitude) =>

        await _mapsCollection.Find(x => x.login.username == username).ForEachAsync(u =>
        {
            u.location.cordinates.type = "Point";
            u.location.cordinates.latitude = latitude;
            u.location.cordinates.longitude = longitude;
        });


        public async Task<User?> GetAsync(string username)
        {
            try{
            return await _mapsCollection.Find(x => x.login.username == username).FirstOrDefaultAsync();
            }
            catch(Exception e)
            {
                Log.Error($"no user under that name. Error: {e}");
                return user;
            }


        }

        public async Task<User> GetNearbyAsync(User user)
        { 

            if(user == null){
                 throw new ArgumentNullException();
            }

            // Instantiate builder
            var builder = Builders<User>.Filter;

            // Set center point to Magnolia Bakery on Bleecker Street
            var point = user.location.cordinates.geo;

            // Create geospatial query that searches for Users at most 10,000 meters away,
            // and at least 2,000 meters away from Magnolia Bakery (AKA, our center point)
            var filter = builder.Near(db => db.location.cordinates.geo, point, maxDistance: 10000, minDistance: 5);
            Logg("$near", filter);

            return await _mapsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateOneAsync(User newUser) =>
            await _mapsCollection.InsertOneAsync(newUser);

        public async Task CreateManyAsync(List<User> newUsers) =>
            await _mapsCollection.InsertManyAsync(newUsers);

        // public async Task UpdateLocationAsync(Coordinates coord, User updatedUser) =>
        //     await _mapsCollection.ReplaceOneAsync(x => x.location.cordinates == coord, updatedUser);

        public async Task UpdateAsync()
        {
            var documents = await _mapsCollection.Find(Builders<User>.Filter.Empty).Skip(500).Limit(300).ToListAsync();

            Coordinates cordinates = new Coordinates();
            cordinates.type = "Point";
            cordinates.latitude = (float)(52 - _m32.Next() / 10);
            cordinates.longitude = (float)(21 - _m32.Next() / 10);

            foreach(var document in documents)
            {
            document.location.cordinates = cordinates;
            await _mapsCollection.InsertOneAsync(document);
            }
        }

        public async Task RemoveAsync(string id) =>
            await _mapsCollection.DeleteOneAsync(x => x.identity.value == id);

            
        private static void Logg(string exampleName, FilterDefinition<User> filter)
        {
            var serializerRegistry = BsonSerializer.SerializerRegistry;
            var documentSerializer = serializerRegistry.GetSerializer<User>();
            var rendered = filter.Render(documentSerializer, serializerRegistry);
            Console.WriteLine($"{exampleName} example:");
            Console.WriteLine(rendered.ToJson(new JsonWriterSettings { Indent = true }));
            Console.WriteLine();
        }


    }
}