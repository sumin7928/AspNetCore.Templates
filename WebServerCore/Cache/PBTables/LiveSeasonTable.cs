using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Common.Define;
using ApiWebServer.Common;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Entity;

namespace ApiWebServer.Cache.PBTables
{
    public class LiveSeasonTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public SortedDictionary<int, PB_COMPETITIVE_PLAY> _competitivePlay { get; private set; } = new SortedDictionary<int, PB_COMPETITIVE_PLAY>();
        private Dictionary<int, Dictionary<int, List<PB_COMPETITIVE_TEAM_LINEUP>>> _competitiveBotLineup = new Dictionary<int, Dictionary<int, List<PB_COMPETITIVE_TEAM_LINEUP>>>();
        private Dictionary<byte, List<int>> _countryTeams = new Dictionary<byte, List<int>>();
        private Dictionary<int, int> _coachSkills = new Dictionary<int, int>();

        public bool LoadTable(MaguPBTableContext context)
        {
            // PB_COMPETITIVE_PLAY
            foreach (var data in context.PB_COMPETITIVE_PLAY.ToList())
            {
                _competitivePlay.Add(data.idx, data);
            }

            // PB_TEAM_INFO
            foreach (var data in context.PB_TEAM_INFO.ToList())
            {
                if (_countryTeams.ContainsKey(data.country_flg) == false)
                {
                    _countryTeams.Add(data.country_flg, new List<int>());
                }

                _countryTeams[data.country_flg].Add(data.team_idx);
            }

            // PB_COACH
            foreach (var data in context.PB_COACH.ToList())
            {
                _coachSkills.Add(data.coach_idx, data.coaching_skill);
            }


            // PB_COMPETITIVE_TEAM_LINEUP
            foreach (var data in context.PB_COMPETITIVE_TEAM_LINEUP.ToList())
            {
                if (data.isuse == 0)
                {
                    continue;
                }

                if (_competitiveBotLineup.ContainsKey(data.rank) == false)
                {
                    _competitiveBotLineup.Add(data.rank, new Dictionary<int, List<PB_COMPETITIVE_TEAM_LINEUP>>());
                }

                var groupData = _competitiveBotLineup[data.rank];
                if (groupData.ContainsKey(data.lineup_group_idx) == false)
                {
                    groupData.Add(data.lineup_group_idx, new List<PB_COMPETITIVE_TEAM_LINEUP>());
                }

                groupData[data.lineup_group_idx].Add(data);
            }

            return true;
        }

        public bool IsContainRaingIdx(int ratingIdx)
        {
            return _competitivePlay.ContainsKey(ratingIdx);
        }

        public byte GetMatchingTarget(int ratingIdx)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[ratingIdx];
            return competitivePlayData.matching_target;
        }

        public List<GameRewardInfo> GetPromotionRatingReward(int ratingIdx)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[ratingIdx];

            List<GameRewardInfo> rewardList = null;

            if (competitivePlayData.rankup_reward_type1 > 0)
            {
                AddReward(competitivePlayData.rankup_reward_type1, competitivePlayData.rankup_reward_idx1, competitivePlayData.rankup_reward_count1, ref rewardList);
            }
            if (competitivePlayData.rankup_reward_type2 > 0)
            {
                AddReward(competitivePlayData.rankup_reward_type2, competitivePlayData.rankup_reward_idx2, competitivePlayData.rankup_reward_count2, ref rewardList);
            }
            if (competitivePlayData.rankup_reward_type3 > 0)
            {
                AddReward(competitivePlayData.rankup_reward_type3, competitivePlayData.rankup_reward_idx3, competitivePlayData.rankup_reward_count3, ref rewardList);
            }

            return rewardList;
        }

        public List<GameRewardInfo> GetCompetitionSeasonReward(int seasonRewardIdx)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[seasonRewardIdx];

            List<GameRewardInfo> rewardList = null;

            if (competitivePlayData.season_reward_type1 > 0)
            {
                AddReward(competitivePlayData.season_reward_type1, competitivePlayData.season_reward_idx1, competitivePlayData.season_reward_count1, ref rewardList);
            }
            if (competitivePlayData.season_reward_type2 > 0)
            {
                AddReward(competitivePlayData.season_reward_type2, competitivePlayData.season_reward_idx2, competitivePlayData.season_reward_count2, ref rewardList);
            }
            if (competitivePlayData.season_reward_type3 > 0)
            {
                AddReward(competitivePlayData.season_reward_type3, competitivePlayData.season_reward_idx3, competitivePlayData.season_reward_count3, ref rewardList);
            }

            return rewardList;
        }

        public void ResetCompetitionSeason(int nowRatingIdx, out int resetRatingIdx)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[nowRatingIdx];
            resetRatingIdx = competitivePlayData.season_reset_rank;
        }

        public List<GameRewardInfo> WinCompetition(CompetitionInfo info, out int addExp, out bool isRankUp)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[info.rating_idx];
            addExp = competitivePlayData.exp_manager_win;
            isRankUp = false;

            if (info.winning_streak >= 1)
            {
                info.point += competitivePlayData.straight_win_point;
                info.winning_streak += 1;
            }
            else
            {
                info.point += competitivePlayData.win_point;
                info.winning_streak = 1;
            }

            // 레전드 미만일경우 승급 처리 가능
            if (info.rating_idx < _competitivePlay.Last().Key)
            {
                // 승급 포인트 체크
                if (info.point > competitivePlayData.rankup_point)
                {
                    info.promotion_reward_idx = info.rating_idx;
                    info.point = info.point - competitivePlayData.rankup_point;

                    // 승급 처리
                    info.rating_idx = NextRating(info.rating_idx);
                    isRankUp = true;
                }
            }

            List<GameRewardInfo> rewardList = null;

            if (competitivePlayData.win_reward_type1 > 0)
            {
                AddReward(competitivePlayData.win_reward_type1, competitivePlayData.win_reward_idx1, competitivePlayData.win_reward_count1, ref rewardList);
            }
            if (competitivePlayData.win_reward_type2 > 0)
            {
                AddReward(competitivePlayData.win_reward_type2, competitivePlayData.win_reward_idx2, competitivePlayData.win_reward_count2, ref rewardList);
            }
            if (competitivePlayData.win_reward_type3 > 0)
            {
                AddReward(competitivePlayData.win_reward_type3, competitivePlayData.win_reward_idx3, competitivePlayData.win_reward_count3, ref rewardList);
            }

            return rewardList;
        }

        public List<GameRewardInfo> DropCompetition(CompetitionInfo info, out int addExp)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[info.rating_idx];
            addExp = competitivePlayData.exp_manager_lose;

            info.winning_streak = 0;

            List<GameRewardInfo> rewardList = null;

            if (competitivePlayData.lose_reward_type1 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type1, competitivePlayData.lose_reward_idx1, competitivePlayData.lose_reward_count1, ref rewardList);
            }
            if (competitivePlayData.lose_reward_type2 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type2, competitivePlayData.lose_reward_idx2, competitivePlayData.lose_reward_count2, ref rewardList);
            }
            if (competitivePlayData.lose_reward_type3 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type3, competitivePlayData.lose_reward_idx3, competitivePlayData.lose_reward_count3, ref rewardList);
            }

            return rewardList;
        }

        public List<GameRewardInfo> LoseCompetition(CompetitionInfo info, out int addExp, out bool isRankDown)
        {
            PB_COMPETITIVE_PLAY competitivePlayData = _competitivePlay[info.rating_idx];
            addExp = competitivePlayData.exp_manager_lose;
            isRankDown = false;

            info.point -= competitivePlayData.lose_point;

            if (info.winning_streak > 0)
            {
                info.winning_streak = 0;
            }

            if (info.point < 0)
            {
                if (info.rating_idx > _competitivePlay.First().Key)
                {
                    // 강등 포인트 체크
                    if (competitivePlayData.rankdown_lose_value > 0)
                    {
                        info.winning_streak += -1;

                        if (info.winning_streak <= -competitivePlayData.rankdown_lose_value)
                        {
                            // 강등 처리
                            info.rating_idx = BeforeRating(info.rating_idx);
                            info.point = _competitivePlay[info.rating_idx].rankup_point;
                            isRankDown = true;
                            info.winning_streak = 0;
                        }
                    }
                }
            }
            if (info.point < 0)
            {
                info.point = 0;
            }

            List<GameRewardInfo> rewardList = null;

            if (competitivePlayData.lose_reward_type1 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type1, competitivePlayData.lose_reward_idx1, competitivePlayData.lose_reward_count1, ref rewardList);
            }
            if (competitivePlayData.lose_reward_type2 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type2, competitivePlayData.lose_reward_idx2, competitivePlayData.lose_reward_count2, ref rewardList);
            }
            if (competitivePlayData.lose_reward_type3 > 0)
            {
                AddReward(competitivePlayData.lose_reward_type3, competitivePlayData.lose_reward_idx3, competitivePlayData.lose_reward_count3, ref rewardList);
            }

            return rewardList;
        }

        public BattleInfo GetBotBattleData(byte nationType, int ratingIdx)
        {
            // TODO: 후에 국가 처리 나오면 넣기로..
            //int teamIdx = RandomManager.Instance.GetRandomFromList(_countryTeams[nationType]);
            int index = RandomManager.Instance.GetIndex(_competitiveBotLineup[ratingIdx].Count);
            int teamIdx = _competitiveBotLineup[ratingIdx].ElementAt(index).Key;
            List<PB_COMPETITIVE_TEAM_LINEUP> botData = _competitiveBotLineup[ratingIdx].ElementAt(index).Value;
            List<BattlePlayer> playerList = new List<BattlePlayer>();
            List<BattleCoach> coachList = new List<BattleCoach>();
            foreach (var data in botData)
            {
                if (data.player_type == (byte)PLAYER_TYPE.TYPE_COACH)
                {
                    BattleCoach coach = new BattleCoach();
                    coach.position = data.position;
                    coach.coach_idx = data.player_idx;
                    coach.coaching_skill = _coachSkills[(int)data.player_idx];
                    coach.leadership_idx1 = data.player_potential_1;
                    coach.leadership_idx2 = data.player_potential_2;
                    coach.leadership_idx3 = data.player_potential_3;

                    coachList.Add(coach);
                }
                else
                {
                    BattlePlayer player = new BattlePlayer();
                    player.position = data.position;
                    player.player_idx = data.player_idx;
                    player.player_type = data.player_type;
                    player.order = data.order;
                    player.reinforce_grade = data.player_reinforce_grade;
                    player.potential_idx1 = data.player_potential_1;
                    player.potential_idx2 = data.player_potential_2;
                    player.potential_idx3 = data.player_potential_3;

                    playerList.Add(player);
                }
            }

            return new BattleInfo()
            {
                team_Idx = teamIdx,
                rating_idx = ratingIdx,
                nick_name = "bot",
                nation_type = nationType,
                player_list = playerList,
                coach_list = coachList
            };
        }

        public bool IsLastRank(int ratingIdx)
        {
            return ratingIdx == _competitivePlay.Last().Key;
        }

        public string GetRankKey(int seasonIdx)
        {
            return $"rank:all:{seasonIdx}_{_competitivePlay.Last().Key}";
        }

        public string GetMatchKey(int seasonIdx, int ratingIdx)
        {
            return $"match:{seasonIdx}_{ratingIdx}";
        }

        public string GetBattleKey(long user)
        {
            return $"battle:{user}";
        }

        public int NextRating(int ratingIdx, int offset = 1)
        {
            int nextIdx = ratingIdx;

            for (int i = 0; i < offset; ++i)
            {
                if (ratingIdx < _competitivePlay.Last().Key)
                {
                    nextIdx = ratingIdx + 1;
                    if (_competitivePlay.ContainsKey(nextIdx) == false)
                    {
                        nextIdx = ((ratingIdx / 100) + 1) * 100 + 1;
                    }
                }
            }

            return nextIdx;
        }

        public int BeforeRating(int ratingIdx, int offset = 1)
        {
            int beforeIdx = ratingIdx;

            for (int i = 0; i < offset; ++i)
            {
                if (ratingIdx > _competitivePlay.First().Key)
                {
                    beforeIdx = ratingIdx - 1;
                    if (_competitivePlay.ContainsKey(beforeIdx) == false)
                    {
                        beforeIdx = ((ratingIdx / 100) - 1) * 100 + 5;
                    }
                }
            }

            return beforeIdx;
        }

        private void AddReward(byte rewardType, int rewardIdx, int rewardCount, ref List<GameRewardInfo> rewardList)
        {
            if (rewardList == null)
            {
                rewardList = new List<GameRewardInfo>();
            }

            rewardList.Add(new GameRewardInfo()
            {
                reward_type = rewardType,
                reward_idx = rewardIdx,
                reward_cnt = rewardCount
            });
        }

    }
}
