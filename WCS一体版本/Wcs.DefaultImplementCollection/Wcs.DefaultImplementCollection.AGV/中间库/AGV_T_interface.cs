using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wcs.DefaultImplementCollection.AGV
{
    public class AGV_T_interface
    {
        public AGV_T_interface()
        { }
        private int? _id;
        private string _intercode;
        private string _beginloc;
        private int _beginLevel;
        private string _endloc;
        private int _endLevel;
        private string _intertype;
        private string _state;
        private DateTime _createdatetime;
        private DateTime? _usedatetime;
        private DateTime? _finishdatetime;
        private string _remark;
        private int? _grade;
        private string _category;
        private string _sort;
        private int? _agvId;
        private string _salverType;
        private string _TrayCode;

        //private string _Bintercode;
        /// <summary>
        /// 
        /// </summary>
        public int? ID
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? agvId
        {
            set { _agvId = value; }
            get { return _agvId; }
        }
        /// <summary>
        /// 指令号
        /// </summary>
        public string interCode
        {
            set { _intercode = value; }
            get { return _intercode; }
        }
        /// <summary>
        /// 任务类别(3充电)
        /// </summary>
        public string sort
        {
            set { _sort = value; }
            get { return _sort; }
        }
        /// <summary>
        /// 种类(1自动;2手动)
        /// </summary>
        public string category
        {
            set { _category = value; }
            get { return _category; }
        }
        /// <summary>
        /// 开始位置
        /// </summary>
        public string begionLoc
        {
            set { _beginloc = value; }
            get { return _beginloc; }
        }
        /// <summary>
        /// 开始位置层
        /// </summary>
        public int begionLevel
        {
            set { _beginLevel = value; }
            get { return _beginLevel; }
        }
        /// <summary>
        /// 结束位置
        /// </summary>
        public string endLoc
        {
            set { _endloc = value; }
            get { return _endloc; }
        }
        /// <summary>
        /// 结束位置层
        /// </summary>
        public int endLevel
        {
            set { _endLevel = value; }
            get { return _endLevel; }
        }
        /// <summary>
        /// 类型(1空托接料2满托回位3取货放货4空托回库5配料出库6配料回库7上料出库8上料回库9空托入库10小车行走)
        /// </summary>
        public string interType
        {
            set { _intertype = value; }
            get { return _intertype; }
        }
        /// <summary>
        /// 状态(1新增;2执行中;3已完成;4错误;5待执行)
        /// </summary>
        public string state
        {
            set { _state = value; }
            get { return _state; }
        }
        /// <summary>
        /// 优先级(1-10,10最高:5MES,8AGV)
        /// </summary>
        public int? grade
        {
            set { _grade = value; }
            get { return _grade; }
        }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createDatetime
        {
            set { _createdatetime = value; }
            get { return _createdatetime; }
        }
        /// <summary>
        /// 执行时间
        /// </summary>
        public DateTime? useDatetime
        {
            set { _usedatetime = value; }
            get { return _usedatetime; }
        }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? finishDatetime
        {
            set { _finishdatetime = value; }
            get { return _finishdatetime; }
        }
        /// <summary>
        /// 托盘类型
        /// </summary>
        public string SalverType
        {
            set { _salverType = value; }
            get { return _salverType; }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark
        {
            set { _remark = value; }
            get { return _remark; }
        }
        /// <summary>
        /// 托盘号
        /// </summary>
        public string TrayCode
        {
            set { _TrayCode = value; }
            get { return _TrayCode; }
        }

        /// <summary>
        /// 虚拟任务号
        /// </summary>
        //public string BinterCode
        //{
        //    set { _Bintercode = value; }
        //    get { return _Bintercode; }
        //}

    }
}
