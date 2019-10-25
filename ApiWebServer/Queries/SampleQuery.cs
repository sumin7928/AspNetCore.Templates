using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Queries
{
    public static class SampleQuery
    {
        public static string SampleTable_C = "INSERT INTO SampleTable VALUES (@AccountNo, @Name, @Address)";
        public static string SampleTable_R = "SELECT Name, Address FROM SampleTable WHERE AccountNo = @AccountNo";
        public static string SampleTable_U = "UPDATE SampleTable SET Address = @Address WHERE AccountNo = @AccountNo";
        public static string SampleTable_D = "DELETE SampleTable WHERE AccountNo = @AccountNo";
    }
}
