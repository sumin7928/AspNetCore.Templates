using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ApiWebServer.Cache;
using ApiWebServer.Common;
using ApiWebServer.Common.Define;
using ApiWebServer.Core.Helper;
using WebSharedLib.Core.NPLib;
using WebSharedLib.Core.Packet;
using WebSharedLib.Error;

namespace ApiWebServer.Core
{
    public class WebService<Request, Response> : IWebService<Request, Response>
        where Request : PacketCommon, new()
        where Response : PacketCommon, new()
    {
        public ILogger Logger { get; set; }
        public byte[] Key { get; set; } = AppConfig.LoginKey;
        public byte[] Iv { get; set; } = AppConfig.LoginIV;

        public HttpContext Context { get; private set; }
        public long RequestNo { get; private set; }
        public string Uri { get; private set; }

        public WebPacket<Request, Response> WebPacket { get; private set; } = new WebPacket<Request, Response>();
        public WebSession WebSession { get; private set; }
        public ErrorCode ErrorCode { get; set; }

        public void WrapRequestData(HttpContext context, NPWebRequest requestBody)
        {
            ParseRequestData(context, requestBody);

            if (ErrorCode == ErrorCode.SUCCESS)
            {
                Logger?.LogInformation("[{0}] Req - [header] {1} [data] {2}",
                    RequestNo,
                    WebPacket.ReqHeader,
                    ServerUtils.ReduceJsonLog(JsonConvert.SerializeObject(WebPacket.ReqData)));
            }
        }

        public void WrapRequestDataWithSession(HttpContext context, NPWebRequest requestBody)
        {
            Key = AppConfig.ClientCryptKey;
            Iv = AppConfig.ClientCryptIV;

            ErrorCode = ParseRequestData(context, requestBody);
            if (ErrorCode != ErrorCode.SUCCESS)
            {
                return;
            }

            try
            {
                WebSession = WebSession.CreateFromToken(WebPacket.ReqHeader.LogExID);
                if (WebSession == default(WebSession))
                {
                    ErrorCode = ErrorCode.ERROR_SESSION_TIMEOUT;
                    return;
                }

                if (Uri.ToLower().Contains("createusername") == false)
                {
                    if (WebSession.UserName == null || WebSession.UserName == string.Empty)
                    {
                        ErrorCode = ErrorCode.ERROR_INVALID_SESSION_DATA;
                        return;
                    }
                }

                if (WebPacket.ReqData.ValidCheck(out string errorMessage) == false)
                {
                    Logger?.LogError("[{0}] Invalid request data - reason:{1}", RequestNo, errorMessage);
                    ErrorCode = ErrorCode.ERROR_INVALID_DATA_RANGE;
                    return;
                }

                ErrorCode = WebSession.ValidateSequence(WebPacket.ReqHeader.SeniorID);
                if (ErrorCode != ErrorCode.SUCCESS)
                {
                    if (ErrorCode == ErrorCode.ERROR_LAST_PACKET_RETRY)
                    {
                        Response result = WebSession.GetLastPacket<Response>();
                        if (result == null)
                        {
                            Logger?.LogError("[{0}] Failed to get last packet - Pcid:{1}", RequestNo, WebSession.TokenInfo.Pcid);
                            ErrorCode = ErrorCode.ERROR_SEQUENCE;
                            return;
                        }

                        WebPacket.ResData = result;
                        return;
                    }
                    return;
                }

                ErrorCode = WebSession.RegisterSessionLock();

                if (ErrorCode == ErrorCode.SUCCESS)
                {
                    Logger?.LogInformation("[{0}] Req - [Session] {1} [header] {2} [data] {3}",
                        RequestNo,
                        WebSession,
                        WebPacket.ReqHeader,
                        ServerUtils.ReduceJsonLog(JsonConvert.SerializeObject(WebPacket.ReqData)));
                }

                return;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "[{0}] Exception from session", RequestNo);
                ErrorCode = ErrorCode.ERROR_UNKNOWN;
            }
        }

        public bool WrapLoginSession(WebSession webSession)
        {
            if (WebSessionHelper.InputSession(webSession) == false)
            {
                ErrorCode = ErrorCode.ERROR_SESSION;
                return false;
            }

            return true;
        }

        public NPWebResponse End(ErrorCode errorCode, string mainMessage = "", string subMessage = "")
        {
            ErrorCode = errorCode;
            LoggingMessage(mainMessage, subMessage);
            return End();
        }

        public NPWebResponse End(bool isSetLastPacket = true)
        {
            if (ErrorCode == ErrorCode.ERROR_LAST_PACKET_RETRY)
            {
                return EndComplete();
            }

            if (ErrorCode != ErrorCode.SUCCESS)
            {
                WebPacket.ResHeader.ErrorCode = (int)ErrorCode;
                WebPacket.ResHeader.ErrorMessage = Encoding.UTF8.GetBytes(Enum.GetName(typeof(ErrorCode), ErrorCode));
                if (CacheManager.PBTable.MessageTable != null)
                {
                    WebPacket.ResHeader.ShowMessage = Encoding.UTF8.GetBytes(CacheManager.PBTable.MessageTable.GetMessage((int)ErrorCode));
                }
                else
                {
                    WebPacket.ResHeader.ShowMessage = Encoding.UTF8.GetBytes("not found error message");
                }

                WebPacket.ResHeader.NextStep = (int)GetBehaviorType(ErrorCode);
            }
            else
            {
                //성공일때는 체크만해서 로그에 쌓기만한다.
                if (WebPacket.ResData.ValidCheck( out string errorMessage ) == false)
                {
                    Logger?.LogWarning("[{0}] Response invalidation - data:{1}, reason:{2}", RequestNo, ServerUtils.ReduceJsonLog(JsonConvert.SerializeObject(WebPacket.ResData)), errorMessage);
                }
            }

            if (ErrorCode.SUCCESS < ErrorCode && ErrorCode < ErrorCode.ERROR_CRITICAL_RANGE)
            {
                string pushMessage = "";

                if (WebSession != null)
                {
                    pushMessage = $"{Uri} {WebPacket.ResHeader.ErrorCode} : {ErrorCode} Pcid:{WebSession.TokenInfo.Pcid} - " +
                        $"{Encoding.UTF8.GetString(WebPacket.ResHeader.ShowMessage)}";
                }
                else
                {
                    pushMessage = $"{Uri} {WebPacket.ResHeader.ErrorCode} : {ErrorCode} - " +
                        $"{Encoding.UTF8.GetString(WebPacket.ResHeader.ShowMessage)}";
                }

                Context.Items.Add("alarm", pushMessage);
            }

            if (WebSession != null)
            {
                ErrorCode sessionLock = WebSession.RemoveSessionLock();
                if (sessionLock != ErrorCode.SUCCESS)
                {
                    ErrorCode = sessionLock;
                    WebPacket.ResHeader.NextStep = (int)GetBehaviorType(ErrorCode);
                }

                if (isSetLastPacket)
                {
                    WebSession.SetLastPacket(WebPacket.ResData);
                }

                WebSession.PreviousUrl = Uri;
                ErrorCode sessionError = WebSession.UpdateSession();

                if (sessionError != ErrorCode.SUCCESS)
                {
                    ErrorCode = sessionError;
                    WebPacket.ResHeader.NextStep = (int)GetBehaviorType(ErrorCode);
                }
            }

            return EndComplete();
        }

        private ErrorCode ParseRequestData(HttpContext context, NPWebRequest requestBody)
        {
            try
            {
                Context = context;
                RequestNo = long.Parse(Context.Request.Headers[WEB_HEADER_PROPERTIES.REQUEST_NO.ToString()]);
                Uri = Context.Request.Path;

                if (requestBody.header == null || requestBody.data == null)
                {
                    Logger?.LogError("[{0}] Req - Body is null", RequestNo);
                    return ErrorCode.ERROR_UNKNOWN;
                }

                WebPacket.DeserializeRequest(Key, Iv, requestBody);
                CheckRequestValidation();
            }
            catch (CryptographicException ce)
            {
                Logger?.LogError(ce, "[{0}] Failed to Decrypt requst data", RequestNo);
                return ErrorCode.ERROR_CRYPTO;
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "[{0}] Failed to create WebService", RequestNo);
                return ErrorCode.ERROR_UNKNOWN;
            }

            return ErrorCode.SUCCESS;
        }

        private NPWebResponse EndComplete()
        {
            NPWebResponse response = WebPacket.SerializeResponse(Key, Iv);
            if (WebPacket.ResData != null)
            {
                Logger?.LogInformation("[{0}] Res - [Session] {1} [header] {2} [data] {3}",
                    RequestNo,
                    WebSession,
                    WebPacket.ResHeader,
                    ServerUtils.ReduceJsonLog(JsonConvert.SerializeObject(WebPacket.ResData)));
            }
            else
            {
                Logger?.LogInformation("[{0}] Res - [Session] {1} [header] {2}",
                    RequestNo,
                    WebSession,
                    WebPacket.ResHeader);
            }

            return response;
        }

        private void LoggingMessage(string mainMessage, string subMessage)
        {
            if (SkipErrorLog() == true)
            {
                return;
            }

            if (WebSession != null)
            {
                Logger?.LogError("[{0}] - {1}:{2} - pubId:{3}, pcId:{4} {5}",
                    RequestNo, ErrorCode.ToString(), mainMessage, WebSession.PubId, WebSession.TokenInfo.Pcid, subMessage);
            }
            else
            {
                Logger?.LogError("[{0}] - {1}:{2} {3}",
                    RequestNo, ErrorCode.ToString(), mainMessage, subMessage);
            }
        }

        private bool SkipErrorLog()
        {
            if (ErrorCode == ErrorCode.SUCCESS ||
                ErrorCode == ErrorCode.ERROR_NO_ACCOUNT ||
                ErrorCode == ErrorCode.ERROR_SECESSION_ACCOUNT ||
                ErrorCode == ErrorCode.ERROR_BLOCK_ACCOUNT ||
                ErrorCode == ErrorCode.ERROR_OVERLAP_NICKNAME ||
                ErrorCode == ErrorCode.ERROR_SESSION_TIMEOUT)
            {
                return true;
            }

            return false;
        }

        private void CheckRequestValidation()
        {
            // check header
            if (WebPacket.ReqHeader == null)
            {
                ErrorCode = ErrorCode.ERROR_HEADER_STRING;
                return;
            }

            if (Uri.ToLower().Contains("login") == false && Uri.ToLower().Contains("createaccount") == false)
            {
                if (WebPacket.ReqHeader.LogExID == null || WebPacket.ReqHeader.LogExID == string.Empty)
                {
                    ErrorCode = ErrorCode.ERROR_HEADER_STRING;
                    return;
                }
            }

            // check data
            if (WebPacket.ReqData == null)
            {
                ErrorCode = ErrorCode.ERROR_DATA_STRING;
                return;
            }
        }

        private NEXT_STEP GetBehaviorType(ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.ERROR_HEADER_STRING:
                case ErrorCode.ERROR_SESSION:
                case ErrorCode.ERROR_SEQUENCE:
                case ErrorCode.ERROR_LAST_PACKET_RETRY:
                    return NEXT_STEP.TRY_RE_LOGIN;

                default:
                    return NEXT_STEP.NONE;
            }
        }
    }
}
