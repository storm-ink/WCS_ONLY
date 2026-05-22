using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wcs.Framework.EquipmentActions;
using Wcs.Framework.Devices;

namespace Wcs.Framework.LogicMovements
{
    /// <summary>
    /// 表示堆垛机的一个半自动的取货放货任务
    /// 1、先运行到 A 点
    /// 2、在 A 点取货
    /// 3、运行到 B 点
    /// 4、在 B 点卸货
    /// </summary>
    public class CraneHalfAutomaticTransferMovement:LogicMovement
    {
        protected CraneHalfAutomaticTransferMovement()
            : base()
        {
        }
        public CraneHalfAutomaticTransferMovement(Devices.Device device, Int32? routeId, Location startLocation, Location endLocation)
            : base(device,routeId, startLocation,endLocation)
        {
        }

        protected override void InitializeEquipmentActions()
        {
            EquipmentActionGroup group = new EquipmentActionGroup();
            EquipmentAction action1 = new CraneMoveToAction(this.DeviceInfo.GetDevice(), 0, 0, this.StartLocation.GetLocation <RackLocation>());
            action1.Ordering = 0;
            EquipmentAction action2 = new CraneLoadAction(this.DeviceInfo.GetDevice(), 0, 0, CraneControl.EForkLR.L);
            action2.Ordering = 1;
            EquipmentAction action3 = new CraneMoveToAction(this.DeviceInfo.GetDevice(), 0, 0, this.EndLocation.GetLocation<RackLocation>());
            action1.Ordering = 2;
            EquipmentAction action4 = new CraneLoadAction(this.DeviceInfo.GetDevice(), 0, 0, CraneControl.EForkLR.L);
            action1.Ordering = 3;

            group.AcceptAction(action1);
            group.AcceptAction(action2);
            group.AcceptAction(action3);
            group.AcceptAction(action4);

            this.EquipmentActions = new Iesi.Collections.Generic.HashedSet<EquipmentAction> 
            { 
                action1,
                action2,
                action3,
                action4
            };
        }
    }
}
