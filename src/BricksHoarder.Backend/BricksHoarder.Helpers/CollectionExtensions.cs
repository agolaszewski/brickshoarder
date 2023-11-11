using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BricksHoarder.Helpers
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
