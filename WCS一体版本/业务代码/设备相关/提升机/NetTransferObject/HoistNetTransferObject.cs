using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Wcs;
using Wcs.Framework;

namespace BOE
{
    [DisplayName("提升机状态")]
    public class HoistNetTransferObject:NetTransferObject
    {
        #region 属性
        /// <summary>
        /// 诊断位
        /// </summary>
        [DisplayName("DP Coupler诊断位")]
        public Byte Diagnosis { get; set; }
     
        /// <summary>
        /// 工作模式
        /// </summary>
        [DisplayName("工作模式")]
        public HoistCurrentMode CurrentMode { get; set; }

        /// <summary>
        /// 行走状态
        /// </summary>
        [DisplayName("行走状态")]
        public HoistWalkingStatus WalkingStatus { get; set; }

        /// <summary>
        /// 链一状态
        /// </summary>
        [DisplayName("链一状态")]
        public HoistChainStatus ChainOneStatus { get; set; }

        /// <summary>
        /// 链二状态
        /// </summary>
        [DisplayName("链二状态")]
        public HoistChainStatus ChainTwoStatus { get; set; }

        /// <summary>
        /// 物料状态
        /// </summary>
        [DisplayName("物料状态")]
        public HoistGoodsStatus GoodsStatus { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        [DisplayName("任务状态")]
        public HoistTaskStatus TaskStatus { get; set; }

        /// <summary>
        /// 当前站号
        /// </summary>
        [DisplayName("当前站号")]
        public UInt16 CurrentStationNO { get; set; }

        /// <summary>
        /// 当前位置
        /// </summary>
        [DisplayName("当前位置条码")]
        public UInt32 CurrentPosition { get; set; }

        /// <summary>
        /// 链一左在站点
        /// </summary>
        [DisplayName("链一左在站点")]
        public Boolean SCToConv_ChOneLOnPosition { get; set; }

        /// <summary>
        /// 链一左准备卸货
        /// </summary>
        [DisplayName("链一左准备卸货")]
        public Boolean SCToConv_ChOneLReadyDownLoad { get; set; }

        /// <summary>
        /// 链一左卸货中
        /// </summary>
        [DisplayName("链一左卸货中")]
        public Boolean SCToConv_ChOneLDownLoading { get; set; }

        /// <summary>
        /// 链一左卸货完成
        /// </summary>
        [DisplayName("链一左卸货完成")]
        public Boolean SCToConv_ChOneLDownLoadFinish { get; set; }

        /// <summary>
        /// 链一左备用
        /// </summary>
        [DisplayName("链一左备用")]
        public Boolean SCToConv_ChOneLSpare { get; set; }

        /// <summary>
        /// 链一左准备接货
        /// </summary>
        [DisplayName("链一左准备接货")]
        public Boolean SCToConv_ChOneLReadyUpLoad { get; set; }

        /// <summary>
        /// 链一左接货中
        /// </summary>
        [DisplayName("链一左接货中")]
        public Boolean SCToConv_ChOneLUpLoading { get; set; }

        /// <summary>
        /// 链一左接货完成
        /// </summary>
        [DisplayName("链一左接货完成")]
        public Boolean SCToConv_ChOneLUpLoadingFinish { get; set; }

        /// <summary>
        /// 链一右在站点
        /// </summary>
        [DisplayName("链一右在站点")]
        public Boolean SCToConv_ChOneROnPosition { get; set; }

        /// <summary>
        /// 链一右准备卸货
        /// </summary>
        [DisplayName("链一右准备卸货")]
        public Boolean SCToConv_ChOneRReadyDownLoad { get; set; }

        /// <summary>
        /// 链一右卸货中
        /// </summary>
        [DisplayName("链一右卸货中")]
        public Boolean SCToConv_ChOneRDownLoading { get; set; }

        /// <summary>
        /// 链一右卸货完成
        /// </summary>
        [DisplayName("链一右卸货完成")]
        public Boolean SCToConv_ChOneRDownLoadFinish { get; set; }

        /// <summary>
        /// 链一右备用
        /// </summary>
        [DisplayName("链一右备用")]
        public Boolean SCToConv_ChOneRSpare { get; set; }

        /// <summary>
        /// 链一右准备接货
        /// </summary>
        [DisplayName("链一右准备接货")]
        public Boolean SCToConv_ChOneRReadyUpLoad { get; set; }

        /// <summary>
        /// 链一右接货中
        /// </summary>
        [DisplayName("链一右接货中")]
        public Boolean SCToConv_ChOneRUpLoading { get; set; }

        /// <summary>
        /// 链一右接货完成
        /// </summary>
        [DisplayName("链一右接货完成")]
        public Boolean SCToConv_ChOneRUpLoadingFinish { get; set; }

        /// <summary>
        /// 链二左在站点
        /// </summary>
        [DisplayName("链二左在站点")]
        public Boolean SCToConv_ChTwoLOnPosition { get; set; }

        /// <summary>
        /// 链二左准备卸货
        /// </summary>
        [DisplayName("链二左准备卸货")]
        public Boolean SCToConv_ChTwoLReadyDownLoad { get; set; }

        /// <summary>
        /// 链二左卸货中
        /// </summary>
        [DisplayName("链二左卸货中")]
        public Boolean SCToConv_ChTwoLDownLoading { get; set; }

        /// <summary>
        /// 链二左卸货完成
        /// </summary>
        [DisplayName("链二左卸货完成")]
        public Boolean SCToConv_ChTwoLDownLoadFinish { get; set; }

        /// <summary>
        /// 链二左备用
        /// </summary>
        [DisplayName("链二左备用")]
        public Boolean SCToConv_ChTwoLSpare { get; set; }

        /// <summary>
        /// 链二左准备接货
        /// </summary>
        [DisplayName("链二左准备接货")]
        public Boolean SCToConv_ChTwoLReadyUpLoad { get; set; }

        /// <summary>
        /// 链二左接货中
        /// </summary>
        [DisplayName("链二左接货中")]
        public Boolean SCToConv_ChTwoLUpLoading { get; set; }

        /// <summary>
        /// 链二左接货完成
        /// </summary>
        [DisplayName("链二左接货完成")]
        public Boolean SCToConv_ChTwoLUpLoadingFinish { get; set; }

        /// <summary>
        /// 链二右在站点
        /// </summary>
        [DisplayName("链二右在站点")]
        public Boolean SCToConv_ChTwoROnPosition { get; set; }

        /// <summary>
        /// 链二右准备卸货
        /// </summary>
        [DisplayName("链二右准备卸货")]
        public Boolean SCToConv_ChTwoRReadyDownLoad { get; set; }

        /// <summary>
        /// 链二右卸货中
        /// </summary>
        [DisplayName("链二右卸货中")]
        public Boolean SCToConv_ChTwoRDownLoading { get; set; }

        /// <summary>
        /// 链二右卸货完成
        /// </summary>
        [DisplayName("链二右卸货完成")]
        public Boolean SCToConv_ChTwoRDownLoadFinish { get; set; }

        /// <summary>
        /// 链二右备用
        /// </summary>
        [DisplayName("链二右备用")]
        public Boolean SCToConv_ChTwoRSpare { get; set; }

        /// <summary>
        /// 链二右准备接货
        /// </summary>
        [DisplayName("链二右准备接货")]
        public Boolean SCToConv_ChTwoRReadyUpLoad { get; set; }

        /// <summary>
        /// 链二右接货中
        /// </summary>
        [DisplayName("链二右接货中")]
        public Boolean SCToConv_ChTwoRUpLoading { get; set; }

        /// <summary>
        /// 链二右接货完成
        /// </summary>
        [DisplayName("链二右接货完成")]
        public Boolean SCToConv_ChTwoRUpLoadingFinish { get; set; }

        /// <summary>
        /// 链一左超限
        /// </summary>
        [DisplayName("链一左超限")]
        public Boolean Err_ChainOneLeftOverhang { get; set; }

        /// <summary>
        /// 链一右超限
        /// </summary>
        [DisplayName("链一右超限")]
        public Boolean Err_ChainOneRightOverhang { get; set; }

        /// <summary>
        /// 链二左超限
        /// </summary>
        [DisplayName("链二左超限")]
        public Boolean Err_ChainTwoLeftOverhang { get; set; }

        /// <summary>
        /// 链二右超限
        /// </summary>
        [DisplayName("链二右超限")]
        public Boolean Err_ChainTwoRightOverhang { get; set; }

        /// <summary>
        /// 行走超时
        /// </summary>
        [DisplayName("提升运行超时")]
        public Boolean Err_WalkingOvertime { get; set; }

        /// <summary>
        /// 链一输送运行超时
        /// </summary>
        [DisplayName("链一输送运行超时")]
        public Boolean Err_ChainOneConvOvertime { get; set; }

        /// <summary>
        /// 链二运行超时
        /// </summary>
        [DisplayName("链二运行超时")]
        public Boolean Err_ChainTwoConvOverTime { get; set; }

        /// <summary>
        /// 上极限
        /// </summary>
        [DisplayName("上极限")]
        public Boolean Err_UpLimit { get; set; }

        /// <summary>
        /// 下极限
        /// </summary>
        [DisplayName("下极限")]
        public Boolean Err_DownLimit { get; set; }

        /// <summary>
        /// 急停
        /// </summary>
        [DisplayName("急停")]
        public Boolean Err_EmergencyStop { get; set; }

        /// <summary>
        /// 提升变频器故障
        /// </summary>
        [DisplayName("提升变频器故障")]
        public Boolean Err_LiftInverterErr { get; set; }

        /// <summary>
        /// 链一输送变频器故障
        /// </summary>
        [DisplayName("链一输送变频器故障")]
        public Boolean Err_ConvInverterOneErr { get; set; }

        /// <summary>
        /// 链二输送变频器故障
        /// </summary>
        [DisplayName("链二输送变频器故障")]
        public Boolean Err_InverterTwoErr { get; set; }

        /// <summary>
        /// 备用1,提升机未启用
        /// </summary>
        [DisplayName("备用1")]
        public Boolean Err_Spare1 { get; set; }

        //<summary>
        //条码扫描/编码器故障
        //</summary>
        [DisplayName("条码扫描/编码器读零故障")]
        public Boolean Err_LRFErr { get; set; }

        /// <summary>
        /// 与输送线接口超时故障
        /// </summary>
        [DisplayName("与输送线接口超时故障")]
        public Boolean Err_TimeoutExceptionConveyor { get; set; }

        /// <summary>
        /// 接收站点超限
        /// </summary>
        [DisplayName("接收站点超限")]
        public Boolean Err_ReceivingSitesOverrun { get; set; }

        /// <summary>
        /// 提升超出最大站
        /// </summary>
        [DisplayName("提升超出最大站")]
        public Boolean Err_LiftOverMaxStation { get; set; }

        /// <summary>
        /// 行走超出最小站
        /// </summary>
        [DisplayName("提升超出最小站")]
        public Boolean Err_LiftOverMinStation { get; set; }

        /// <summary>
        /// 接收任务格式错误
        /// </summary>
        [DisplayName("接收任务格式错误")]
        public Boolean Err_ReceiveTaskFormatError { get; set; }

        /// <summary>
        /// 提升电机断路器断开
        /// </summary>
        [DisplayName("提升电机断路器断开")]
        public Boolean Err_LiftmotorBreakerDisconne { get; set; }

        /// <summary>
        /// 链一输送电机断路器断开
        /// </summary>
        [DisplayName("链一输送电机断路器断开")]
        public Boolean Err_ChainOneConvmotorBreaker { get; set; }

        /// <summary>
        /// 链二输送电机断路器断开
        /// </summary>
        [DisplayName("链二输送电机断路器断开")]
        public Boolean Err_ChainTwoConvmotorBreaker { get; set; }

        /// <summary>
        /// 主接触器断开
        /// </summary>
        [DisplayName("主接触器断开")]
        public Boolean Err_MainContactorDisconnect { get; set; }

        /// <summary>
        /// 提升变频器子站丢失故障
        /// </summary>
        [DisplayName("提升变频器子站丢失故障")]
        public Boolean Err_ErrSpare6 { get; set; }

        /// <summary>
        /// 激光测距/条码扫描子站丢失故障
        /// </summary>
        [DisplayName("激光测距/条码扫描子站丢失故障")]
        public Boolean Err_ErrSpare7 { get; set; }

        /// <summary>
        /// 与输送机通信故障
        /// </summary>
        [DisplayName("与输送机通信故障")]
        public Boolean Err_CommunicateErr { get; set; }

        /// <summary>
        /// 备用9
        /// </summary>
        [DisplayName("备用9")]
        public Boolean Err_ErrSpare9 { get; set; }

        /// <summary>
        /// 备用10
        /// </summary>
        [DisplayName("备用10")]
        public Boolean Err_ErrSpare10 { get; set; }

        /// <summary>
        /// 备用11
        /// </summary>
        [DisplayName("备用11")]
        public Boolean Err_ErrSpare11 { get; set; }

        /// <summary>
        /// 备用12
        /// </summary>
        [DisplayName("备用12")]
        public Boolean Err_ErrSpare12 { get; set; }

        /// <summary>
        /// 备用13
        /// </summary>
        [DisplayName("备用13")]
        public Boolean Err_ErrSpare13 { get; set; }
        #endregion

        public override object this[string name]
        {
            get
            {
                switch (name)
                {
                    case "Diagnosis":
                        return this.Diagnosis;
                    case "CurrentMode":
                        return this.CurrentMode;
                    case "WalkingStatus":
                        return this.WalkingStatus;
                    case "ChainOneStatus":
                        return this.ChainOneStatus;
                    case "ChainTwoStatus":
                        return this.ChainTwoStatus;
                    case "GoodsStatus":
                        return this.GoodsStatus;
                    case "TaskStatus":
                        return this.TaskStatus;
                    case "CurrentStationNO":
                        return this.CurrentStationNO;
                    case "CurrentPosition":
                        return this.CurrentPosition;
                    case "SCToConv_ChOneLOnPosition":
                        return this.SCToConv_ChOneLOnPosition;
                    case "SCToConv_ChOneLReadyDownLoad":
                        return this.SCToConv_ChOneLReadyDownLoad;
                    case "SCToConv_ChOneLDownLoading":
                        return this.SCToConv_ChOneLDownLoading;
                    case "SCToConv_ChOneLDownLoadFinish":
                        return this.SCToConv_ChOneLDownLoadFinish;
                    case "SCToConv_ChOneLSpare":
                        return this.SCToConv_ChOneLSpare;
                    case "SCToConv_ChOneLReadyUpLoad":
                        return this.SCToConv_ChOneLReadyUpLoad;
                    case "SCToConv_ChOneLUpLoading":
                        return this.SCToConv_ChOneLUpLoading;
                    case "SCToConv_ChOneLUpLoadingFinish":
                        return this.SCToConv_ChOneLUpLoadingFinish;
                    case "SCToConv_ChOneROnPosition":
                        return this.SCToConv_ChOneROnPosition;
                    case "SCToConv_ChOneRReadyDownLoad":
                        return this.SCToConv_ChOneRReadyDownLoad;
                    case "SCToConv_ChOneRDownLoading":
                        return this.SCToConv_ChOneRDownLoading;
                    case "SCToConv_ChOneRDownLoadFinish":
                        return this.SCToConv_ChOneRDownLoadFinish;
                    case "SCToConv_ChOneRSpare":
                        return this.SCToConv_ChOneRSpare;
                    case "SCToConv_ChOneRReadyUpLoad":
                        return this.SCToConv_ChOneRReadyUpLoad;
                    case "SCToConv_ChOneRUpLoading":
                        return this.SCToConv_ChOneRUpLoading;
                    case "SCToConv_ChOneRUpLoadingFinish":
                        return this.SCToConv_ChOneRUpLoadingFinish;
                    case "SCToConv_ChTwoLOnPosition":
                        return this.SCToConv_ChTwoLOnPosition;
                    case "SCToConv_ChTwoLReadyDownLoad":
                        return this.SCToConv_ChTwoLReadyDownLoad;
                    case "SCToConv_ChTwoLDownLoading":
                        return this.SCToConv_ChTwoLDownLoading;
                    case "SCToConv_ChTwoLDownLoadFinish":
                        return this.SCToConv_ChTwoLDownLoadFinish;
                    case "SCToConv_ChTwoLSpare":
                        return this.SCToConv_ChTwoLSpare;
                    case "SCToConv_ChTwoLReadyUpLoad":
                        return this.SCToConv_ChTwoLReadyUpLoad;
                    case "SCToConv_ChTwoLUpLoading":
                        return this.SCToConv_ChTwoLUpLoading;
                    case "SCToConv_ChTwoLUpLoadingFinish":
                        return this.SCToConv_ChTwoLUpLoadingFinish;
                    case "SCToConv_ChTwoROnPosition":
                        return this.SCToConv_ChTwoROnPosition;
                    case "SCToConv_ChTwoRReadyDownLoad":
                        return this.SCToConv_ChTwoRReadyDownLoad;
                    case "SCToConv_ChTwoRDownLoading":
                        return this.SCToConv_ChTwoRDownLoading;
                    case "SCToConv_ChTwoRDownLoadFinish":
                        return this.SCToConv_ChTwoRDownLoadFinish;
                    case "SCToConv_ChTwoRSpare":
                        return this.SCToConv_ChTwoRSpare;
                    case "SCToConv_ChTwoRReadyUpLoad":
                        return this.SCToConv_ChTwoRReadyUpLoad;
                    case "SCToConv_ChTwoRUpLoading":
                        return this.SCToConv_ChTwoRUpLoading;
                    case "SCToConv_ChTwoRUpLoadingFinish":
                        return this.SCToConv_ChTwoRUpLoadingFinish;
                    case "Err_ChainOneLeftOverhang":
                        return this.Err_ChainOneLeftOverhang;
                    case "Err_ChainOneRightOverhang":
                        return this.Err_ChainOneRightOverhang;
                    case "Err_ChainTwoLeftOverhang":
                        return this.Err_ChainTwoLeftOverhang;
                    case "Err_ChainTwoRightOverhang":
                        return this.Err_ChainTwoRightOverhang;
                    case "Err_WalkingOvertime":
                        return this.Err_WalkingOvertime;
                    case "Err_ChainOneConvOvertime":
                        return this.Err_ChainOneConvOvertime;
                    case "Err_ChainTwoConvOverTime":
                        return this.Err_ChainTwoConvOverTime;
                    case "Err_UpLimit":
                        return this.Err_UpLimit;
                    case "Err_DownLimit":
                        return this.Err_DownLimit;
                    case "Err_EmergencyStop":
                        return this.Err_EmergencyStop;
                    case "Err_LiftInverterErr":
                        return this.Err_LiftInverterErr;
                    case "Err_ConvInverterOneErr":
                        return this.Err_ConvInverterOneErr;
                    case "Err_InverterTwoErr":
                        return this.Err_InverterTwoErr;
                    case "Err_Spare1":
                        return this.Err_Spare1;
                    case "Err_LRFErr":
                        return this.Err_LRFErr;
                    case "Err_TimeoutExceptionConveyor":
                        return this.Err_TimeoutExceptionConveyor;
                    case "Err_ReceivingSitesOverrun":
                        return this.Err_ReceivingSitesOverrun;
                    case "Err_LiftOverMaxStation":
                        return this.Err_LiftOverMaxStation;
                    case "Err_LiftOverMinStation":
                        return this.Err_LiftOverMinStation;
                    case "Err_ReceiveTaskFormatError":
                        return this.Err_ReceiveTaskFormatError;
                    case "Err_LiftmotorBreakerDisconne":
                        return this.Err_LiftmotorBreakerDisconne;
                    case "Err_ChainOneConvmotorBreaker":
                        return this.Err_ChainOneConvmotorBreaker;
                    case "Err_ChainTwoConvmotorBreaker":
                        return this.Err_ChainTwoConvmotorBreaker;
                    case "Err_MainContactorDisconnect":
                        return this.Err_MainContactorDisconnect;
                    case "Err_ErrSpare6":
                        return this.Err_ErrSpare6;
                    case "Err_ErrSpare7":
                        return this.Err_ErrSpare7;
                    case "Err_CommunicateErr":
                        return this.Err_CommunicateErr;
                    case "Err_ErrSpare9":
                        return this.Err_ErrSpare9;
                    case "Err_ErrSpare10":
                        return this.Err_ErrSpare10;
                    case "Err_ErrSpare11":
                        return this.Err_ErrSpare11;
                    case "Err_ErrSpare12":
                        return this.Err_ErrSpare12;
                    case "Err_ErrSpare13":
                        return this.Err_ErrSpare13;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的取值操作", this.GetType(), name));

                }
            }
            set
            {
                switch (name)
                {
                    case "Diagnosis":
                        this.Diagnosis = Convert.ToByte(value);
                        break;
                    case "CurrentMode":
                        this.CurrentMode = (HoistCurrentMode)Convert.ToByte(value);
                        break;
                    case "WalkingStatus":
                        this.WalkingStatus = (HoistWalkingStatus)Convert.ToByte(value);
                        break;
                    case "ChainOneStatus":
                        this.ChainOneStatus = (HoistChainStatus)Convert.ToByte(value);
                        break;
                    case "ChainTwoStatus":
                        this.ChainTwoStatus = (HoistChainStatus)Convert.ToByte(value);
                        break;
                    case "GoodsStatus":
                        this.GoodsStatus =(HoistGoodsStatus)Convert.ToByte(value);
                        break;
                    case "TaskStatus":
                        this.TaskStatus = (HoistTaskStatus)Convert.ToByte(value);
                        break;
                    case "CurrentStationNO":
                        this.CurrentStationNO = Convert.ToUInt16(value);
                        break;
                    case "CurrentPosition":
                        this.CurrentPosition = Convert.ToUInt32(value);
                        break;
                    case "SCToConv_ChOneLOnPosition":
                        this.SCToConv_ChOneLOnPosition = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLReadyDownLoad":
                        this.SCToConv_ChOneLReadyDownLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLDownLoading":
                        this.SCToConv_ChOneLDownLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLDownLoadFinish":
                        this.SCToConv_ChOneLDownLoadFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLSpare":
                        this.SCToConv_ChOneLSpare = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLReadyUpLoad":
                        this.SCToConv_ChOneLReadyUpLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLUpLoading":
                        this.SCToConv_ChOneLUpLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneLUpLoadingFinish":
                        this.SCToConv_ChOneLUpLoadingFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneROnPosition":
                        this.SCToConv_ChOneROnPosition = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRReadyDownLoad":
                        this.SCToConv_ChOneRReadyDownLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRDownLoading":
                        this.SCToConv_ChOneRDownLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRDownLoadFinish":
                        this.SCToConv_ChOneRDownLoadFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRSpare":
                        this.SCToConv_ChOneRSpare = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRReadyUpLoad":
                        this.SCToConv_ChOneRReadyUpLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRUpLoading":
                        this.SCToConv_ChOneRUpLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChOneRUpLoadingFinish":
                        this.SCToConv_ChOneRUpLoadingFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLOnPosition":
                        this.SCToConv_ChTwoLOnPosition = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLReadyDownLoad":
                        this.SCToConv_ChTwoLReadyDownLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLDownLoading":
                        this.SCToConv_ChTwoLDownLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLDownLoadFinish":
                        this.SCToConv_ChTwoLDownLoadFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLSpare":
                        this.SCToConv_ChTwoLSpare = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLReadyUpLoad":
                        this.SCToConv_ChTwoLReadyUpLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLUpLoading":
                        this.SCToConv_ChTwoLUpLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoLUpLoadingFinish":
                        this.SCToConv_ChTwoLUpLoadingFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoROnPosition":
                        this.SCToConv_ChTwoROnPosition = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRReadyDownLoad":
                        this.SCToConv_ChTwoRReadyDownLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRDownLoading":
                        this.SCToConv_ChTwoRDownLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRDownLoadFinish":
                        this.SCToConv_ChTwoRDownLoadFinish = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRSpare":
                        this.SCToConv_ChTwoRSpare = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRReadyUpLoad":
                        this.SCToConv_ChTwoRReadyUpLoad = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRUpLoading":
                        this.SCToConv_ChTwoRUpLoading = Convert.ToBoolean(value);
                        break;
                    case "SCToConv_ChTwoRUpLoadingFinish":
                        this.SCToConv_ChTwoRUpLoadingFinish = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainOneLeftOverhang":
                        this.Err_ChainOneLeftOverhang = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainOneRightOverhang":
                        this.Err_ChainOneRightOverhang = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainTwoLeftOverhang":
                        this.Err_ChainTwoLeftOverhang = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainTwoRightOverhang":
                        this.Err_ChainTwoRightOverhang = Convert.ToBoolean(value);
                        break;
                    case "Err_WalkingOvertime":
                        this.Err_WalkingOvertime = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainOneConvOvertime":
                        this.Err_ChainOneConvOvertime = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainTwoConvOverTime":
                        this.Err_ChainTwoConvOverTime = Convert.ToBoolean(value);
                        break;
                    case "Err_UpLimit":
                        this.Err_UpLimit = Convert.ToBoolean(value);
                        break;
                    case "Err_DownLimit":
                        this.Err_DownLimit = Convert.ToBoolean(value);
                        break;
                    case "Err_EmergencyStop":
                        this.Err_EmergencyStop = Convert.ToBoolean(value);
                        break;
                    case "Err_LiftInverterErr":
                        this.Err_LiftInverterErr = Convert.ToBoolean(value);
                        break;
                    case "Err_ConvInverterOneErr":
                        this.Err_ConvInverterOneErr = Convert.ToBoolean(value);
                        break;
                    case "Err_InverterTwoErr":
                        this.Err_InverterTwoErr = Convert.ToBoolean(value);
                        break;
                    case "Err_Spare1":
                        this.Err_Spare1 = Convert.ToBoolean(value);
                        break;
                    case "Err_LRFErr":
                        this.Err_LRFErr = Convert.ToBoolean(value);
                        break;
                    case "Err_TimeoutExceptionConveyor":
                        this.Err_TimeoutExceptionConveyor = Convert.ToBoolean(value);
                        break;
                    case "Err_ReceivingSitesOverrun":
                        this.Err_ReceivingSitesOverrun = Convert.ToBoolean(value);
                        break;
                    case "Err_LiftOverMaxStation":
                        this.Err_LiftOverMaxStation = Convert.ToBoolean(value);
                        break;
                    case "Err_LiftOverMinStation":
                        this.Err_LiftOverMinStation = Convert.ToBoolean(value);
                        break;
                    case "Err_ReceiveTaskFormatError":
                        this.Err_ReceiveTaskFormatError = Convert.ToBoolean(value);
                        break;
                    case "Err_LiftmotorBreakerDisconne":
                        this.Err_LiftmotorBreakerDisconne = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainOneConvmotorBreaker":
                        this.Err_ChainOneConvmotorBreaker = Convert.ToBoolean(value);
                        break;
                    case "Err_ChainTwoConvmotorBreaker":
                        this.Err_ChainTwoConvmotorBreaker = Convert.ToBoolean(value);
                        break;
                    case "Err_MainContactorDisconnect":
                        this.Err_MainContactorDisconnect = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare6":
                        this.Err_ErrSpare6 = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare7":
                        this.Err_ErrSpare7 = Convert.ToBoolean(value);
                        break;
                    case "Err_CommunicateErr":
                        this.Err_CommunicateErr = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare9":
                        this.Err_ErrSpare9 = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare10":
                        this.Err_ErrSpare10 = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare11":
                        this.Err_ErrSpare11 = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare12":
                        this.Err_ErrSpare12 = Convert.ToBoolean(value);
                        break;
                    case "Err_ErrSpare13":
                        this.Err_ErrSpare13 = Convert.ToBoolean(value);
                        break;
                    default:
                        throw new NotImplementedException(String.Format("{0} 未实现对属性 {1} 的赋值操作", this.GetType(), name));
                }
            }
        }

        //运行数据不做记录
        public override Wcs.Framework.ReceivedDataLog ToLogData(Wcs.Framework.Device device)
        {
            return null;
        }

        /// <summary>
        /// 获取报警数据
        /// </summary>
        /// <returns></returns>
        internal String[] GetWarings()
        {
            List<String> result = new List<string>();

            if (this.Err_ChainOneLeftOverhang)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainOneLeftOverhang").GetDisplayName());
            if (this.Err_ChainOneRightOverhang)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainOneRightOverhang").GetDisplayName());
            if (this.Err_ChainTwoLeftOverhang)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainTwoLeftOverhang").GetDisplayName());
            if (this.Err_ChainTwoRightOverhang)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainTwoRightOverhang").GetDisplayName());
            if (this.Err_WalkingOvertime)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_WalkingOvertime").GetDisplayName());
            if (this.Err_ChainOneConvOvertime)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainOneConvOvertime").GetDisplayName());
            if (this.Err_ChainTwoConvOverTime)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainTwoConvOverTime").GetDisplayName());
            if (this.Err_UpLimit)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_UpLimit").GetDisplayName());
            if (this.Err_DownLimit)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_DownLimit").GetDisplayName());
            if (this.Err_EmergencyStop)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_EmergencyStop").GetDisplayName());
            if (this.Err_LiftInverterErr)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_LiftInverterErr").GetDisplayName());
            if (this.Err_ConvInverterOneErr)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ConvInverterOneErr").GetDisplayName());
            if (this.Err_InverterTwoErr)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_InverterTwoErr").GetDisplayName());
            if (this.Err_Spare1)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_Spare1").GetDisplayName());
            if (this.Err_LRFErr)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_LRFErr").GetDisplayName());
            if (this.Err_TimeoutExceptionConveyor)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_TimeoutExceptionConveyor").GetDisplayName());
            if (this.Err_ReceivingSitesOverrun)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ReceivingSitesOverrun").GetDisplayName());
            if (this.Err_LiftOverMaxStation)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_LiftOverMaxStation").GetDisplayName());
            if (this.Err_LiftOverMinStation)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_LiftOverMinStation").GetDisplayName());
            if (this.Err_ReceiveTaskFormatError)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ReceiveTaskFormatError").GetDisplayName());
            if (this.Err_LiftmotorBreakerDisconne)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_LiftmotorBreakerDisconne").GetDisplayName());
            if (this.Err_ChainOneConvmotorBreaker)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainOneConvmotorBreaker").GetDisplayName());
            if (this.Err_ChainTwoConvmotorBreaker)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ChainTwoConvmotorBreaker").GetDisplayName());
            if (this.Err_MainContactorDisconnect)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_MainContactorDisconnect").GetDisplayName());
            if (this.Err_ErrSpare6)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare6").GetDisplayName());
            if (this.Err_ErrSpare7)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare7").GetDisplayName());
            if (this.Err_CommunicateErr)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_CommunicateErr").GetDisplayName());
            if (this.Err_ErrSpare9)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare9").GetDisplayName());
            if (this.Err_ErrSpare10)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare10").GetDisplayName());
            if (this.Err_ErrSpare11)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare11").GetDisplayName());
            if (this.Err_ErrSpare12)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare12").GetDisplayName());
            if (this.Err_ErrSpare13)
                result.Add(typeof(HoistNetTransferObject).GetProperty("Err_ErrSpare13").GetDisplayName());

            return result.ToArray();
        }
    }
}
