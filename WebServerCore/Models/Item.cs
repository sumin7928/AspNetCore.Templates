using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiWebServer.Models
{
    public class ItemCard
    {
        public int serialIdx;
        public byte rewardCardType;
        public int singleRate;
        public int accumulRate;
        public ItemCard(int _serialIdx, byte _rewardCardType, int _singleRate, int _accumulRate)
        {
            serialIdx = _serialIdx;
            rewardCardType = _rewardCardType;
            singleRate = _singleRate;
            accumulRate = _accumulRate;
        }
    }
}
