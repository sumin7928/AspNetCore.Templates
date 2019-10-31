using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Models
{
    /// <summary>
    /// sample api result body
    /// </summary>
    public class SampleResult
    {
        /// <summary>
        /// api result code
        /// </summary>
        public int ResultCode { get; set; }
        /// <summary>
        /// api result message
        /// </summary>
        public string ResultMessage { get; set; }

    }
}
