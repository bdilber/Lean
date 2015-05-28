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

using System;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;

namespace QuantConnect.Tests.Common.Securities
{
    [TestFixture]
    public class SecurityTransactionModelTests
    {
        private const string Symbol = "SPY";

        [Test]
        public void PerformsMarketFillBuy()
        {
            var model = new SecurityTransactionModel();
            var order = new MarketOrder(Symbol, 100, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101.123m));

            var fill = model.MarketFill(security, order);
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(security.Price, fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }
        [Test]
        public void PerformsMarketFillSell()
        {
            var model = new SecurityTransactionModel();
            var order = new MarketOrder(Symbol, -100, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101.123m));

            var fill = model.MarketFill(security, order);
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(security.Price, fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsLimitFillBuy()
        {
            var model = new SecurityTransactionModel();
            var order = new LimitOrder(Symbol, 100, 101.5m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 102m));

            var fill = model.LimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new TradeBar(DateTime.Now, Symbol, 102m, 103m, 101m, 102.3m, 100));

            fill = model.LimitFill(security, order);

            // this fills worst case scenario, so it's at the limit price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(Math.Min(order.LimitPrice, security.High), fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsLimitFillSell()
        {
            var model = new SecurityTransactionModel();
            var order = new LimitOrder(Symbol, -100, 101.5m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101m));

            var fill = model.LimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new TradeBar(DateTime.Now, Symbol, 102m, 103m, 101m, 102.3m, 100));

            fill = model.LimitFill(security, order);

            // this fills worst case scenario, so it's at the limit price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(Math.Max(order.LimitPrice, security.Low), fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsStopLimitFillBuy()
        {
            var model = new SecurityTransactionModel();
            var order = new StopLimitOrder(Symbol, 100, 101.5m, 101.75m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 100m));

            var fill = model.StopLimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 102m));

            fill = model.StopLimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101.66m));

            fill = model.StopLimitFill(security, order);

            // this fills worst case scenario, so it's at the limit price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(order.LimitPrice, fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsStopLimitFillSell()
        {
            var model = new SecurityTransactionModel();
            var order = new StopLimitOrder(Symbol, -100, 101.75m, 101.50m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 102m));

            var fill = model.StopLimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101m));

            fill = model.StopLimitFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101.66m));

            fill = model.StopLimitFill(security, order);

            // this fills worst case scenario, so it's at the limit price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(order.LimitPrice, fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsStopMarketFillBuy()
        {
            var model = new SecurityTransactionModel();
            var order = new StopMarketOrder(Symbol, 100, 101.5m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101m));

            var fill = model.StopMarketFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 102.5m));

            fill = model.StopMarketFill(security, order);

            // this fills worst case scenario, so it's min of asset/stop price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(Math.Max(security.Price, order.StopPrice), fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsStopMarketFillSell()
        {
            var model = new SecurityTransactionModel();
            var order = new StopMarketOrder(Symbol, -100, 101.5m, DateTime.Now, type: SecurityType.Equity);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1);
            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 102m));

            var fill = model.StopMarketFill(security, order);

            Assert.AreEqual(0, fill.FillQuantity);
            Assert.AreEqual(0, fill.FillPrice);
            Assert.AreEqual(OrderStatus.None, fill.Status);
            Assert.AreEqual(OrderStatus.None, order.Status);

            security.SetMarketPrice(DateTime.Now, new IndicatorDataPoint(Symbol, DateTime.Now, 101m));

            fill = model.StopMarketFill(security, order);

            // this fills worst case scenario, so it's min of asset/stop price
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(Math.Min(security.Price, order.StopPrice), fill.FillPrice);
            Assert.AreEqual(OrderStatus.Filled, fill.Status);
            Assert.AreEqual(OrderStatus.Filled, order.Status);
        }

        [Test]
        public void PerformsMarketOnOpenUsingOpenPrice()
        {
            var reference = DateTime.Today.AddHours(9);// before market open
            var model = new SecurityTransactionModel();
            var order = new MarketOnOpenOrder(Symbol, SecurityType.Equity, 100, reference, 1m);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1) {Exchange = new EquityExchange()};
            var time = reference;
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1m, 2m, 0.5m, 1.33m, 100));

            var fill = model.MarketOnOpenFill(security, order);
            Assert.AreEqual(0, fill.FillQuantity);
            
            // market opens after 30min, so this is just before market open
            time = reference.AddMinutes(29);
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1.33m, 2.75m, 1.15m, 1.45m, 100));

            fill = model.MarketOnOpenFill(security, order);
            Assert.AreEqual(0, fill.FillQuantity);

            // market opens after 30min
            time = reference.AddMinutes(30);
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1.45m, 2.0m, 1.1m, 1.40m, 100));

            fill = model.MarketOnOpenFill(security, order);
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(security.Open, fill.FillPrice);
        }

        [Test]
        public void PerformsMarketOnCloseUsingClosingPrice()
        {
            var reference = DateTime.Today.AddHours(15);// before market close
            var model = new SecurityTransactionModel();
            var order = new MarketOnCloseOrder(Symbol, SecurityType.Equity, 100, reference, 1m);
            var config = new SubscriptionDataConfig(typeof(TradeBar), SecurityType.Equity, Symbol, Resolution.Minute, true, true, true, true, false, 0);
            var security = new Security(config, 1) { Exchange = new EquityExchange() };
            var time = reference;
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1m, 2m, 0.5m, 1.33m, 100));

            var fill = model.MarketOnCloseFill(security, order);
            Assert.AreEqual(0, fill.FillQuantity);

            // market closes after 60min, so this is just before market Close
            time = reference.AddMinutes(59);
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1.33m, 2.75m, 1.15m, 1.45m, 100));

            fill = model.MarketOnCloseFill(security, order);
            Assert.AreEqual(0, fill.FillQuantity);

            // market closes
            time = reference.AddMinutes(60);
            security.SetMarketPrice(time, new TradeBar(time, Symbol, 1.45m, 2.0m, 1.1m, 1.40m, 100));

            fill = model.MarketOnCloseFill(security, order);
            Assert.AreEqual(order.Quantity, fill.FillQuantity);
            Assert.AreEqual(security.Close, fill.FillPrice);
        }
    }
}
