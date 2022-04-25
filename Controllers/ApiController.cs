using Microsoft.AspNetCore.Mvc;
using mapsmvcwebapp.Services;
using mapsmvcwebapp.Models;
using System.Text.Json;
using Serilog;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver;
using System.Text.Json.Serialization;

namespace mapsmvcwebapp.Controllers
{

    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private MongoCollection _userDB;
        private IGetClaimsProvider _userClaims;

        public ApiController(MongoCollection userDB, IGetClaimsProvider userClaims)
        {
            _userDB = userDB;
            _userClaims = userClaims;
        }

        [HttpPost("position")]
        public async void Position([FromBody] JsonElement pos)
        {
            string uid = _userClaims.Username;
             if(uid == null){
                 Log.Error("User logged in but their claims was inaccessible.");
                 throw new InvalidDataException();
            }
            var user = await _userDB.GetAsync(uid);
            var userLocs = JsonSerializer.Deserialize<List<TimeStampedLocation>>(pos.ToString());
            userLocs.ForEach(u => u.username = uid);
            var timestamps = user.timestamp;
            if(timestamps == null){
                    timestamps = new List<List<TimeStampedLocation>>();
            }
            timestamps.Add(userLocs);
            user.timestamp = timestamps;
            await _userDB.ReplaceAsync(user, user);
        }


        [HttpGet("nearbyusers")]
        public async Task<string> NearbyUsers()
        {
          
            string uid = _userClaims.Username;
            var user = await _userDB.GetAsync(uid);
             if(user == null){
                 return String.Empty;
                 throw new InvalidDataException();
            }
            var nearestUser = await _userDB.GetNearbyAsync(user); 
            var nearesresUserJson = JsonSerializer.Serialize(nearestUser);
            if(nearestUser == null){
                return String.Empty;
            }
            return nearesresUserJson;
        }

    }
}