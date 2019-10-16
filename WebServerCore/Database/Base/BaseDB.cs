using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ApiWebServer.Database.Executor;

namespace ApiWebServer.Database.Base
{
    public abstract class BaseDB //: IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public string ConnString { get; protected set; }

        public long RequestNo { get; protected set; }

        public IDbTransaction SqlTransaction { get; protected set; }

        public virtual IDbTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            SqlConnection conn = new SqlConnection(ConnString);
            conn.Open();
            SqlTransaction = conn.BeginTransaction(level);
            return SqlTransaction;
        }

        public void Commit()
        {
            if (SqlTransaction != null)
            {
                SqlTransaction.Commit();
            }
        }

        public int Update(string query)
        {
            if (SqlTransaction != null)
            {
                return InsertUpdateExecute(SqlTransaction.Connection, query);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    return InsertUpdateExecute(conn, query);
                }
            }
        }
        public int Insert(string query)
        {
            if (SqlTransaction != null)
            {
                return InsertUpdateExecute(SqlTransaction.Connection, query);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    return InsertUpdateExecute(conn, query);
                }
            }
        }
        public DataTable Select(string query)
        {
            if (SqlTransaction != null)
            {
                return SelectExecute(SqlTransaction.Connection, query);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    return SelectExecute(conn, query);
                }
            }
        }

        protected bool DBExecute(Predicate<MaguSPExecutor> action)
        {
            if (SqlTransaction != null)
            {
                return InternalDBExecute(SqlTransaction.Connection, action);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    return InternalDBExecute(conn, action);
                }
            }
        }

        protected async Task<bool> DBExecuteAsync(Predicate<MaguSPExecutor> action)
        {
            return await Task.Run(() =>
            {
               if (SqlTransaction != null)
               {
                   return InternalDBExecute(SqlTransaction.Connection, action);
               }
               else
               {
                   using (SqlConnection conn = new SqlConnection(ConnString))
                   {
                        conn.Open();
                        return InternalDBExecute(conn, action);
                   }
               }
            });
        }

        protected DataSet DBExecute(Func<MaguSPExecutor, DataSet> action)
        {
            if (SqlTransaction != null)
            {
                return InternalDBExecute(SqlTransaction.Connection, action);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    return InternalDBExecute(conn, action);
                }
            }
        }

        protected async Task<DataSet> DBExecuteAsync(Func<MaguSPExecutor, DataSet> action)
        {
            return await Task.Run(() =>
            {
                if (SqlTransaction != null)
                {
                    return InternalDBExecute(SqlTransaction.Connection, action);
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(ConnString))
                    {
                        conn.Open();
                        return InternalDBExecute(conn, action);
                    }
                }
            });

        }

        protected bool DBExecute(ref DataSet dataSet, Func<MaguSPExecutor, DataSet> action)
        {
            if (SqlTransaction != null)
            {
                return InternalDBExecute(SqlTransaction.Connection, ref dataSet, action);
            }
            else
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    return InternalDBExecute(conn, ref dataSet, action);
                }
            }
        }


        private int InsertUpdateExecute(IDbConnection connection, string query)
        {
            int result = 0;
            DBExecutor executor = new DBExecutor((SqlConnection)connection, (SqlTransaction)SqlTransaction);

            try
            {
                result = executor.RunQuery(query);
            }
            catch (Exception e)
            {
                _logger.Error("[{0}] Excepton for execute db - {1}", RequestNo, e.Message);
            }

            return result;
        }

        private DataTable SelectExecute(IDbConnection connection, string query)
        {
            DataTable dataTable = null;
            bool result = false;
            DBExecutor executor = new DBExecutor((SqlConnection)connection, (SqlTransaction)SqlTransaction);

            try
            {
                result = executor.RunQuery(query, out dataTable);
            }
            catch (Exception e)
            {
                _logger.Error("[{0}] Excepton for execute db - {1}", RequestNo, e.Message);
            }

            if (result == false)
            {
                _logger.Error("[{0}] Failed to execute db call - {1}", RequestNo, executor.QueryString);
            }

            return dataTable;
        }


        private bool InternalDBExecute(IDbConnection connection, Predicate<MaguSPExecutor> action)
        {
            bool rst = false;
            MaguSPExecutor executor = new MaguSPExecutor((SqlConnection)connection, (SqlTransaction)SqlTransaction, RequestNo);
            try
            {
                rst = action(executor);
            }
            catch (Exception e)
            {
                _logger.Error("[{0}] Excepton for execute db - {1}", RequestNo, e.Message);
                return false;
            }

            if (rst == false)
            {
                _logger.Error("[{0}] Failed to execute db call - {1}", RequestNo, executor.QueryString);
            }

            return rst;
        }
        private DataSet InternalDBExecute(IDbConnection connection, Func<MaguSPExecutor, DataSet> action)
        {
            DataSet dataSet = null;
            MaguSPExecutor executor = new MaguSPExecutor((SqlConnection)connection, (SqlTransaction)SqlTransaction, RequestNo);
            try
            {
                dataSet = action(executor);
            }
            catch (Exception e)
            {
                _logger.Error("[{0}] Excepton for execute db call - {1}", RequestNo, e.Message);
            }

            if (dataSet == null)
            {
                _logger.Error("[{0}] Failed to execute db call - {1}", RequestNo, executor.QueryString);
            }

            return dataSet;
        }

        private bool InternalDBExecute(IDbConnection connection, ref DataSet dataSet, Func<MaguSPExecutor, DataSet> action)
        {
            bool rst = false;
            MaguSPExecutor executor = new MaguSPExecutor((SqlConnection)connection, (SqlTransaction)SqlTransaction, RequestNo);
            try
            {
                dataSet = action(executor);
                if (dataSet != null)
                {
                    rst = true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("[{0}] Excepton for execute db call - {1}", RequestNo, e.Message);
                return false;
            }

            if (rst == false)
            {
                _logger.Error("[{0}] Failed to execute db call - {1}", RequestNo, executor.QueryString);
            }

            return rst;
        }

    }
}
