using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Models;
using WebSharedLib.Entity;

namespace ApiWebServer.Logic
{
    public class RankServer
    {
        private readonly ILogger _logger;

        public ICacheClient Client { get; private set; }

        public RankServer(ICacheClient redisClient, ILogger logger)
        {
            Client = redisClient;
            _logger = logger;
        }

        public async Task<double> IncreaseScore(string key, long pcId, double incrementValue)
        {
            return await Client.Database.SortedSetIncrementAsync(key, pcId, incrementValue);
        }
        public async Task<double> DecreaseScore(string key, long pcId, double decrementValue)
        {
            return await Client.Database.SortedSetDecrementAsync(key, pcId, decrementValue);
        }

        public async Task SetScore(string key, long pcId, double score)
        {
            if (await Client.Database.SortedSetAddAsync(key, pcId, score) == false)
            {
                _logger.LogError("[RankServer] Failed to set score at key:{0}, pcId:{1}, score:{2}", key, pcId, score);
            }
        }

        public async Task<double> GetScore(string key, long pcId)
        {
            double? score = await Client.Database.SortedSetScoreAsync(key, pcId);
            if (score.HasValue == false)
            {
                return 0;
            }
            return score.Value;
        }

        public async Task RemoveScore(string key, long pcId)
        {
            if (await Client.Database.SortedSetRemoveAsync(key, pcId) == false)
            {
                _logger.LogError("[RankServer] Failed to remove score at key:{0}, pcId:{1}", key, pcId);
            }
        }

        public async Task<long> GetScoreRank(string key, long pcId)
        {
            long? cacheRank = await Client.Database.SortedSetRankAsync(key, pcId, Order.Descending);
            if (cacheRank.HasValue == false)
            {
                return 0;
            }

            // 랭킹 0 베이스 보정
            return cacheRank.Value + 1;
        }

        public async Task<SortedSetEntry[]> GetScoreRankRange(string key, long start = 0, long stop = 1000)
        {
            return await Client.Database.SortedSetRangeByRankWithScoresAsync(key, start, stop, Order.Descending);
        }

        public async Task<bool> IsExistKey(string key)
        {
            return await Client.Database.KeyExistsAsync(key);
        }

        public async Task SetExpiredTime(string key, TimeSpan time)
        {
            if (await Client.UpdateExpiryAsync(key, time) == false)
            {
                _logger.LogError("[RankServer] Failed to set expire time at key:{0}", key);
            }
        }

        public async Task<long> GetRankTotalCount(string key)
        {
            return await Client.Database.SortedSetLengthAsync(key);
        }

        public List<long> GetMatchUsers(string key)
        {
            return Client.Database.SortedSetScan(key).Select(x => (long)x.Element).ToList();
        }

        public List<long> GetMatchUsers(List<string> keys)
        {
            List<long> members = new List<long>();
            foreach (string key in keys)
            {
                IEnumerable<SortedSetEntry> values = Client.Database.SortedSetScan(key);
                members.AddRange(values.Select(x => (long)x.Element));
            }

            return members;
        }

        public async Task<List<long>> GetRangeMatchUsers(string key, int startOverall, int endOverall)
        {
            List<long> members = new List<long>();

            RedisValue[] values = await Client.Database.SortedSetRangeByScoreAsync(key, startOverall, endOverall);
            members.AddRange(values.Where(x => x.HasValue).Select(s => (long)s.Box()));
            return members;
        }

        public async Task SetHashData<T>(string key, string column, T data, TimeSpan expiredTime)
        {
            if (await Client.HashSetAsync(key, column, data) == false)
            {
                _logger.LogError("[RankServer] Failed to set hash data at key:{0}, column:{1}", key, column);
            }
            if (await Client.UpdateExpiryAsync(key, expiredTime) == false)
            {
                _logger.LogError("[RankServer] Failed to set expire time at key:{0}", key);
            }
        }

        public async Task<T> GetHashData<T>(string key, string column)
        {
            return await Client.HashGetAsync<T>(key, column);
        }

        public async Task<Dictionary<string, T>> GetHashAllData<T>(string key)
        {
            return await Client.HashGetAllAsync<T>(key);
        }

        public async Task<T> GetData<T>(string key)
        {
            return await Client.GetAsync<T>(key);
        }

        public async Task<IDictionary<string, T>> GetData<T>(List<string> keys)
        {
            return await Client.GetAllAsync<T>(keys);
        }

        public async Task SetData<T>(string key, T info, TimeSpan expiredTime)
        {
            if (await Client.AddAsync(key, info, expiredTime) == false)
            {
                _logger.LogError("[RankServer] Failed to set data at key:{0}", key);
            }
        }

    }
}
