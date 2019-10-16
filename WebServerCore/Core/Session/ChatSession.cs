using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiWebServer.Core.Session
{
    public class ChatSession
    {
        public int PacketNo { get; set; }
        public long Pcid { get; set; }
        public string UserName { get; set; }
        public long LoginTime { get; set; }
        public long ClanNo { get; set; }

    }
}
