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
    public class MongoCollection
    {
        private IMongoCollection<User> _mapsCollection;
        private IGetClaimsProvider _userClaims;
        private Mulberry32 _m32 = new Mulberry32((int)DateTime.UtcNow.Second);


        private User user;
        private User nearestUser;

        public MongoCollection(IOptions<DatabaseSettings> DatabaseSettings, IGetClaimsProvider userClaims)
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
            try
            {
                return await _mapsCollection.Find(x => x.login.username == username).FirstOrDefaultAsync();
            }
            catch (Exception e)
            {
                Log.Error($"no user under that name. Error: {e}");
                return user;
            }


        }

        public async Task<User> GetNearbyAsync(User user)
        {

            if (user == null)
            {
                throw new ArgumentNullException();
            }

            // Instantiate builder
            var builder = Builders<User>.Filter;

            // Set center point to User's most recent location
            var points = user.timestamp[user.timestamp.Count - 1][user.timestamp[user.timestamp.Count - 1].Count - 1];
            GeoJsonPoint<GeoJson2DCoordinates> point = GeoJson.Point(GeoJson.Position(points.longitude, points.latitude));

            // Create geospatial query that searches for Users at most 10000 meters away,
            // and at least 5 meters away from our center point.
            var filter = builder.Near(db => db.location.cordinates.geo, point, maxDistance: 10000, minDistance: 5);

            return await _mapsCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateOneAsync(User newUser) =>
            await _mapsCollection.InsertOneAsync(newUser);

        public async Task CreateManyAsync(List<User> newUsers) =>
            await _mapsCollection.InsertManyAsync(newUsers);

        public async Task ReplaceAsync(User oldUser, User updatedUser) =>
            await _mapsCollection.ReplaceOneAsync(user => user.Id == oldUser.Id, updatedUser);

        public async Task UpdateAsync()
        {
            var documents = await _mapsCollection.Find(Builders<User>.Filter.Empty).Skip(500).Limit(300).ToListAsync();

            Coordinates cordinates = new Coordinates();
            cordinates.type = "Point";
            cordinates.latitude = (float)(52 - _m32.Next() / 10);
            cordinates.longitude = (float)(21 - _m32.Next() / 10);

            foreach (var document in documents)
            {
                document.location.cordinates = cordinates;
                await _mapsCollection.InsertOneAsync(document);
            }
        }

        public async Task RemoveAsync(string id) =>
            await _mapsCollection.DeleteOneAsync(x => x.identity.value == id);

    }
}