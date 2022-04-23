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

        private readonly FakeUsers _fetchUsers;
        private mongoServiceUser _userDB;
        private IGetClaimsProvider _userClaims;

        public ApiController(FakeUsers users, mongoServiceUser userDB, IGetClaimsProvider userClaims)
        {
            _fetchUsers = users;
            _userDB = userDB;
            _userClaims = userClaims;
        }

        [HttpPost("position")]
        public async Task<IActionResult> Position([FromBody] JsonElement pos)
        {
            string uid = _userClaims.Username;
            var user = await _userDB.GetAsync(uid);
             if(user == null){
                 throw new ArgumentNullException();
            }
            var userLocs = JsonSerializer.Deserialize<List<TimeStampedLocation>>(pos.ToString());
            userLocs.ForEach(u => u.username = uid);
            user.timestamp.Add(userLocs);
            await _userDB.CreateOneAsync(user);
            return Ok();
        }


        [HttpGet("nearbyusers")]
        public string NearbyUsers()
        {
          
            string uid = _userClaims.Username;
            var user = _userDB.GetAsync(uid);
             if(user.Result == null){
                 throw new ArgumentNullException();
            }
            var nearestUser = _userDB.GetNearbyAsync(user.Result); 
            var nearesresUserJson = JsonSerializer.Serialize(nearestUser.Result);
            if(nearestUser == null){
                throw new ArgumentNullException();
            }
            return nearesresUserJson;
        }

    }
}