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
using QuantConnect.Util;

namespace QuantConnect.Data
{
    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Abstract base data class of QuantConnect. It is intended to be extended to define 
    /// generic user customizable data types while at the same time implementing the basics of data where possible
    /// </summary>
    public abstract class BaseData : IBaseData
    {
        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/
        private MarketDataType _dataType = MarketDataType.Base;
        private DateTime _time = new DateTime();
        private string _symbol = "";
        private decimal _value = 0;
        private bool _isFillForward = false;

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/
        /// <summary>
        /// Market Data Type of this data - does it come in individual price packets or is it grouped into OHLC.
        /// </summary>
        /// <remarks>Data is classed into two categories - streams of instantaneous prices and groups of OHLC data.</remarks>
        public MarketDataType DataType
        {
            get 
            {
                return _dataType;
            }
            set 
            {
                _dataType = value;
            }
        }

        /// <summary>
        /// True if this is a fill forward piece of data
        /// </summary>
        public bool IsFillForward
        {
            get { return _isFillForward; }
        }

        /// <summary>
        /// Current time marker of this data packet.
        /// </summary>
        /// <remarks>All data is timeseries based.</remarks>
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }

        /// <summary>
        /// The end time of this data. Some data covers spans (trade bars) and as such we want
        /// to know the entire time span covered
        /// </summary>
        public virtual DateTime EndTime
        {
            get { return _time; }
            set { _time = value; }
        }
        
        /// <summary>
        /// String symbol representation for underlying Security
        /// </summary>
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
            }
        }

        /// <summary>
        /// Value representation of this data packet. All data requires a representative value for this moment in time.
        /// For streams of data this is the price now, for OHLC packets this is the closing price.
        /// </summary>
        public decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// As this is a backtesting platform we'll provide an alias of value as price.
        /// </summary>
        public decimal Price
        {
            get 
            {
                return Value;
            }
        }
        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Constructor for initialising the dase data class
        /// </summary>
        public BaseData() 
        { 
            //Empty constructor required for fast-reflection initialization
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        
        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. 
        /// </summary>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        public virtual BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            // stub implementation to prevent compile errors in user algorithms
            var dataFeed = isLiveMode ? DataFeedEndpoint.LiveTrading : DataFeedEndpoint.Backtesting;
            return Reader(config, line, date, dataFeed);
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream 
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public virtual SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            // stub implementation to prevent compile errors in user algorithms
            var dataFeed = isLiveMode ? DataFeedEndpoint.LiveTrading : DataFeedEndpoint.Backtesting;
            var source = GetSource(config, date, dataFeed);

            if (isLiveMode)
            {
                // live trading by default always gets a rest endpoint
                return new SubscriptionDataSource(source, SubscriptionTransportMedium.Rest);
            }
            
            // construct a uri to determine if we have a local or remote file
            var uri = new Uri(source, UriKind.RelativeOrAbsolute);

            if (uri.IsAbsoluteUri && !uri.IsLoopback)
            {
                return new SubscriptionDataSource(source, SubscriptionTransportMedium.RemoteFile);
            }
                
            return new SubscriptionDataSource(source, SubscriptionTransportMedium.LocalFile);
        }

        /// <summary>
        /// Update routine to build a bar/tick from a data update. 
        /// </summary>
        /// <param name="lastTrade">The last trade price</param>
        /// <param name="bidPrice">Current bid price</param>
        /// <param name="askPrice">Current asking price</param>
        /// <param name="volume">Volume of this trade</param>
        public virtual void Update(decimal lastTrade, decimal bidPrice, decimal askPrice, decimal volume)
        {
            Value = lastTrade;
        }

        /// <summary>
        /// Return a new instance clone of this object, used in fill forward
        /// </summary>
        /// <remarks>
        /// This base implementation uses reflection to copy all public fields and properties
        /// </remarks>
        /// <param name="fillForward">True if this is a fill forward clone</param>
        /// <returns>A clone of the current object</returns>
        public BaseData Clone(bool fillForward)
        {
            var clone = Clone();
            clone._isFillForward = fillForward;
            return clone;
        }

        /// <summary>
        /// Return a new instance clone of this object, used in fill forward
        /// </summary>
        /// <remarks>
        /// This base implementation uses reflection to copy all public fields and properties
        /// </remarks>
        /// <returns>A clone of the current object</returns>
        public virtual BaseData Clone()
        {
            return ObjectActivator.Clone(this);
        }

        /// <summary>
        /// Formats a string with the symbol and value.
        /// </summary>
        /// <returns>string - a string formatted as SPY: 167.753</returns>
        public override string ToString()
        {
            return string.Format("{0}: {1}", Symbol, Value.ToString("C"));
        }

        /// <summary>
        /// Reader converts each line of the data source into BaseData objects. Each data type creates its own factory method, and returns a new instance of the object 
        /// each time it is called. 
        /// </summary>
        /// <remarks>OBSOLETE:: This implementation is added for backward/forward compatibility purposes. This function is no longer called by the LEAN engine.</remarks>
        /// <param name="config">Subscription data config setup object</param>
        /// <param name="line">Line of the source document</param>
        /// <param name="date">Date of the requested data</param>
        /// <param name="datafeed">Type of datafeed we're requesting - a live or backtest feed.</param>
        /// <returns>Instance of the T:BaseData object generated by this line of the CSV</returns>
        [Obsolete("Reader(SubscriptionDataConfig, string, DateTime, DataFeedEndpoint) method has been made obsolete, use Reader(SubscriptionDataConfig, string, DateTime, bool) instead.")]
        public virtual BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            throw new InvalidOperationException("Please implement Reader(SubscriptionDataConfig, string, DateTime, bool) on your custom data type: " + GetType().Name);
        }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream 
        /// </summary>
        /// <remarks>OBSOLETE:: This implementation is added for backward/forward compatibility purposes. This function is no longer called by the LEAN engine.</remarks>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="datafeed">Type of datafeed we're reqesting - backtest or live</param>
        /// <returns>String URL of source file.</returns>
        [Obsolete("GetSource(SubscriptionDataConfig, DateTime, DataFeedEndpoint) method has been made obsolete, use GetSource(SubscriptionDataConfig, DateTime, bool) instead.")]
        public virtual string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            throw new InvalidOperationException("Please implement GetSource(SubscriptionDataConfig, DateTime, bool) on your custom data type: " + GetType().Name);
        }
    }
}
