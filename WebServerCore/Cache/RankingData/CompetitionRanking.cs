using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSharedLib.Entity;

namespace ApiWebServer.Cache.RankingData
{
    public class CompetitionRanking
    {
        private Dictionary<int, List<RankingInfo>> _rankingData = new Dictionary<int, List<RankingInfo>>();

        public int NowSeasonIdx { get; set; }
        public long ExpiredCacheTime { get; set; }

        public List<RankingInfo> GetRankData(int seasonIdx)
        {
            if(_rankingData.ContainsKey(seasonIdx) == false)
            {
                return null;
            }

            return _rankingData[seasonIdx];
        }
        public void SetRankData(int seasonIdx, List<RankingInfo> info)
        {
            if (_rankingData.ContainsKey(seasonIdx) == false)
            {
                lock(_rankingData)
                {
                    if (_rankingData.ContainsKey(seasonIdx) == false)
                    {
                        _rankingData.Add(seasonIdx, info);
                    }
                }
            }
            else
            {
                _rankingData[seasonIdx] = info;
            }
        }

        public void RemoveRankData(int removeSeasonIdx)
        {
            _rankingData.Remove(removeSeasonIdx);
        }
    }
}
