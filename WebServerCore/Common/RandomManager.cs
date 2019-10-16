using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiWebServer.Common.Define;

namespace ApiWebServer.Common
{
    public sealed class RandomManager
    {
        static RandomManager()
        {
            Instance = new RandomManager();
        }

        private RandomManager()
        {
            foreach (var type in Enum.GetNames(typeof(RANDOM_TYPE)))
            {
                _randomSeeds.Add((RANDOM_TYPE)Enum.Parse(typeof(RANDOM_TYPE), type), new Random());
                _randomLocks.Add((RANDOM_TYPE)Enum.Parse(typeof(RANDOM_TYPE), type), new object());
            }
        }

        public static RandomManager Instance { get; }

        private Dictionary<RANDOM_TYPE, Random> _randomSeeds = new Dictionary<RANDOM_TYPE, Random>();
        private Dictionary<RANDOM_TYPE, object> _randomLocks = new Dictionary<RANDOM_TYPE, object>();

        /// <summary>
        /// 최대 값에 대한 랜덤 인덱스 값을 가져옴 ex) maxValue = 10 -> 0 ~ 9 값 리턴
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값(인덱스)</returns>
        public int GetIndex(RANDOM_TYPE type, int maxValue)
        {
            int result = 0;
            lock (_randomLocks[type])
            {
                result = _randomSeeds[type].Next(maxValue);
            }

            return result;
        }

        /// <summary>
        /// 최대 값에 대한 랜덤 인덱스 값을 가져옴 ex) maxValue = 10 -> 0 ~ 9 값 리턴
        /// </summary>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값(인덱스)</returns>
        public int GetIndex(int maxValue)
        {
            return GetIndex(RANDOM_TYPE.GLOBAL, maxValue);
        }

        /// <summary>
        /// 최대 값에 대한 랜덤 값을 가져옴 ex) maxValue = 10 -> 1 ~ 10 값 리턴
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값</returns>
        public int GetCount(RANDOM_TYPE type, int maxValue)
        {
            int result = 0;
            lock (_randomLocks[type])
            {
                result = _randomSeeds[type].Next(maxValue);
            }

            result += 1; // 최소값이 1베이스이며 최대값에 maxValue 포함하는 보정값 추가

            return result;
        }

        /// <summary>
        /// 최대 값에 대한 랜덤 값을 가져옴 ex) maxValue = 10 -> 1 ~ 10 값 리턴
        /// </summary>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값</returns>
        public int GetCount(int maxValue)
        {
            return GetCount(RANDOM_TYPE.GLOBAL, maxValue);
        }

        /// <summary>
        /// 최소 최대 값 사이의 랜덤 값을 가져옴 ex) minValue = 1, maxValue = 10 -> 1 ~ 10 값 리턴
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="minValue">최소 값</param>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값</returns>
        public int GetCount(RANDOM_TYPE type, int minValue, int maxValue)
        {
            int result = 0;
            lock (_randomLocks[type])
            {
                result = _randomSeeds[type].Next(minValue, maxValue + 1); // maxValue 포함하는 보정값 추가
            }

            return result;
        }

        /// <summary>
        /// 최소 최대 값 사이의 랜덤 값을 가져옴 ex) minValue = 1, maxValue = 10 -> 1 ~ 10 값 리턴
        /// </summary>
        /// <param name="minValue">최소 값</param>
        /// <param name="maxValue">최대 값</param>
        /// <returns>랜덤 결과 값</returns>
        public int GetCount(int minValue, int maxValue)
        {
            return GetCount(RANDOM_TYPE.GLOBAL, minValue, maxValue);
        }

        /// <summary>
        /// 해당 확률에 대한 성공 여부를 리턴 ex) ratioValue = 10, maxRatio = 100 -> 10프로의 확률 성공 여부
        /// 설정 확률 값을 포함한 확률로 계산됨
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="ratioValue">설정 확률 값</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <returns>확률에 포함되는지 결과 리턴(성공/실패)</returns>
        public bool IsSuccessRatio(RANDOM_TYPE type, int ratioValue, int maxRatio)
        {
            int randomValue = GetCount(type, maxRatio);

            if (randomValue <= ratioValue)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 해당 확률에 대한 성공 여부를 리턴 ex) ratioValue = 10, maxRatio = 100 -> 10프로의 확률 성공 여부
        /// 설정 확률 값을 포함한 확률로 계산됨
        /// </summary>
        /// <param name="ratioValue">설정 확률 값</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <returns>확률에 포함되는지 결과 리턴(성공/실패)</returns>
        public bool IsSuccessRatio(int ratioValue, int maxRatio)
        {
            return IsSuccessRatio(RANDOM_TYPE.GLOBAL, ratioValue, maxRatio);
        }


        /// <summary>
        /// 확률 배열에 포함된 확률에 속한 인덱스값을 리턴 ex) ratioList = [10,90] -> 100% 확률 대비 10% : 0 리턴, 90% :1 리턴
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="ratioList">확률 배열</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <param name="exceptIdxList">확률에서 제외되는 인덱스 (default = null)</param>
        /// <returns>확률에 포함된 배열 인덱스 ( -1일경우 결과 없음 )</returns>
        public int GetSuccessIdxFromRatioList(RANDOM_TYPE type, int[] ratioList, int maxRatio, int[] exceptIdxList = null)
        {
            int index = -1;
            int accumulation = 0;
            int maxValue = maxRatio;

            if (exceptIdxList != null && exceptIdxList.Length > 0)
            {
                foreach (int i in exceptIdxList)
                {
                    // 인덱스에 없으므로 스킵 처리
                    if (i > ratioList.Length - 1)
                    {
                        continue;
                    }

                    maxValue -= ratioList[i];
                }

                int rationValue = GetCount(type, maxValue);

                for (int i = 0; i < ratioList.Length; ++i)
                {
                    accumulation += ratioList[i];
                    if (exceptIdxList.Contains(i))
                    {
                        rationValue += ratioList[i];
                        continue;
                    }

                    if (rationValue <= accumulation)
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            else
            {
                int rationValue = GetCount(type, maxValue);

                for (int i = 0; i < ratioList.Length; ++i)
                {
                    accumulation += ratioList[i];
                    if (rationValue <= accumulation)
                    {
                        index = i;
                        break;
                    }
                }

                return index;
            }
        }

        /// <summary>
        /// 확률 배열에 포함된 확률에 속한 인덱스값을 리턴 ex) ratioList = [10,90] -> 100% 확률 대비 10% : 0 리턴, 90% :1 리턴
        /// </summary>
        /// <param name="ratioList">확률 배열</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <param name="exceptIdxList">확률에서 제외되는 인덱스 (default = null)</param>
        /// <returns>확률에 포함된 배열 인덱스 ( -1일경우 결과 없음 )</returns>
        public int GetSuccessIdxFromRatioList(int[] ratioList, int maxRatio, int[] exceptIdxList = null)
        {
            return GetSuccessIdxFromRatioList(RANDOM_TYPE.GLOBAL, ratioList, maxRatio, exceptIdxList);
        }

        /// <summary>
        ///  확률 배열에 포함된 확률에 속한 인덱스 리스트를 리턴 ex) ratioList = [10,30,60] -> 100% 확률 대비 10% : 0 리턴, 30%:1 리턴, 60% :2 리턴
        ///  확률 배열보다 getCount가 많을 경우 에러가 아니라 최대 확률 배열만큼 인덱스를 다 줌
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="ratioList">확률 배열</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <param name="getCount">요청 결과 인덱스 갯수</param>
        /// <returns>요청한 갯수 만큼 확률에 포함된 중복되지 않는 인덱스 리스트</returns>
        public List<int> GetSuccessIdxListFromRatioList(RANDOM_TYPE type, int[] ratioList, int maxRatio, int getCount)
        {
            List<int> resultList = new List<int>();
            int maxValue = maxRatio;

            if (getCount >= ratioList.Length)
            {
                for (int i = 0; i < ratioList.Length; ++i)
                {
                    resultList.Add(i);
                }
            }

            if (resultList.Count == 0)
            {
                for (int i = 0; i < getCount; ++i)
                {
                    int rationValue = GetCount(type, maxValue);
                    int accumulation = 0;

                    for (int j = 0; j < ratioList.Length; ++j)
                    {
                        accumulation += ratioList[j];
                        if (resultList.Contains(j))
                        {
                            rationValue += ratioList[j];
                            continue;
                        }

                        if (rationValue <= accumulation)
                        {
                            resultList.Add(j);
                            maxValue -= ratioList[j];
                            break;
                        }
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        ///  확률 배열에 포함된 확률에 속한 인덱스 리스트를 리턴 ex) ratioList = [10,30,60] -> 100% 확률 대비 10% : 0 리턴, 30%:1 리턴, 60% :2 리턴
        ///  확률 배열보다 getCount가 많을 경우 에러가 아니라 최대 확률 배열만큼 인덱스를 다 줌
        /// </summary>
        /// <param name="ratioList">확률 배열</param>
        /// <param name="maxRatio">최대 확률 값</param>
        /// <param name="getCount">요청 결과 인덱스 갯수</param>
        /// <returns>요청한 갯수 만큼 확률에 포함된 중복되지 않는 인덱스 리스트</returns>
        public List<int> GetSuccessIdxListFromRatioList(int[] ratioList, int maxRatio, int getCount)
        {
            return GetSuccessIdxListFromRatioList(RANDOM_TYPE.GLOBAL, ratioList, maxRatio, getCount);
        }

        /// <summary>
        /// 누적 확률 배열에서 속한 인덱스 값을 리턴 ex) accumulateRatioList = [10,100] -> 100% 확률 대비 10% : 0 리턴, 90% :1 리턴
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="accumulateRatioList">누적 확률 리스트</param>
        /// <param name="exceptIdxList">확률에서 제외되는 인덱스 (default = null)</param>
        /// <returns></returns>
        public int GetSuccessIdxFromAccumulateRatioList(RANDOM_TYPE type, List<int> accumulateRatioList, List<int> exceptIdxList = null)
        {
            int index = -1;
            int maxValue = accumulateRatioList[ accumulateRatioList.Count - 1 ];

            if (exceptIdxList != null && exceptIdxList.Count > 0)
            {
                foreach (int i in exceptIdxList)
                {
                    // 인덱스에 없으므로 스킵 처리
                    if (i > accumulateRatioList.Count - 1)
                    {
                        continue;
                    }

                    maxValue -= (i == 0) ? accumulateRatioList[i] : accumulateRatioList[i] - accumulateRatioList[i - 1];
                }

                int rationValue = GetCount(type, maxValue);

                for (int i = 0; i < accumulateRatioList.Count; ++i)
                {
                    if (exceptIdxList.Contains(i))
                    {
                        rationValue += (i == 0) ? accumulateRatioList[i] : accumulateRatioList[i] - accumulateRatioList[i - 1];
                        continue;
                    }

                    if (rationValue <= accumulateRatioList[i])
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
            else
            {
                int rationValue = GetCount(type, maxValue);
                for (int i = 0; i < accumulateRatioList.Count; ++i)
                {
                    if (rationValue <= accumulateRatioList[i])
                    {
                        index = i;
                        break;
                    }
                }

                return index;
            }
        }

        /// <summary>
        /// 누적 확률 배열에서 속한 인덱스 값을 리턴 ex) accumulateRatioList = [10,100] -> 100% 확률 대비 10% : 0 리턴, 90% :1 리턴
        /// </summary>
        /// <param name="accumulateRatioList">누적 확률 리스트</param>
        /// <param name="exceptIdxList">확률에서 제외되는 인덱스 (default = null)</param>
        /// <returns></returns>
        public int GetSuccessIdxFromAccumulateRatioList(List<int> accumulateRatioList, List<int> exceptIdxList = null)
        {
            return GetSuccessIdxFromAccumulateRatioList(RANDOM_TYPE.GLOBAL, accumulateRatioList, exceptIdxList);
        }

        /// <summary>
        ///  누적 확률 배열에 포함된 확률에 속한 인덱스 리스트를 리턴 ex) ratioList = [10,30,100] -> 100% 확률 대비 10% : 0 리턴, 20%:1 리턴, 60% :2 리턴
        ///  누적 확률 배열보다 getCount가 많을 경우 에러가 아니라 최대 확률 배열만큼 인덱스를 다 줌
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="accumulateRatioList">누적 확률 배열</param>
        /// <param name="getCount">요청 결과 인덱스 갯수</param>
        /// <returns>요청한 갯수 만큼 확률에 포함된 중복되지 않는 인덱스 리스트</returns>
        public List<int> GetSuccessIdxListFromAccumulateRatioList(RANDOM_TYPE type, List<int> accumulateRatioList, int getCount)
        {
            List<int> resultList = new List<int>();

            if (getCount >= accumulateRatioList.Count)
            {
                for (int i = 0; i < accumulateRatioList.Count; ++i)
                {
                    resultList.Add(i);
                }
            }

            if (resultList.Count == 0)
            {
                int maxValue = accumulateRatioList[accumulateRatioList.Count - 1];

                for (int i = 0; i < getCount; ++i)
                {
                    int rationValue = GetCount(type, maxValue);

                    for (int j = 0; j < accumulateRatioList.Count; ++j)
                    {
                        if (resultList.Contains(j))
                        {
                            rationValue += (j == 0) ? accumulateRatioList[j] : accumulateRatioList[j] - accumulateRatioList[j - 1];
                            continue;
                        }

                        if (rationValue <= accumulateRatioList[j])
                        {
                            resultList.Add(j);
                            maxValue -= (j == 0) ? accumulateRatioList[j] : accumulateRatioList[j] - accumulateRatioList[j - 1];
                            break;
                        }
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        ///  확률이 다똑같을때 해당리스트들중에 중복없이 여러개 선택
        /// </summary>
        /// <param name="type">랜덤 타입</param>
        /// <param name="listCount">리스트 갯수</param>
        /// <param name="getCount">요청 결과 인덱스 갯수</param>
        /// <returns>요청한 갯수 만큼 확률에 포함된 중복되지 않는 인덱스 리스트</returns>
        public List<int> GetSuccessIdxListFromIndexList(RANDOM_TYPE type, int listCount, int getCount)
        {
            List<int> resultList = new List<int>();

            if (getCount >= listCount)
            {
                for (int i = 0; i < listCount; ++i)
                {
                    resultList.Add(i);
                }
            }

            if (resultList.Count == 0)
            {
                int maxValue = listCount;

                for (int i = 0; i < getCount; ++i)
                {
                    int rationValue = GetIndex(type, maxValue);

                    for (int j = 0; j < listCount; ++j)
                    {
                        if (resultList.Contains(j))
                        {
                            rationValue += 1;
                            continue;
                        }

                        if (rationValue <= j)
                        {
                            resultList.Add(j);
                            maxValue -= 1;
                            break;
                        }
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// 리스트에서 랜덤으로 선택된 요소 하나를 가져옴
        /// </summary>
        /// <typeparam name="T">리스트 타입</typeparam>
        /// <param name="type">랜덤 타입</param>
        /// <param name="list">요청 리스트</param>
        /// <returns>리스트 내에서 추출된 랜덤 값</returns>
        public T GetRandomFromList<T>(RANDOM_TYPE type, List<T> list)
        {
            int index = GetIndex(type, list.Count);
            return list[index];
        }

        /// <summary>
        /// 리스트에서 랜덤으로 선택된 요소 하나를 가져옴
        /// </summary>
        /// <typeparam name="T">리스트 타입</typeparam>
        /// <param name="list">요청 리스트</param>
        /// <returns>리스트 내에서 추출된 랜덤 값</returns>
        public T GetRandomFromList<T>(List<T> list)
        {
            return GetRandomFromList(RANDOM_TYPE.GLOBAL, list);
        }
    }
}
