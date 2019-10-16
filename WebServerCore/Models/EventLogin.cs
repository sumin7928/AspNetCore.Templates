namespace ApiWebServer.Models
{
    public class EventLogin
    {
        public byte RewardFlag { get; set; }
        public string EventTitle { get; set; }
        public string EventContents { get; set; }
        public int ItemCode { get; set; }
        public int ItemTypeFlag { get; set; }
        public int ItemCnt { get; set; }
        //public string LoginKey { get; set; }
    }
}