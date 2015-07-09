/*
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

using System;

namespace QuantConnect.Securities.Futures
{
    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Futures exchange information - Based on CME GLOBEX Futures exchange.
    /// </summary>
    /// <seealso cref="SecurityExchange"/>
    public class FuturesExchange : SecurityExchange
    {
        /// <summary>
        /// Number of trading days per year for this security, used for performance statistics.
        /// </summary>
        public override int TradingDaysPerYear
        {
            // 365 - Saturdays = 313;
            get { return 313; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuturesExchange"/> class using market hours
        /// derived from the market-hours-database for the FXCM Forex market
        /// </summary>
        public FuturesExchange()
            : base(SecurityExchangeHoursProvider.FromDataFolder().GetExchangeHours("usa", null, SecurityType.Future))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuturesExchange"/> class using the specified
        /// exchange hours to determine open/close times
        /// </summary>
        /// <param name="exchangeHours">Contains the weekly exchange schedule plus holidays</param>
        public FuturesExchange(SecurityExchangeHours exchangeHours)
            : base(exchangeHours)
        {
        }
    } //End of FuturesExchange

}