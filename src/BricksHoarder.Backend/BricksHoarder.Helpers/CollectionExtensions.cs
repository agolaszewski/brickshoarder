﻿namespace BricksHoarder.Helpers
{
    public static class CollectionExtensions
    {
        public static void WhileTrue<T>(this IEnumerable<T> collection, Func<T, bool> fn)
        {
            foreach (var item in collection)
            {
                if (!fn(item))
                {
                    break;
                }
            }
        }
    }
}