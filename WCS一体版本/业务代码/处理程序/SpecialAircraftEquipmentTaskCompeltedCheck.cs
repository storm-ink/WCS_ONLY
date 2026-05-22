using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wcs;
using Wcs.Framework;
using NLog;
using Wcs.DefaultImplementCollection.Conveyor;
using NHibernate.Linq;

namespace ZHQXC
{
    /// <summary>
    /// 对到达专机的输送线任务完成时，需要给专机传送到达命令
    /// </summary>
    public class SpecialAircraftEquipmentTaskCompeltedCheck : ThreadRunningLog, ITaskEventHandler<TaskCompletedEventArgs>
    {
        Logger _logger;

        public SpecialAircraftEquipmentTaskCompeltedCheck()
        {
            _logger = LogManager.CreateNullLogger();
            this.Init("SpecialAircraftEquipmentTaskCompeltedCheck日志");
        }

        public void Handle(TaskableDevice device, NHibernate.ISession session, TaskCompletedEventArgs args)
        {
            try
            {
                if (!(device is ConveyorDevice))
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,其非 {2} 设备类型，被 {3} 忽略。", device, args.EquipmentTaskId, typeof(ConveyorDevice), this);
                    _logger.Debug(msg, this, args, null, args.EquipmentTaskId);
                    args.Handled = true;
                    this.Log($"{msg}");
                    return;
                }

                _logger.Trace(string.Format("{0} 开始处理设备任务 {1} 的完成事件...", this, args.EquipmentTaskId), this, args, null, args.EquipmentTaskId);
                this.Log($"\r\n==》{args.EquipmentTaskId}开始处理设备任务的完成事件...{this.ToString()}");
                //是否存在输送线存在的物理输送动作
                var action = session.Query<ConveyorTransferAction>().FirstOrDefault(x => x.EquipmentTaskId == args.EquipmentTaskId);
                if (action == null)
                {
                    String msg = string.Format("收到了 {0} 发送来的 {1} 任务完成信号,但未找到对应的 {2} 对象，忽略本信号", device, args.EquipmentTaskId, typeof(ConveyorDevice));
                    args.Handled = true;
                    _logger.Trace(msg, this, args, null, args.EquipmentTaskId);
                    this.Log($"{msg}");
                    return;
                }

                var conveyor = DeviceConverter.ToDevice<ConveyorDevice>("专机CV");
                string trayBarcode = "";
                string isfull = "";
                if (action.Movement.Task.AdditionalInfo.ContainsKey("TrayBarcode"))
                    trayBarcode = action.Movement.Task.AdditionalInfo["TrayBarcode"];
                if (action.Movement.Task.AdditionalInfo.ContainsKey("IsFull"))
                    isfull = action.Movement.Task.AdditionalInfo["IsFull"];
                ///对到达专机的输送线任务完成时，需要给专机传送到达命令
                ///2011位置：进行拆盖和合盖操作
                if (action.EndLocation.DeviceCode == "1021")
                {
                    SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                    {
                        PosNo = (UInt16)int.Parse(action.EndLocation.DeviceCode),
                        IsFull = (UInt16)int.Parse(isfull),
                        TaskType = (UInt16)int.Parse(action.Movement.Task.TaskType),
                        TrayBarcode = trayBarcode,
                        DataID = (UInt16)new Random().Next(0, UInt16.MaxValue)
                    };
                    try
                    {
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                ///到达异常工位进行人工处理
                ///2010异常工位点
                else if (action.EndLocation.DeviceCode == "2010")
                {
                    SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                    {
                        PosNo = 2010,
                        IsFull = 2,
                        TaskType = 4,
                        TrayBarcode = trayBarcode,
                        DataID = (UInt16)new Random().Next(0, UInt16.MaxValue)
                    };
                    try
                    {
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                else if (action.EndLocation.DeviceCode == "2013" ||
                    action.EndLocation.DeviceCode == "2014" ||
                    action.EndLocation.DeviceCode == "2015" ||
                    action.EndLocation.DeviceCode == "2016" ||
                    action.EndLocation.DeviceCode == "2017" ||
                    action.EndLocation.DeviceCode == "2018")
                {
                    SpecialAircraftEquipment_Command specialaircraftequipment_Command = new SpecialAircraftEquipment_Command()
                    {
                        PosNo = (UInt16)int.Parse(action.EndLocation.DeviceCode),
                        IsFull = (UInt16)int.Parse(isfull),
                        TaskType = (UInt16)int.Parse(action.Movement.Task.TaskType),
                        TrayBarcode = trayBarcode,
                        DataID = (UInt16)new Random().Next(0, UInt16.MaxValue) 
                    };
                    try
                    {
                        conveyor.Write<SpecialAircraftEquipment_Command>(specialaircraftequipment_Command, specialaircraftequipment_Command.SendSuccess);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                args.Handled = true;
                return;
            }
            catch (Exception ex)
            {
                this.Log($"{ex}");
                _logger.Error1(ex, this);
                Wcs.Framework.MessageBoard.AbstractMessageBoard.Instance.Add(
                        new Wcs.Framework.MessageBoard.Messages.TipMessage(Wcs.Framework.MessageBoard.MessageLevel.Emergency,
                        "ShapeCheckHand",
                        ex.Message,
                        null));
                args.Handled = false;
                return;
            }
        }
    }
}
