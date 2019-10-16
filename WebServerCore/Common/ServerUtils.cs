using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiWebServer.Common
{
    public static class ServerUtils
    {
        public static string MakeSplittedString<T>( List<T> list )
        {
            StringBuilder sb = new StringBuilder();
            for( int i = 0; i < list.Count; ++i )
            {
                if( i != 0 )
                {
                    sb.Append( ',' );
                }
                sb.Append( list[ i ] );
            }

            return sb.ToString();
        }


        public static T GetConfigValue<T>( IConfigurationSection section, string key, T defaultValue )
        {
            string value = section[ key ];
            if ( value == null )
            {
                return defaultValue;
            }

            return ( T )Convert.ChangeType( value, typeof( T ) );
        }

        public static T GetConfigValue<T>(IConfigurationSection section, string key)
        {
            string value = section[key];
            if (value == null)
            {
                return default(T);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static long GetNowUtcTimeStemp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        public static long GetNowUtcTimeStemp( TimeSpan addTime )
        {
            return new DateTimeOffset(DateTime.UtcNow.Add(addTime)).ToUnixTimeSeconds();
        }

        public static long GetNowLocalTimeStemp()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }
        public static long GetNowLocalMilliTimeStemp()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        }


        public static string ReduceJsonLog(string log, int validLogSize = 1024)
        {
            if (log == null || log.Length <= validLogSize)
            {
                return log;
            }

            var json = JObject.Parse(log);
            bool isConverted = false;
            foreach (var data in json.Properties())
            {
                if (data.Value.Type == JTokenType.Array)
                {
                    // value is json array
                    var jsonArray = (JArray)data.Value;
                    json[data.Name] = "JArray`count:" + jsonArray.Count;
                    isConverted = true;
                }
                else if (data.Value.Type == JTokenType.Object)
                {
                    // value is json object ( dictionary )
                    var jsonObject = (JObject)data.Value;
                    json[data.Name] = "JObject`count:" + jsonObject.Count;
                    isConverted = true;
                }
                else if (data.Value.Type == JTokenType.String)
                {
                    string value = (string)data.Value;
                    if (value == null || value == string.Empty)
                    {
                        continue;
                    }

                    // value is json string array
                    if (value.First().Equals('[') && value.Last().Equals(']'))
                    {
                        var jsonArray = JArray.Parse(value);
                        json[data.Name] = "JArray`count:" + jsonArray.Count;
                        isConverted = true;
                    }
                }
            }
            if (isConverted)
            {
                return JsonConvert.SerializeObject(json);
            }
            else
            {
                return log;
            }
        }

    }
}
