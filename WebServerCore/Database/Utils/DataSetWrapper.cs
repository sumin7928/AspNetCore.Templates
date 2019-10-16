using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace ApiWebServer.Database.Utils
{
    public class DataSetWrapper
    {
        private readonly DataSet _dataSet;

        public DataSetWrapper(DataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public int GetTableCount()
        {
            return _dataSet.Tables.Count;
        }

        public int GetRowCount(int index)
        {
            if (GetTableCount() <= index)
            {
                return 0;
            }

            return _dataSet.Tables[index].Rows.Count;
        }

        public T GetValue<T>(int index, string column)
        {
            if (GetRowCount(index) < 1)
            {
                return default(T);
            }

            var rst = _dataSet.Tables[index].Rows[0][column];
            if (DBNull.Value.Equals(rst))
            {
                return default(T);
            }

            if (typeof(T) == typeof(bool))
            {
                rst = Convert.ToBoolean(rst);
            }

            return (T)rst;
        }

        public T GetValue<T>(int index, string column, T defaultValue)
        {
            if (GetRowCount(index) < 1)
            {
                return defaultValue;
            }

            var rst = _dataSet.Tables[index].Rows[0][column];
            if (DBNull.Value.Equals(rst))
            {
                return defaultValue;
            }

            if (typeof(T) == typeof(bool))
            {
                rst = Convert.ToBoolean(rst);
            }

            return (T)rst;
        }

        public List<T> GetValueList<T>(int index, string column)
        {
            if (GetRowCount(index) < 1)
            {
                return new List<T>();
            }

            List<T> rst = new List<T>();
            foreach (DataRow row in _dataSet.Tables[index].Rows)
            {
                rst.Add((T)row[column]);
            }

            return rst;
        }

        public T GetObject<T>(int index) where T : new()
        {
            if (GetRowCount(index) < 1)
            {
                return default(T);
            }

            var result = JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(_dataSet.Tables[index]));
            return result[0];
        }

        public List<T> GetObjectList<T>(int index) where T : new()
        {
            if (GetRowCount(index) < 1)
            {
                return new List<T>();
            }

            var tt = JsonConvert.SerializeObject(_dataSet.Tables[index]);

            return JsonConvert.DeserializeObject<List<T>>(JsonConvert.SerializeObject(_dataSet.Tables[index]));
        }

    }
}
