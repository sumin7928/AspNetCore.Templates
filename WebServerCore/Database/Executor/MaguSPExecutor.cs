using System.Data;
using System.Data.SqlClient;

namespace ApiWebServer.Database.Executor
{
    public sealed class MaguSPExecutor : DBExecutor
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public MaguSPExecutor( SqlConnection sqlConnection ) : base( sqlConnection, null )
        {
        }

        public MaguSPExecutor( SqlConnection sqlConnection, long requestNo ) : base( sqlConnection, null )
        {
            if ( requestNo > 0 )
            {
                Identity = requestNo.ToString();
            }
        }

        public MaguSPExecutor( SqlConnection sqlConnection, SqlTransaction sqlTransaction, long requestNo ) : base( sqlConnection, sqlTransaction )
        {
            if( requestNo > 0 )
            {
                Identity = requestNo.ToString();
            }
        }

        public DataSet RunStoredProcedureWithResult( string procedure )
        {
            AddDefaultOutParams();

            bool baseResult = base.RunStoredProcedure( procedure, out DataSet dataSet );
            if ( !baseResult )
            {
                return null;
            }

            if ( !IsValid( procedure ) )
            {
                return null;
            }

            return dataSet;
        }

        public override bool RunStoredProcedure( string procedure, out DataSet dataSet )
        {
            AddDefaultOutParams();

            bool baseResult = base.RunStoredProcedure( procedure, out dataSet );
            if( !baseResult )
            {
                return false;
            }

            return IsValid( procedure );
        }

        public override bool RunStoredProcedure( string procedure )
        {
            AddDefaultOutParams();

            bool baseResult = base.RunStoredProcedure( procedure );
            if ( !baseResult )
            {
                return false;
            }

            return IsValid( procedure );
        }

        private void AddDefaultOutParams()
        {
            AddOutputParam( "@o_sp_rtn", SqlDbType.Int, 0 );
            AddOutputParam( "@o_sp_msg", SqlDbType.NVarChar, 4000, "" );
        }

        private bool IsValid( string procedure )
        {
            int errorCode = ( int )GetOutputParam( "@o_sp_rtn", -1 );
            string errorMessage = ( string )GetOutputParam( "@o_sp_msg", "GetResultException" );

            if ( errorCode != 0 )
            {
                _logger.Error( $"[{Identity}] Error in procedure - errorCode:{errorCode}, msg:{errorMessage}" );
                return false;
            }

            return true;
        }
    }
}
