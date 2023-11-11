SELECT * FROM [dbo].[Cache]
WHERE [Key] = @Key AND [ExpireAt] > @Now