using System;
using System.Collections.Generic;
using System.Linq;
using ApiWebServer.Models;
using ApiWebServer.PBTables;
using WebSharedLib.Contents;
using WebSharedLib.Entity;
using WebSharedLib.Error;

namespace ApiWebServer.Cache.PBTables
{
    public class ManagerTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private List<PB_MANAGER_EXP> _managerExp = new List<PB_MANAGER_EXP>();

        private Dictionary<int, PB_SKILL_MASTERY> _skillMastery = new Dictionary<int, PB_SKILL_MASTERY>();

        public int MaxExp { get; private set; }
        public int MaxLevel { get; private set; }

        public bool LoadTable( MaguPBTableContext context )
        {
            // PB_SKILL_MASTERY
            foreach ( var data in context.PB_SKILL_MASTERY.ToList() )
            {
                _skillMastery.Add( data.idx, data );
            }

            // PB_MANAGER_EXP
            foreach ( var data in context.PB_MANAGER_EXP.ToList() )
            {
                _managerExp.Add( data );
            }

            // 최대 레벨 도달일 경우 그전의 요구 경험치가 최대치
            MaxExp = _managerExp[_managerExp.Count - 2].max_exp;
            MaxLevel = _managerExp.Count;

            return true;
        }

        public ErrorCode RegisterMasterySkill( ReqSkillMasteryRegister request, AccountGame account, List<SkillMastery> nowRegisterdList )
        {
            int nowSkillCnt = 0;
            int nowSkillPoint = 0;
            int addedPoint = 0;
            int registeredCnt = 0;

            foreach ( var info in nowRegisterdList )
            {
                //nowSkillPoint += info.skill_level;
                nowSkillCnt += info.skill_level;
                nowSkillPoint += (int)Math.Ceiling(((registeredCnt + 1) / (decimal)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.mastery_cost));
                registeredCnt += 1;
            }

            for (int i = 0; i < request.RegisterMasteryIdxList.Count; i++)
            {
                addedPoint += (int)Math.Ceiling((nowSkillCnt + i + 1) / (decimal)ApiWebServer.Cache.CacheManager.PBTable.ConstantTable.Const.mastery_cost);
            }

            if (addedPoint != request.UseSkillPoint)
            {
                return ErrorCode.ERROR_INVALID_SKILL_POINT;
            }

            var masteryList = request.RegisterMasteryIdxList.Select( x => _skillMastery[ x ] ).OrderBy( x => x.idx );

            foreach ( var mastery in masteryList )
            {
                var conditionList = nowRegisterdList.Where( x => x.condition_idx == mastery.conditionIdx );
                int conditionCount = conditionList.Count();

                // 해당 작전 및 상황 등록 요청이 처음일 경우
                if ( conditionCount == 0 )
                {
                    if( mastery.preference > 1 )
                    {
                        return ErrorCode.ERROR_INVALID_MASTERY_CREATE;
                    }

                    var createdMastery = CreateMastery( mastery, conditionList, account.user_lv );
                    if( createdMastery == null )
                    {
                        return ErrorCode.ERROR_INVALID_MASTERY_CREATE;
                    }
                    nowRegisterdList.Add( createdMastery );
                    continue;
                }

                var groupList= conditionList.Where( x => x.group == mastery.group );
               
                // 이미 있고 레벨업일 경우
                if ( groupList.Count() > 0 )
                {
                    var list = groupList.ToList();
                    if( list.Count != 1 )
                    {
                        return ErrorCode.ERROR_NOT_FOUND_MASTERY_SKILL;
                    }

                    list[ 0 ].mastery_idx = mastery.idx;
                    byte now = list[ 0 ].skill_level;
                    byte point = ( byte )( mastery.idx % 100 );
                    list[ 0 ].skill_level = point;
                }
                // 새로운 마스터리 추가일 경우
                else
                {
                    if ( mastery.group_count <= conditionCount )
                    {
                        return ErrorCode.ERROR_ALREADY_MAX_MASTERY_GROUP;
                    }

                    var createdMastery = CreateMastery( mastery, conditionList, account.user_lv );
                    if ( createdMastery == null )
                    {
                        return ErrorCode.ERROR_NOT_FOUND_MASTERY_SKILL;
                    }
                    nowRegisterdList.Add( createdMastery );
                    continue;
                }
            }
            //nowRegisterdList.RemoveAll(x => request.RegisterMasteryIdxList.Contains(x.mastery_idx) == false);

            return ErrorCode.SUCCESS;
        }

        private SkillMastery CreateMastery( PB_SKILL_MASTERY mastery, IEnumerable<SkillMastery> myMasteryList, int userLevel )
        {
            if ( mastery.precondition == 1 )
            {
                if ( mastery.precondition_value > userLevel )
                {
                    return null;
                }
            }
            else if ( mastery.precondition == 2 )
            {
                int totalMasteryPoint = 0;
                foreach ( var myMastery in myMasteryList )
                {
                    totalMasteryPoint += myMastery.skill_level;
                }

                if ( mastery.precondition_value > totalMasteryPoint )
                {
                    return null;
                }
            }
            /* 3번 조건 마스터리 개편하면서 삭제
            else if ( mastery.precondition == 3 )
            {
                var beforeMasteryList = myMasteryList.Where( x => x.group == mastery.preference - 1 );

                int beforeMasteryPoint = 0;
                foreach ( var beforeMastery in beforeMasteryList )
                {
                    beforeMasteryPoint += beforeMastery.skill_level;
                }

                if ( mastery.precondition_value > beforeMasteryPoint )
                {
                    return null;
                }
            }*/

            byte point = ( byte )( mastery.idx % 100 );

            return new SkillMastery()
            {
                category = mastery.category,
                condition_idx = mastery.conditionIdx,
                mastery_idx = mastery.idx,
                group = mastery.group,
                skill_level = point
            };
        }

        public void AddExpResult(int Lv, int Exp, int AddExp, out int afterLv, out int afterExp, out int addMasteryPoint)
        {
            afterLv = Lv;
            afterExp = Exp + AddExp;
            addMasteryPoint = 0;

            //원래부터 만랩이었으면 체크 필요없음
            if(Lv == MaxLevel)
            {
                afterLv = MaxLevel;
                afterExp = MaxExp;

                return;
            }

            // 최대 경험치 체크
            if (afterExp > MaxExp)
            {
                afterExp = MaxExp;
            }

            // 레벨업 진행
            for (int i = Lv; i < MaxLevel; ++i)
            {
                if (afterExp < _managerExp[i - 1].max_exp)
                {
                    break;
                }

                addMasteryPoint += _managerExp[i - 1].reward_skill_point;
                afterLv = i + 1;
            }
        }
    }
}
