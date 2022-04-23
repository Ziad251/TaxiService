using mapsmvcwebapp.Models;

namespace mapsmvcwebapp.Services
{
    public interface IFakeUsers
    {
        public Task<Users> GetRandomUsers(int amount);
    }
}