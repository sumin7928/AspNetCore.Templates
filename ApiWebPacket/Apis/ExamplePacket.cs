using System;
using System.Collections.Generic;
using System.Text;
using ApiWebPacket.Core;
using ApiWebPacket.Entity;

namespace ApiWebPacket.Apis
{
    /// <summary>
    /// 요청 응답 패킷
    /// </summary>
    public class ReqExamplePacket
    {
        /// <summary>
        /// 맴버 변수
        /// </summary>
        public string memberVariable;

        /// <summary>
        /// List 객체
        /// </summary>
        public List<string> memberList;

        /// <summary>
        /// Dictionary 객체
        /// </summary>
        public Dictionary<int,string> memberDictionary;

        /// <summary>
        /// 프로퍼티 변수
        /// </summary>
        public string PropertiesVariable { get; set; }

        /// <summary>
        /// Class 객체
        /// </summary>
        public ExampleEntity ExampleEntity { get; set; }

    }

    /// <summary>
    /// 응답 예제 패킷
    /// </summary>
    public class ResExamplePacket
    {
        /// <summary>
        /// 에러 코드
        /// </summary>
        public int ErrorCode { get; set; }
    }

    /// <summary>
    /// 예제 패킷
    /// </summary>
    public class ExamplePacket : WebPacket<ReqExamplePacket, ResExamplePacket>
    {
        /// <summary>
        /// 패킷 Url 주소
        /// </summary>
        public const string Path = "/api/Example";
    }
}
