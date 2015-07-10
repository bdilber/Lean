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
using QuantConnect.Orders;

namespace QuantConnect.Securities.Futures
{
    /******************************************************** 
     * CLASS DEFINITIONS
     *********************************************************/
    /// <summary>
    /// Holdings class for equities securities: no specific properties here but it is a placeholder for future equities specific behaviours.
    /// </summary>
    /// <seealso cref="SecurityHolding"/>
    public class FuturesHolding : SecurityHolding
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private Futures _futures;

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/

        /// <summary>
        /// Constructor for futures holdings.
        /// </summary>
        public FuturesHolding(Futures futures)
            : base(futures)
        {
            _futures = futures;
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        
        /// <summary>
        /// Acquisition cost of the security total holdings.
        /// </summary>
        public override decimal HoldingsCost
        {
            get
            {
                return AveragePrice * Convert.ToDecimal(Quantity) * _futures.ContractSize;
            }
        }

        /// <summary>
        /// Unlevered Acquisition cost of the security total holdings.
        /// </summary>
        public override decimal UnleveredHoldingsCost
        {
            get
            {
                return _futures.InitialMarginRequirement * Convert.ToDecimal(Quantity);
            }
        }

        /// <summary>
        /// Market value of our holdings.
        /// </summary>
        public override decimal HoldingsValue
        {
            get
            {
                return UnleveredHoldingsCost + UnrealizedProfit;
            }
        }


        /******************************************************** 
        * CLASS METHODS 
        *********************************************************/
        
        /// <summary>
        /// Profit if we closed the holdings right now including the approximate fees.
        /// </summary>
        /// <remarks>Does not use the transaction model for market fills but should.</remarks>
        public override decimal TotalCloseProfit()
        {
            if (AbsoluteQuantity == 0)
            {
                return 0;
            }

            // this is in the account currency
            var marketOrder = new MarketOrder(_futures.Symbol, -Quantity, _futures.LocalTime.ConvertToUtc(_futures.Exchange.TimeZone), type: _futures.Type) { Price = Price };
            var orderFee = _futures.TransactionModel.GetOrderFee(_futures, marketOrder);

            return (Price - AveragePrice) * Quantity * _futures.ContractSize - orderFee;
        }
    } // End Futures Holdings:
}