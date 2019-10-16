using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Database;

namespace ApiWebServer.Core
{
    public class DBService : IDBService
    {
        public readonly string accoutDBName = "AccountDB";
        public readonly string gameDBName = "GameDB";
        public readonly string postDBName = "PostDB";
        public readonly string tableDBName = "TableDB";

        public string AccountDBConnString { get; private set; }
        public string TableDBConnString { get; private set; }
        public Dictionary<int, string> GameDBConnString { get; private set; }
        public Dictionary<int, string> PostDBConnString { get; private set; }

        public DBService( IConfiguration configuration )
        {
            AccountDBConnString = configuration[ accoutDBName ];
            TableDBConnString = configuration[ tableDBName ];

            GameDBConnString = configuration.AsEnumerable().Where( x => x.Key.Contains( gameDBName ) && x.Key.Split( "_" ).Length >= 2 )
                .ToDictionary( x =>
                {
                    var splited = x.Key.Split( "_" );
                    return int.Parse( splited[ 1 ] );
                }, x => x.Value );

            PostDBConnString = configuration.AsEnumerable().Where( x => x.Key.Contains( postDBName ) && x.Key.Split( "_" ).Length >= 2 )
                .ToDictionary( x =>
                {
                    var splited = x.Key.Split( "_" );
                    return int.Parse( splited[ 1 ] );
                }, x => x.Value );
        }

        public AccountDB CreateAccountDB( long requestNo )
        {
            return new AccountDB( AccountDBConnString, requestNo );
        }

        public GameDB CreateGameDB( long requestNo, byte dbNum )
        {
            return new GameDB( GameDBConnString[ dbNum ], requestNo );
        }

        public PostDB CreatePostDB( long requestNo, byte dbNum )
        {
            return new PostDB( PostDBConnString[ dbNum ], requestNo );
        }
    }
}
