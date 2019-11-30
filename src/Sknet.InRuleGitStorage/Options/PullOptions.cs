using System.Runtime.CompilerServices;

namespace Sknet.InRuleGitStorage
{
    /// <summary>
    /// Collection of parameters controlling Fetch and Merge behavior
    /// </summary>
    public class PullOptions
    {
        /// <summary>
        /// Collection of parameters controlling Fetch behavior
        /// </summary>
        public FetchOptions FetchOptions { get; set; }

        // <summary>
        /// Collection of parameters controlling Merge behavior
        /// </summary>
        public MergeOptions MergeOptions { get; set; }
    }
}
