namespace mapsmvcwebapp.Services

{
    public interface IGetClaimsProvider
    {
         public string Username {get;   }
        public string UserGivenName { get; set; }
        public string UserLastName { get; set; } 

        public string UserEmail { get; set; }
        public string UserMobilePhone { get; set; }     

        
    }
}
