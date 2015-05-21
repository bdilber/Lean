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
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private TimeSpan _marketOpen = TimeSpan.FromHours(0);
        private TimeSpan _marketClose = TimeSpan.FromHours(24).Subtract(TimeSpan.FromTicks(1));

        /******************************************************** 
        * CLASS CONSTRUCTION
        *********************************************************/
        /// <summary>
        /// Initialise Futures exchange objects
        /// </summary>
        public FuturesExchange() :
            base()
        {
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Boolean flag indicating the futures exchange markets are open
        /// </summary>
        public override bool ExchangeOpen
        {
            get
            {
                return DateTimeIsOpen(Time);
            }
        }

        /// <summary>
        /// Number of trading days in an future calendar year - All days except saturdays and holidays.
        /// </summary>
        public override int TradingDaysPerYear
        {
            get
            {
                return 304;
            }
        }

        /// <summary>
        /// Futures markets open time/hour of day.
        /// </summary>
        public override TimeSpan MarketOpen
        {
            get { return _marketOpen; }
            set { _marketOpen = value; }
        }

        /// <summary>
        /// Futures markets closing time/hour of day.
        /// </summary>
        public override TimeSpan MarketClose
        {
            get { return _marketClose; }
            set { _marketClose = value; }
        }


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Check if the datetime specified is open.
        /// </summary>
        /// <param name="dateToCheck">Time to check</param>
        /// <remarks>Ignores early market closing times</remarks>
        /// <returns>True if open</returns>
        public override bool DateTimeIsOpen(DateTime dateToCheck)
        {
            if (dateToCheck.DayOfWeek == DayOfWeek.Saturday)
            {
                return false;
            }

            // CME Globex trading starts at 18:00 ET on Sundays.
            if (dateToCheck.DayOfWeek == DayOfWeek.Sunday && dateToCheck.TimeOfDay.TotalHours < 18)
            {
                return false;                
            }

            // CME Globex trading stops betweeen 16:15 ET and 16:30 ET on weekdays
            if (dateToCheck.TimeOfDay >= TimeSpan.Parse("16:15") && dateToCheck.TimeOfDay < TimeSpan.Parse("16:30"))
            {
                return false;
            }

            // CME Globex trading stops betweeen 17:15 ET and 18:00 ET on weekdays
            if (dateToCheck.TimeOfDay >= TimeSpan.Parse("17:15") && dateToCheck.TimeOfDay < TimeSpan.Parse("18:00"))
            {
                return false;
            }

            //Check holiday
            if (USHoliday.Dates.Contains(dateToCheck.Date))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Set the incoming datetime object date to the market open time.
        /// </summary>
        /// <param name="time">Date we want to set</param>
        /// <returns>DateTime adjusted to market open</returns>
        public override DateTime TimeOfDayOpen(DateTime time)
        {
            //Currently we assume 24 hour trading, since the exchange closes only for 1 hour during weekdays.
            return time.Date;
        }


        /// <summary>
        /// Set the datetime object to the time of day closed.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>Return datetime date to market close</returns>
        public override DateTime TimeOfDayClosed(DateTime time)
        {
            //Currently we assume 24 hour trading, since the exchange closes only for 1 hour during weekdays.
            return time.Date.AddDays(1);
        }


        /// <summary>
        /// Check if the US Futures markets are open on today's *date*. Check the calendar holidays as well.
        /// </summary>
        /// <param name="dateToCheck">Datetime to check</param>
        /// <returns>True if open</returns>
        public override bool DateIsOpen(DateTime dateToCheck)
        {
            //CME Globex closed on saturdays  
            if (dateToCheck.DayOfWeek == DayOfWeek.Saturday)
            {
                return false;
            }

            //Check holiday.
            if (USHoliday.Dates.Contains(dateToCheck.Date))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Check if this datetime is open, including extended market hours:
        /// </summary>
        /// <param name="time">Time to check</param>
        /// <returns>Bool true if in normal+extended market hours.</returns>
        public override bool DateTimeIsExtendedOpen(DateTime time)
        {
            // Actually there are extended hours on CME (15 min before each open), but we currently ignore them.
            return DateIsOpen(time);
        }

    } //End of FuturesExchange

}