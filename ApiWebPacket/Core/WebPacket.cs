using System;
using System.Collections.Generic;
using System.Text;

namespace ApiWebPacket.Core
{
    /// <summary>
    /// 패킷 공통 포멧 클래스
    /// </summary>
    /// <typeparam name="Request">요청 Body</typeparam>
    /// <typeparam name="Response">응답 Body</typeparam>
    public class WebPacket<Request, Response>
        where Request : class, new()
        where Response : class, new()
    {
        /// <summary>
        /// 요청 데이터
        /// </summary>
        public Request ReqData { get; set; } = new Request();

        /// <summary>
        /// 응답 데이터
        /// </summary>
        public Response ResData { get; set; } = new Response();
    }
}
