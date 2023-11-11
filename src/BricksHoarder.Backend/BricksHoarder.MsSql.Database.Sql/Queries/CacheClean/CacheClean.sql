DELETE FROM [dbo].[Cache]
WHERE [ExpireAt] < @Time