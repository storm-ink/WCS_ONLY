using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC.WebAPI.Entity
{
    public class PackingKeyFosbDataTaskHistory
    {
        /// <summary>
        /// 打包线中间数据流转记录 纸箱，FOSB，栈板 主键是位置编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// WMS校验的FOSB
        /// </summary>
        public string FosbId { get; set; }
        /// <summary>
        /// WMS给出的BoxId
        /// </summary>
        public string BoxId { get; set; }
        /// <summary>
        /// fosb扫码7003的扫码集合：顶面+侧面合集
        /// </summary>
        public string FosbScanCode { get; set; }
        /// <summary>
        /// 贴标7004贴标数据，贴标后扫码校验数据
        /// </summary>
        public string TopLabelingData { get; set; }

        /// <summary>
        /// 有效期  0-新料上线有效； 1-申请归档；2-归档成功删除留存失效；3-异常申请归档数据；4-异常归档成功删除失效
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 栈板Id /HyboxId
        /// </summary>
        public string PalletId { get; set; }

        /// <summary>
        /// box侧面扫码数据-贴标位置 人工检查8012
        /// </summary>
        public string BoxLabelingDataSide { get; set; }

        /// <summary>
        /// box正面扫码数据贴标位置 人工检查8012
        /// </summary>
        public string BoxLabelingDataFront { get; set; }
        /// <summary>
        /// 在PLC中携带主要key,关联WmsTaskId 后8位数字：B来料；C料盒；P/H 是整跺
        /// </summary>
        public string FosbKeyId { get; set; }
        /// <summary>
        /// 返库扫码FOSB 扫码集合
        /// </summary>
        public string FosbScanCodeReturn { get; set; }

        /// <summary>
        /// 返库扫码料盒扫码数据
        /// </summary>
        public string BoxIdScanCodeReturn { get; set; }

        /// <summary>
        /// 数据业务类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 配方数据留存
        /// </summary>
        public string RecipeJsonData { get; set; }

        /// <summary>
        /// 来料 FOSB 在哪面
        /// </summary>
        public string IfSide { get; set; }

        /// <summary>
        /// 任务触发上线的WMS 数据留存
        /// </summary>
        public string FosbPackingWMSTaskJSon { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// WMS上料的任务ID
        /// </summary>
        public string TaskCode { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpTateTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string FinishTime { get; set; }


        /// <summary>
        /// Hybox专属状态：0-已采集，等待绑定；1-已经绑定；2-绑定完成
        /// </summary>
        public string HyboxCodeState { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Message { get; set; }
    }
}
