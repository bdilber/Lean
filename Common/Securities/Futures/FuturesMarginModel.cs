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
    public class FuturesMarginModel : ISecurityMarginModel
    {
        private readonly decimal _initialMarginRequirement;
        private readonly decimal _maintenanceMarginRequirement;

        public decimal InitialmarginRequirement
        {
            get { return _initialMarginRequirement; }
        }

        public decimal MaintenanceMarginRequirement
        {
            get { return _maintenanceMarginRequirement; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FuturesMarginModel"/>
        /// </summary>
        /// <param name="initialMarginRequirement">The initial margin requirement per contract.</param>
        /// <param name="maintenanceMarginRequirement">The maintenance margin requierement per contract.</param>
        public FuturesMarginModel(decimal initialMarginRequirement, decimal maintenanceMarginRequirement)
        {
            _initialMarginRequirement = initialMarginRequirement;
            _maintenanceMarginRequirement = maintenanceMarginRequirement;
        }

        /// <summary>
        /// Gets the current leverage of the futures contract. (Value of a contract / maintenance margin requirement.)
        /// </summary>
        /// <param name="security">The security to get leverage for. (Must be an instance of the Futures class)</param>
        /// <returns>The current leverage in the security</returns>
        public decimal GetLeverage(Security security)
        {
            var futures = security as Futures;
            if (futures == null) throw new Exception("Security must be of Futures type.");
            return (futures.Price == 0) ? 1 : futures.Price * futures.ContractSize / _maintenanceMarginRequirement;  
        }

                
        /// <summary>
        /// Setting leverage is not supported for FuturesMarginModel.
        /// </summary>
        /// <remarks>
        /// This is added to maintain backwards compatibility with the old margin/leverage system
        /// </remarks>
        /// <param name="security"></param>
        /// <param name="leverage">The new leverage</param>
        public void SetLeverage(Security security, decimal leverage)
        {
            throw new NotSupportedException("Leverage cannot be set for futures contracts.");
        }

        /// <summary>
        /// Gets the total margin required to execute the specified order in units of the account currency
        /// </summary>
        /// <param name="security">The security to compute initial margin for</param>
        /// <param name="order">The order to be executed</param>
        /// <returns>The total margin in terms of the currency quoted in the order</returns>
        public decimal GetInitialMarginRequiredForOrder(Security security, Order order)
        {
            //Get the order value from the non-abstract order classes (MarketOrder, LimitOrder, StopMarketOrder)
            //Market order is approximated from the current security price and set in the MarketOrder Method in QCAlgorithm.
            var orderFees = security.TransactionModel.GetOrderFee(security, order);
            return order.Quantity * _initialMarginRequirement + orderFees;
        }

        /// <summary>
        /// Gets the margin currently alloted to the specified holding
        /// </summary>
        /// <param name="security">The security to compute maintenance margin for</param>
        /// <returns>The maintenance margin required for the </returns>
        public decimal GetMaintenanceMargin(Security security)
        {
            return security.Holdings.AbsoluteQuantity*_maintenanceMarginRequirement;
        }

        public decimal GetMarginRemaining(SecurityPortfolioManager portfolio, Security security, OrderDirection direction)
        {
            var holdings = security.Holdings;

            if (direction == OrderDirection.Hold)
            {
                return portfolio.MarginRemaining;
            }

            //If the order is in the same direction as holdings, our remaining cash is our cash
            //In the opposite direction, our remaining cash is 2 x current value of assets + our cash
            if (holdings.IsLong)
            {
                switch (direction)
                {
                    case OrderDirection.Buy:
                        return portfolio.MarginRemaining;
                    case OrderDirection.Sell:
                        return (holdings.UnrealizedProfit + holdings.UnleveredAbsoluteHoldingsCost) * 2 + portfolio.MarginRemaining;
                }
            }
            else if (holdings.IsShort)
            {
                switch (direction)
                {
                    case OrderDirection.Buy:
                        return (holdings.UnrealizedProfit + holdings.UnleveredAbsoluteHoldingsCost) * 2 + portfolio.MarginRemaining;
                    case OrderDirection.Sell:
                        return portfolio.MarginRemaining;
                }
            }

            //No holdings, return cash
            return portfolio.MarginRemaining;
        }

        /// <summary>
        /// Generates a new order for the specified security taking into account the total margin
        /// used by the account. Returns null when no margin call is to be issued.
        /// </summary>
        /// <param name="security">The security to generate a margin call order for</param>
        /// <param name="netLiquidationValue">The net liquidation value for the entire account</param>
        /// <param name="totalMargin">The totl margin used by the account in units of base currency</param>
        /// <returns>An order object representing a liquidation order to be executed to bring the account within margin requirements</returns>
        public virtual SubmitOrderRequest GenerateMarginCallOrder(Security security, decimal netLiquidationValue, decimal totalMargin)
        {

            var futures = security as Futures;
            if (futures == null) throw new Exception("Security must be of Futures type.");


            if (totalMargin <= netLiquidationValue)
            {
                return null;
            }

            if (!futures.Holdings.Invested)
            {
                return null;
            }

            // Compute the number of contracts we can keep with our remaining margin. (Initial margin requirement is used for this calculation.)
            var remainingQuantity = (int) Math.Floor(netLiquidationValue / _initialMarginRequirement);

            // Compute the number of contracts we need to liquidate.
            var quantity = (int)futures.Holdings.AbsoluteQuantity - remainingQuantity;

            // don't try and liquidate more share than we currently hold, minimum value of 1, maximum value for absolute quantity
            quantity = Math.Max(1, Math.Min((int)futures.Holdings.AbsoluteQuantity, quantity));
            if (futures.Holdings.IsLong)
            {
                // adjust to a sell for long positions
                quantity *= -1;
            }

            return new SubmitOrderRequest(OrderType.Market, security.Type, security.Symbol, quantity, 0, 0, security.LocalTime.ConvertToUtc(security.Exchange.TimeZone), "Margin Call");
        }
    }
}
