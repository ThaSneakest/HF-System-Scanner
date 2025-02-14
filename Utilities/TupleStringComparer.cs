using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wildlands_System_Scanner.Utilities
{
    // Custom comparer for HashSet<Tuple<string, string, string>>
    public class TupleStringComparer : IEqualityComparer<Tuple<string, string, string>>
    {
        public bool Equals(Tuple<string, string, string> x, Tuple<string, string, string> y)
        {
            if (x == null || y == null) return false;
            return x.Item1.Equals(y.Item1, StringComparison.OrdinalIgnoreCase)
                && x.Item2.Equals(y.Item2, StringComparison.OrdinalIgnoreCase)
                && x.Item3.Equals(y.Item3, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Tuple<string, string, string> obj)
        {
            return obj.Item1.ToLowerInvariant().GetHashCode()
                 ^ obj.Item2.ToLowerInvariant().GetHashCode()
                 ^ obj.Item3.ToLowerInvariant().GetHashCode();
        }
    }
}
