using System.Collections.Generic;
using System.Linq;

namespace CRM.Infrastructure.Dictionary
{
    public class ObjectDictionary
    {
        public static Dictionary<string, object> CreateObjectMap((string key, object value)[] items)
        {
            return items.ToDictionary(x => x.key, x => x.value);
        }
    }
}
