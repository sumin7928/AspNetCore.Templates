using System;

namespace ApiWebServer.Models
{
    public class Account
    {
        public long pc_id;
        public byte is_guest;
        public string pc_name;
        public byte db_num;
        public byte is_firsttime_flag;
        public byte is_out_user;
        public int block_range;
        public string block_reason;
        public DateTime date_time;
    }

    public class AccountPublisher
    {
        public byte pub_type;
        public string pub_id;
    }
}
