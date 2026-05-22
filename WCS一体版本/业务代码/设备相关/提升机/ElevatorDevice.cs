using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wcs.DefaultImpls.Conveyor;
using Wcs.Framework;

namespace BOE
{
    /// <summary>
    /// 为方便检查提升机状态及问题分析
    /// 添加此设备,对应的DB块挂在对应的输送线DB2
    /// 由输送线主动发送数据；
    /// </summary>
    [System.ComponentModel.Description("提升机")]
    public  class ElevatorDevice : Wcs.Framework.TaskableDevice
    {
        String _ownerConveyorDeviceName;

        string _posNo;
        public String PosNo
        {
            get { return  _posNo;}
        }
        public ConveyorDevice OwnerConveyorDevice
        {
            get
            {
                return DeviceConverter.ToDevice<ConveyorDevice>(_ownerConveyorDeviceName);
            }
        }
        public ElevatorDevice(string name, int no, int receiveTimeout, int connectTimeout, int sendTimeout, String ownerConveyorDeviceName,string posNo)
            : base(name, no, receiveTimeout, connectTimeout, sendTimeout, false)
        {
            _ownerConveyorDeviceName = ownerConveyorDeviceName;
            _posNo = posNo;
        }

        public override bool IsConnected
        {
            get 
            {
                return this.OwnerConveyorDevice.IsConnected;
            }
        }

        public override bool IsIdle
        {
            get
            {
                if (!this.OwnerConveyorDevice.IsIdle)
                {
                    return false;
                }

                if (this.Locker != null && !this.Locker.IsEmpty)
                {
                    return false;
                }

                return true;
            }
        }


        public override int[] OccupiedEquipmentTasks
        {
            get
            {
                return this.OwnerConveyorDevice.OccupiedEquipmentTasks;
            }
        }
        public override bool Connect()
        {
            return this.OwnerConveyorDevice.IsConnected;
        }

        public override IDeviceUserInterface CreateUserInterface()
        {
            return new ElevatorDeviceUI();
           
        }

        public override void Disconnect()
        {
            throw new NotImplementedException("提升机设备不支持手动断开.");
        }

      
        public override string[] Warnings
        {
            get
            {
                List<string> result = new List<string>();
                if (!this.OwnerConveyorDevice.IsConnected)
                {
                    result.Add(string.Format("所属输送线({0})未连接", this.OwnerConveyorDevice));
                }

                if (this.OwnerConveyorDevice.ReadStatus<HoistNetTransferObject>() == null || this.OwnerConveyorDevice.ReadStatus<HoistNetTransferObject>().Length == 0)
                {
                    result.Add(string.Format("输送线({0})提升机设备状态未同步", this.OwnerConveyorDevice));
                    return result.ToArray();
                }

                var status = this.OwnerConveyorDevice.ReadStatus<HoistNetTransferObject>().Where(x => x.AtPacketIndex == this.No).FirstOrDefault();
                if (status == null)
                    result.Add("设备状态获取失败<设备编号{0}无效>");
                else
                    result.AddRange(status.GetWarings());

                return result.ToArray();
            }
        }




        public override void CancelTask(EquipmentAction action)
        {
            throw new NotImplementedException();
        }

        public override TState Read<TState>()
        {
            throw new NotImplementedException();
        }

        public override void SendTask(EquipmentAction action)
        {
            throw new NotImplementedException();
        }

        public override void Write<TCommand>(TCommand data, Func<TaskableDevice, TCommand, bool> isSuccess)
        {
            throw new NotImplementedException();
        }
    }
}