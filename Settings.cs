namespace BridgeTestlinkTuleap
{
    /// <summary>
    /// Settings of the application.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Api key to access Testlink.
        /// </summary>
        public string TestlinkApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// URL to access Testlink.
        /// </summary>
        public string TestlinkUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Project read from Testlink.
        /// </summary>
        public string TestlinkProjecName
        {
            get;
            set;
        }

        /// <summary>
        /// Api key to access Tuleap.
        /// </summary>
        public string TuleapApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// URL to access Tuleap.
        /// </summary>
        public string TuleapUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Tracker written in Tuleap.
        /// </summary>
        public int TuleapTrackerId
        {
            get;
            set;
        }

        /// <summary>
        /// Timeout of a request on Tuleap in ms
        /// </summary>
        public int TuleapTimeout
        {
            get;
            set;
        }
    }
}
