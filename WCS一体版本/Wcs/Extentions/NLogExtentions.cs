using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Wcs
{
    public static class NLogExtentions
    {
        public static void Trace1(this Logger logger, String msg, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Trace, msg, sender, null, null,taskCode,equipmentTaskId);
        }
        public static void Trace1(this Logger logger, String msg, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Trace, msg, sender, refData, null, taskCode, equipmentTaskId);
        }

        public static void Debug1(this Logger logger, String msg, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Debug, msg, sender, null, null, taskCode, equipmentTaskId);
        }
        public static void Debug1(this Logger logger, String msg, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Debug, msg, sender, refData, null, taskCode, equipmentTaskId);
        }

        public static void Info1(this Logger logger, String msg, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Info, msg, sender, null, null, taskCode, equipmentTaskId);
        }
        public static void Info1(this Logger logger, String msg, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Info, msg, sender, refData, null, taskCode, equipmentTaskId);
        }

        public static void Warn1(this Logger logger, String msg, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Warn, msg, sender, null, null, taskCode, equipmentTaskId);
        }
        public static void Warn1(this Logger logger, String msg, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Warn, msg, sender, refData, null, taskCode, equipmentTaskId);
        }

        public static void Error1(this Logger logger, Exception ex, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Error, ex.Message, sender, null, ex, taskCode, equipmentTaskId);
        }
        public static void Error1(this Logger logger, Exception ex, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Error, ex.Message, sender, refData, ex, taskCode, equipmentTaskId);
        }

        public static void Fatal1(this Logger logger, Exception ex, Object sender, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Fatal, ex.Message, sender, null, ex, taskCode, equipmentTaskId);
        }
        public static void Fatal1(this Logger logger, Exception ex, Object sender, Object refData, String taskCode = null, Int32? equipmentTaskId = null)
        {
            Log(logger, LogLevel.Fatal, ex.Message, sender, refData, ex, taskCode, equipmentTaskId);
        }
        
        static void Log(this Logger logger, LogLevel level, String msg, Object sender, Object refData,Exception exception,String taskCode, Int32? equipmentTaskId)
        {
            try
            {
                LogEventInfo eventInfo;
                if (level == LogLevel.Error || level == LogLevel.Fatal)
                {
                    eventInfo = LogEventInfo.Create(level, logger.Name, msg, exception);
                }
                else
                {
                    eventInfo = LogEventInfo.Create(level, logger.Name, msg);
                }
                eventInfo.Properties.Add("sender", sender);
                eventInfo.Properties.Add("data", refData);

                if (!Wcs.Security.WcsPrincipal.CurrentPrincipal.IsEmpty)
                {
                    var id = (Wcs.Security.WcsIdentity)Wcs.Security.WcsPrincipal.CurrentPrincipal.Identity;
                    eventInfo.Properties.Add("userName", id.Name);
                    eventInfo.Properties.Add("realName", id.RealName);
                    eventInfo.Properties.Add("roles", String.Join(",", id.RoleNames));
                }

                //if (refData != null)
                //{
                //    dynamic data = (dynamic)refData;

                //    Type type = refData.GetType();
                //    if (type.IsType("Wcs.Framework.EquipmentAction"))
                //    {
                //        if (equipmentTaskId == null)
                //        {
                //            eventInfo.Properties.Add("equipmentTaskId", data.EquipmentTaskId);
                //        }

                //        if (String.IsNullOrWhiteSpace(taskCode) && data.Movement != null && data.Movement.Task != null)
                //        {
                //            eventInfo.Properties.Add("taskCode", data.Movement.Task.TaskCode);
                //        }
                //    }
                //    else if (String.IsNullOrWhiteSpace(taskCode) && type.IsType("Wcs.Framework.LogicMovement"))
                //    {
                //        if (data.Task != null)
                //        {
                //            eventInfo.Properties.Add("taskCode", data.Task.TaskCode);
                //        }
                //    }
                //    else if (String.IsNullOrWhiteSpace(taskCode) && type.IsType("Wcs.Framework.Task"))
                //    {
                //        eventInfo.Properties.Add("taskCode", data.TaskCode);
                //    }

                //}


                //if (!String.IsNullOrWhiteSpace(taskCode))
                //{
                //    eventInfo.Properties.Add("taskCode", taskCode);
                //}

                //if (equipmentTaskId != null)
                //{
                //    eventInfo.Properties.Add("equipmentTaskId", equipmentTaskId);
                //}

                object log_taskCode = taskCode, log_equipmentTaskId = equipmentTaskId;

                if (refData != null)
                {
                    dynamic data = (dynamic)refData;

                    Type type = refData.GetType();
                    if (type.IsType("Wcs.Framework.EquipmentAction"))
                    {
                        if (equipmentTaskId == null)
                        {
                            log_equipmentTaskId = data.EquipmentTaskId;
                        }

                        if (String.IsNullOrWhiteSpace(taskCode) && data.Movement != null && data.Movement.Task != null)
                        {
                            log_taskCode = data.Movement.Task.TaskCode;
                        }
                    }
                    else if (String.IsNullOrWhiteSpace(taskCode) && type.IsType("Wcs.Framework.LogicMovement"))
                    {
                        if (data.Task != null)
                        {
                            log_taskCode = data.Task.TaskCode;
                        }
                    }
                    else if (String.IsNullOrWhiteSpace(taskCode) && type.IsType("Wcs.Framework.Task"))
                    {
                        log_taskCode = data.TaskCode;
                    }

                    if(log_equipmentTaskId==null && type.GetProperty("EquipmentTaskId") != null)
                    {
                        log_equipmentTaskId = data.EquipmentTaskId;
                    }

                    if (log_taskCode == null && type.GetProperty("TaskCode") != null)
                    {
                        log_taskCode = data.TaskCode;
                    }
                }


                if (log_taskCode!=null)
                {
                    eventInfo.Properties.Add("taskCode", log_taskCode);
                }

                if (log_equipmentTaskId != null)
                {
                    eventInfo.Properties.Add("equipmentTaskId", log_equipmentTaskId);
                }


                logger.Log(eventInfo);
            }
            catch (Exception ex)
            {
                
            }
        }
        }
}
