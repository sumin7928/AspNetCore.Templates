using System.Data;
using System.Threading.Tasks;
using ApiWebServer.Database.Base;

namespace ApiWebServer.Database
{
    public class AccountDB : BaseDB
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public AccountDB(string connString, long requestNo)
        {
            ConnString = connString;
            RequestNo = requestNo;
        }

        public virtual DataSet USP_AC_ACCOUNT_INFO_R(byte pubType, string pubId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_ACCOUNT_INFO_R");
            });
        }

        public virtual DataSet USP_AC_CREATE_ACCOUNT_R(byte pubType, string pubId, string nickName)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, nickName);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_CREATE_ACCOUNT_R");
            });
        }

        public virtual bool USP_AC_CREATE_ACCOUNT(long pcId, byte dbNum, byte pubType, string pubId, byte osType, string deviceID,
                                                byte serviceNationType, string nickName, string realNation )
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@db_num", SqlDbType.TinyInt, dbNum);
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);
                executor.AddInputParam("@os_type", SqlDbType.TinyInt, osType);
                executor.AddInputParam("@device_id", SqlDbType.VarChar, 100, deviceID);
                executor.AddInputParam("@nation_type", SqlDbType.TinyInt, serviceNationType);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, nickName);
                executor.AddInputParam("@real_nation", SqlDbType.VarChar, 10, realNation);

                return executor.RunStoredProcedure("dbo.USP_AC_CREATE_ACCOUNT");
            });
        }

        public virtual bool USP_GS_AC_ACCOUNT_DETAIL_LOGIN_TIME_U(long pcId, int storeType)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@store_type", SqlDbType.Int, storeType);

                return executor.RunStoredProcedure("dbo.USP_GS_AC_ACCOUNT_DETAIL_LOGIN_TIME_U");
            });
        }

        public virtual DataSet USP_AC_BIND_ACCOUNT_R(long pcId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_BIND_ACCOUNT_R");
            });
        }

        public virtual bool USP_AC_BIND_ACCOUNT(long pcId, byte pubType, string pubId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcId);
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);

                return executor.RunStoredProcedure("dbo.USP_AC_BIND_ACCOUNT");
            });
        }

        public virtual bool GameLeave(byte pubType, string pubId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);

                return executor.RunStoredProcedure("dbo.USP_GS_AC_GAME_LEAVE_U");
            });
        }

        public virtual bool GameLeaveCancel(byte pubType, string pubId)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pub_type", SqlDbType.TinyInt, pubType);
                executor.AddInputParam("@pub_id", SqlDbType.NVarChar, 40, pubId);

                return executor.RunStoredProcedure("dbo.USP_GS_AC_GAME_LEAVE_CANCEL_U");
            });
        }

        public virtual DataSet USP_AC_CREATE_USER_NAME_R(long pc_id, string userName)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, userName);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_CREATE_USER_NAME_R");
            });
        }

        public virtual bool USP_AC_CREATE_USER_NAME(long pc_id, string userName)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);
                executor.AddInputParam("@pc_name", SqlDbType.NVarChar, 50, userName);

                return executor.RunStoredProcedure("dbo.USP_AC_CREATE_USER_NAME");
            });
        }

        public virtual DataSet USP_AC_SESSION_R(long pc_id)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_SESSION_R");
            });
        }

        public virtual bool USP_AC_SESSION(long pc_id, string sessionData)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);
                executor.AddInputParam("@session_data", SqlDbType.VarChar, 2048, sessionData);

                return executor.RunStoredProcedure("dbo.USP_AC_SESSION");
            });
        }

        public virtual DataSet USP_AC_SESSION_LAST_PACKET_R(long pc_id)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);

                return executor.RunStoredProcedureWithResult("dbo.USP_AC_SESSION_LAST_PACKET_R");
            });
        }

        public virtual bool USP_AC_SESSION_LAST_PACKET(long pc_id, string lastPacketData)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pc_id);
                executor.AddInputParam("@last_packet", SqlDbType.VarChar, -1, lastPacketData);

                return executor.RunStoredProcedure("dbo.USP_AC_SESSION_LAST_PACKET");
            });
        }


        public virtual bool USP_AC_SESSION_ID_LOCK(string sessionId, byte status, int timeout)
        {
            return DBExecute(executor =>
            {
                executor.AddInputParam("@session_id", SqlDbType.VarChar, 128, sessionId);
                executor.AddInputParam("@request_status", SqlDbType.TinyInt, status);
                executor.AddInputParam("@time_out", SqlDbType.Int, timeout);

                return executor.RunStoredProcedure("dbo.USP_AC_SESSION_ID_LOCK");
            });
        }

        public virtual DataSet USP_AC_LIVESEASON_SCHEDULE_R()
        {
            return DBExecute(executor =>
            {
                return executor.RunStoredProcedureWithResult("dbo.USP_AC_LIVESEASON_SCHEDULE_R");
            });
        }
    }
}
