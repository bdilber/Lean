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
using System.Collections.Generic;

namespace QuantConnect.Securities.Futures
{

    public static class FuturesContracts
    {

        private const string Months = "FGHJKMNQUVXZ";

        private static readonly Dictionary<string, FuturesContractSpecs> Specs = new Dictionary
            <string, FuturesContractSpecs>()
        {
            // S&P 500 E-Mini
            {
                "ES",
                new FuturesContractSpecs()
                {
                    GeneralSymbol = "ES",
                    ContinuousSymbol = "@ES#", //IQFeed notation
                    InitialMarginRequirement = 5060,
                    MaintenanceMarginRequirement = 4600,
                    ContractSize = 50,
                    TickSize = 0.25m,
                    ExpirationCycle = ContractExpirationCycle.MarchQuarterly
                }
            },

            // Nasdaq 100 E-Mini
            {
                "NQ",
                new FuturesContractSpecs()
                {
                    GeneralSymbol = "NQ",
                    ContinuousSymbol = "@NQ#", //IQFeed notation
                    InitialMarginRequirement = 3960,
                    MaintenanceMarginRequirement = 3600,
                    ContractSize = 20,
                    TickSize = 0.25m,
                    ExpirationCycle = ContractExpirationCycle.MarchQuarterly
                }
            },

            // Crude oil E-Mini
            {
                "QM",
                new FuturesContractSpecs()
                {
                    GeneralSymbol = "NQ",
                    ContinuousSymbol = "@NQ#", //IQFeed notation
                    InitialMarginRequirement = 2695,
                    MaintenanceMarginRequirement = 2450,
                    ContractSize = 500,
                    TickSize = 0.025m,
                    ExpirationCycle = ContractExpirationCycle.Monthly
                }
            }
        };

        public static FuturesContractSpecs GetSpecs(string generalSymbol, DateTime date)
        {
            var specs = Specs[generalSymbol];
            var leadMonth = GetLeadMonth(date, specs.ExpirationCycle);
            specs.ContractSymbol = specs.GeneralSymbol + GetMonthSymbol(leadMonth) +
                                   date.ToString("yy");

            specs.ExpirationDate = (date >= GetExpirationDate(leadMonth, date.Year))
                ? GetExpirationDate(leadMonth + 1, date.Year)
                : GetExpirationDate(leadMonth, date.Year);

            specs.RollDate = (date >= GetRollDate(leadMonth, date.Year))
                ? GetRollDate(leadMonth + 1, date.Year)
                : GetRollDate(leadMonth, date.Year);

            return specs;
        }


        private static char GetMonthSymbol(int month)
        {
            return Months[month - 1];
        }


        private static int GetLeadMonth(DateTime time, ContractExpirationCycle cycle, bool useRollDate = true)
        {
            int contractMonth;
            switch (cycle)
            {
                case ContractExpirationCycle.MarchQuarterly:
                    contractMonth = time.Month - (time.Month - 1)%3 + 2;
                    break;
                case ContractExpirationCycle.FebruaryQuarterly:
                    contractMonth = (time.Month - (time.Month)%3 + 2)%12;
                    break;
                case ContractExpirationCycle.JanuaryQuarterly:
                    contractMonth = (time.Month - (time.Month + 1)%3 + 2)%12;
                    break;
                default:
                    contractMonth = time.Month;
                    break;
            }
            var tradeEnd = useRollDate ? GetRollDate(contractMonth, time.Year) : GetExpirationDate(contractMonth, time.Year);
            if (time >= tradeEnd && cycle == ContractExpirationCycle.Monthly)
            {
                contractMonth++;
            }
            else if (time >= tradeEnd)
            {
                contractMonth += 3;
            }
            return contractMonth;
        }

        private static DateTime GetRollDate(int month, int year)
        {
            var rollDate = GetThirdFriday(month, year).AddDays(-8);
            if (USHoliday.Dates.Contains(rollDate.Date))
            {
                rollDate = rollDate.AddDays(-1);
            }
            return rollDate.Add(TimeSpan.Parse("17:15:00"));
        }

        private static DateTime GetExpirationDate(int month, int year)
        {
            var expirationDate = GetThirdFriday(month, year);
            if (USHoliday.Dates.Contains(expirationDate.Date))
            {
                expirationDate = expirationDate.AddDays(-1);
            }
            return expirationDate.Add(TimeSpan.Parse("17:15:00"));
        }

        private static DateTime GetThirdFriday(int month, int year)
        {
            var tempDate = new DateTime(year, month, 1);
            while (tempDate.DayOfWeek != DayOfWeek.Friday)
                tempDate = tempDate.AddDays(1);
            return tempDate.AddDays(14);
        }
    }

    public enum ContractExpirationCycle
    {
        JanuaryQuarterly,
        FebruaryQuarterly,
        MarchQuarterly,
        Monthly
    }

    /// <summary>
    /// Container class for futures contract specific information
    /// </summary>
    public struct FuturesContractSpecs
    {
        /// <summary>
        /// The general generalSymbol for the futures contract. e.g. "ES" for the S & P E-mini
        /// </summary>
        public string GeneralSymbol { get; set; }
        
        /// <summary>
        /// The symnbol for continuous contract data. This is platform-specific. e.g. "@ES#" for the S & P E-mini on IQFeed
        /// </summary>
        public string ContinuousSymbol { get; set; }

        /// <summary>
        /// Full symnbol for the contract. e.g. "ESH15" for the S & P E-mini
        /// </summary>
        public string ContractSymbol { get; set; }

        /// <summary>
        /// Initial margin requirement for a single contract
        /// </summary>
        public decimal InitialMarginRequirement { get; set; }

        /// <summary>
        /// Maintenance margin requirement for a single contract
        /// </summary>
        public decimal MaintenanceMarginRequirement { get; set; }

        /// <summary>
        /// Represents how many underlying assets a single conract holds
        /// </summary>
        public decimal ContractSize { get; set; }

        /// <summary>
        /// Minimum allowed movement in "price". e.g. 0.25 index points for the S & P E-mini
        /// </summary>
        public decimal TickSize { get; set; }

        /// <summary>
        /// Expiration cycle for the contract. e.g. MarchQuarterly for the S & P E-mini
        /// </summary>
        public ContractExpirationCycle ExpirationCycle { get; set; }

        /// <summary>
        /// Trading start date for the contract
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Expiration date for the contract. This is usually the third Friday of the expiration month.
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Roll date for the contract. This is usually 8 days before contract expiration.
        /// </summary>
        public DateTime RollDate { get; set; }
    }
}
