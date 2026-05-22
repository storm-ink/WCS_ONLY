using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;
using NHibernate.Linq;

namespace Wcs.DefaultImplementCollection.Conveyor
{
    /// <summary>
    /// 默认的已使用的输送线设备任务地址索引获取器。
    /// 该类会获取设备中任务块中的占用索引和类型为 <see cref="T:Wcs.DefaultImpls.ConveyorTransferAction"/> 的物理动作所占用的任务地址索引集合。
    /// </summary>
    public class DefaultUsedConveyorTaskBlockIndexGetter:IUsedConveyorTaskBlockIndexGetter
    {
        public int[] GetUsedIndexs(ConveyorDevice conveyorDevice)
        {
            if (conveyorDevice.TaskBlocks == null || conveyorDevice.TaskBlocks.Length == 0)
            {
                throw new Exception(String.Format("{0} 未连接或状态同步失败，当前无法获取空闲的任务位置。", conveyorDevice));
            }

            //获取设备中占用的索引位置
            List<int> usedIndexs = conveyorDevice
                .TaskBlocks
                .Where(x => x.HandShake != TaskHandShakes.Empty)
                .Select(x => x.AtPacketIndex)
                .ToList();

            
            //获取数据库中任务占用的索引位置
            using (NHUnitOfWork newUnitOfWork = new NHUnitOfWork(System.Data.IsolationLevel.ReadUncommitted))
            {
                var indexs = newUnitOfWork
                    .session
                    .Query<ConveyorTransferAction>()
                .Where(x => x.DeviceName == conveyorDevice.Name
                        && (x.Status == EquipmentActionStatus.New
                        || x.Status == EquipmentActionStatus.Executing
                        || x.Status == EquipmentActionStatus.Error
                        || x.Status == EquipmentActionStatus.Suspend))
                    .ToList()
                    .Where(x => x.AtPlcDBIndex.GetValueOrDefault(0) > 0)
                    .Select(x => x.AtPlcDBIndex.GetValueOrDefault(0))
                    .ToList();

                newUnitOfWork.Commit();

                usedIndexs = usedIndexs.Concat(indexs).ToList();
            }

            return usedIndexs.ToArray();
        }
    }
}
