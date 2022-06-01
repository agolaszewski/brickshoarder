namespace BricksHoarder.Core.Services
{
    public interface IUserService
    {
        object UserInfo(string region, string authorization);
    }
}
