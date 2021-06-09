using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nacos
{
    /// <summary>
    ///
    /// </summary>
    public class INacosUniqueConfigurationEqualityComparer : IEqualityComparer<INacosUniqueConfiguration>
    {
        /// <inheritdoc/>
        public bool Equals(INacosUniqueConfiguration? x, INacosUniqueConfiguration? y)
        {
            return x is not null
                   && y is not null
                   && x.GetUniqueKey().GetHashCode() == y.GetUniqueKey().GetHashCode();
        }

        /// <inheritdoc/>
        public int GetHashCode([DisallowNull] INacosUniqueConfiguration obj) => obj.GetUniqueKey().GetHashCode();
    }
}