using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiWebServer.Common
{
    public static class ShuffleExtension
    {
        static int _seed = Environment.TickCount;

        static readonly ThreadLocal<Random> _random = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Value.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void ShuffleForSelectedCount<T>(this IList<T> list, int selectCnt)
        {
            if (selectCnt > list.Count)
                selectCnt = list.Count;

            int totalCnt = list.Count;
            int n = 0;

            while (n < selectCnt)
            {
                int k = _random.Value.Next(totalCnt - n) + n;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
                ++n;
            }
        }
    }
}
