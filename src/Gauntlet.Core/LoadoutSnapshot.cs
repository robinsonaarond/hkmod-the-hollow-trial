using System;
using System.Collections.Generic;

namespace Gauntlet.Core
{
    // Values are copied at capture AND application: live charm lists, save-copy
    // charm lists and the original snapshot must never share a mutable list.
    public sealed class LoadoutSnapshot
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        public LoadoutSnapshot(IEnumerable<KeyValuePair<string, object>> fields)
        { foreach (var field in fields) values.Add(field.Key, Copy(field.Value)); }
        private static object Copy(object value)
        {
            if (value is List<int> list) return new List<int>(list);
            if (value is int || value is bool || value is float) return value;
            throw new ArgumentException("Unsupported loadout snapshot field.");
        }
        public void Apply(Action<string, object> write)
        { foreach (var entry in values) write(entry.Key, Copy(entry.Value)); }
    }
}
