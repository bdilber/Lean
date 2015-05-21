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
using QuantConnect.Data.Market;
using QuantConnect.Orders;
using QuantConnect.Securities.Interfaces;

namespace QuantConnect.Securities.Futures
{
    /******************************************************** 
     * CLASS DEFINITIONS
     *********************************************************/
    /// <summary>
    /// Transaction model for Futures security trades. 
    /// </summary>
    /// <seealso cref="SecurityTransactionModel"/>
    /// <seealso cref="ISecurityTransactionModel"/>
    public class FuturesTransactionModel : SecurityTransactionModel, ISecurityTransactionModel
    {
        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the transaction model class
        /// </summary>
        public FuturesTransactionModel()
        {

        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/


        /// <summary>
        /// Get the minimum and maximum price for this security in the last bar:
        /// </summary>
        /// <param name="asset">Security asset we're checking</param>
        /// <param name="minimumPrice">Minimum price in the last data bar</param>
        /// <param name="maximumPrice">Minimum price in the last data bar</param>
        public override void DataMinMaxPrices(Security asset, out decimal minimumPrice, out decimal maximumPrice)
        {
            var marketData = asset.GetLastData();
            
            //Handle contract size for futures contracts
            var size = 1m;
            var future = asset as Futures;
            if (future != null) size = future.ContractSize;


            if (marketData.DataType == MarketDataType.TradeBar)
            {
                minimumPrice = ((TradeBar)marketData).Low * size;
                maximumPrice = ((TradeBar)marketData).High * size;
            }
            else
            {
                minimumPrice = marketData.Value * size;
                maximumPrice = marketData.Value * size;
            }
        }


        /// <summary>
        /// Get the slippage approximation for this order as a decimal value
        /// </summary>
        /// <param name="security">Security object we're working with</param>
        /// <param name="order">Order to approximate the slippage</param>
        /// <returns>Decimal value for he approximate slippage</returns>
        public override decimal GetSlippageApproximation(Security security, Order order)
        {
            return 0;
        }


        /// <summary>
        /// Get the fees from one order
        /// </summary>
        /// <param name="quantity">Quantity of shares processed</param>
        /// <param name="price">Price of the orders filled</param>
        /// <remarks>Default implementation uses the Interactive Brokers fee model of 1c per share with a maximum of 0.5% per order.</remarks>
        /// <returns>Decimal value of the order fee given this quantity and order price</returns>
        public override decimal GetOrderFee(decimal quantity, decimal price)
        {

            quantity = Math.Abs(quantity);

            if (quantity == 0) return 0;

            //Brokerage fee : $1.50 per contract
            var brokerageFee = 1.5m * quantity;
            
            //Min. $10 per transaction
            brokerageFee = (brokerageFee < 10) ? 10 : brokerageFee;
            
            //Exchange fee is currently $1.17 per contract
            var exchangeFee = 1.17m * quantity;


            var tradeFee = exchangeFee + brokerageFee;

            //Always return a positive fee.
            return Math.Abs(tradeFee);
        }

    } // End Algorithm Transaction Filling Classes

}