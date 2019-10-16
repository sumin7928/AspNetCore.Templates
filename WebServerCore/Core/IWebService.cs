using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Core.Packet;
using WebSharedLib.Error;

namespace ApiWebServer.Core
{
    public interface IWebService<Request, Response>
        where Request : PacketCommon, new()
        where Response : PacketCommon, new()
    {
        ILogger Logger { get; set; }
        byte[] Key { get; set; }
        byte[] Iv { get; set; }
        HttpContext Context { get; }
        long RequestNo { get; }
        ErrorCode ErrorCode { get; set; }

        WebPacket<Request, Response> WebPacket { get; }
        WebSession WebSession { get; }

        void WrapRequestData(HttpContext context, NPWebRequest requestBody);
        void WrapRequestDataWithSession(HttpContext context, NPWebRequest requestBody);
        bool WrapLoginSession(WebSession webSession);

        NPWebResponse End(ErrorCode errorCode, string mainMessage = "", string subMessage = "");
        NPWebResponse End(bool isSetLastPacket = false);
    }
}
