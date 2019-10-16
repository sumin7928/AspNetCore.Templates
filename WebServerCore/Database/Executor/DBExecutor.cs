using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ApiWebServer.Database.Executor
{
    public class DBExecutor : IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly int _commandTimeout = 5;

        private readonly SqlConnection _conn;

        private SqlTransaction _trans;
        private List<SqlParameter> _sqlParameters = new List<SqlParameter>();

        public string QueryString { get; private set; }
        public string Identity { get; set; } = "DB";

        public DBExecutor(SqlConnection sqlConnection, SqlTransaction sqlTransaction = null)
        {
            _conn = sqlConnection;
            _conn.InfoMessage += new SqlInfoMessageEventHandler(OnInfoMessage);

            _trans = sqlTransaction;
        }

        public void Dispose()
        {
            if (_trans != null)
            {
                _trans.Dispose();
            }
            if (_conn != null)
            {
                _conn.Dispose();
            }
        }

        public void AddParam(string paramName, ParameterDirection direction, SqlDbType dbType, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType) { Value = value, Direction = direction });
        }
        public void AddParam(string paramName, ParameterDirection direction, SqlDbType dbType, int size, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType, size) { Value = value, Direction = direction });
        }
        public void AddInputParam(string paramName, SqlDbType dbType, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType) { Value = value, Direction = ParameterDirection.Input });
        }
        public void AddInputParam(string paramName, SqlDbType dbType, int size, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType, size) { Value = value, Direction = ParameterDirection.Input });
        }
        public void AddOutputParam(string paramName, SqlDbType dbType, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType) { Value = value, Direction = ParameterDirection.Output });
        }
        public void AddOutputParam(string paramName, SqlDbType dbType, int size, object value)
        {
            _sqlParameters.Add(new SqlParameter(paramName, dbType, size) { Value = value, Direction = ParameterDirection.Output });
        }

        public object GetOutputParam(string paramName, object defaultValue = null)
        {
            foreach (SqlParameter param in _sqlParameters)
            {
                if (param.ParameterName.Equals(paramName) && (param.Direction == ParameterDirection.Output))
                {
                    return param.Value;
                }
            }
            return defaultValue;
        }

        public virtual bool RunQuery(string query, out DataTable dataTable)
        {
            dataTable = new DataTable();

            SqlCommand command = new SqlCommand(query, _conn)
            {
                Transaction = _trans,
                CommandType = CommandType.Text,
                CommandTimeout = _commandTimeout
            };
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                dataTable.Load(reader);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"[{Identity}] Run Query - {QueryString}");
                return false;
            }
        }

        public virtual int RunQuery(string query)
        {
            int result = 0;
            QueryString = MakeQueryString(query);

            SqlCommand command = new SqlCommand(query, _conn)
            {
                Transaction = _trans,
                CommandType = CommandType.Text,
                CommandTimeout = _commandTimeout
            };
            try
            {
                result = command.ExecuteNonQuery();
                _logger.Info($"[{Identity}] Run Query - dataset:{result}, {QueryString}");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"[{Identity}] Run Query - {QueryString}");
            }

            return result;
        }

        public virtual async Task<DataSet> RunStoredProcedureAsync(string procedure)
        {
            QueryString = MakeQueryString(procedure);

            if (procedure == null || procedure == string.Empty)
            {
                return null;
            }

            SqlCommand command = new SqlCommand(procedure, _conn)
            {
                Transaction = _trans,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = _commandTimeout
            };
            command.Parameters.AddRange(_sqlParameters.ToArray());

            DataSet dataSet = new DataSet();
            int result = 0;

            await Task.Run(() =>
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                result = adapter.Fill(dataSet);
            });

            _logger.Info($"[{Identity}] Run Procedure - dataset:{result}, {QueryString}");

            return dataSet;
        }


        public virtual bool RunStoredProcedure(string procedure, out DataSet dataSet)
        {
            QueryString = MakeQueryString(procedure);

            dataSet = null;

            if (procedure == null || procedure == string.Empty)
            {
                return false;
            }

            SqlCommand command = new SqlCommand(procedure, _conn)
            {
                Transaction = _trans,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = _commandTimeout
            };
            command.Parameters.AddRange(_sqlParameters.ToArray());

            SqlDataAdapter adapter = new SqlDataAdapter(command);
            dataSet = new DataSet();
            int result = adapter.Fill(dataSet);

            _logger.Info($"[{Identity}] Run Procedure - dataset:{result}, {QueryString}");

            return true;
        }

        public virtual bool RunStoredProcedure(string procedure)
        {
            QueryString = MakeQueryString(procedure);

            if (procedure == null || procedure == string.Empty)
            {
                return false;
            }

            SqlCommand command = new SqlCommand(procedure, _conn)
            {
                Transaction = _trans,
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = _commandTimeout
            };
            command.Parameters.AddRange(_sqlParameters.ToArray());

            try
            {
                int result = command.ExecuteNonQuery();
                _logger.Info($"[{Identity}] Run Procedure - {QueryString}");
            }
            catch (Exception e)
            {
                _logger.Error($"[{Identity}] Failed to run procedure - exception:{e.Message}, {QueryString}");
                return false;
            }
            return true;
        }

        protected virtual void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            _logger.Info($"[{Identity}] Database Log - {args.Message}");
        }

        private string MakeQueryString(string procedure)
        {
            return string.Concat(procedure, " ", string.Join(", ", _sqlParameters.Select(p =>
         {
             if (p.SqlDbType == SqlDbType.Char || p.SqlDbType == SqlDbType.NChar || p.SqlDbType == SqlDbType.VarChar || p.SqlDbType == SqlDbType.NVarChar)
             {
                 string value = (string)p.Value;
                 if (value != null && value != string.Empty)
                 {
                     // value is json array
                     if (value.First().Equals('[') && value.Last().Equals(']'))
                     {
                         var jsonArray = JArray.Parse(value);
                         return string.Concat(p.ParameterName, ":JArray`count:", jsonArray.Count);
                     }
                     // value is json object
                     else if (value.First().Equals('{') && value.Last().Equals('}'))
                     {
                         var jsonObject = JObject.Parse(value);
                         return string.Concat(p.ParameterName, ":JObject`count:", jsonObject.Count);
                     }
                 }
                 return string.Concat("'", p.Value, "'");
             }

             return p.Value;

         })));
        }

    }
}
