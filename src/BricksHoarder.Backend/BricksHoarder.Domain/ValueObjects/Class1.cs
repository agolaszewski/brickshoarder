using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BricksHoarder.Domain.ValueObjects
{
    public class LegoSetId
    {
        public LegoSetId()
        {

        }

        private LegoSetId(string value)
        {
            Value = value;
        }

        public string Value { get; init; }

        public static LegoSetId FromAggregateId(string aggregateId)
        {
            var array = aggregateId.Split(":");
            var setArray = array[1].Split("-");
            return new LegoSetId(setArray[0]);
        }

        public static implicit operator string(LegoSetId id) => id.Value;
    }
}
