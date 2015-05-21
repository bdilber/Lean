namespace QuantConnect.Securities
{
    /******************************************************** 
     * CLASS DEFINITIONS
     ********************************************************/
    /// <summary>
    /// Futures cache override.
    /// </summary>
    /// <remarks>Scheduled for obsolesence</remarks>
    /// <seealso cref="SecurityCache"/>
    public class FuturesCache : SecurityCache
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Start a new Cache for the set Index Code
        /// </summary>
        public FuturesCache() :
            base()
        {
            //Nothing to do:
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/

    } //End FuturesCache Class
}