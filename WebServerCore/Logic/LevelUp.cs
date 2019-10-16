using ApiWebServer.Cache;
using ApiWebServer.Models;
using WebSharedLib.Entity;

namespace ApiWebServer.Logic
{
    public class LevelUp
    {
        private AccountGame _accountGame;

        public LevelUp( AccountGame accountGame )
        {
            _accountGame = accountGame;
        }
        public LevelUp( int userLevel, int userExp, int userMasteryPoint )
        {
            _accountGame = new AccountGame
            {
                user_lv = userLevel,
                user_exp = userExp,
                mastery_point = userMasteryPoint
            };
        }

        public void AddExp( int addExpValue, out bool isLevelUp )
        {
            isLevelUp = false;

            CacheManager.PBTable.ManagerTable.AddExpResult( _accountGame.user_lv, _accountGame.user_exp, addExpValue,
                                                                            out int afterLv, out int afterExp, out int addMasteryPoint );

            if ( _accountGame.user_lv < afterLv )
            {
                isLevelUp = true;
            }

            _accountGame.mastery_point += addMasteryPoint;
            _accountGame.user_lv = afterLv;
            _accountGame.user_exp = afterExp;
        }
    }
}
