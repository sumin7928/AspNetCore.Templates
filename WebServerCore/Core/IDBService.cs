using ApiWebServer.Database;

namespace ApiWebServer.Core
{
    public interface IDBService
    {
        AccountDB CreateAccountDB( long requestNo );
        GameDB CreateGameDB( long requestNo, byte dbNum );
        PostDB CreatePostDB( long requestNo, byte dbNum );
    }
}
