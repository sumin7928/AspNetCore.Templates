using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.DB
{
    public class DbService : IDbService
    { 
        public Dictionary<string, string> ConnStrings { get; private set; }

        public IDbConnection this[string name]
        {
            get
            {
                return new SqlConnection(ConnStrings[name]);
            }
        }

        public DbService(Dictionary<string,string> connStrings)
        {
            ConnStrings = connStrings;
        }
    }
}
