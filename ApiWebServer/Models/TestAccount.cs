using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Models
{
    /// <summary>
    /// 테스트용 계정 정보
    /// </summary>
    public class SampleAccount
    {
        /// <summary>
        /// 계정 시퀀스 넘버
        /// </summary>
        public int SeqNo { get; set; }
        /// <summary>
        /// 계정 아이디
        /// </summary>
        public string AccountId { get; set; }
        /// <summary>
        /// 계정 비번
        /// </summary>
        public string Password { get; set; }
    }
}
