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

using QuantConnect.Securities.Interfaces;

namespace QuantConnect.Securities.Equity
{
    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Holdings class for equities securities: no specific properties here but it is a placeholder for future equities specific behaviours.
    /// </summary>
    /// <seealso cref="SecurityHolding"/>
    public class EquityHolding : SecurityHolding 
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/

        /// <summary>
        /// Constructor for equities holdings.
        /// </summary>
        /// <param name="security">The security being held</param>
        /// <param name="transactionModel">The transaction model used for the security</param>
        /// <param name="marginModel">The margin model used for the security</param>
        public EquityHolding(Security security, ISecurityTransactionModel transactionModel, ISecurityMarginModel marginModel)
            : base(security, transactionModel, marginModel)
        {
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
            

        /******************************************************** 
        * CLASS METHODS 
        *********************************************************/
    }
}