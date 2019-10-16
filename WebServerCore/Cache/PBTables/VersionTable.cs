using System.Collections.Generic;
using System.Linq;
using ApiWebServer.PBTables;

namespace ApiWebServer.Cache.PBTables
{
    public class VersionTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int VERSION_SIZE = 4;

        // 버전 [main].[major].[minor].[bugfix]
        private int[] _arrVersion = new int[ VERSION_SIZE ];               
        // CDN 주소 : Dictionary<국가,<OS, url>>
        private Dictionary<byte, Dictionary<byte, string>> _CDNUrl = new Dictionary<byte, Dictionary<byte, string>>();
        // 마켓 주소 : Dictionary<국가,<market, url>>
        private Dictionary<byte, Dictionary<int, string>> _marketUrl = new Dictionary<byte, Dictionary<int, string>>(); 

        public bool LoadTable( MaguPBTableContext context )
        {
            // PB_VERSION
            //foreach ( var data in context.PbVersion.ToList() )
            //{
            //    _arrVersion.Add( data );
            //}

            // PB_CDN_URL
            foreach ( var data in context.PB_CDN_URL.ToList() )
            {
                if( _CDNUrl.ContainsKey( data.country_type ) == false )
                {
                    _CDNUrl.Add( data.country_type, new Dictionary<byte, string>() );
                }

                _CDNUrl[ data.country_type ].Add( data.os_type, data.url );
            }

            // PB_MARKET_URL
            foreach ( var data in context.PB_MARKET_URL.ToList() )
            {
                if ( _marketUrl.ContainsKey( data.country_type ) == false )
                {
                    _marketUrl.Add( data.country_type, new Dictionary<int, string>() );
                }

                _marketUrl[ data.country_type ].Add( data.market_type, data.url );
            }

            return true;
        }

        public int GetVersion( int index )
        {
            if ( index < 0 || index >= VERSION_SIZE )
            {
                return 0;
            }

            return _arrVersion[ index ];
        }
        
        public string GetCDNUrl( byte country, byte os )
        {
            Dictionary<byte, string> dicURL;
            if ( _CDNUrl.TryGetValue( country, out dicURL ) )
            {
                string url;
                if ( dicURL.TryGetValue( os, out url ) )
                {
                    return url;
                }
            }

            return null;
        }
        
        public string GetMarketUrl( byte country, int market )
        {
            Dictionary<int, string> dicURL;
            if ( _marketUrl.TryGetValue( country, out dicURL ) )
            {
                string url;
                if ( dicURL.TryGetValue( market, out url ) )
                {
                    return url;
                }
            }

            return null;
        }
    }
}
