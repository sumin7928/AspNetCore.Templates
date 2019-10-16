using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using ApiWebServer.Common.Define;
using ApiWebServer.Database;
using ApiWebServer.Models;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Logic
{
    public class ConsumeReward
    {
        private readonly long pcid;
        private readonly GameDB gameDB;

        private ErrorCode RewardErrorCodeFirst;

        private string ItemListStr = "";

        private List<GameRewardInfo> ConsumeList;
        private List<GameRewardInfo> RewardList;

        private List<ItemInven> UpdateItemList;
        private List<ItemInven> ConsumeItemList;

        private List<Player> AddPlayerList;                 //우편함 받기에서만 쓰임
        private List<Coach> AddCoachList;                   //우편함 받기에서만 쓰임

        public bool postReceive = false;
        private List<GameRewardInfo> FailedRewadList;

        private bool isFinished;
        private bool isRewardFailAllow;

        //processType: consume만 진행할경우 Consume, reward만 진행할경우 Reward, 둘다 일경우 ConsumeReward
        //isRewardFailAllow 리워드시 실패가 되도 진행할 것인지 선택  (컨슘 에러시는 해당값과 상관없이 무조건 실패)
        public ConsumeReward(long pcId, GameDB gameDB, CONSUME_REWARD_TYPE processType, bool isRewardFailAllow, bool postReceive = false)
        {
            //1이면 add, 2이면 consume, 3이면 둘다
            pcid = pcId;
            this.gameDB = gameDB;

            isFinished = false;
            RewardErrorCodeFirst = ErrorCode.SUCCESS;

            this.postReceive = postReceive;   

            if (processType == CONSUME_REWARD_TYPE.CONSUME)
            {
                ConsumeList = new List<GameRewardInfo>();
            }
            else if (processType == CONSUME_REWARD_TYPE.REWARD)
            {
                RewardList = new List<GameRewardInfo>();
            }
            else
            {
                ConsumeList = new List<GameRewardInfo>();
                RewardList = new List<GameRewardInfo>();
            }

            this.isRewardFailAllow = isRewardFailAllow;
            if (isRewardFailAllow == true)
            {
                FailedRewadList = new List<GameRewardInfo>();
            }
        }

        public void AddReward(GameRewardInfo info)
        {
            if (info == null)
                return;

            RewardList.Add(info);
        }

        public void AddReward(List<GameRewardInfo> listInfo)
        {
            if (listInfo == null)
                return;

            RewardList.AddRange(listInfo);
        }

        public void AddConsume(GameRewardInfo info)
        {
            if (info == null)
                return;

            ConsumeList.Add(info);
        }

        public void AddConsume(List<GameRewardInfo> listInfo)
        {
            if (listInfo == null)
                return;

            ConsumeList.AddRange(listInfo);
        }

        public ErrorCode Run(ref AccountGame accountGameInfo, bool isOnlyCurrency)
        {
            if (isFinished == true)
                return ErrorCode.ERROR_FINISHED_PROCESS;

            ErrorCode errCode;

            //취합하기
            if (ConsumeList != null && ConsumeList.Count > 0)
            {
                errCode = ConsumeCurrencyAndCheckItem(isOnlyCurrency, ref accountGameInfo);
                if (errCode != ErrorCode.SUCCESS)
                    return errCode;

            }

            if (RewardList != null && RewardList.Count > 0)
            {
                errCode = RewardCurrencyAndCheckItem(isOnlyCurrency, ref accountGameInfo);
                if (errCode != ErrorCode.SUCCESS)
                    return errCode;
            }

            if (string.IsNullOrEmpty(ItemListStr) == false)
            {
                List<ItemInven> haveInvenItemInfo = GetHaveInvenItem();
                if (haveInvenItemInfo == null)
                    return ErrorCode.ERROR_DB;

                if (ConsumeItemList != null)
                {
                    if (UpdateItemList == null)
                        UpdateItemList = new List<ItemInven>();

                    foreach (ItemInven consumeInfo in ConsumeItemList)
                    {
                        ItemInven haveitemInfo = haveInvenItemInfo.Find(x => x.item == consumeInfo.item);
                        //이렇게 가져온 itemInfo 를 고쳐도 원래 값까지 수정되는지 확인해보자
                        if (haveitemInfo == null || haveitemInfo.cnt < consumeInfo.cnt)
                        {
                            return ErrorCode.ERROR_CONSUME_NOT_ENOUGH_COUNT;
                        }

                        //현재 리스트에 같은 아이템이 있는지 확인(한꺼번에 db update를 하기위해서)
                        ItemInven itemInfo = UpdateItemList.Find(x => x.item == consumeInfo.item);

                        if (itemInfo != null)
                            itemInfo.cnt -= consumeInfo.cnt;
                        else
                        {
                            var itemInven = new ItemInven
                            {
                                item = consumeInfo.item,
                                cnt = consumeInfo.cnt * -1
                            };

                            UpdateItemList.Add(itemInven);
                        }
                    }
                }

                //아이템 갱신
                UpdateItem(haveInvenItemInfo);
            }

            //선수카드 갱신
            //코치카드 갱신
            //장비카드 갱신

            isFinished = true;

            return ErrorCode.SUCCESS;
        }

        private ErrorCode ConsumeCurrencyAndCheckItem(bool isOnlyCurrency, ref AccountGame accountGameInfo)
        {
            ConsumeItemList = null;

            foreach (GameRewardInfo info in ConsumeList)
            {
                if (info.reward_cnt <= 0)
                {
                    return ErrorCode.ERROR_CONSUME_INVALID_INFO;
                }

                switch ((REWARD_TYPE)info.reward_type)
                {
                    case REWARD_TYPE.DIA:               //다이아
                        if ((long)accountGameInfo.dia + accountGameInfo.event_dia < info.reward_cnt)
                            return ErrorCode.ERROR_CONSUME_NOT_ENOUGH_COUNT;
                        else
                        {
                            if (accountGameInfo.dia < info.reward_cnt)
                            {
                                accountGameInfo.event_dia -= (info.reward_cnt - accountGameInfo.dia);
                                accountGameInfo.dia = 0;
                            }
                            else
                            {
                                accountGameInfo.dia -= info.reward_cnt;
                            }
                        }
                        break;
                    case REWARD_TYPE.GOLD:              //골드
                        if (accountGameInfo.gold < info.reward_cnt)
                            return ErrorCode.ERROR_CONSUME_NOT_ENOUGH_COUNT;
                        else
                            accountGameInfo.gold -= info.reward_cnt;
                        break;
                    case REWARD_TYPE.TRAIN_POINT:       //훈련포인트
                        if (accountGameInfo.train_point < info.reward_cnt)
                            return ErrorCode.ERROR_CONSUME_NOT_ENOUGH_COUNT;
                        else
                            accountGameInfo.train_point -= info.reward_cnt;
                        break;
                    case REWARD_TYPE.MASTERY_POINT:
                        if (accountGameInfo.mastery_point < info.reward_cnt)
                            return ErrorCode.ERROR_CONSUME_NOT_ENOUGH_COUNT;
                        else
                            accountGameInfo.mastery_point -= info.reward_cnt;
                        break;
                    default:
                        {
                            if (isOnlyCurrency == true)
                            {
                                return ErrorCode.ERROR_CONSUME_INVALID_INFO;
                            }

                            if ((REWARD_TYPE)info.reward_type == REWARD_TYPE.NORMAL_ITEM)
                            {
                                //아이템이 하나라도 있으면 리스트 생성
                                if (ConsumeItemList == null)
                                {
                                    ConsumeItemList = new List<ItemInven>();
                                }

                                //현재 리스트에 같은 아이템이 있는지 확인(한꺼번에 db update를 하기위해서)
                                ItemInven itemInfo = ConsumeItemList.Find(x => x.item == info.reward_idx);

                                if (itemInfo != null)
                                    itemInfo.cnt = AddCount(itemInfo.cnt, info.reward_cnt);
                                else
                                {
                                    var itemInven = new ItemInven
                                    {
                                        item = info.reward_idx,
                                        cnt = info.reward_cnt
                                    };

                                    ConsumeItemList.Add(itemInven);
                                    ItemListStr += (ItemListStr == "") ? info.reward_idx.ToString() : "," + info.reward_idx.ToString();
                                }
                            }
                            //else if((REWARD_TYPE)info.reward_type == REWARD_TYPE.PLAYER_CARD)
                            //{
                            //
                            //}
                            else
                            {
                                return ErrorCode.ERROR_CONSUME_INVALID_INFO;
                            }

                        }
                        break;

                }
            }

            return ErrorCode.SUCCESS;
        }

        private ErrorCode RewardCurrencyAndCheckItem(bool isOnlyCurrency, ref AccountGame accountGameInfo)
        {
            foreach (GameRewardInfo info in RewardList)
            {
                if (info.reward_cnt <= 0)
                {
                    if (isRewardFailAllow == true)
                    {
                        if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                            RewardErrorCodeFirst = ErrorCode.ERROR_DB_DATA;

                        FailedRewadList.Add(info);
                        continue;
                    }
                    else
                    {
                        return ErrorCode.ERROR_REWARD_INVALID_INFO;
                    }
                }

                switch ((REWARD_TYPE)info.reward_type)
                {
                    case REWARD_TYPE.DIA:     //다이아
                        accountGameInfo.dia = AddCount(accountGameInfo.dia, info.reward_cnt);
                        break;
                    case REWARD_TYPE.GOLD:     //골드
                        accountGameInfo.gold = AddCount(accountGameInfo.gold, info.reward_cnt);
                        break;
                    case REWARD_TYPE.TRAIN_POINT:     //훈련포인트
                        accountGameInfo.train_point = AddCount(accountGameInfo.train_point, info.reward_cnt);
                        break;
                    case REWARD_TYPE.MASTERY_POINT:     //마스터리포인트
                        accountGameInfo.mastery_point = AddCount(accountGameInfo.mastery_point, info.reward_cnt);
                        break;
                    case REWARD_TYPE.DIA_BONUS:     //보너스다이아
                        accountGameInfo.event_dia = AddCount(accountGameInfo.event_dia, info.reward_cnt);
                        break;
                    default:
                        {
                            if (isOnlyCurrency == true)
                            {
                                if (isRewardFailAllow == true)
                                {
                                    if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                        RewardErrorCodeFirst = ErrorCode.ERROR_DB_DATA;

                                    FailedRewadList.Add(info);
                                    continue;
                                }
                                else
                                {
                                    return ErrorCode.ERROR_REWARD_INVALID_INFO;
                                }
                            }

                            if ((REWARD_TYPE)info.reward_type == REWARD_TYPE.NORMAL_ITEM)
                            {
                                //아이템이 하나라도 있으면 리스트 생성
                                if (UpdateItemList == null)
                                    UpdateItemList = new List<ItemInven>();

                                //현재 리스트에 같은 아이템이 있는지 확인(한꺼번에 db update를 하기위해서)
                                ItemInven itemInfo = UpdateItemList.Find(x => x.item == info.reward_idx);

                                if (itemInfo != null)
                                    itemInfo.cnt = AddCount(itemInfo.cnt, info.reward_cnt);
                                else
                                {
                                    var itemInven = new ItemInven
                                    {
                                        item = info.reward_idx,
                                        cnt = info.reward_cnt
                                    };

                                    UpdateItemList.Add(itemInven);

                                    if (ConsumeItemList == null || ConsumeItemList.Find(x => x.item == info.reward_idx) == null)
                                        ItemListStr += (ItemListStr == "") ? info.reward_idx.ToString() : "," + info.reward_idx.ToString();
                                }
                            }
                            else
                            {
                                bool bFail = true;
                                if(postReceive == true)
                                {
                                    if ((REWARD_TYPE)info.reward_type == REWARD_TYPE.PLAYER_CARD)
                                    {
                                        if(accountGameInfo.now_player < accountGameInfo.max_player)
                                        {
                                            if (AddPlayer(info.reward_cnt, info.reward_idx) == true)
                                            {
                                                ++accountGameInfo.now_player;
                                                bFail = false; 
                                            }
                                            /*else      //나중에 선수 인덱스나 강화레벨 유효성에러는 다른에러로 처리할수있으므로 주석처리
                                            {
                                                if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                                    RewardErrorCodeFirst = ErrorCode.ERROR_DB_DATA;
                                            }*/
                                        }
                                        else
                                        {
                                            if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                                RewardErrorCodeFirst = ErrorCode.ERROR_PLAYER_MAX_COUNT_EXCESS;
                                        }
                                    }
                                    else if ((REWARD_TYPE)info.reward_type == REWARD_TYPE.COACH_CARD)
                                    {
                                        if (accountGameInfo.now_coach < accountGameInfo.max_coach)
                                        {
                                            if (AddCoach(info.reward_cnt) == true)
                                            {
                                                ++accountGameInfo.now_coach;
                                                bFail = false;
                                            }
                                            /*else      //나중에 코치 인덱스나 강화레벨 유효성에러는 다른에러로 처리할수있으므로 주석처리
                                            {
                                                if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                                    RewardErrorCodeFirst = ErrorCode.ERROR_DB_DATA;
                                            }*/
                                        }
                                        else
                                        {
                                            if (RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                                RewardErrorCodeFirst = ErrorCode.ERROR_COACH_MAX_COUNT_EXCESS;
                                        }
                                    }

                                }

                                if(bFail == true)
                                { 
                                    if (isRewardFailAllow == true)
                                    {
                                        if(RewardErrorCodeFirst == ErrorCode.SUCCESS)
                                            RewardErrorCodeFirst = ErrorCode.ERROR_DB_DATA;

                                        FailedRewadList.Add(info);
                                        continue;
                                    }
                                    else
                                    {
                                        return ErrorCode.ERROR_REWARD_INVALID_INFO;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return ErrorCode.SUCCESS;
        }


        private void UpdateItem(List<ItemInven> haveInvenItemInfo)
        {
            if (UpdateItemList == null)
                return;

            //획득 아이템 정보에 db에서 가져온 기존 아이템 갯수를 더해준다(나중에 바로 업뎃치려고함)
            foreach(ItemInven updateItem in UpdateItemList)
            {
                int itemIdx = updateItem.item;
                ItemInven itemInfo = haveInvenItemInfo.Find(x => x.item == itemIdx);

                if (itemInfo != null)
                {
                    updateItem.cnt = AddCount(itemInfo.cnt, updateItem.cnt);
                }
            }

            return;
        }

        private List<ItemInven> GetHaveInvenItem()
        {
            //남은슬롯만 보고 넘어갈수도 있지만 최종 갯수를 알기위해 무조건 db에서 획득할 아이템정보를 가져온다.
            DataSet dataSet = gameDB.USP_GS_GM_ACCOUNT_ITEM_R(pcid, ItemListStr);
            if (dataSet == null)
            {
                if (UpdateItemList != null)
                {
                    UpdateItemList.Clear();
                    UpdateItemList = null;
                }

                return null;
            }
            if ( dataSet.Tables[ 0 ].Rows.Count > 0 )
                return JsonConvert.DeserializeObject<List<ItemInven>>( JsonConvert.SerializeObject( dataSet.Tables[ 0 ] ) );
            else
                return new List<ItemInven>();
        }

        private int AddCount(int srcGoodsCnt, int addGoodsCnt)
        {
            if (addGoodsCnt > int.MaxValue - srcGoodsCnt)
                return int.MaxValue;
            else
                return srcGoodsCnt + addGoodsCnt;
        }

        private double AddCount(double srcGoodsCnt, double addGoodsCnt)
        {
            if (addGoodsCnt > double.MaxValue - srcGoodsCnt)
                return double.MaxValue;
            else
                return srcGoodsCnt + addGoodsCnt;
        }

        private double AddCount(double srcGoodsCnt, int addGoodsCnt)
        {
            if (addGoodsCnt > double.MaxValue - srcGoodsCnt)
                return double.MaxValue;
            else
                return srcGoodsCnt + addGoodsCnt;
        }

        private bool AddPlayer(int playerIdx, int reinforceGrade)
        {
            if(reinforceGrade > PlayerDefine.PlayerLimitUpMax)
                return false;

            Player playerInfo = Cache.CacheManager.PBTable.PlayerTable.CreatePlayerInfo(playerIdx, reinforceGrade);

            if(playerInfo == null)
            {
                return false;
            }

            if (AddPlayerList == null)
                AddPlayerList = new List<Player>();

            playerInfo.account_player_idx = AddPlayerList.Count;

            AddPlayerList.Add(playerInfo);


            return true;
        }

        private bool AddCoach(int coachIdx)
        {
            Coach coachInfo = Cache.CacheManager.PBTable.PlayerTable.CreateCoachInfo(coachIdx);

            if (coachInfo == null)
            {
                return false;
            }

            if (AddCoachList == null)
                AddCoachList = new List<Coach>();

            coachInfo.account_coach_idx = AddCoachList.Count;

            AddCoachList.Add(coachInfo);

            return true;
        }

        public List<ItemInven> GetUpdateItemList()
        {
            return UpdateItemList;
        }

        public List<GameRewardInfo> GetFailedRewadList()
        {
            return FailedRewadList;
        }

        public List<GameRewardInfo> GetRewardList()
        {
            return RewardList;
        }

        public List<Player> GetAddPlayerList()
        {
            return AddPlayerList;
        }

        public List<Coach> GetAddCoachList()
        {
            return AddCoachList;
        }

        public int GetRewardCount()
        {
            return RewardList.Count;
        }

        public ErrorCode GetRewardErrorFirst()
        {
            return RewardErrorCodeFirst;
        }
    }
}
