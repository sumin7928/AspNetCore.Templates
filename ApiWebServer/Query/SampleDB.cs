using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ApiServer.Database
{
    public class SampleDB
    {
        private readonly IConfiguration _config;

        public SampleDB(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("MyConnectionString"));
            }
        }

        public async Task GetTempData()
        {
            using (IDbConnection conn = Connection)
            {
                var result = await conn.QueryAsync("query");
            }
        }

    }
}
