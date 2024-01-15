namespace BricksHoarder.Core.Services
{
    public interface ICurrentUserService
    {
        Guid CurrentUserId { get; }
    }
}