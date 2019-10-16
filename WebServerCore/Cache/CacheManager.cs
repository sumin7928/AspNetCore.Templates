using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Cache.PBTables;
using ApiWebServer.Cache.RankingData;
using ApiWebServer.PBTables;

namespace ApiWebServer.Cache
{

    public class PBTable : IDisposable
    {
        public ConstantTable ConstantTable { get; set; } = new ConstantTable();
        public PlayerTable PlayerTable { get; set; } = new PlayerTable();
        public VersionTable VersionTable { get; set; } = new VersionTable();
        public ForbiddenWordTable ForbiddenWordTable { get; set; } = new ForbiddenWordTable();
        public ManagerTable ManagerTable { get; set; } = new ManagerTable();
        public MessageTable MessageTable { get; set; } = new MessageTable();
        public MissionAchievementTable MissionAchievementTable { get; set; } = new MissionAchievementTable();
        public CareerModeTable CareerModeTable { get; set; } = new CareerModeTable();
        public ItemTable ItemTable { get; set; } = new ItemTable();
        public LiveSeasonTable LiveSeasonTable { get; set; } = new LiveSeasonTable();

        public void Dispose()
        {
            ConstantTable = null;
            PlayerTable = null;
            VersionTable = null;
            ForbiddenWordTable = null;
            ManagerTable = null;
            MessageTable = null;
            MissionAchievementTable = null;
            CareerModeTable = null;
            ItemTable = null;
            LiveSeasonTable = null;
        }
    }

    public class CacheManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static PBTable PBTable { get; private set; }

        public static PBTable LoadingTable { get; private set; }

        public static CompetitionRanking CompetitonRanking { get; private set; } = new CompetitionRanking();

        public static bool LoadPBTable(MaguPBTableContext context)
        {
            LoadingTable = new PBTable();

            Dictionary<string, ICommonPBTable> tableList = new Dictionary<string, ICommonPBTable>
            {
                { typeof( ConstantTable ).Name, LoadingTable.ConstantTable = new ConstantTable() },
                { typeof( PlayerTable ).Name, LoadingTable.PlayerTable = new PlayerTable() },
                { typeof( VersionTable ).Name, LoadingTable.VersionTable =  new VersionTable() },
                { typeof( ForbiddenWordTable ).Name, LoadingTable.ForbiddenWordTable = new ForbiddenWordTable() },
                { typeof( ManagerTable ).Name, LoadingTable.ManagerTable = new ManagerTable() },
                { typeof( MessageTable ).Name, LoadingTable.MessageTable = new MessageTable() },
                { typeof( MissionAchievementTable ).Name, LoadingTable.MissionAchievementTable = new MissionAchievementTable() },
                { typeof( CareerModeTable ).Name, LoadingTable.CareerModeTable = new CareerModeTable() },
                { typeof( ItemTable ).Name, LoadingTable.ItemTable = new ItemTable() },
                { typeof( LiveSeasonTable ).Name, LoadingTable.LiveSeasonTable = new LiveSeasonTable() }
            };
            try
            {
                foreach (KeyValuePair<string, ICommonPBTable> table in tableList)
                {
                    if (table.Value.LoadTable(context) == false)
                    {
                        return false;
                    }
                }

                // swap tables
                if (PBTable != null)
                {
                    lock (PBTable)
                    {
                        PBTable.Dispose();
                        PBTable = LoadingTable;
                    }

                    _logger.Info("Complete reload PBTable for cache");
                }
                else
                {
                    PBTable = LoadingTable;
                }

                LoadingTable = null;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load PB data from table -");
                return false;

            }
            return true;
        }
    }
}
