using System.Collections.Generic;
using System.Linq;
using ApiWebServer.PBTables;

namespace ApiWebServer.Cache.PBTables
{
    public class ForbiddenWordTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<string> _forbiddenWord = new List<string>();

        public bool LoadTable( MaguPBTableContext context )
        {
            // PB_SLANG
            foreach ( var data in context.PB_SLANG.ToList() )
            {
                _forbiddenWord.Add( data.slang );
            }

            return true;
        }

        /// <summary>
        /// 금지어인가
        /// </summary>
        public bool IsForbiddenWord( string word )
        {
            foreach ( string forbiddenWord in _forbiddenWord )
            {
                if (word.IndexOf(forbiddenWord) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
