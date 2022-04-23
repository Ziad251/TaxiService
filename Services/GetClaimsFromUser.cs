using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace mapsmvcwebapp.Services
{
    public class GetClaimsFromUser : IGetClaimsProvider
    {
        // public const string Name = "last";
        // public const string GivenName = "GivenName"; 
        // public const string MobilePhone = "MobilePhone"; 
        // public const string Email = "Email";  
         public const string UserName = "username";


        public string Username {get; private set; }
        public string UserGivenName { get; set; }
        public string UserLastName { get; set; } 

        public string UserEmail { get; set; }
        public string UserMobilePhone { get; set; }     


        public GetClaimsFromUser(IHttpContextAccessor accessor)
        {
            Username =accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == UserName)?.Value;
            UserGivenName = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
            UserLastName = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            UserEmail = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            UserMobilePhone = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ClaimTypes.MobilePhone)?.Value;
        }
    }


        // var shopKeyString = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == ShopKeyClaimName)?.Value;

        // public int ShopKey { get; private set; }
        // public string DistrictManagername { get; private set; }
            // DistrictManagername = accessor.HttpContext?.User.Claims.SingleOrDefault(x => x.Type == DistrictManagernameClaimName)?.Value;
            // if (shopKeyString != null)
            // {
            //     int.TryParse(shopKeyString, out var shopKey);
            //     ShopKey = shopKey;
            // }
}