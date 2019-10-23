using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ApiServer.Core.DB
{
    public interface IDbService
    {
        IDbConnection this[string name] { get; }
    }
}
