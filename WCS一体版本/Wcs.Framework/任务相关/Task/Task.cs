using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wcs.Framework.Cfg;
using Wcs.Framework.Events;

namespace Wcs.Framework
{
    /// <summary>
    /// Wcs 主任务，描述一个完整的作业
    /// </summary>
    [JsonObject]
    public class Task : Comparable<Task>, IComparer<Task>
    {
        String m_group;
        public virtual Int32 Id { get; set; }
        /// <summary>
        /// 关联的主控任务号（父任务号）
        /// </summary>
        public virtual String MasterTaskCode { get; set; }
        /// <summary>
        /// 任务编号
        /// </summary>
        public virtual String TaskCode { get; set; }
        /// <summary>
        /// 该任务来自哪个请求(Wms在下任务时应将请求的主键传回给Wcs)
        /// </summary>
        public virtual Request FromRequest { get; set; }
        /// <summary>
        /// 任务来源
        /// </summary>
        public virtual TaskSource Source { get; set; }
        /// <summary>
        /// 任务起始位置
        /// </summary>
        public virtual LocationInfo StartLocation { get; set; }
        /// <summary>
        /// 终点位置
        /// </summary>
        public virtual LocationInfo EndLocation { get; set; }
        /// <summary>
        /// 当前位置
        /// </summary>
        public virtual LocationInfo CurrentLocation { get; set; }
        /// <summary>
        /// 任务方向
        /// </summary>
        public virtual TaskDirection Direction { get; set; }
        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual TaskStatus Status { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public virtual TaskBizType BizType { get; set; }
        /// <summary>
        /// 任务类型
        /// <para>此属性的值来源于业务系统，在与业务系统交互时可能需要用到。</para>
        /// </summary>
        public virtual String TaskType { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedAt { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public virtual DateTime? FinishedAt { get; set; }
        /// <summary>
        /// 备注信息
        /// </summary>
        public virtual String Description { get; set; }
        /// <summary>
        /// 任务优先级（默认为 0），值越大，优先级越高。
        /// </summary>
        public virtual Int32 Priority { get; set; }
        /// <summary>
        /// WCS任务优先级（默认为 0），值越大，优先级越高。
        /// 该优先级为WCS系统自动调整
        /// 防呆措施，防止任务长时间呆死在任务池
        /// </summary>
        public virtual Int32 WcsPriority { get; set; }
        /// <summary>
        /// 人工指定优先执行
        /// 该字段设计是在条件允许的情况下优先执行该条任务其余任务即使优先级高也必须延后执行
        /// </summary>
        public virtual Boolean ManualPriority { get; set; }
        /// <summary>
        /// 任务组(双货叉堆垛机中，同一个组的两个任务必须同时取放)。
        /// 获取或设置一个值，表示任务所属的组。
        /// 默认返回当前任务的任务号。
        /// </summary>
        public virtual String Group
        {
            get
            {
                if (String.IsNullOrWhiteSpace(m_group))
                {
                    return this.TaskCode;
                }
                else
                {
                    return m_group;
                }
            }
            set
            {
                m_group = value;
            }
        }
        /// <summary>
        /// 同一组内的任务数量（该值主要体现的下一步操作只配对任务数量，某些时候当前步骤中有多个，但到下一步骤只将会减少）
        /// </summary>
        public virtual Int32 PartnersCount { get; set; }
        /// <summary>
        /// 已存在的动作
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<LogicMovement> Movements { get; protected set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        public virtual Iesi.Collections.Generic.ISet<String> ContainerCodes { get; protected set; }
        /// <summary>
        /// 附加属性集合
        /// </summary>
        public virtual IDictionary<String, String> AdditionalInfo { get; set; }
        /// <summary>
        /// 即将行走路径集合
        /// </summary>
        public virtual IDictionary<Int32, Int32> TaskPredictRoutes { get; set; }

        protected static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        protected Task()
        {
            Movements = new Iesi.Collections.Generic.HashedSet<LogicMovement>();
            ContainerCodes = new Iesi.Collections.Generic.HashedSet<String>();
            AdditionalInfo = new Dictionary<String, String>();
            CreatedAt = DateTime.Now;
            Priority = 0;
            BizType = TaskBizType.Normal;
            Status = TaskStatus.New;
            PartnersCount = 1;
        }

        public Task(String taskCode, LocationInfo startLocation, LocationInfo endLocation)
            : this()
        {
            this.TaskCode = taskCode;
            this.Group = taskCode;
            this.StartLocation = startLocation;
            this.EndLocation = endLocation;
            this.CurrentLocation = startLocation;
        }

        /// <summary>
        /// 添加一个新的逻辑动作
        /// </summary>
        /// <param name="movement">新逻辑动作</param>
        public virtual void AddMovement(LogicMovement movement)
        {
            movement.Task = this;
            movement.Ordering = this.Movements.Count();
            this.Movements.Add(movement);

            _logger.Debug1(String.Format("为 {0} 拆分了一个从 {1} 到 {2} 的逻辑动作", this, movement.StartLocation, movement.EndLocation), this, movement);
        }

        /// <summary>
        /// 获取下一个物理动作
        /// </summary>
        /// <remarks>
        /// 在当前任务中尝试获取下一个待执行的 LogicMovement，如果不存在，将尝试使用FindNextPath 函数搜索连通网络并拆分形成一个新的 LogicMovement对象返回。否则返回 null.
        /// </remarks>
        /// <param name="unitOfWork">数据库持久化上下文</param>
        /// <param name="isNewMovement">指示返回的 LogicMovemenet 对象是否是新创建的</param>
        /// <returns>当前可用的待执行的 LogicMovement 对象</returns>
        public virtual LogicMovement GetNextMovement(NHUnitOfWork unitOfWork, out Boolean isNewMovement)
        {
            isNewMovement = false;
            LogicMovement logicMovement = null;
            if (Movements.Count() == 0)
            {
                var routes = PathHelper.FindNextPath(this, LocationConverter.ToLocation(this.StartLocation).ToTaskableLocation(),
                    LocationConverter.ToLocation(this.EndLocation).ToTaskableLocation(),
                    LocationConverter.ToLocation(this.CurrentLocation).ToTaskableLocation(), null,
                    this.BizType == TaskBizType.Counting ? RouteType.Counting : RouteType.Normal);
                if (routes.Count == 0)
                {
                    return null;
                }

                //如果分数为 0，不再下发
                //var firstRoute = routes.OrderBy(x => x.Value.Locations.Count()).First();
                var firstRoute = routes.OrderBy(x => x.Value.ReachedLocations.Count()).FirstOrDefault();
                var rate = firstRoute.Value.GetRate(firstRoute.Key, this, LocationConverter.ToLocation(this.StartLocation), LocationConverter.ToLocation(this.EndLocation));
                if (rate <= 0m)
                {
                    return null;
                }

                logicMovement = firstRoute.Value.CreateLogicMovement(this);
                AddMovement(logicMovement);

                isNewMovement = true;

                string msg = string.Format("共为 {0} 从 {1} 到 {2} 找如下路径:{3},选择当前使用的路径为 {4}",
                    this,
                    this.CurrentLocation,
                    this.EndLocation,
                    string.Join(",", routes.Select(x => x.Value.ToString()).ToArray()),
                    firstRoute.Value);

                _logger.Trace1(msg, this, this);

                return logicMovement;
            }
            else
            {
                //如果全是状态为 New 的 movement，就直接返回第一个
                if (this.Movements.All(x => x.Status == LogicMovementStatus.New))
                {
                    return this.Movements.OrderBy(x => x.Ordering).First();
                }

                //如果有运行的 movement 就直接返回
                var executingMovement = this.Movements.SingleOrDefault(x => x.Status == LogicMovementStatus.Executing);
                if (executingMovement != null)
                {
                    return executingMovement;
                }

                //如果有发生错误的 movement，返回 null，等待用户处理
                var errorMovement = this.Movements.SingleOrDefault(x => x.Status == LogicMovementStatus.Error);
                if (errorMovement != null)
                {
                    return null;
                }

                //如果有暂停的 movement，返回 null，等待用户处理
                var suspendMovement = this.Movements.SingleOrDefault(x => x.Status == LogicMovementStatus.Suspend);
                if (suspendMovement != null)
                {
                    return null;
                }

                //如果从后往前找，第一个是 Cancelled 或 Completed，就取下一个
                LogicMovement newMovement = null;
                foreach (var movement in this.Movements.OrderByDescending(x => x.Ordering))
                {
                    if (movement.Status != LogicMovementStatus.New)
                    {
                        if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                        {
                            //如果此任务的后面已经有生成的状态为 new 的 movement就返回，否则生成新的 movement
                            if (newMovement != null)
                            {
                                return newMovement;
                            }
                            else
                            {
                                var lastMovement = this.Movements.OrderBy(x => x.Ordering).LastOrDefault();
                                Int32? lastMovementRouteId = lastMovement == null ? null : lastMovement.RouteId;

                                //处理任务当前任务不正确，第二个任务无法下发的情况
                                if (lastMovement != null)
                                {
                                    if (!this.CurrentLocation.Equals(lastMovement.EndLocation))
                                    {
                                        _logger.Warn1(string.Format("在为 {0} 拆分任务时发现当前位置 {1} 并非 {2} 的结束位置，自动修正当前位置为 {3}", this, this.CurrentLocation, lastMovement.EndLocation, lastMovement.EndLocation), this, this);
                                        this.CurrentLocation = lastMovement.EndLocation;
                                    }

                                    //尝试修正最后一个路径之前未完成的任务（任务在完成后，经过一段时间又更新回了运行中的情况）
                                    {
                                        var allMovements = this.Movements.OrderBy(x => x.Ordering).ToList();
                                        var lastCompletedMovementIndex = allMovements.FindIndex(x => x.Id == lastMovement.Id);
                                        if (lastCompletedMovementIndex > 0)
                                        {
                                            foreach (var item in allMovements.Take(lastCompletedMovementIndex).Where(x => x.Status != LogicMovementStatus.Completed))
                                            {
                                                var oldMovementStatus = item.Status;
                                                item.Status = LogicMovementStatus.Completed;
                                                _logger.Warn1(string.Format("在为 {0} 拆分任务时发现最后一个已完成的 {1} 之前存在未完成的前置 {2}，自动将其状态由 {3} 更改为 {4}", this, lastMovement, item, oldMovementStatus.GetDescription(), item.Status.GetDescription()), this, this);
                                            }
                                        }
                                    }
                                }

                                var routes = PathHelper.FindNextPath(this, LocationConverter.ToLocation(this.StartLocation).ToTaskableLocation(),
                                    LocationConverter.ToLocation(this.EndLocation).ToTaskableLocation(),
                                    LocationConverter.ToLocation(this.CurrentLocation).ToTaskableLocation(),
                                    lastMovementRouteId, this.BizType == TaskBizType.Counting ? RouteType.Counting : RouteType.Normal);
                                if (routes.Count == 0)
                                {
                                    return null;
                                }

                                //如果分数为 0，不再下发
                                var firstRoute = routes.First();
                                var rate = firstRoute.Value.GetRate(firstRoute.Key, this, LocationConverter.ToLocation(this.StartLocation), LocationConverter.ToLocation(this.EndLocation));
                                if (rate <= 0m)
                                {
                                    return null;
                                }

                                logicMovement = firstRoute.Value.CreateLogicMovement(this);

                                AddMovement(logicMovement);

                                isNewMovement = true;

                                string msg = string.Format("共为 {0} 从 {1} 到 {2} 找如下路径:{3},选择当前使用的路径为 {4}",
                                                   this,
                                                   this.CurrentLocation,
                                                   this.EndLocation,
                                                   string.Join(",", routes.Select(x => x.Value.ToString()).ToArray()),
                                                   firstRoute.Value);

                                _logger.Trace1(msg, this, this);

                                return logicMovement;
                            }
                        }

                        break;
                    }
                    else
                    {
                        newMovement = movement;
                    }
                }
                //如果从后找，找到了第一条都是new,那么直接返回第一条
                return newMovement;
            }
        }
        /// <summary>
        /// 获取下一个物理动作
        /// </summary>
        /// <param name="isNewMovement">指示返回的 LogicMovemenet 对象是否是新创建的</param>
        /// <returns>当前可用的待执行的 LogicMovement 对象</returns>
        public virtual LogicMovement GetNextMovement(out Boolean isNewMovement, out String predictRouteIds, PreLogicMovement preLogicMovement = null)
        {
            isNewMovement = false;
            predictRouteIds = null;
            LogicMovement logicMovement = null;
            Location start, end;
            List<Stack<Int32>> routes;
            Stack<Int32> firstRoute;
            if (preLogicMovement == null)
            {
                if (Movements.Where(x => x.Status != LogicMovementStatus.Cancelled).Count() == 0)
                {
                    start = LocationConverter.ToLocation(CurrentLocation);
                    end = LocationConverter.ToLocation(EndLocation);

                    calcRoutes(start, end, out routes, out firstRoute, out predictRouteIds);
                    if (firstRoute == null || routes.Count == 0)
                        return null;

#warning 这里需要增加逻辑判断和节点选择策略，动态路径调整也在这里


                    //foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                    //{
                    //    handler.GetRate(Net net, Route route,Task task, Location startLocation,Location endLocation);
                    //}

                    Next:
                    var routeHeadId = firstRoute.Pop();
                    var routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == routeHeadId);
                    var routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                    //if (start.DeviceCode == routeDetial.Last().Path || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                    if ((routeDetial.Length != 1 && start.DeviceCode == routeDetial.Last().Path) || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                    {
                        routeHeadId = firstRoute.Pop();
                        routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == routeHeadId);
                        routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                    }


                    ///起始点校正 结束点校正
                    Location[] locations;
                    Location _end = null;
                    //if (routeDetial.Count() == 1 && routeDetial.First().Path == routeDetial.First().Device) //兼容区域概念
                    if (routeDetial.Count() == 1)
                    {
                        var device = DeviceConverter.ToDevice<TaskableDevice>(routeHead.Device);
                        locations = device.Locations;
                        if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                            throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                        if (firstRoute.Count() == 0)
                        {
                            _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                            if (_end == null)
                                throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", this.EndLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));
                        }
                        else
                        {
                            ///end查找失败的情况下说明end不在当前单任务设备内，需要连续往下找，例如：堆垛机+穿梭车的模式
                            _end = GetEndLocation(firstRoute.ToArray(), routeHeadId, end);

                            //if (start.UnifiedCode == end.UnifiedCode)
                            //    goto Next;
                        }
                    }
                    else
                    {
                        locations = routeDetial.Select(x => LocationConverter.ConvertibleCodeToLcation(x.ConvertibleCodeToLcation())).ToArray();
                        if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                            throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                        if (!locations.Any(x => x.UnifiedCode == end.UnifiedCode))
                            _end = locations.Last();
                        else
                        {
                            //修正一下在输送线路径中 任务终点在起点之前的异常
                            if (locations.FindIndex(x => x.UnifiedCode == start.UnifiedCode) > locations.FindIndex(x => x.UnifiedCode == end.UnifiedCode))
                                _end = locations.Last();
                            else
                                _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                        }
                    }

                    if (_end == null)
                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未计算出终点位置", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                    if (start.UnifiedCode == _end.UnifiedCode)
                        goto Next;

                    foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                    {
                        if (handler.GetRate(null, null, this, LocationConverter.ToLocation(CurrentLocation), _end) == 0)
                            return logicMovement;
                    }

                    logicMovement = routeHead.CreateLogicMovement(this, LocationConverter.ToLocation(CurrentLocation), _end);
                    AddMovement(logicMovement);

                    isNewMovement = true;

                    return logicMovement;
                }
                else
                {
                    //如果从后往前找，第一个是 Cancelled 或 Completed，就取下一个
                    LogicMovement newMovement = null;
                    var movement = this.Movements.OrderByDescending(x => x.Ordering).First();
                    if (movement.Status != LogicMovementStatus.New)
                    {
                        if (movement.Status == LogicMovementStatus.Cancelled || movement.Status == LogicMovementStatus.Completed)
                        {
                            //如果此任务的后面已经有生成的状态为 new 的 movement就返回，否则生成新的 movement
                            if (newMovement != null)
                            {
                                return newMovement;
                            }
                            else
                            {
                                start = LocationConverter.ToLocation(CurrentLocation);
                                end = LocationConverter.ToLocation(EndLocation);
                                calcRoutes(start, end, out routes, out firstRoute, out predictRouteIds);
                                if (firstRoute == null || routes.Count == 0)
                                    return null;

                                var lastMovement = this.Movements.OrderBy(x => x.Ordering).LastOrDefault();
                                Int32? lastMovementRouteId = lastMovement == null ? null : lastMovement.RouteId;

                                //处理任务当前任务不正确，第二个任务无法下发的情况
                                if (lastMovement != null)
                                {
                                    if (lastMovement.Status != LogicMovementStatus.Cancelled && !this.CurrentLocation.Equals(lastMovement.EndLocation))
                                    {
                                        _logger.Warn1(string.Format("在为 {0} 拆分任务时发现当前位置 {1} 并非 {2} 的结束位置，自动修正当前位置为 {3}", this, this.CurrentLocation, lastMovement.EndLocation, lastMovement.EndLocation), this, this);
                                        this.CurrentLocation = lastMovement.EndLocation;
                                    }

                                    //尝试修正最后一个路径之前未完成的任务（任务在完成后，经过一段时间又更新回了运行中的情况）
                                    {
                                        var allMovements = this.Movements.OrderBy(x => x.Ordering).ToList();
                                        var lastCompletedMovementIndex = allMovements.FindIndex(x => x.Id == lastMovement.Id);
                                        if (lastCompletedMovementIndex > 0)
                                        {
                                            foreach (var item in allMovements.Take(lastCompletedMovementIndex).Where(x => x.Status != LogicMovementStatus.Completed))
                                            {
                                                var oldMovementStatus = item.Status;
                                                item.Status = LogicMovementStatus.Completed;
                                                _logger.Warn1(string.Format("在为 {0} 拆分任务时发现最后一个已完成的 {1} 之前存在未完成的前置 {2}，自动将其状态由 {3} 更改为 {4}", this, lastMovement, item, oldMovementStatus.GetDescription(), item.Status.GetDescription()), this, this);
                                            }
                                        }
                                    }
                                }

                                if (routes.Count == 0)
                                    return null;

                                //var firstRoute = routes.OrderBy(x => RouteHelper.GetOneRouteLocationSequences(start, end, x).Count()).FirstOrDefault();

                                Next:
                                var routeHeadId = firstRoute.Pop();
                                var routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == routeHeadId);
                                var routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                                if (start.DeviceCode == routeDetial.Last().Path || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                                {
                                    routeHeadId = firstRoute.Pop();
                                    routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == routeHeadId);
                                    routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                                }

                                ///起始点校正 结束点校正
                                Location[] locations;
                                Location _end = null;
                                if (routeDetial.Count() == 1 && routeDetial.First().Path == routeDetial.First().Device)
                                {
                                    var device = DeviceConverter.ToDevice<TaskableDevice>(routeHead.Device);
                                    locations = device.Locations;
                                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                                    if (firstRoute.Count() == 0)
                                    {
                                        _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                                        if (_end == null)
                                            throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", this.EndLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));
                                    }
                                    else
                                    {
                                        ///end查找失败的情况下说明end不在当前单任务设备内，需要连续往下找，例如：堆垛机+穿梭车的模式
                                        _end = GetEndLocation(firstRoute.ToArray(), routeHeadId, end);

                                        //if (start.UnifiedCode == end.UnifiedCode)
                                        //    goto Next;
                                    }
                                }
                                else
                                {
                                    locations = routeDetial.Select(x => LocationConverter.ConvertibleCodeToLcation(x.ConvertibleCodeToLcation())).ToArray();
                                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                                    if (!locations.Any(x => x.UnifiedCode == end.UnifiedCode))
                                        _end = locations.Last();
                                    else
                                        _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                                }

                                if (_end == null)
                                    throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未计算出终点位置", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                                if (start.UnifiedCode == _end.UnifiedCode)
                                    goto Next;

                                foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                                {
                                    if (handler.GetRate(null, null, this, LocationConverter.ToLocation(CurrentLocation), _end) == 0)
                                        return logicMovement;
                                }

                                logicMovement = routeHead.CreateLogicMovement(this, LocationConverter.ToLocation(CurrentLocation), _end);

                                AddMovement(logicMovement);

                                isNewMovement = true;

                                return logicMovement;
                            }
                        }
                    }
                    else
                        newMovement = movement;
                    //如果从后找，找到了第一条都是new,那么直接返回第一条
                    return newMovement;
                }
            }
            else
            {
                var routeHead = RouteHelper.RouteHeads.First(x => x.HeadID == preLogicMovement.RouteId);
                logicMovement = routeHead.CreateLogicMovement(this, LocationConverter.ToLocation(preLogicMovement.StartLocation), LocationConverter.ToLocation(preLogicMovement.EndLocation));

                AddMovement(logicMovement);

                isNewMovement = true;

                return logicMovement;
            }
        }
        /// <summary>
        /// 获取下一个物理动作
        /// </summary>
        /// <param name="isNewMovement">指示返回的 LogicMovemenet 对象是否是新创建的</param>
        /// <returns>当前可用的待执行的 LogicMovement 对象</returns>
        public virtual PreLogicMovement GetNextPreMovement(out Boolean isNewMovement, out String predictRouteIds, ThreadRunningLog threadRunningLog)
        {
            isNewMovement = false;
            predictRouteIds = null;
            PreLogicMovement logicMovement = null;
            Location start, end;
            List<Stack<Int32>> routes;
            Stack<Int32> firstRoute;
            if (Movements.Where(x => x.Status != LogicMovementStatus.Cancelled).Count() == 0)
            {
                start = LocationConverter.ToLocation(CurrentLocation);
                end = LocationConverter.ToLocation(EndLocation);
                threadRunningLog.Log($"本次预拆分任务 {this} 逻辑任务列表中非任务数量为0 计算 起点 {start.UserCode} 终点 {end.UserCode}");

                calcRoutes(start, end, out routes, out firstRoute, out predictRouteIds);
                if (firstRoute == null || routes.Count == 0)
                    return null;

#warning 这里需要增加逻辑判断和节点选择策略，动态路径调整也在这里


                //foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                //{
                //    handler.GetRate(Net net, Route route,Task task, Location startLocation,Location endLocation);
                //}

                Next:
                var routeHeadId = firstRoute.Pop();
                var routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == routeHeadId);
                var routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                //if (start.DeviceCode == routeDetial.Last().Path || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                if ((routeDetial.Length != 1 && start.DeviceCode == routeDetial.Last().Path) || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                {
                    routeHeadId = firstRoute.Pop();
                    routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == routeHeadId);
                    routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                }


                ///起始点校正 结束点校正
                Location[] locations;
                Location _end = null;
                //if (routeDetial.Count() == 1 && routeDetial.First().Path == routeDetial.First().Device) //兼容区域概念
                if (routeDetial.Count() == 1)
                {
                    var device = DeviceConverter.ToDevice<TaskableDevice>(routeHead.Device);
                    locations = device.Locations;
                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                    if (firstRoute.Count() == 0)
                    {
                        _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                        if (_end == null)
                            throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", this.EndLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));
                    }
                    else
                    {
                        ///end查找失败的情况下说明end不在当前单任务设备内，需要连续往下找，例如：堆垛机+穿梭车的模式
                        _end = GetEndLocation(firstRoute.ToArray(), routeHeadId, end);

                        //if (start.UnifiedCode == end.UnifiedCode)
                        //    goto Next;
                    }
                }
                else
                {
                    locations = routeDetial.Select(x => LocationConverter.ConvertibleCodeToLcation(x.ConvertibleCodeToLcation())).ToArray();
                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                    if (!locations.Any(x => x.UnifiedCode == end.UnifiedCode))
                        _end = locations.Last();
                    else
                    {
                        //修正一下在输送线路径中 任务终点在起点之前的异常
                        if (locations.FindIndex(x => x.UnifiedCode == start.UnifiedCode) > locations.FindIndex(x => x.UnifiedCode == end.UnifiedCode))
                            _end = locations.Last();
                        else
                            _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                    }
                }

                if (_end == null)
                    throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未计算出终点位置", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                if (start.UnifiedCode == _end.UnifiedCode)
                    goto Next;

                foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                {
                    if (handler.GetRate(null, null, this, LocationConverter.ToLocation(CurrentLocation), _end) == 0)
                        return logicMovement;
                }

                //logicMovement = routeHead.CreateLogicMovement(this, LocationConverter.ToLocation(CurrentLocation), _end);
                logicMovement = new PreLogicMovement(routeHead.Id, LocationConverter.ToLocation(CurrentLocation), _end);
                AddMovement(logicMovement);

                isNewMovement = true;

                return logicMovement;
            }
            else
            {
                //如果从后往前找，第一个是 Cancelled 或 Completed，就取下一个
                LogicMovement newMovement = null;
                var movement = this.Movements.Where(x => x.Status != LogicMovementStatus.Cancelled).OrderByDescending(x => x.Ordering).First();

                start = LocationConverter.ToLocation(movement.EndLocation);
                end = LocationConverter.ToLocation(EndLocation);
                threadRunningLog.Log($"本次预拆分任务 {this} 根据上一条逻辑动作(起点 {movement.StartLocation.UserCode} 终点 {movement.EndLocation.UserCode})计算 起点 {start.UserCode} 终点 {end.UserCode}");
                calcRoutes(start, end, out routes, out firstRoute, out predictRouteIds);
                if (firstRoute == null || routes.Count == 0)
                    return null;

                var lastMovement = this.Movements.OrderBy(x => x.Ordering).LastOrDefault();
                Int32? lastMovementRouteId = lastMovement == null ? null : lastMovement.RouteId;

                //处理任务当前任务不正确，第二个任务无法下发的情况
                if (lastMovement != null)
                {
                    if (lastMovement.Status != LogicMovementStatus.Cancelled && !this.CurrentLocation.Equals(lastMovement.EndLocation))
                    {
                        _logger.Warn1(string.Format("在为 {0} 拆分任务时发现当前位置 {1} 并非 {2} 的结束位置，自动修正当前位置为 {3}", this, this.CurrentLocation, lastMovement.EndLocation, lastMovement.EndLocation), this, this);
                        this.CurrentLocation = lastMovement.EndLocation;
                    }

                    //尝试修正最后一个路径之前未完成的任务（任务在完成后，经过一段时间又更新回了运行中的情况）
                    {
                        var allMovements = this.Movements.OrderBy(x => x.Ordering).ToList();
                        var lastCompletedMovementIndex = allMovements.FindIndex(x => x.Id == lastMovement.Id);
                        if (lastCompletedMovementIndex > 0)
                        {
                            foreach (var item in allMovements.Take(lastCompletedMovementIndex).Where(x => x.Status != LogicMovementStatus.Completed))
                            {
                                var oldMovementStatus = item.Status;
                                item.Status = LogicMovementStatus.Completed;
                                _logger.Warn1(string.Format("在为 {0} 拆分任务时发现最后一个已完成的 {1} 之前存在未完成的前置 {2}，自动将其状态由 {3} 更改为 {4}", this, lastMovement, item, oldMovementStatus.GetDescription(), item.Status.GetDescription()), this, this);
                            }
                        }
                    }
                }

                if (routes.Count == 0)
                    return null;

                //var firstRoute = routes.OrderBy(x => RouteHelper.GetOneRouteLocationSequences(start, end, x).Count()).FirstOrDefault();

                Next:
                var routeHeadId = firstRoute.Pop();
                var routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == routeHeadId);
                var routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                if (start.DeviceCode == routeDetial.Last().Path || start.Synonymous.Any(x => x.DeviceCode == routeDetial.Last().Path))
                {
                    routeHeadId = firstRoute.Pop();
                    routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == routeHeadId);
                    routeDetial = RouteHelper.RouteDetails.Where(x => x.Id == routeHeadId).OrderBy(x => x.DetailID).ToArray();
                }

                ///起始点校正 结束点校正
                Location[] locations;
                Location _end = null;
                if (routeDetial.Count() == 1 && routeDetial.First().Path == routeDetial.First().Device)
                {
                    var device = DeviceConverter.ToDevice<TaskableDevice>(routeHead.Device);
                    locations = device.Locations;
                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                    if (firstRoute.Count() == 0)
                    {
                        _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                        if (_end == null)
                            throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", this.EndLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));
                    }
                    else
                    {
                        ///end查找失败的情况下说明end不在当前单任务设备内，需要连续往下找，例如：堆垛机+穿梭车的模式
                        _end = GetEndLocation(firstRoute.ToArray(), routeHeadId, end);

                        //if (start.UnifiedCode == end.UnifiedCode)
                        //    goto Next;
                    }
                }
                else
                {
                    locations = routeDetial.Select(x => LocationConverter.ConvertibleCodeToLcation(x.ConvertibleCodeToLcation())).ToArray();
                    if (!locations.Any(x => x.UnifiedCode == start.UnifiedCode))
                        throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未匹配到任何元素", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                    if (!locations.Any(x => x.UnifiedCode == end.UnifiedCode))
                        _end = locations.Last();
                    else
                        _end = locations.FirstOrDefault(x => x.UnifiedCode == end.UnifiedCode);
                }

                if (_end == null)
                    throw new Exception(string.Format("位置 {0} 在路径 {1}({2}) 中未计算出终点位置", CurrentLocation.UserCode, routeHead.Id, String.Join(",", routeDetial.Select(x => x.ConvertibleCodeToLcation()))));

                if (start.UnifiedCode == _end.UnifiedCode)
                    goto Next;

                foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteStrategysElement.RouteStrategys)
                {
                    if (handler.GetRate(null, null, this, LocationConverter.ToLocation(CurrentLocation), _end) == 0)
                        return logicMovement;
                }

                //logicMovement = routeHead.CreateLogicMovement(this, LocationConverter.ToLocation(CurrentLocation), _end);

                logicMovement = new PreLogicMovement(routeHead.Id, LocationConverter.ToLocation(CurrentLocation), _end);
                AddMovement(logicMovement);

                isNewMovement = true;

                return logicMovement;
            }
        }

        private void calcRoutes(Location start, Location end, out List<Stack<Int32>> routes, out Stack<Int32> firstRoute, out String predictRouteIds)
        {
            firstRoute = null;
            routes = RouteHelper.GetAllRouteIdSequences(start, end);
            predictRouteIds = "";
            if (start.Synonymous.Count() != 0)
            {
                foreach (var item in LocationConverter.ToLocation(CurrentLocation).Synonymous)
                {
                    var _routes = RouteHelper.GetAllRouteIdSequences(item, end);
                    foreach (var _route in _routes)
                    {
                        if (!routes.Any(x => String.Join("", x) == String.Join("", _route)))
                            routes.Add(_route);
                    }
                }
            }

            if (Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteSelectorsElement.RouteSelectors.Count() != 0)
            {
                if (!Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteSelectorsElement.RouteSelectors.Any(x => x.Allowed(this)))
                    ///默认取取最短路径
                    firstRoute = routes.OrderBy(x => x.Count()).ThenBy(x => RouteHelper.GetOneRouteLocationSequences(start, end, x).Count()).FirstOrDefault();
                else
                {
                    foreach (var handler in Wcs.Framework.Cfg.WcsConfiguration.Instance.RouteSelectorsElement.RouteSelectors)
                    {
                        if (handler.Allowed(this))
                        {
                            firstRoute = handler.GetBestRoute(this, routes, start, end);
                            break;
                        }
                    }
                }
            }
            else
                ///默认取取最短路径
                firstRoute = routes.OrderBy(x => x.Count()).ThenBy(x => RouteHelper.GetOneRouteLocationSequences(start, end, x).Count()).FirstOrDefault();

            if (firstRoute == null)
            {
                _logger.Error1(new Exception($"尝试为任务{this}拆分路径时未计算出合适的路径，请检查 RouteSelectors 配置"), this.TaskCode);
                return;
            }
            ///是否需要校正start
            //var _routeHeader = RouteHelper.RouteHeads.First(x => x.Id == firstRoute.First());

            predictRouteIds = string.Join(",", firstRoute);
        }

        //RouteExternalPlugin
        private Location GetEndLocation(int[] firstRoute, int routeHeadId, Location end)
        {
            Location _end;
            var nextRouteHeadId = firstRoute.First();
            var nextRouteHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == nextRouteHeadId);
            var nextRouteDetial = RouteHelper.RouteDetails.Where(x => x.Id == nextRouteHeadId).OrderBy(x => x.DetailID).ToArray();
            if (nextRouteDetial.Count() == 1 && nextRouteDetial.First().Path == nextRouteDetial.First().Device)
            {
                var currentRouteHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.Id == routeHeadId);
                var routeExternalPlugins = RouteHelper.RouteExternalPlugins.Where(x => x.StartsWith(currentRouteHead.Device) && x.EndsWith(nextRouteHead.Device));
                if (routeExternalPlugins.Count() == 1)
                {
                    var items = routeExternalPlugins.First().Split(',').ToArray();
                    if (items.Length == 3)
                        return LocationConverter.ConvertibleCodeToLcation(items[1]);
                }
                else if (routeExternalPlugins.Count() > 1)
                    //这里可以尝试提供一个外挂程序来处理
                    throw new Exception($"连续两个单任务设备存在多个{currentRouteHead.Device},yyyy@xxxx,{nextRouteHead.Device}({string.Join(";", routeExternalPlugins)})，程序暂时不能处理这种情况，需要升级");

                var nextDevice = DeviceConverter.ToDevice<TaskableDevice>(nextRouteHead.Device);
                var nextLocations = nextDevice.Locations.Where(x => !(x is ILocationWildcard) && x.Synonymous.Length > 0);

                var currentDevice = DeviceConverter.ToDevice<TaskableDevice>(currentRouteHead.Device);
                var currentLocations = currentDevice.Locations.Where(x => !(x is ILocationWildcard) && x.Synonymous.Length > 0);
                var ends = currentLocations.Where(x =>
                    nextLocations.Any(y => y.UserCode == x.UserCode || y.Synonymous.Any(z => z.UserCode == x.UserCode) || x.Synonymous.Any(z => z.UserCode == y.UserCode))
                    || nextLocations.Any(y => y.Synonymous.Any(z => z.UserCode == x.UserCode || x.Synonymous.Any(zz => zz.UserCode == z.UserCode)))).ToArray();
                if (ends.Count() == 1)
                    return ends.First();
                //if (!nextLocations.Any(x => x.UserCode == end.UserCode) && !nextLocations.Any(x => x.Synonymous.Any(y => y.UserCode == end.UserCode)))
                //    //这里可以尝试提供一个外挂程序来处理
                //    throw new Exception("连续两个单任务设备都未找到任务结束点，程序暂时不能处理这种情况，需要升级");
                else if (ends.Count() > 1)
                {
                    //20230414更新尝试选取ends的synonymous中同一个非当前设备的位置来暂时替代，后续创建逻辑动作时会尝试转换成对应设备中的位置，就堆垛机来说创建逻辑动作时是存在此项转换的不一定通用
                    var synonymous = ends.SelectMany(x => x.Synonymous.Where(y => y.Device.Name != currentDevice.Name)).ToArray();
                    foreach (var item in synonymous)
                    {
                        if (synonymous.Count(x => x.UserCode == item.UserCode) == ends.Length)
                        {
                            return item;
                        }
                    }

                    //这里可以尝试提供一个外挂程序来处理,来规划规则
                    throw new Exception($"连续两个单任务设备需要在Settings.xml文件中提供RouteExternalPlugin规范，否则多个交互点({string.Join(";", ends.Select(x => x.UserCode))})无法确认");
                }
                else
                    throw new Exception("连续两个单任务设备联通的情况下需要在对应设备的位置文件中指定synonymous位置，否则无法确认交互点");
            }
            else
            {
                var locations = nextRouteDetial.Select(x => LocationConverter.ConvertibleCodeToLcation(x.ConvertibleCodeToLcation())).ToArray();
                //if (!locations.Any(x => x.UserCode == end.UserCode) && !locations.Any(x => x.Synonymous.Any(y => y.UserCode == end.UserCode)))
                _end = locations.First();
                //else
                //    _end = locations.FirstOrDefault(x => x.UserCode == end.UserCode || x.Synonymous.Any(y => y.UserCode == end.UserCode));
            }
            return _end;
        }

        public virtual int Compare(Task x, Task y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            if (x.Id != y.Id
             || x.BizType != y.BizType
             || x.CreatedAt != y.CreatedAt
             || x.CurrentLocation.Compare(x.CurrentLocation, y.CurrentLocation) != 0
             || x.Description != y.Description
             || x.Direction != y.Direction
             || x.EndLocation.Compare(x.EndLocation, y.EndLocation) != 0
             || x.FinishedAt != y.FinishedAt
             || x.StartLocation.Compare(x.StartLocation, y.StartLocation) != 0
             || x.Status != y.Status
             || x.TaskCode != y.TaskCode
                )
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return string.Format("执行任务 {0}#{1}", this.Id, this.TaskCode);
        }

        ///// <summary>
        ///// 深克隆
        ///// </summary>
        ///// <returns></returns>
        //public virtual Task Clone()
        //{
        //    Type t = this.GetType();
        //    PropertyInfo[] properties = t.GetProperties();
        //    Object p = t.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, this, null);
        //    foreach (PropertyInfo pi in properties)
        //    {
        //        if (pi.CanWrite)
        //        {
        //            object value = pi.GetValue(this, null);
        //            pi.SetValue(p, value, null);
        //        }
        //    }
        //    return p;
        //}
    }
}
