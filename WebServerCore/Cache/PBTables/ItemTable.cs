using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Common.Define;
using ApiWebServer.PBTables;
using WebSharedLib.Entity;
using ApiWebServer.Models;
using WebSharedLib.Error;
using ApiWebServer.Common;

namespace ApiWebServer.Cache.PBTables
{
    public class ItemTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        const int correctionCardCoachPostion = 100;     //선수풀 키값에 필요
        const int correctionCardTeam = 1000;            //선수풀 키값에 필요

        private Dictionary<int, PB_ITEM> _totalItem = new Dictionary<int, PB_ITEM>();
        private Dictionary<int, List<GameRewardInfo>> _itemPackage = new Dictionary<int, List<GameRewardInfo>>();
        private Dictionary<int, List<PB_ITEM_GACHA>> _itemGacha = new Dictionary<int, List<PB_ITEM_GACHA>>();
        private Dictionary<int, PB_ITEM_SINGLE> _itemSingle = new Dictionary<int, PB_ITEM_SINGLE>();
        private Dictionary<int, PB_ITEM_CONTENTS> _itemContents = new Dictionary<int, PB_ITEM_CONTENTS>();

        private Dictionary<int, PB_ITEM_CARD> _itemCard = new Dictionary<int, PB_ITEM_CARD>();
        private Dictionary<int, Dictionary<int, List<ItemCard>>> _itemCardCaseData = new Dictionary<int, Dictionary<int, List<ItemCard>>>();
        private Dictionary<int, List<PB_ITEM_CARD_GACHA>> _itemCardGacha = new Dictionary<int, List<PB_ITEM_CARD_GACHA>>();
        private Dictionary<int, List<int>> _itemCardGachaRate = new Dictionary<int, List<int>>();

        //스카우트 (아이템 방식이므로 아이템 테이블에 넣어둠)
        private Dictionary<int, PB_SCOUT> _scoutTotal = new Dictionary<int, PB_SCOUT>();
        private Dictionary<int, PB_ITEM_CARD> _scoutBase = new Dictionary<int, PB_ITEM_CARD>();
        private Dictionary<int, Dictionary<int, List<ItemCard>>> _scoutBaseData = new Dictionary<int, Dictionary<int, List<ItemCard>>>();
        private Dictionary<int, List<PB_ITEM_CARD_GACHA>> _scoutGacha = new Dictionary<int, List<PB_ITEM_CARD_GACHA>>();
        private Dictionary<int, List<int>> _scoutGachaRate = new Dictionary<int, List<int>>();

        //전체 바인더 정보
        private Dictionary<int, PB_SCOUT_BINDER> _scoutBinder = new Dictionary<int, PB_SCOUT_BINDER>();
        //나라별 바인더 정보리스트
        private Dictionary<int, List<PB_SCOUT_BINDER>> nationScoutBinderInfo = new Dictionary<int, List<PB_SCOUT_BINDER>>();
        //나라별 바인더 확률 리스트(위의 바인더 정보리스트와 세트)
        private Dictionary<int, List<int>> nationScoutBinderRate = new Dictionary<int, List<int>>();

        //나라별, 바인더타입별(선수, 코치, 선수+코치), 레이트별 선수리스트 풀
        private Dictionary<int, Dictionary<int, Dictionary<int, List<ItemCard>>>> binderCharacterRatePoolData = new Dictionary<int, Dictionary<int, Dictionary<int, List<ItemCard>>>>();

        //국가별로 현재 설정된 스카우트 인덱스(Scout_Base에있는 인덱스)
        public Dictionary<int, int> scoutBasePlayerKey = new Dictionary<int, int>();
        public Dictionary<int, int> scoutBaseCoachKey = new Dictionary<int, int>();

        //바인더 초기화 비용(계산으로 이뤄지는거므로 미리 계산해둠)
        private int[] scoutBinderResetCost;

        public int itemIdxCareermodeSimulration = -1;
        public int itemIdxScoutBinderReset = -1;
        public int itemIdxScoutSearchDirect = -1;

        public bool LoadTable(MaguPBTableContext context)
        {
            // PB_ITEM
            foreach (var data in context.PB_ITEM.ToList())
            {
                if (data.use_flag == 1)
                    _totalItem.Add(data.item_idx, data);
            }

            // PB_ITEM_PACKAGE
            foreach (var data in context.PB_ITEM_PACKAGE.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_ITEM_PACKAGE)
                        return false;

                    if (_itemPackage.ContainsKey(data.item_idx) == false)
                        _itemPackage.Add(data.item_idx, new List<GameRewardInfo>());

                    _itemPackage[data.item_idx].Add(new GameRewardInfo(data.reward_type, data.reward_idx, data.reward_count));
                }
            }

            // PB_ITEM_GACHA
            foreach (var data in context.PB_ITEM_GACHA.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_ITEM_GACHA)
                        return false;

                    if (_itemGacha.ContainsKey(data.item_idx) == false)
                        _itemGacha.Add(data.item_idx, new List<PB_ITEM_GACHA>());

                    _itemGacha[data.item_idx].Add(data);
                }
            }

            // PB_ITEM_SINGLE
            foreach (var data in context.PB_ITEM_SINGLE.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_ITEM_SINGLE)
                        return false;

                    _itemSingle.Add(data.item_idx, data);
                }
            }

            // PB_ITEM_CONTENTS
            foreach (var data in context.PB_ITEM_CONTENTS.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_ITEM_CONTENT)
                        return false;

                    _itemContents.Add(data.item_idx, data);
                }
            }


            //가챠 확률 유효성 체크
            foreach (KeyValuePair<int, List<PB_ITEM_GACHA>> item in _itemGacha)
            {
                int totalRate = 0;
                foreach (PB_ITEM_GACHA obj in item.Value)
                {
                    totalRate += obj.get_rate;
                }

                if (totalRate != ItemDefine.ItemTotalRate)
                {
                    return false;
                }
            }

            //PB_ITEM_CARD
            foreach (var data in context.PB_ITEM_CARD.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_CARD)
                        return false;

                    if (data.country == 0 || data.country > (byte)NATION_LEAGUE_TYPE.NPB_MLB)
                        return false;

                    if (data.product_type < (byte)PLAYER_TYPE.TYPE_BATTER || data.product_type > (byte)PLAYER_TYPE.TYPE_BATTER_PITCHER)
                        return false;

                    //임시테스트
                    /*if (data.item_idx == 10000)
                    {
                        data.select_type = 3;
                        data.product_type = 0;
                    }*/

                    Dictionary<int, List<ItemCard>> tempGachaData;

                    if (data.product_type == (byte)PLAYER_TYPE.TYPE_COACH)
                        tempGachaData = GetItemCardCoachList(data);
                    else
                        tempGachaData = GetItemCardPlayerList(data);

                    if (tempGachaData == null)// || tempGachaData.Count == 0)
                        return false;

                    _itemCard.Add(data.item_idx, data);
                    _itemCardCaseData.Add(data.item_idx, tempGachaData);
                }
            }

            Dictionary<int, int> _tempItemCardGachaRate = new Dictionary<int, int>();
            Dictionary<int, int> _tempPackTypeCheck = new Dictionary<int, int>();

            // PB_ITEM_CARD_GACHA
            foreach (var data in context.PB_ITEM_CARD_GACHA.ToList())
            {
                if (_totalItem.ContainsKey(data.item_idx) == true)
                {
                    if (_totalItem[data.item_idx].item_type != (byte)ITEM_TYPE.TYPE_CARD_GACHA)
                        return false;

                    if (_itemCardGacha.ContainsKey(data.item_idx) == false)
                    {
                        _itemCardGacha.Add(data.item_idx, new List<PB_ITEM_CARD_GACHA>());
                        _itemCardGachaRate.Add(data.item_idx, new List<int>());
                        _tempItemCardGachaRate.Add(data.item_idx, 0);
                        _tempPackTypeCheck.Add(data.item_idx, data.pack_type);
                    }
                    else
                    {
                        if (_tempPackTypeCheck[data.item_idx] != data.pack_type)
                            return false;
                    }


                    _tempItemCardGachaRate[data.item_idx] += data.importance;
                    _itemCardGacha[data.item_idx].Add(data);
                    _itemCardGachaRate[data.item_idx].Add(_tempItemCardGachaRate[data.item_idx]);
                }
            }

            //------------------------------------------------ 여기서부터 영입관련 아이템
            // PB_SCOUT
            foreach (var data in context.PB_SCOUT.ToList())
            {

                if (data.korea != (byte)SCOUT_USE_TYPE.NONE)
                {
                    if (_scoutTotal.ContainsKey(data.scout_idx) == false)
                        _scoutTotal.Add(data.scout_idx, data);

                    if (data.scout_type == (byte)SCOUT_TYPE.TYPE_CARD)
                    {
                        if (data.korea == (byte)SCOUT_USE_TYPE.PLAYER)
                            scoutBasePlayerKey.Add((int)SERVICE_NATION_TYPE.KOREA, data.scout_idx);
                        else if (data.korea == (byte)SCOUT_USE_TYPE.COACH)
                            scoutBaseCoachKey.Add((int)SERVICE_NATION_TYPE.KOREA, data.scout_idx);
                    }
                }

                if (data.america != (byte)SCOUT_USE_TYPE.NONE)
                {
                    if (_scoutTotal.ContainsKey(data.scout_idx) == false)
                        _scoutTotal.Add(data.scout_idx, data);

                    if (data.scout_type == (byte)SCOUT_TYPE.TYPE_CARD)
                    {
                        if (data.america == (byte)SCOUT_USE_TYPE.PLAYER)
                            scoutBasePlayerKey.Add((int)SERVICE_NATION_TYPE.AMERICA, data.scout_idx);
                        else if (data.america == (byte)SCOUT_USE_TYPE.COACH)
                            scoutBaseCoachKey.Add((int)SERVICE_NATION_TYPE.AMERICA, data.scout_idx);
                    }
                }

                if (data.japan != (byte)SCOUT_USE_TYPE.NONE)
                {
                    if (_scoutTotal.ContainsKey(data.scout_idx) == false)
                        _scoutTotal.Add(data.scout_idx, data);

                    if (data.scout_type == (byte)SCOUT_TYPE.TYPE_CARD)
                    {
                        if (data.japan == (byte)SCOUT_USE_TYPE.PLAYER)
                            scoutBasePlayerKey.Add((int)SERVICE_NATION_TYPE.JAPAN, data.scout_idx);
                        else if (data.japan == (byte)SCOUT_USE_TYPE.COACH)
                            scoutBaseCoachKey.Add((int)SERVICE_NATION_TYPE.JAPAN, data.scout_idx);
                    }
                }

                if (data.taiwan != (byte)SCOUT_USE_TYPE.NONE)
                {
                    if (_scoutTotal.ContainsKey(data.scout_idx) == false)
                        _scoutTotal.Add(data.scout_idx, data);

                    if (data.scout_type == (byte)SCOUT_TYPE.TYPE_CARD)
                    {
                        if (data.taiwan == (byte)SCOUT_USE_TYPE.PLAYER)
                            scoutBasePlayerKey.Add((int)SERVICE_NATION_TYPE.TAIWAN, data.scout_idx);
                        else if (data.taiwan == (byte)SCOUT_USE_TYPE.COACH)
                            scoutBaseCoachKey.Add((int)SERVICE_NATION_TYPE.TAIWAN, data.scout_idx);
                    }
                }
            }

            if (scoutBasePlayerKey.Count != AccountDefine.ServiceNationCount || scoutBaseCoachKey.Count != AccountDefine.ServiceNationCount)
                return false;



            //PB_SCOUT_BASE
            foreach (var data in context.PB_SCOUT_BASE.ToList())
            {
                if (_scoutTotal.ContainsKey(data.scout_idx) == true)
                {
                    if (_scoutTotal[data.scout_idx].scout_type != (byte)SCOUT_TYPE.TYPE_CARD)
                        return false;

                    if (data.country == 0 || data.country > (byte)NATION_LEAGUE_TYPE.NPB_MLB)
                        return false;

                    if (data.product_type < (byte)PLAYER_TYPE.TYPE_BATTER || data.product_type >= (byte)PLAYER_TYPE.MAX)
                        return false;



                    //스카우트는 유저가 선택할수 있는게 아니므로 select_type이 무조건 0이어야한다.
                    if (scoutBasePlayerKey.ContainsValue(data.scout_idx) == true)
                    {
                        if (data.select_type != (byte)ITEM_CARD_SELECT_TYPE.SELECT_NOT)
                            return false;

                        if (data.product_type == (byte)PLAYER_TYPE.TYPE_COACH)
                            return false;
                    }

                    if (scoutBaseCoachKey.ContainsValue(data.scout_idx) == true)
                    {
                        if (data.select_type != (byte)ITEM_CARD_SELECT_TYPE.SELECT_NOT)
                            return false;

                        if (data.product_type != (byte)PLAYER_TYPE.TYPE_COACH)
                            return false;
                    }


                    Dictionary<int, List<ItemCard>> tempGachaData;

                    PB_ITEM_CARD obj = ScoutBaseConvertItemCard(data);

                    if (data.product_type == (byte)PLAYER_TYPE.TYPE_COACH)
                        tempGachaData = GetItemCardCoachList(obj);
                    else
                        tempGachaData = GetItemCardPlayerList(obj);

                    if (tempGachaData == null || tempGachaData.ContainsKey(0) == false || tempGachaData[0].Count == 0)  //tempGachaData.Count == 0
                        return false;



                    _scoutBase.Add(data.scout_idx, obj);
                    _scoutBaseData.Add(data.scout_idx, tempGachaData);
                }
            }

            Dictionary<int, int> _tempScoutGachaRate = new Dictionary<int, int>();
            Dictionary<int, int> _tempScoutPackTypeCheck = new Dictionary<int, int>();

            // PB_SCOUT_GACHA
            foreach (var data in context.PB_SCOUT_GACHA.ToList())
            {
                if (_scoutTotal.ContainsKey(data.scout_idx) == true)
                {
                    if (_scoutTotal[data.scout_idx].scout_type != (byte)SCOUT_TYPE.TYPE_CARD_GACHA)
                        return false;

                    if (_scoutGacha.ContainsKey(data.scout_idx) == false)
                    {
                        _scoutGacha.Add(data.scout_idx, new List<PB_ITEM_CARD_GACHA>());
                        _scoutGachaRate.Add(data.scout_idx, new List<int>());
                        _tempScoutGachaRate.Add(data.scout_idx, 0);
                        _tempScoutPackTypeCheck.Add(data.scout_idx, data.pack_type);
                    }
                    else
                    {
                        if (_tempScoutPackTypeCheck[data.scout_idx] != data.pack_type)
                            return false;
                    }



                    _tempScoutGachaRate[data.scout_idx] += data.importance;
                    _scoutGacha[data.scout_idx].Add(ScoutGachaConvertItemCardGacha(data));
                    _scoutGachaRate[data.scout_idx].Add(_tempScoutGachaRate[data.scout_idx]);
                }
            }

            Dictionary<int, int> tempNationRateSum = new Dictionary<int, int>();
            for (int i = (int)SERVICE_NATION_TYPE.KOREA; i < (int)SERVICE_NATION_TYPE.MAX; ++i)
            {
                nationScoutBinderInfo.Add(i, new List<PB_SCOUT_BINDER>());
                nationScoutBinderRate.Add(i, new List<int>());
                tempNationRateSum.Add(i, 0);
            }



            //PB_SCOUT_BINDER
            foreach (var data in context.PB_SCOUT_BINDER.ToList())
            {
                if (data.binder_type >= (byte)SCOUT_BINDER_TYPE.MAX)
                    return false;

                if (data.binder_slot_type == 0 || data.binder_slot_type >= (byte)SCOUT_BINDER_SLOT_TYPE.MAX)
                    return false;

                if (data.rate == 0)
                    continue;

                if (data.korea != 0)
                {
                    if (_scoutBinder.ContainsKey(data.idx) == false)
                        _scoutBinder.Add(data.idx, data);

                    tempNationRateSum[(int)SERVICE_NATION_TYPE.KOREA] += data.rate;

                    nationScoutBinderInfo[(int)SERVICE_NATION_TYPE.KOREA].Add(data);
                    nationScoutBinderRate[(int)SERVICE_NATION_TYPE.KOREA].Add(tempNationRateSum[(int)SERVICE_NATION_TYPE.KOREA]);
                }

                if (data.america != 0)
                {
                    if (_scoutBinder.ContainsKey(data.idx) == false)
                        _scoutBinder.Add(data.idx, data);

                    tempNationRateSum[(int)SERVICE_NATION_TYPE.AMERICA] += data.rate;

                    nationScoutBinderInfo[(int)SERVICE_NATION_TYPE.AMERICA].Add(data);
                    nationScoutBinderRate[(int)SERVICE_NATION_TYPE.AMERICA].Add(tempNationRateSum[(int)SERVICE_NATION_TYPE.AMERICA]);
                }

                if (data.japan != 0)
                {
                    if (_scoutBinder.ContainsKey(data.idx) == false)
                        _scoutBinder.Add(data.idx, data);

                    tempNationRateSum[(int)SERVICE_NATION_TYPE.JAPAN] += data.rate;

                    nationScoutBinderInfo[(int)SERVICE_NATION_TYPE.JAPAN].Add(data);
                    nationScoutBinderRate[(int)SERVICE_NATION_TYPE.JAPAN].Add(tempNationRateSum[(int)SERVICE_NATION_TYPE.JAPAN]);
                }

                if (data.taiwan != 0)
                {
                    if (_scoutBinder.ContainsKey(data.idx) == false)
                        _scoutBinder.Add(data.idx, data);

                    tempNationRateSum[(int)SERVICE_NATION_TYPE.TAIWAN] += data.rate;

                    nationScoutBinderInfo[(int)SERVICE_NATION_TYPE.TAIWAN].Add(data);
                    nationScoutBinderRate[(int)SERVICE_NATION_TYPE.TAIWAN].Add(tempNationRateSum[(int)SERVICE_NATION_TYPE.TAIWAN]);
                }
            }

            //영입바인더에 들어갈 선수 풀 생성 및 유효성 체크
            if (SetNationScoutBinderCard() == false)
                return false;

            SetScoutBinderResetCost();

            List<PB_ITEM_CONTENTS> tempItemContentsList = _itemContents.Values.ToList();

            //컨텐츠 아이템 인덱스 셋팅
            itemIdxCareermodeSimulration = tempItemContentsList.Find(x => x.item_use_type == (byte)ITEM_CONTENTS_USE_TYPE.CAREERMODE && x.effect_type == (byte)ITEM_CONTENTS_EFFECT_TYPE.SIMULRATION).item_idx;
            itemIdxScoutBinderReset = tempItemContentsList.Find(x => x.item_use_type == (byte)ITEM_CONTENTS_USE_TYPE.SCOUT && x.effect_type == (byte)ITEM_CONTENTS_EFFECT_TYPE.SCOUT_BINDER_RESET).item_idx;
            itemIdxScoutSearchDirect = tempItemContentsList.Find(x => x.item_use_type == (byte)ITEM_CONTENTS_USE_TYPE.SCOUT && x.effect_type == (byte)ITEM_CONTENTS_EFFECT_TYPE.SCOUT_DIRECT).item_idx;

            return true;
        }

        private void SetScoutBinderResetCost()
        {
            scoutBinderResetCost = new int[CacheManager.LoadingTable.ConstantTable.Const.binder_reset_cost_maxnum];

            scoutBinderResetCost[0] = CacheManager.LoadingTable.ConstantTable.Const.binder_reset_cost_value;
            for (int i = 1; i < scoutBinderResetCost.Length; ++i)
            {
                scoutBinderResetCost[i] = scoutBinderResetCost[i - 1] * CacheManager.LoadingTable.ConstantTable.Const.binder_reset_cost_const / 10;
            }
        }

        public int GetBinderResetCost(int tryResetCount)
        {
            return scoutBinderResetCost[tryResetCount - 1];
        }

        public PB_ITEM GetItemData(int index)
        {
            if (_totalItem.TryGetValue(index, out PB_ITEM data) == false)
            {
                return null;
            }

            return data;
        }

        private void SetItemCardListCreate(List<ItemCard> resultCardList, List<ItemCard> cardList)
        {
            // 지금은 setScoutRatePoolCharacterPool 이 안에서만 쓰임
            int tempSumVal = 0;

            if (cardList == null || cardList.Count == 0)
                return;

            if (resultCardList.Count > 0)           //하던게 있다면 누적값을 거기서부터 이어서 간다.
                tempSumVal = resultCardList[resultCardList.Count - 1].accumulRate;

            for (int k = 0; k < cardList.Count; ++k)
            {
                tempSumVal += cardList[k].singleRate;
                resultCardList.Add(new ItemCard(cardList[k].serialIdx, cardList[k].rewardCardType, cardList[k].singleRate, tempSumVal));
            }
        }

        private bool SetScoutRatePoolCharacterPool(SCOUT_BINDER_TYPE binderType, List<ItemCard> playerList, List<ItemCard> coachList, int[] rateList, Dictionary<int, List<ItemCard>> poolData)
        {
            for (int idx = 0; idx < rateList.Length; ++idx)
            {
                //이미 해당 레이트가 풀에 있으면 패스
                if (poolData.ContainsKey(rateList[idx]) == true)
                    continue;

                poolData.Add(rateList[idx], new List<ItemCard>());

                if (binderType == SCOUT_BINDER_TYPE.PLAYER)
                {
                    SetItemCardListCreate(poolData[rateList[idx]], playerList.FindAll(x => x.singleRate == rateList[idx]));
                }
                else if (binderType == SCOUT_BINDER_TYPE.COACH)
                {
                    SetItemCardListCreate(poolData[rateList[idx]], coachList.FindAll(x => x.singleRate == rateList[idx]));
                }
                else if (binderType == SCOUT_BINDER_TYPE.PLAYER_COACH)
                {
                    SetItemCardListCreate(poolData[rateList[idx]], playerList.FindAll(x => x.singleRate == rateList[idx]));
                    SetItemCardListCreate(poolData[rateList[idx]], coachList.FindAll(x => x.singleRate == rateList[idx]));
                }

                //중복이 일어나면안되므로 한 레이트에 최소 5명이상의 풀이 있어야함
                if (poolData[rateList[idx]].Count < ScoutDefine.binderCount)
                    return false;
            }

            return true;
        }

        private bool CheckPoolCharacterIdx(SCOUT_BINDER_TYPE binderType, List<ItemCard> playerList, List<ItemCard> coachList, int[] characterIdxs)
        {
            bool result = true;

            if (binderType == SCOUT_BINDER_TYPE.PLAYER)
            {
                for (int idx = 0; idx < characterIdxs.Length; ++idx)
                {
                    if (playerList.FindIndex(x => x.serialIdx == characterIdxs[idx]) == -1)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else if (binderType == SCOUT_BINDER_TYPE.COACH)
            {
                for (int idx = 0; idx < characterIdxs.Length; ++idx)
                {
                    if (coachList.FindIndex(x => x.serialIdx == characterIdxs[idx]) == -1)
                    {
                        result = false;
                        break;
                    }
                }
            }
            else if (binderType == SCOUT_BINDER_TYPE.PLAYER_COACH)
            {
                for (int idx = 0; idx < characterIdxs.Length; ++idx)
                {
                    if (playerList.FindIndex(x => x.serialIdx == characterIdxs[idx]) == -1)
                    {
                        if (coachList.FindIndex(x => x.serialIdx == characterIdxs[idx]) == -1)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }
            else
                result = false;

            return result;
        }

        private bool SetNationScoutBinderCard()
        {
            for (int i = (int)SERVICE_NATION_TYPE.KOREA; i < (int)SERVICE_NATION_TYPE.MAX; ++i)
            {
                //해당 나라에 바인더가 한개도 없으면 에러
                if (nationScoutBinderInfo[i].Count == 0)
                    return false;

                int scoutNationBasePlayerKey = scoutBasePlayerKey[i];
                int scoutNationBaseCoachKey = scoutBaseCoachKey[i];

                //영입Base데이터에 해당 나라의 선수풀이나 코치풀이 없다면 에러 
                if (_scoutBaseData.ContainsKey(scoutNationBasePlayerKey) == false || _scoutBaseData.ContainsKey(scoutNationBaseCoachKey) == false)
                    return false;

                //나라별 key값
                binderCharacterRatePoolData.Add(i, new Dictionary<int, Dictionary<int, List<ItemCard>>>());

                //바인더타입별
                for (int j = 0; j < (int)SCOUT_BINDER_TYPE.MAX; ++j)
                {
                    binderCharacterRatePoolData[i].Add(j, new Dictionary<int, List<ItemCard>>());
                }

                //현재 영입에서 나올수 있는 선수, 코치 리스트 가져오기(나라별)
                List<ItemCard> playerList = _scoutBaseData[scoutNationBasePlayerKey][0];
                List<ItemCard> coachList = _scoutBaseData[scoutNationBaseCoachKey][0];

                foreach (PB_SCOUT_BINDER info in nationScoutBinderInfo[i])
                {
                    int[] binderSlotVal = { info.binder_slot1, info.binder_slot2, info.binder_slot3, info.binder_slot4, info.binder_slot5 };

                    if (info.binder_slot_type == (byte)SCOUT_BINDER_SLOT_TYPE.RATE)
                    {
                        if (SetScoutRatePoolCharacterPool((SCOUT_BINDER_TYPE)info.binder_type, playerList, coachList, binderSlotVal, binderCharacterRatePoolData[i][info.binder_type]) == false)
                            return false;

                    }
                    else if (info.binder_slot_type == (byte)SCOUT_BINDER_SLOT_TYPE.IDX)
                    {
                        if (CheckPoolCharacterIdx((SCOUT_BINDER_TYPE)info.binder_type, playerList, coachList, binderSlotVal) == false)
                            return false;
                    }

                }
            }
            return true;
        }

        private PB_ITEM_CARD ScoutBaseConvertItemCard(PB_SCOUT_BASE data)
        {
            PB_ITEM_CARD obj = new PB_ITEM_CARD();

            obj.item_idx = data.scout_idx;
            obj.country = data.country;
            obj.league_flg = data.league_flg;
            obj.area_flg = data.area_flg;
            obj.product_type = data.product_type;
            obj.select_type = data.select_type;
            obj.team_condition = data.team_condition;
            obj.posion_condition = data.posion_condition;
            obj.overall_condition = data.overall_condition;

            return obj;
        }

        private PB_ITEM_CARD_GACHA ScoutGachaConvertItemCardGacha(PB_SCOUT_GACHA data)
        {
            PB_ITEM_CARD_GACHA obj = new PB_ITEM_CARD_GACHA();

            obj.item_idx = data.scout_idx;
            obj.rate_idx = data.rate_idx;
            obj.pack_type = data.pack_type;
            obj.play_type = data.play_type;
            obj.player_idx = data.player_idx;
            obj.importance = data.importance;

            return obj;
        }

        private int GetKeyVal(int teamIdx, int positionIdx, bool isCoach)
        {
            if (isCoach == true)
            {
                if (positionIdx == 0)
                    return teamIdx * correctionCardTeam;
                else
                    return teamIdx * correctionCardTeam + (positionIdx + correctionCardCoachPostion);
            }
            else
                return teamIdx * correctionCardTeam + positionIdx;
        }

        private List<int> GetPossibleTeamList(PB_ITEM_CARD data)
        {
            List<int> teamList = new List<int>();

            //step1 속한 팀 구하기
            if (data.team_condition == 0)
            {
                List<PB_TEAM_INFO> teamPBInfo = CacheManager.LoadingTable.PlayerTable._teamCountryInfo[data.country];

                for (int j = 0; j < teamPBInfo.Count; ++j)
                {
                    if (data.league_flg != 0 && data.league_flg != teamPBInfo[j].league_flg)
                        continue;

                    if (data.area_flg != 0 && data.area_flg != teamPBInfo[j].area_flg)
                        continue;

                    teamList.Add(teamPBInfo[j].team_idx);
                }
            }
            else
            {

                //팀지정이라면 해당 팀이 해당 컨츄리에꺼가 맞는지 체크
                if (data.select_type != (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM && data.select_type != (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM_POSITION &&
                    CacheManager.LoadingTable.PlayerTable._teamCountryInfo[data.country].FindIndex(x => x.team_idx == data.team_condition) > 0)
                    teamList.Add(data.team_condition);
            }

            return teamList;
        }

        private void SetItemCardPossible(Dictionary<int, List<ItemCard>> itemInfo, int key, List<PBPlayer> playerList)
        {
            if (playerList == null || playerList.Count == 0)
                return;

            itemInfo.Add(key, new List<ItemCard>());

            int sumVal = 0;
            for (int k = 0; k < playerList.Count; ++k)
            {
                sumVal += playerList[k].get_rate;
                itemInfo[key].Add(new ItemCard(playerList[k].player_idx, (byte)REWARD_TYPE.PLAYER_CARD, playerList[k].get_rate, sumVal));
            }
        }

        private void SetItemCardPossible(Dictionary<int, List<ItemCard>> itemInfo, int key, List<PB_COACH> coachList)
        {
            if (coachList == null || coachList.Count == 0)
                return;

            itemInfo.Add(key, new List<ItemCard>());

            int sumVal = 0;
            for (int k = 0; k < coachList.Count; ++k)
            {
                sumVal += coachList[k].get_rate;
                itemInfo[key].Add(new ItemCard(coachList[k].coach_idx, (byte)REWARD_TYPE.COACH_CARD, coachList[k].get_rate, sumVal));
            }
        }

        private Dictionary<int, List<ItemCard>> GetItemCardPlayerList(PB_ITEM_CARD data)
        {
            List<int> teamList = GetPossibleTeamList(data);

            if (teamList.Count == 0)
                return null;
            Dictionary<int, List<ItemCard>> resultInfo = new Dictionary<int, List<ItemCard>>();
            List<PBPlayer> tagetAllList;

            int startPosition;
            int endPosition;

            if (data.product_type == (byte)PLAYER_TYPE.TYPE_BATTER)
            {
                tagetAllList = CacheManager.LoadingTable.PlayerTable._gachaBatterList;
                startPosition = (int)PLAYER_POSITION.DH;
                endPosition = (int)PLAYER_POSITION.RF;
            }
            else if (data.product_type == (byte)PLAYER_TYPE.TYPE_PITCHER)
            {
                tagetAllList = CacheManager.LoadingTable.PlayerTable._gachaPitcherList;
                startPosition = (int)PLAYER_POSITION.SP;
                endPosition = (int)PLAYER_POSITION.CP;
            }
            else
            {
                tagetAllList = CacheManager.LoadingTable.PlayerTable._gachaPlayerList;
                startPosition = (int)PLAYER_POSITION.DH;
                endPosition = (int)PLAYER_POSITION.CP;
            }


            if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM)
            {
                foreach (int tIdx in teamList)
                {
                    List<PBPlayer> objList = tagetAllList.FindAll(x => x.team_idx == tIdx);

                    if (data.posion_condition != 0)
                        objList = objList.FindAll(x => x.position == data.posion_condition);

                    if (data.overall_condition != 0)
                        objList = objList.FindAll(x => x.overall >= data.overall_condition);

                    SetItemCardPossible(resultInfo, GetKeyVal(tIdx, 0, false), objList);
                }
            }
            else if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM_POSITION)
            {
                foreach (int tIdx in teamList)
                {
                    List<PBPlayer> objList = tagetAllList.FindAll(x => x.team_idx == tIdx);

                    for (int i = startPosition; i <= endPosition; ++i)
                    {
                        List<PBPlayer> positionList = objList.FindAll(x => x.position == i);

                        if (data.overall_condition != 0)
                            positionList = positionList.FindAll(x => x.overall >= data.overall_condition);

                        SetItemCardPossible(resultInfo, GetKeyVal(tIdx, i, false), positionList);
                    }

                }
            }
            else if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_POSITION)
            {
                List<PBPlayer> objList = tagetAllList.FindAll(x => teamList.Contains(x.team_idx));

                for (int i = startPosition; i <= endPosition; ++i)
                {
                    List<PBPlayer> positionList = objList.FindAll(x => x.position == i);

                    if (data.overall_condition != 0)
                        positionList = positionList.FindAll(x => x.overall >= data.overall_condition);

                    SetItemCardPossible(resultInfo, GetKeyVal(0, i, false), positionList);
                }

            }
            else
            {
                List<PBPlayer> objList = tagetAllList.FindAll(x => teamList.Contains(x.team_idx));

                if (data.posion_condition != 0)
                    objList = objList.FindAll(x => x.position == data.posion_condition);

                if (data.overall_condition != 0)
                    objList = objList.FindAll(x => x.overall >= data.overall_condition);

                SetItemCardPossible(resultInfo, 0, objList);
            }

            return resultInfo;
        }

        private Dictionary<int, List<ItemCard>> GetItemCardCoachList(PB_ITEM_CARD data)
        {
            List<int> teamList = GetPossibleTeamList(data);

            if (teamList.Count == 0)
                return null;

            Dictionary<int, List<ItemCard>> resultInfo = new Dictionary<int, List<ItemCard>>();
            List<PB_COACH> tagetAllList = CacheManager.LoadingTable.PlayerTable._gachaCoachList;

            if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM)
            {
                foreach (int tIdx in teamList)
                {
                    List<PB_COACH> objList = tagetAllList.FindAll(x => x.teamidx == tIdx);

                    if (data.posion_condition != 0)
                        objList = objList.FindAll(x => x.master_position == data.posion_condition - correctionCardCoachPostion);

                    if (data.overall_condition != 0)
                        objList = objList.FindAll(x => x.power >= data.overall_condition);

                    SetItemCardPossible(resultInfo, GetKeyVal(tIdx, 0, true), objList);
                }
            }
            else if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM_POSITION)
            {
                foreach (int tIdx in teamList)
                {
                    List<PB_COACH> objList = tagetAllList.FindAll(x => x.teamidx == tIdx);

                    for (int i = (int)COACH_MASTER_TYPE.TYPE_ALL; i <= (int)COACH_MASTER_TYPE.TYPE_TRAINER; ++i)
                    {
                        List<PB_COACH> positionList = objList.FindAll(x => x.master_position == i);

                        if (data.overall_condition != 0)
                            positionList = positionList.FindAll(x => x.power >= data.overall_condition);

                        SetItemCardPossible(resultInfo, GetKeyVal(tIdx, i, true), positionList);
                    }

                }
            }
            else if (data.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_POSITION)
            {
                List<PB_COACH> objList = tagetAllList.FindAll(x => teamList.Contains(x.teamidx));

                for (int i = (int)COACH_MASTER_TYPE.TYPE_ALL; i <= (int)COACH_MASTER_TYPE.TYPE_TRAINER; ++i)
                {
                    List<PB_COACH> positionList = objList.FindAll(x => x.master_position == i);

                    if (data.overall_condition != 0)
                        positionList = positionList.FindAll(x => x.power >= data.overall_condition);

                    SetItemCardPossible(resultInfo, GetKeyVal(0, i, true), positionList);
                }

            }
            else
            {
                List<PB_COACH> objList = tagetAllList.FindAll(x => teamList.Contains(x.teamidx));

                if (data.posion_condition != 0)
                    objList = objList.FindAll(x => x.master_position == data.posion_condition - correctionCardCoachPostion);

                if (data.overall_condition != 0)
                    objList = objList.FindAll(x => x.power >= data.overall_condition);

                SetItemCardPossible(resultInfo, 0, objList);
            }

            return resultInfo;
        }

        public List<GameRewardInfo> GetPackegeItem(int itemIdx)
        {
            if (_itemPackage.TryGetValue(itemIdx, out List<GameRewardInfo> data) == false)
            {
                return null;
            }

            List<GameRewardInfo> result = new List<GameRewardInfo>();
            foreach (GameRewardInfo rewardInfo in data)
                result.Add(new GameRewardInfo(rewardInfo.reward_type, rewardInfo.reward_idx, rewardInfo.reward_cnt));

            return result;
        }

        public List<GameRewardInfo> GetGachaItem(int itemIdx, int count)
        {
            if (_itemGacha.TryGetValue(itemIdx, out List<PB_ITEM_GACHA> data) == false)
            {
                return null;
            }

            List<GameRewardInfo> ItemList = new List<GameRewardInfo>();

            for (int i = 0; i < count; ++i)
            {
                int randVal = RandomManager.Instance.GetIndex(ItemDefine.ItemTotalRate);

                int start = 0;
                int rateIdx = 0;
                bool isSuccess = false;

                for (rateIdx = 0; rateIdx < data.Count; ++rateIdx)
                {
                    int end = start + data[rateIdx].get_rate;

                    if (randVal < end)
                    {
                        ItemList.Add(new GameRewardInfo(data[rateIdx].reward_type, data[rateIdx].reward_idx, data[rateIdx].reward_count));
                        isSuccess = true;
                        break;
                    }

                    start = end;
                }

                if (isSuccess == false)
                    return null;

            }

            return ItemList;

        }

        public GameRewardInfo GetSingleItem(int itemIdx)
        {
            if (_itemSingle.ContainsKey(itemIdx) == false)
                return null;

            return new GameRewardInfo(_itemSingle[itemIdx].reward_type, _itemSingle[itemIdx].reward_idx, _itemSingle[itemIdx].reward_count);
        }

        public ErrorCode GetCharacterCardGacha(CHARACTER_OBTAIN_TYPE obtainType, int itemIdx, int count, AccountGame accountGame, out List<Player> obtainPlayer, out List<Coach> obtainCoach)
        {
            obtainPlayer = new List<Player>();
            obtainCoach = new List<Coach>();

            List<PB_ITEM_CARD_GACHA> gachaData = null;
            List<int> gachaRate = null;

            if (obtainType == CHARACTER_OBTAIN_TYPE.OBTAIN_ITEM)
            {
                if (_itemCardGacha.TryGetValue(itemIdx, out gachaData) == false)
                    return ErrorCode.ERROR_REQUEST_DATA;

                gachaRate = _itemCardGachaRate[itemIdx];
            }
            else if (obtainType == CHARACTER_OBTAIN_TYPE.OBTAIN_SCOUT)
            {
                if (_scoutGacha.TryGetValue(itemIdx, out gachaData) == false)
                    return ErrorCode.ERROR_STATIC_DATA;

                gachaRate = _scoutGachaRate[itemIdx];
            }
            else
            {
                return ErrorCode.ERROR_STATIC_DATA;
            }

            if (gachaData[0].pack_type == (byte)ITEM_CARD_GACHA_PACK_TYPE.PLAYER)
            {
                if (accountGame.now_player >= accountGame.max_player)
                    return ErrorCode.ERROR_PLAYER_MAX_COUNT_EXCESS;
            }
            else if (gachaData[0].pack_type == (byte)ITEM_CARD_GACHA_PACK_TYPE.COACH)
            {
                if (accountGame.now_coach >= accountGame.max_coach)
                    return ErrorCode.ERROR_COACH_MAX_COUNT_EXCESS;
            }
            else
            {
                if (accountGame.now_player >= accountGame.max_player)
                    return ErrorCode.ERROR_PLAYER_MAX_COUNT_EXCESS;

                if (accountGame.now_coach >= accountGame.max_coach)
                    return ErrorCode.ERROR_COACH_MAX_COUNT_EXCESS;
            }


            for (int k = 0; k < count; ++k)
            {
                int randVal = RandomManager.Instance.GetIndex(gachaRate[gachaRate.Count - 1]);
                bool isSuccess = false;

                for (int i = 0; i < gachaRate.Count; ++i)
                {
                    if (randVal < gachaRate[i])
                    {
                        if (gachaData[i].play_type == (byte)PLAYER_TYPE.TYPE_COACH)
                        {
                            Coach coachInfo = CacheManager.PBTable.PlayerTable.CreateCoachInfo(gachaData[i].player_idx);
                            coachInfo.account_coach_idx = obtainCoach.Count;
                            obtainCoach.Add(coachInfo);
                            ++accountGame.now_coach;
                        }
                        else
                        {
                            Player playerInfo = CacheManager.PBTable.PlayerTable.CreatePlayerInfo(gachaData[i].player_idx, 0);
                            playerInfo.account_player_idx = obtainPlayer.Count;
                            obtainPlayer.Add(playerInfo);
                            ++accountGame.now_player;
                        }
                        isSuccess = true;
                        break;
                    }
                }

                if (isSuccess == false)
                    return ErrorCode.ERROR_STATIC_DATA;

            }

            return ErrorCode.SUCCESS;
        }

        public ErrorCode GetCharacterCard(CHARACTER_OBTAIN_TYPE obtainType, int itemIdx, int count, int selectTeam, int selectPosition, AccountGame accountGame, out List<Player> obtainPlayer, out List<Coach> obtainCoach)
        {
            obtainPlayer = new List<Player>();
            obtainCoach = new List<Coach>();

            PB_ITEM_CARD cardBase = null;
            Dictionary<int, List<ItemCard>> cardData = null;

            if (obtainType == CHARACTER_OBTAIN_TYPE.OBTAIN_ITEM)
            {
                if (_itemCard.TryGetValue(itemIdx, out cardBase) == false)
                    return ErrorCode.ERROR_REQUEST_DATA;

                cardData = _itemCardCaseData[itemIdx];
            }
            else if (obtainType == CHARACTER_OBTAIN_TYPE.OBTAIN_SCOUT)
            {
                if (_scoutBase.TryGetValue(itemIdx, out cardBase) == false)
                    return ErrorCode.ERROR_STATIC_DATA;

                cardData = _scoutBaseData[itemIdx];
            }
            else
            {
                return ErrorCode.ERROR_STATIC_DATA;
            }

            bool isCoach = false;

            if (cardBase.product_type == (byte)PLAYER_TYPE.TYPE_COACH)
            {
                if (accountGame.now_coach >= accountGame.max_coach)
                    return ErrorCode.ERROR_COACH_MAX_COUNT_EXCESS;

                isCoach = true;
            }
            else
            {
                if (accountGame.now_player >= accountGame.max_player)
                    return ErrorCode.ERROR_PLAYER_MAX_COUNT_EXCESS;
            }

            List<ItemCard> possibleList;

            //상황에 맞는 풀을 가져온다
            if (cardBase.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM)
            {
                cardData.TryGetValue(GetKeyVal(selectTeam, 0, isCoach), out possibleList);
            }
            else if (cardBase.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_POSITION)
            {
                cardData.TryGetValue(GetKeyVal(0, selectPosition, isCoach), out possibleList);
            }
            else if (cardBase.select_type == (byte)ITEM_CARD_SELECT_TYPE.SELECT_TEAM_POSITION)
            {
                cardData.TryGetValue(GetKeyVal(selectTeam, selectPosition, isCoach), out possibleList);
            }
            else
            {
                cardData.TryGetValue(0, out possibleList);
            }

            if (possibleList == null || possibleList.Count == 0)
                return ErrorCode.ERROR_REQUEST_DATA;

            for (int k = 0; k < count; ++k)
            {
                int randVal = RandomManager.Instance.GetIndex(possibleList[possibleList.Count - 1].accumulRate);
                bool isSuccess = false;

                for (int i = 0; i < possibleList.Count; ++i)
                {
                    if (randVal < possibleList[i].accumulRate)
                    {
                        if (possibleList[i].rewardCardType == (byte)REWARD_TYPE.COACH_CARD)
                        {
                            Coach coachInfo = CacheManager.PBTable.PlayerTable.CreateCoachInfo(possibleList[i].serialIdx);
                            coachInfo.account_coach_idx = obtainCoach.Count;
                            obtainCoach.Add(coachInfo);
                            ++accountGame.now_coach;
                        }
                        else
                        {
                            Player playerInfo = CacheManager.PBTable.PlayerTable.CreatePlayerInfo(possibleList[i].serialIdx, 0);
                            playerInfo.account_player_idx = obtainPlayer.Count;
                            obtainPlayer.Add(playerInfo);
                            ++accountGame.now_player;
                        }

                        isSuccess = true;
                        break;
                    }
                }

                if (isSuccess == false)
                    return ErrorCode.ERROR_STATIC_DATA;

            }

            return ErrorCode.SUCCESS;
        }

        public ErrorCode SetScoutBinderInfo(AccountScoutBinder userBinderInfo, int dateNo, byte nationType, bool isReset)
        {
            if (nationScoutBinderRate.ContainsKey(nationType) == false)
                return ErrorCode.ERROR_REQUEST_DATA;

            int listIdx = -1;
            int randVal = RandomManager.Instance.GetIndex(nationScoutBinderRate[nationType][nationScoutBinderRate[nationType].Count - 1]);

            for (int i = 0; i < nationScoutBinderRate[nationType].Count; ++i)
            {
                if (randVal < nationScoutBinderRate[nationType][i])
                {
                    listIdx = i;
                    break;
                }
            }

            PB_SCOUT_BINDER selectBinder = nationScoutBinderInfo[nationType][listIdx];
            if (selectBinder.binder_slot_type == (byte)SCOUT_BINDER_SLOT_TYPE.IDX)
            {
                userBinderInfo.slot1_character_idx = selectBinder.binder_slot1;
                userBinderInfo.slot2_character_idx = selectBinder.binder_slot2;
                userBinderInfo.slot3_character_idx = selectBinder.binder_slot3;
                userBinderInfo.slot4_character_idx = selectBinder.binder_slot4;
                userBinderInfo.slot5_character_idx = selectBinder.binder_slot5;
            }
            else
            {
                SetScoutBinderSlotRateIdx(userBinderInfo, binderCharacterRatePoolData[nationType][selectBinder.binder_type],
                    new int[] { selectBinder.binder_slot1, selectBinder.binder_slot2, selectBinder.binder_slot3, selectBinder.binder_slot4, selectBinder.binder_slot5 });
            }

            userBinderInfo.date_no = dateNo;
            userBinderInfo.binder_idx = selectBinder.idx;
            userBinderInfo.slot1_complete = 0;
            userBinderInfo.slot2_complete = 0;
            userBinderInfo.slot3_complete = 0;
            userBinderInfo.slot4_complete = 0;
            userBinderInfo.slot5_complete = 0;
            userBinderInfo.reward_flag = 0;

            if (isReset == true)
            {
                ++userBinderInfo.reset_count;
            }
            else
            {
                userBinderInfo.reset_count = 0;
            }

            return ErrorCode.SUCCESS;
        }

        private void SetScoutBinderSlotRateIdx(AccountScoutBinder userBinderInfo, Dictionary<int, List<ItemCard>> binderPoolList, int[] rateIdxArr)
        {
            //중복이 일어나지않게 5명 셋팅(각 레이트에 맞는 선수 또는 코치)
            Dictionary<int, List<int>> exceptListIdx = new Dictionary<int, List<int>>();
            Dictionary<int, int> exceptRateVal = new Dictionary<int, int>();

            for (int i = 0; i < rateIdxArr.Length; ++i)
            {
                int rateIdx = rateIdxArr[i];
                List<ItemCard> ratePoolList = binderPoolList[rateIdx];
                int poolListIdx = -1;

                if (exceptListIdx.ContainsKey(rateIdx) == false)
                {
                    exceptListIdx.Add(rateIdx, new List<int>());
                    exceptRateVal.Add(rateIdx, 0);

                    int rand = RandomManager.Instance.GetIndex(ratePoolList[ratePoolList.Count - 1].accumulRate);

                    for (int j = 0; j < ratePoolList.Count; ++j)
                    {
                        if (rand < ratePoolList[j].accumulRate)
                        {
                            poolListIdx = j;
                            break;
                        }
                    }
                }
                else
                {
                    int rand = RandomManager.Instance.GetIndex(ratePoolList[ratePoolList.Count - 1].accumulRate - exceptRateVal[rateIdx]);

                    for (int j = 0; j < ratePoolList.Count; ++j)
                    {
                        if (exceptListIdx[rateIdx].Contains(j) == true)
                        {
                            rand += ratePoolList[j].singleRate;
                            continue;
                        }
                        else if (rand < ratePoolList[j].accumulRate)
                        {
                            poolListIdx = j;
                            break;
                        }
                    }
                }

                exceptListIdx[rateIdx].Add(poolListIdx);
                exceptRateVal[rateIdx] += ratePoolList[poolListIdx].singleRate;


                if (i == 0)
                    userBinderInfo.slot1_character_idx = ratePoolList[poolListIdx].serialIdx;
                else if (i == 1)
                    userBinderInfo.slot2_character_idx = ratePoolList[poolListIdx].serialIdx;
                else if (i == 2)
                    userBinderInfo.slot3_character_idx = ratePoolList[poolListIdx].serialIdx;
                else if (i == 3)
                    userBinderInfo.slot4_character_idx = ratePoolList[poolListIdx].serialIdx;
                else if (i == 4)
                    userBinderInfo.slot5_character_idx = ratePoolList[poolListIdx].serialIdx;

            }
        }

        public void SetScoutSearchStart(AccountScoutSlot userSlotInfo, byte nationType, byte characterType, out GameRewardInfo consumeCost)
        {

            int scoutIdx = -1;
            if (characterType == (byte)SCOUT_USE_TYPE.PLAYER)
                scoutIdx = scoutBasePlayerKey[nationType];
            else if (characterType == (byte)SCOUT_USE_TYPE.COACH)
                scoutIdx = scoutBaseCoachKey[nationType];

            userSlotInfo.character_type = characterType;
            userSlotInfo.remain_sec = _scoutTotal[scoutIdx].scout_time;

            consumeCost = new GameRewardInfo(_scoutTotal[scoutIdx].scout_cost_type, 0, _scoutTotal[scoutIdx].scout_cost_value);

        }

        public ErrorCode SetScoutSearchEnd(out List<Player> obtainPlayer, out List<Coach> obtainCoach, byte nationType, byte characterType, AccountGame accountGameInfo)
        {
            int scoutIdx = -1;
            if (characterType == (byte)SCOUT_USE_TYPE.PLAYER)
                scoutIdx = scoutBasePlayerKey[nationType];
            else if (characterType == (byte)SCOUT_USE_TYPE.COACH)
                scoutIdx = scoutBaseCoachKey[nationType];

            int obtainCnt = RandomManager.Instance.GetCount(_scoutTotal[scoutIdx].scout_num_min, _scoutTotal[scoutIdx].scout_num_max);

            return GetCharacterCard(CHARACTER_OBTAIN_TYPE.OBTAIN_SCOUT, scoutIdx, obtainCnt, 0, 0, accountGameInfo, out obtainPlayer, out obtainCoach);

        }

        public GameRewardInfo GetBinderAllCompleteReward(int binderIdx)
        {
            return new GameRewardInfo(_scoutBinder[binderIdx].reward_type, _scoutBinder[binderIdx].reward_idx, _scoutBinder[binderIdx].reward_count);
        }

        public bool IsPossibleUseItem(int itemIdx, byte nationType)
        {
            bool result = false;

            if (_totalItem.ContainsKey(itemIdx) == true)
            {
                if (nationType == (byte)SERVICE_NATION_TYPE.KOREA)
                {
                    if (_totalItem[itemIdx].korea == 1)
                        result = true;
                }
                else if (nationType == (byte)SERVICE_NATION_TYPE.AMERICA)
                {
                    if (_totalItem[itemIdx].america == 1)
                        result = true;
                }
                else if (nationType == (byte)SERVICE_NATION_TYPE.JAPAN)
                {
                    if (_totalItem[itemIdx].japan == 1)
                        result = true;
                }
                else if (nationType == (byte)SERVICE_NATION_TYPE.TAIWAN)
                {
                    if (_totalItem[itemIdx].taiwan == 1)
                        result = true;
                }
            }

            return result;
        }

        public List<PB_ITEM_CARD> TestGetItemCardList()
        {
            return _itemCard.Values.ToList();
        }

        public Dictionary<int, Dictionary<int, List<ItemCard>>> TestGetItemCardCaseData()
        {
            return _itemCardCaseData;
        }
    }
}
