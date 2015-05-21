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

using QuantConnect.Data;

namespace QuantConnect.Securities.Futures
{
    /******************************************************** 
     * CLASS DEFINITIONS
     *********************************************************/
    /// <summary>
    /// Futures Security Object Implementation for Futures Assets
    /// </summary>
    /// <seealso cref="Security"/>
    public class Futures : Security
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private decimal _contractSize = 1;
        private decimal _initialMarginRequirement;
        private decimal _maintenanceMarginRequirement;


        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Constructor for the Futures security
        /// </summary>
        public Futures(SubscriptionDataConfig config, decimal initialMarginRequirement, decimal maintenanceMarginRequirement, decimal contractSize = 1, bool isDynamicallyLoadedData = false) 
            : base(config, 1, isDynamicallyLoadedData) 
        {
            _contractSize = contractSize;
            _initialMarginRequirement = initialMarginRequirement;
            _maintenanceMarginRequirement = maintenanceMarginRequirement;

            //Holdings for new Vehicle:
            Cache = new FuturesCache();
            Exchange = new FuturesExchange();
            DataFilter = new FuturesDataFilter();

            //Set the Futures Transaction Model
            TransactionModel = new FuturesTransactionModel();
            PortfolioModel = new FuturesPortfolioModel();
            MarginModel = new FuturesMarginModel(_initialMarginRequirement, _maintenanceMarginRequirement);
            Holdings = new FuturesHolding(this);
        }



        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Returns the contractSize for the futures contract. 
        /// </summary>
        public decimal ContractSize
        {
            get
            {
                return _contractSize;
            } 
        }

        /// <summary>
        /// The margin requirement for maintaining a single contract position. Leverage will be calculated based on this.
        /// </summary>
        public decimal MaintenanceMarginRequirement
        {
            get
            {
                return _maintenanceMarginRequirement;
            }
        }

        /// <summary>
        /// The margin requirement for opening a single contract position.
        /// </summary>
        public decimal InitialMarginRequirement
        {
            get
            {
                return _initialMarginRequirement;
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/


    }

// End Market

}
