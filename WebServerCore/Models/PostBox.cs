using System;
using System.Collections.Generic;
using WebSharedLib.Entity;

namespace ApiWebServer.Models
{
    public class PostDelInfo
    {
        public int idx;
        public long post_no;
    }

    public class PostInsert
    {
        /// <summary>
        /// 받은 계정 퍼블리셔 타입
        /// </summary>
        public byte RecvPubType;
        /// <summary>
        /// 받은 계정ID
        /// </summary>
        public string RecvPubID;
        /// <summary>
        /// 보낸 계정 퍼블리셔 타입
        /// </summary>
        public byte SendPubType;
        /// <summary>
        /// 보낸 계정ID
        /// </summary>
        public string SendPubID;
        /// <summary>
        /// 보낸 케릭터 레벨
        /// </summary>
        public int SendPCLevel;
        /// <summary>
        /// 우편 구분 코드 (행동력, 이벤트 미션완료 등등)
        /// </summary>
        public string PostCode;
        /// <summary>
        /// 우편 타입값 (ex) 게임머니:GM, 선수카드:PC, 소모성아이템:CI, 선수팩류:PP, 랜덤팩류:RP, 소셜포인트:SP)
        /// </summary>
		public string ItemTypeFlag;
        /// <summary>
        /// 보상 내역
        /// </summary>
        public List<GameRewardInfo> RewardList;
        /// <summary>
        /// 아이템 타입
        /// </summary>
		public int ItemCode;
        /// <summary>
        /// 아이템 인덱스
        /// </summary>
        public int ItemIdx;
        /// <summary>
        /// 아이템 수량
        /// </summary>
		public int ItemCnt;
        /// <summary>
        /// 넷마블 승인번호
        /// </summary>
		public string TranCode;
        /// <summary>
        /// 메모
        /// </summary>
		public string Memo;
        /// <summary>
        /// 우편만료일자
        /// </summary>
		public DateTime ExpTime;

        public PostInsert(string pubId, List<GameRewardInfo> rewardList)
        {
            RecvPubID = pubId;
            SendPubID = "admin";
            SendPCLevel = 1;
            PostCode = "test";
            ItemTypeFlag = "GM";
            RewardList = rewardList;
            TranCode = "";
            Memo = "memo";
            ExpTime = DateTime.Now.AddDays(Common.Define.PostDefine.RemainDay);
        }
    }
}