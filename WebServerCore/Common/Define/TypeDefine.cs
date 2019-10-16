using System;

namespace ApiWebServer.Common.Define
{
    public static class CareerModeDefine
    {
        public const int MaxPennantraceDegreeInKBO = 144;

        public const int DifficultyCount = 3;      //3개 쉬움, 보통, 어려움
        public const int SpringCampTotalRate = 100000;
        public const int EventTotalRate = 100000;
        public const int InjuryGroupCount = 4;
        public const int MissionCompleteCount = 6;
        public const byte DefaultContractNo = 1;
        public const int OwnerGoalPostDivisionType = 20;
    }

    public static class ActionTypeDefine
    {
        public const int CareerModeRunCount = 1;
        public const int CareerModeWinCount = 2;

        public const int PennantraceLastRank = 117;
        public const int EntrancePostSeason = 118;

        public const int PennantraceWinRecord = 120;
        public const int PennantraceLoseRecord = 121;

        public const int PennantraceTeam_HR_Record = 124;
        public const int PennantraceTeam_H_Record = 125;
        public const int PennantraceTeam_SB_Record = 127;
        public const int PennantraceTeam_AB_Record = 132;
        public const int PennantraceTeam_TR_Record = 133;

        public const int PennantraceTeam_SO_Record = 137;
        public const int PennantraceTeam_BB_Record = 138;
        public const int PennantraceTeam_HLD_Record = 140;
        public const int PennantraceTeam_SV_Record = 141;

        public const int PennantraceCommendCount = 142;
        public const int PennantraceTitlePlayerCount = 145;

        public const int PostRankWinner = 146;
        public const int PostRankSecondWinner = 147;

        public const int PostTeam_HR_Record = 148;
        public const int PostTeam_H_Record = 149;
        public const int PostTeam_SB_Record = 151;
        public const int PostTeam_AB_Record = 156;
        public const int PostTeam_TR_Record = 157;

        public const int PostTeam_SO_Record = 161;
        public const int PostTeam_BB_Record = 162;
        public const int PostTeam_HLD_Record = 164;
        public const int PostTeam_SV_Record = 165;

    }

    public static class PlayerDefine
    {
        public const int LineupPlayerCount = 27;
        public const int LineupBatterCount = 14;
        public const int LineupPitcherCount = 13;

        public const int PlayBatterCount = 9;

        public const int PitcherOrderStartSP = 14;
        public const int PitcherOrderStartRP = 19;
        public const int PitcherOrderStartCP = 26;

        public const int PlayPitcherSPCount = 5;
        public const int PlayPitcherRPCount = 7;
        public const int PlayPitcherCPCount = 1;

        public const int LineupCoachMinSlot = 1;
        public const int LineupCoachMaxSlot = 8;
        public const int LineupCoachCount = 4;
        public const int InvenOrderStartIdx = 100;
        public const int LineupMaxCoachCount = 8;

        public const int PotentialGradeCount = 4;
        public const int LeadershipGradeCount = 4;

        public const int PlayerPotentialRankupMaxIdx = 10000;
		public const int Leadership1SlotNeedGrade = 1;
        public const int Leadership2SlotNeedGrade = 3;
        public const int Leadership3SlotNeedGrade = 5;
        public const int PlayerReinforceTotalRate = 100000;
        public const int PlayerReinforceMax = 10;
        public const int PlayerLimitUpMax = 15;

        public const int LeadershipMaxSlot = 3;

        public const int PlayerPotentialMaterialCount = 5;
        public const int CoachLeadershipMaterialCount = 5;
    }
    public static class ItemDefine
    {
        public const int ItemTotalRate = 100000;
    }

    public static class PostDefine
    {
        public const int RemainDay = 7;
    }

    public static class LiveSeasonDefine
    {
        public const int RedisBattleInfoExpiredTime = 30;
        public const int RedisMatchExpiredTime = 30;
        public const int RedisRankExpiredTime = 60;
    }

    public static class AccountDefine
    {
        public const int ServiceNationCount = 4;
    }

    public static class ScoutDefine
    {
        public const int binderCount = 5;
    }

    public static class MissionAchievementDefine
    {
        public const int DayRandomMissionCount = 2;
    }

}
