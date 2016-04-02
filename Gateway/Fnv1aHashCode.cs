using System;
using System.Text;

namespace Gateway
{
    /// <summary>
    /// http://www.isthe.com/chongo/tech/comp/fnv/index.html
    /// </summary>
    public static class Fnv1aHashCode
    {
        public static long Get64bitHashCode(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length == 0)
            {
                return 0;
            }

            const ulong OffsetBasis = 14695981039346656037;
            const ulong Prime = 1099511628211;

            ulong hashCode = OffsetBasis;

            for (int index = 0; index < value.Length; index++)
            {
                hashCode ^= value[index];
                hashCode *= Prime;
            }

            return (long)hashCode;
        }

        public static long Get64bitHashCode(string value, Encoding encoding = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return Get64bitHashCode((encoding ?? Encoding.UTF8).GetBytes(value));
        }
    }
}
