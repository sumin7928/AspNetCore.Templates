using ApiWebServer.PBTables;

namespace ApiWebServer.Cache.PBTables
{
    public interface ICommonPBTable
    {
        bool LoadTable( MaguPBTableContext context );
    }
}
