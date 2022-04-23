using mapsmvcwebapp.Models;
using Serilog;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Serialization;
using mapsmvcwebapp.Utils;

namespace mapsmvcwebapp.Services
{
    public class FakeUsers : IFakeUsers
    {
        private HttpClient _client;

        public Users? usersList { get; set; }

         public FakeUsers(HttpClient client)
         {
             _client = client;
         }

        public async Task<Users> GetRandomUsers(int amount)
        {
            var response = await _client.GetAsync($"https://randomuser.me/api/?results={amount}&?exc=info");
            try
            {
                var data = await response.Content.ReadAsStringAsync();
                if (data != null)
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new NumberToString() },
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true,
                        NumberHandling = JsonNumberHandling.WriteAsString,
                    };

                    JObject json = JObject.Parse(data);
                    var firstItem = json["results"][0];
                    Log.Information($"first item is {firstItem}");
                    usersList = JsonSerializer.Deserialize<Users>(data, options)!;
                    
                }
            }
            catch (Exception e)
            {
                Log.Error($"error during deserialization{e.Message}");
            }
            return usersList!;

        }
    }
}