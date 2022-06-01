namespace BricksHoarder.Core.Services
{
    public interface ITimeZonesService
    {
        IDictionary<string, string> GetAll();

        bool Exist(string zoneId);
    }
}
