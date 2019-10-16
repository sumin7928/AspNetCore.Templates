using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiWebServer.Models;
using ApiWebServer.PBTables;

namespace ApiWebServer.Cache.PBTables
{
    public class ConstantTable : ICommonPBTable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public PvpConst PvpConst { get; private set; } = new PvpConst();

        public Const Const { get; private set; } = new Const();

        public bool LoadTable(MaguPBTableContext context)
        {
            try
            {
                // PB_PVPCONST
                int pvpConstCount = 0;
                foreach (var data in context.PB_PVPCONST.ToList())
                {
                    FieldInfo info = PvpConst.GetType().GetField(data.pvpconst_key);
                    if (info == null)
                    {
                        continue;
                    }

                    info.SetValue(PvpConst, data.value);
                    ++pvpConstCount;
                }

                int pvpRequiredCount = PvpConst.GetType().GetFields().Count();
                if (pvpRequiredCount != pvpConstCount)
                {
                    _logger.Warn("Invalidate PvpConst count!! Need to check - Set:{0}, Reqired:{1}", pvpConstCount, pvpRequiredCount);
                }

                // PB_CONST
                int constCount = 0;
                foreach (var data in context.PB_CONST.ToList())
                {
                    FieldInfo info = Const.GetType().GetField(data.const_key);
                    if (info == null)
                    {
                        continue;
                    }

                    info.SetValue(Const, data.value);
                    ++constCount;
                }

                int requiredCount = Const.GetType().GetFields().Count();
                if (requiredCount != constCount)
                {
                    _logger.Warn("Invalidate Const count!! Need to check - Set:{0}, Reqired:{1}", constCount, requiredCount);
                }

            }
            catch (Exception e)
            {
                _logger.Error(e, "const data loading error");
                return false;
            }

            return true;
        }
    }
}
