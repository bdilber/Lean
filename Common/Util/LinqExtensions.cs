﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Util
{
    /// <summary>
    /// Provides more extension methods for the enumerable types
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        /// Creates a dictionary multimap from the lookup.
        /// </summary>
        /// <typeparam name="K">The key type</typeparam>
        /// <typeparam name="V">The value type</typeparam>
        /// <param name="lookup">The ILookup instance to convert to a dictionary</param>
        /// <returns>A dictionary holding the same data as 'lookup'</returns>
        public static Dictionary<K, List<V>> ToDictionary<K, V>(this ILookup<K, V> lookup)
        {
            return lookup.ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());
        }

        /// <summary>
        /// Returns true if the specified enumerable is null or has no elements
        /// </summary>
        /// <typeparam name="T">The enumerable's item type</typeparam>
        /// <param name="enumerable">The enumerable to check for a value</param>
        /// <returns>True if the enumerable has elements, false otherwise</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }
    }
}
