using BricksHoarder.Core.Services;

namespace BricksHoarder.Common
{
    public class GuidService : IGuidService
    {
        public Guid New => Guid.NewGuid();
    }
}