using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Models
{
    /// <summary>
    /// sample api account info
    /// </summary>
    public class SampleAccount
    {
        /// <summary>
        /// account sequence no
        /// </summary>
        public int SeqNo { get; set; }
        /// <summary>
        /// account id
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>
        /// account password
        /// </summary>
        public string Password { get; set; }
    }
}
