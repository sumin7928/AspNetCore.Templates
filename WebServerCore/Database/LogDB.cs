using System;
using ApiWebServer.Models;

namespace ApiWebServer.Database
{
    public class LogDB 
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public bool InsertLog( string tableName )
        {
            throw new NotImplementedException();
        }

        //public string ConnString { get; private set; } = AppConfig.LogDBConnString[ 0 ];

        //public bool InsertLog( string tableName, Models.CommonLog commonLog )
        //{
        //    using ( SqlConnection conn = new SqlConnection( ConnString ) )
        //    {
        //        MaguSPExecutor executor = new MaguSPExecutor( conn );
        //        executor.AddInputParam( "@tb_name", SqlDbType.NVarChar, 300, tableName );
        //        executor.AddInputParam( "@pc_id", SqlDbType.BigInt, commonLog.PCID );
        //        executor.AddInputParam( "@type", SqlDbType.Int, commonLog.Type );
        //        executor.AddInputParam( "@title", SqlDbType.NVarChar, string.IsNullOrEmpty(commonLog.Title)?"": commonLog.Title );
        //        executor.AddInputParam( "@data_1", SqlDbType.NVarChar, 50, string.IsNullOrEmpty( commonLog.Data1 ) ? "" : commonLog.Data1 );
        //        executor.AddInputParam( "@data_2", SqlDbType.NVarChar, 50, string.IsNullOrEmpty( commonLog.Data2 ) ? "" : commonLog.Data2 );
        //        executor.AddInputParam( "@data_3", SqlDbType.NVarChar, 50, string.IsNullOrEmpty( commonLog.Data3 ) ? "" : commonLog.Data3 );
        //        executor.AddInputParam( "@data_4", SqlDbType.NVarChar, 50, string.IsNullOrEmpty( commonLog.Data4 ) ? "" : commonLog.Data4 );
        //        executor.AddInputParam( "@data_5", SqlDbType.NVarChar, 50, string.IsNullOrEmpty( commonLog.Data5 ) ? "" : commonLog.Data5 );
        //        executor.AddInputParam( "@json", SqlDbType.NVarChar, 1000, commonLog.Json );
        //        executor.AddInputParam( "@desc", SqlDbType.NVarChar, 500, commonLog.Desc );

        //        if ( !executor.RunStoredProcedure( "dbo.USP_LO_INSERT_LOG" ) )
        //        {
        //            log.ErrorFormat( "[CreateAccount] Faied to execute procedure - tableName:{0}", tableName );
        //            return false;
        //        }
        //    }

        //    return true;
        //}
    }
}
