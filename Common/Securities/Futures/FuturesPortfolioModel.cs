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
using QuantConnect.Logging;
using QuantConnect.Orders;

namespace QuantConnect.Securities.Futures
{
    public class FuturesPortfolioModel : ISecurityPortfolioModel
    {
        /// <summary>
        /// Performs application of an OrderEvent to the portfolio
        /// </summary>
        /// <param name="portfolio">The algorithm's portfolio</param>
        /// <param name="security">The fill's security</param>
        /// <param name="fill">The order event fill object to be applied</param>
        public virtual void ProcessFill(SecurityPortfolioManager portfolio, Security security, OrderEvent fill)
        {
            //First we need to cast the security to Futures
            var futures = security as Futures;
            if (futures == null) throw new Exception("Security must be of Futures type.");

            //Get the required information from the vehicle this order will affect
            var isLong = futures.Holdings.IsLong;
            var isShort = futures.Holdings.IsShort;
            
            //
            var orderSign = fill.Direction == OrderDirection.Sell ? -1 : 1;
            //Determine if we are closing a position or not
            var closedPosition = (isLong && fill.Direction == OrderDirection.Sell) || (isShort && fill.Direction == OrderDirection.Buy);

            try
            {

                var quantityHoldings = futures.Holdings.Quantity;
                var absoluteHoldingsQuantity = futures.Holdings.AbsoluteQuantity;
                var averageHoldingsPrice = futures.Holdings.AveragePrice;

                var liquidatedQuantity = closedPosition
                    ? orderSign*Math.Min(absoluteHoldingsQuantity, fill.AbsoluteFillQuantity)
                    : 0;

                var openedQuantity = fill.FillQuantity - liquidatedQuantity;

                var totalQuantity = quantityHoldings + fill.FillQuantity;

                var initialMarginDifference = ((FuturesMarginModel) futures.MarginModel).InitialmarginRequirement*
                                              (absoluteHoldingsQuantity - Math.Abs(totalQuantity));

                var lastTradeProfit = (averageHoldingsPrice - fill.FillPrice)*liquidatedQuantity*futures.ContractSize;



                //Get the Fee for this Order - Update the Portfolio Cash Balance: Remove Transacion Fees.
                var feeThisOrder =
                    Math.Abs(futures.TransactionModel.GetOrderFee(fill.AbsoluteFillQuantity, fill.FillPrice));

                //Calculate new average holding price
                if ((isLong && fill.Direction == OrderDirection.Buy) || (isShort && fill.Direction == OrderDirection.Sell))
                {
                    averageHoldingsPrice = (averageHoldingsPrice*quantityHoldings + openedQuantity*fill.FillPrice)/
                                           totalQuantity;
                }
                else if (openedQuantity != 0)
                {
                    averageHoldingsPrice = fill.FillPrice;
                }

                //Update the Vehicle approximate total sales volume.
                futures.Holdings.AddNewSale(fill.AbsoluteFillQuantity*fill.FillPrice*futures.ContractSize);
                futures.Holdings.AddNewFee(feeThisOrder);

                if (closedPosition)
                {
                    //Update Vehicle Profit Tracking:
                    futures.Holdings.AddNewProfit(lastTradeProfit);
                    futures.Holdings.SetLastTradeProfit(lastTradeProfit);
                    portfolio.AddTransactionRecord(futures.Time, lastTradeProfit - 2*feeThisOrder);
                }

                //Set the results back to the vehicle.

                portfolio.CashBook[CashBook.AccountCurrency].Quantity += initialMarginDifference + lastTradeProfit - feeThisOrder;
                futures.Holdings.SetHoldings(averageHoldingsPrice, totalQuantity);
            }
            catch (Exception err)
            {
                Log.Error("SecurityPortfolioManager.ProcessFill(orderEvent): " + err.Message);
            }

        }
    }
}
