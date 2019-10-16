using System;
using System.Collections.Generic;
using System.Threading;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Common
{
    public sealed class KeyGenerator
    {
        static KeyGenerator()
        {
            Instance = new KeyGenerator();
        }

        private KeyGenerator() { }

        public static KeyGenerator Instance { get; }

        public int ServerNumber { get; set; }
        private int incrementNo = 0;

        public void SetIncrementNo(int number)
        {
            Interlocked.Exchange(ref incrementNo, number);
        }

        public string GetDefaultKey(GAME_KEY_TYPE gameType)
        {
            return $"{(byte)gameType}{ServerNumber:D2}{ServerUtils.GetNowLocalMilliTimeStemp()}";
        }

        public string GetIncrementKey(GAME_KEY_TYPE gameType)
        {
            int number = Interlocked.Increment(ref incrementNo) % 10;
            if (number < 0)
            {
                number = -number;
            }
            return $"{(byte)gameType}{ServerNumber:D2}{ServerUtils.GetNowLocalMilliTimeStemp()}{number}";
        }

        public bool ValidateKey(GAME_KEY_TYPE gameType, string key, int validTime)
        {
            int length = ((byte)gameType).ToString().Length;

            if (byte.TryParse(key.Substring(0, length), out byte type) == false)
            {
                return false;
            }
            if (type != (byte)gameType)
            {
                return false;
            }

            if (long.TryParse(key.Substring(length + 2, 10), out long startTime) == false)
            {
                return false;
            }
            if (startTime + validTime > ServerUtils.GetNowLocalTimeStemp())
            {
                return false;
            }

            return true;
        }
    }
}
