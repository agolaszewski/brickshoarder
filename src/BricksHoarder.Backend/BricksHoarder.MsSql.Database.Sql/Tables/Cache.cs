namespace BricksHoarder.MsSql.Database.Tables
{
    public record Cache(string Key, string Value, System.DateTime? ExpireAt);
}