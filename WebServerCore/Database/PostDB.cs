using System;
using System.Data;
using ApiWebServer.Database.Base;
using WebSharedLib.Contents;

namespace ApiWebServer.Database
{
    public class PostDB : BaseDB
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public PostDB(string connString, long requestNo)
        {
            ConnString = connString;
            RequestNo = requestNo;
        }

        public virtual bool USP_GS_PO_GET_POST_NO_R( out DataSet dataSet )//post_no 발급
        {
            dataSet = null;

            return DBExecute(ref dataSet, executor =>
            {
                return executor.RunStoredProcedureWithResult( "dbo.USP_GS_PO_GET_POST_NO_R" );
            });
        }

        public virtual DataSet USP_GS_PO_POSTBOX_R(Int64 pcID, int dbNum, Int64 startPostNo, Int64 endPostNo)
        {
            if (startPostNo > endPostNo)
            {
                _logger.Error( "[DBPostbox] GetPostboxList - Error!! Search PostNo (Start:{0} > End:{1})", startPostNo, endPostNo);
                return null;
            }

            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@start_post_no", SqlDbType.BigInt, startPostNo);
                executor.AddInputParam("@end_post_no", SqlDbType.BigInt, endPostNo);

                return executor.RunStoredProcedureWithResult( "dbo.USP_GS_PO_POSTBOX_R" );
            } );
        }

        public virtual bool USP_GS_PO_POST_SEND(Int64 recvPcID,
                                        string recvPcName,
                                        Int64 sendPcID,
                                        string sendPcName,
                                        Models.PostInsert input,
                                        byte addType)
        {
            if (input == null)
                return false;
            if( input.RewardList == null )
                return false;

            DataTable rewardTable = new DataTable("rewardList");
            rewardTable.Columns.Add("row_no", typeof(int));
            rewardTable.Columns.Add("reward_type", typeof(int));
            rewardTable.Columns.Add("reward_idx", typeof(int));
            rewardTable.Columns.Add("reward_cnt", typeof(int));
            for (int i = 0; i < input.RewardList.Count; i++)
            {
                rewardTable.Rows.Add(i + 1, input.RewardList[i].reward_type, input.RewardList[i].reward_idx, input.RewardList[i].reward_cnt);
            }

            return DBExecute(executor =>
            {
                executor.AddInputParam("@recv_pub_id", SqlDbType.VarChar, 40, input.RecvPubID);
                executor.AddInputParam("@recv_pc_id", SqlDbType.BigInt, recvPcID);
                executor.AddInputParam("@recv_pc_name", SqlDbType.NVarChar, 20, recvPcName);
                executor.AddInputParam("@send_pub_id", SqlDbType.VarChar, 40, input.SendPubID);
                executor.AddInputParam("@send_pc_id", SqlDbType.BigInt, sendPcID);
                executor.AddInputParam("@send_pc_name", SqlDbType.NVarChar, 20, sendPcName);
                executor.AddInputParam("@send_pc_level", SqlDbType.Int, input.SendPCLevel);
                executor.AddInputParam("@post_code", SqlDbType.Char, 4, input.PostCode);
                executor.AddInputParam("@item_type_flag", SqlDbType.Char, 2, input.ItemTypeFlag);
                executor.AddInputParam("@reward_list", SqlDbType.Structured, rewardTable);
                executor.AddInputParam("@tran_code", SqlDbType.VarChar, 4, input.TranCode);
                executor.AddInputParam("@memo", SqlDbType.NVarChar, 50, input.Memo);
                executor.AddInputParam("@exp_time", SqlDbType.DateTime, input.ExpTime);
                executor.AddInputParam("@add_type", SqlDbType.TinyInt, addType); //0: 아이템별 우편시리얼 하나씩, 1: 하나의 시리얼로 복수지급

                return executor.RunStoredProcedure("dbo.USP_GS_PO_POST_SEND");

            });
        }

        public virtual bool USP_GS_PO_DELETE_POST(Int64 pcID, string postDelInfoList)
        {

            if (postDelInfoList == null)
            {
                _logger.Error( "[DBPostbox] DeletePost failed. postDelInfoList is null or empty.");
                return false;
            }

            return DBExecute(executor =>
            {
                executor.AddInputParam("@pc_id", SqlDbType.BigInt, pcID);
                executor.AddInputParam("@del_info", SqlDbType.VarChar, 4000, postDelInfoList);

                return executor.RunStoredProcedure("dbo.USP_GS_PO_DELETE_POST");

            });
        }
    }
}
