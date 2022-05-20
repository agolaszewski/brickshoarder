using System.Data;

namespace BricksHoarder.Core.Database
{
    public interface IDbContext
    {
        void BeginTransaction();

        void RollbackTransaction();

        void SaveChangesSync();

        Task SaveChangesAsync();

        IDbConnection Connection { get; }
    }
}
