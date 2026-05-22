using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Xml;

namespace Wcs.Framework
{
    public static class RouteHelper
    {
        const string _path = "系统配置\\基本配置";
        const string _basicDataExcelName = "基础数据";
        const string _basicXMLName = "routes";
        const string _basicXMLDeviceErrorType = "DeviceErrorType";
        const string _unableSingleTaskDeviceUnableStations = "unableSingleTaskDeviceUnableStations";

        static object unableSingltTaskDeviceUnableStationsDicLocker = new object();
        static string _unableSingltTaskDeviceUnableStationsDicStr = "";
        static Dictionary<string, List<string>> _unableSingltTaskDeviceUnableStationsDic = null;
        public static Dictionary<string, List<string>> UnableSingltTaskDeviceUnableStationsDic
        {
            get
            {
                lock (unableSingltTaskDeviceUnableStationsDicLocker)
                {
                INITUnableSingltTaskDeviceUnableStationsDic:
                    if (_unableSingltTaskDeviceUnableStationsDic == null)
                    {
                        _unableSingltTaskDeviceUnableStationsDic = new Dictionary<string, List<string>>();
                        _unableSingltTaskDeviceUnableStationsDicStr = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>(_unableSingleTaskDeviceUnableStations, "");
                        var dicstrs = _unableSingltTaskDeviceUnableStationsDicStr.Split(';').Where(x => !string.IsNullOrWhiteSpace(x));
                        foreach (var dicstr in dicstrs)
                        {
                            var _dicstr = dicstr.Split(':').ToArray();
                            var _dicList = _dicstr[1].Split(',').ToList();
                            _unableSingltTaskDeviceUnableStationsDic.Add(_dicstr[0], _dicList);
                        }
                    }
                    else
                    {
                        var __unableSingltTaskDeviceUnableStationsDicStr = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>(_unableSingleTaskDeviceUnableStations, "");
                        if (__unableSingltTaskDeviceUnableStationsDicStr != _unableSingltTaskDeviceUnableStationsDicStr)
                        {
                            _unableSingltTaskDeviceUnableStationsDic = null;
                            goto INITUnableSingltTaskDeviceUnableStationsDic;
                        }
                    }
                }

                return _unableSingltTaskDeviceUnableStationsDic;
            }
        }

        static List<RouteHead> _routeHeads;
        public static List<RouteHead> RouteHeads
        {
            get
            {
                if (_routeHeads == null || _routeHeads.Count() == 0)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        _routeHeads = unitOfWork.session.Query<RouteHead>().ToList();
                        unitOfWork.Commit();
                    }
                }
                return _routeHeads;
            }
        }

        static IEnumerable<Int32> _routeHeadIds;
        public static IEnumerable<Int32> RouteHeadIds
        {
            get
            {
                if (_routeHeadIds == null || _routeHeadIds.Count() == 0)
                    _routeHeadIds = RouteHeads.Select(x => x.HeadID);
                return _routeHeadIds;
            }
        }

        static List<Int32> disableRouteIds;
        public static List<Int32> DisableRouteIds
        {
            get
            {
                if (disableRouteIds == null)
                {
                    var disIds = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("disableRouteIds", "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x));
                    //if (disIds.Count() > 0)
                    disableRouteIds = disIds.Select(x => Convert.ToInt32(x)).ToList();
                }
                return disableRouteIds;
            }
        }

        public static void SetDisableRoute(int routeId, bool unable)
        {
            if (disableRouteIds == null)
            {
                var disIds = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("disableRouteIds", "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x));
                //if (disIds.Count() > 0)
                disableRouteIds = disIds.Select(x => Convert.ToInt32(x)).ToList();
            }
            if (unable)
            {
                if (!disableRouteIds.Contains(routeId))
                {
                    disableRouteIds.Add(routeId);
                    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<string>("disableRouteIds", string.Join(",", disableRouteIds));
                }
            }
            else
            {
                if (disableRouteIds.Contains(routeId))
                {
                    disableRouteIds.Remove(routeId);
                    Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.SetSetting<string>("disableRouteIds", string.Join(",", disableRouteIds));
                }
            }
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        public static void Introduction()
        {
            try
            {
                string path = System.Windows.Forms.Application.StartupPath + "\\" + _path;
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var file = directoryInfo.GetFiles().FirstOrDefault(x => x.FullName.Contains(_basicDataExcelName));
                if (file == null)
                    return;

                var _heads = ExcelToDataTable(file, "RouteHead", true).AsEnumerable().Select(x =>
                            new RouteHead()
                            {
                                HeadID = Convert.ToInt32(x["HeadID"]),
                                ProjectCode = x["ProjectCode"].ToString(),
                                ProjectName = x["ProjectName"].ToString(),
                                Id = Convert.ToInt32(x["Id"]),
                                No = Convert.ToInt32(x["No"]),
                                Direction = x["Direction"].ToString(),
                                Device = x["Device"].ToString(),
                                RouteVsActionRelation = (RouteVsActionRelations)Convert.ToInt32(x["RouteVsActionRelation"]),
                                AllowStartFromMidway = Convert.ToBoolean(x["AllowStartFromMidway"])
                            }).ToArray();
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _heads)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
                var _relations = ExcelToDataTable(file, "RouteRelation", true).AsEnumerable().Select(x =>
                                    new RouteRelation()
                                    {
                                        RelationID = Convert.ToInt32(x["RelationID"]),
                                        Id = Convert.ToInt32(x["Id"]),
                                        Adjoins = Convert.ToInt32(x["Adjoins"])
                                    });
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _relations)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
                var _details = ExcelToDataTable(file, "RouteDetail", true).AsEnumerable().Select(x =>
                                    new RouteDetail()
                                    {
                                        DetailID = Convert.ToInt32(x["DetailID"]),
                                        Id = Convert.ToInt32(x["Id"]),
                                        Path = x["Path"].ToString(),
                                        Device = x["Device"].ToString()
                                    });
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _details)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
                var _deviceErrors = ExcelToDataTable(file, "DeviceErrorType", true).AsEnumerable().Select(x =>
                                    new DeviceErrorType()
                                    {
                                        Id = Convert.ToInt32(x["Id"]),
                                        AlarmCategory = Convert.ToInt32(x["AlarmCategory"]),
                                        DeviceType = x["DeviceType"].ToString().Trim(),
                                        DeviceErrorCode = x["DeviceErrorCode"].ToString().Trim(),
                                        ErrorName = x["ErrorName"].ToString(),
                                        Description = x["Description"].ToString(),
                                        Levle = Convert.ToInt16(x["Levle"]),
                                        Solution = x["Solution"].ToString().Trim(),
                                        IsFault = Convert.ToBoolean(x["IsFault"])
                                    });

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _deviceErrors)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据Routes XML文件导入数据
        /// </summary>
        public static void IntroductionByRoutesXMl()
        {
            try
            {
                string path = System.Windows.Forms.Application.StartupPath + "\\" + _path;
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var file = directoryInfo.GetFiles().FirstOrDefault(x => x.FullName.Contains(_basicXMLName));
                if (file == null)
                    return;
                XmlDocument xmlDoc = new XmlDocument();
                List<RouteHead> routeHeads = new List<RouteHead>();
                List<RouteDetail> routeDetails = new List<RouteDetail>();
                List<RouteRelation> routeRelations = new List<RouteRelation>();
                Dictionary<int, string> adjoins = new Dictionary<int, string>();
                int a = 1;
                int b = 1;
                int c = 1;
                int d = 1;
                int f = 1;
                XmlNodeList routes;

                xmlDoc.Load(file.FullName);
                routes = xmlDoc.SelectNodes("//route");
                foreach (XmlNode route in routes)
                {

                    RouteHead rd = new RouteHead();
                    rd.HeadID = a;
                    rd.ProjectCode = "1";
                    rd.ProjectName = "1";
                    rd.Id = int.Parse(route.Attributes.GetNamedItem("id") == null ? null : route.Attributes.GetNamedItem("id").InnerText);
                    if (route.Attributes.GetNamedItem("no") == null)
                    {
                        rd.No = 1;
                    }
                    else
                    {
                        rd.No = int.Parse(route.Attributes.GetNamedItem("no") == null ? null : route.Attributes.GetNamedItem("no").InnerText);
                    }
                    rd.Direction = "1";
                    //rd.Priority = "1";
                    rd.Device = route.Attributes.GetNamedItem("device") == null ? null : route.Attributes.GetNamedItem("device").InnerText;
                    rd.RouteVsActionRelation = RouteVsActionRelations.OneToMany;
                    rd.AllowStartFromMidway = true;

                    routeHeads.Add(rd);
                    a++;
                }
                foreach (XmlNode item in routes)
                {

                    String[] ls = item.Attributes.GetNamedItem("path").InnerText.Split(',');
                    int id = routeHeads.Where(x => x.Id == int.Parse(item.Attributes.GetNamedItem("id").InnerText)).Select(x => x.HeadID).FirstOrDefault(); ;
                    if (ls != null && ls.Length <= 0)
                    {
                        continue;
                    }
                    foreach (var l in ls)
                    {
                        RouteDetail routeDetail = new RouteDetail();
                        routeDetail.DetailID = b;
                        //routeDetail.ProjectCode = "1";
                        routeDetail.Id = id;
                        //routeDetail.Id = int.Parse(item.Attributes.GetNamedItem("id") == null ? null : item.Attributes.GetNamedItem("id").InnerText);
                        routeDetail.Path = l;
                        routeDetail.Device = item.Attributes.GetNamedItem("device") == null ? null : item.Attributes.GetNamedItem("device").InnerText;
                        routeDetails.Add(routeDetail);
                        b++;
                    }

                }
                foreach (XmlNode item in routes)
                {
                    int id = routeHeads.Where(x => x.Id == int.Parse(item.Attributes.GetNamedItem("id").InnerText)).Select(x => x.HeadID).FirstOrDefault();
                    if (string.IsNullOrEmpty(item.Attributes.GetNamedItem("adjoins").InnerText))
                    {
                        continue;
                    }
                    String[] ls = item.Attributes.GetNamedItem("adjoins").InnerText.Split(',');
                    if (ls != null && ls.Length <= 0)
                    {
                        continue;
                    }
                    foreach (var l in ls)
                    {
                        int adjoin = routeHeads.Where(x => x.Id == int.Parse(l)).Select(x => x.HeadID).FirstOrDefault();
                        RouteRelation routeRelation = new RouteRelation();
                        routeRelation.RelationID = d;
                        //routeRelation.ProjectCode = "1";
                        routeRelation.Id = id;
                        routeRelation.Adjoins = adjoin;
                        routeRelations.Add(routeRelation);
                        d++;
                    }

                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in routeHeads)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    foreach (var item in routeDetails)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    foreach (var item in routeRelations)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入失败，原因为：" + ex.Message);
            }
        }
        /// <summary>
        /// 将Excel导入DataTable
        /// </summary>
        /// <param name="filepath">导入的文件路径（包括文件名）</param>
        /// <param name="sheetname">工作表名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>DataTable</returns>
        public static DataTable ExcelToDataTable(FileInfo file, string sheetname, bool isFirstRowColumn)
        {
            DataTable data = new DataTable();
            var package = new ExcelPackage(file);
            ExcelWorkbook workbook = package.Workbook;

            if (workbook != null)
            {
                if (workbook.Worksheets.Count > 0)
                {
                    ExcelWorksheet worksheet = workbook.Worksheets[sheetname];
                    data = WorksheetToTable(worksheet);
                }
            }

            return data;
        }

        public static void Introduction2()
        {
            try
            {
                string path = System.Windows.Forms.Application.StartupPath + "\\" + _path;
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var file = directoryInfo.GetFiles().FirstOrDefault(x => x.FullName.Contains(_basicDataExcelName));
                if (file == null)
                {
                    MessageBox.Show(String.Join("未读取到配置文件 {0}", file.FullName));
                    return;
                }

                var routes = ExcelToDataTable(file, "Routes", true).AsEnumerable().Select(x =>
                                new _routes()
                                {
                                    HeadId = x["HeadID"].ToString().Trim(),
                                    ProjectCode = x["ProjectCode"].ToString().Trim(),
                                    ProjectName = x["ProjectName"].ToString().Trim(),
                                    No = x["No"].ToString().Trim(),
                                    Direction = x["Direction"].ToString().Trim(),
                                    Device = x["Device"].ToString().Trim(),
                                    RouteVsActionRelation = x["RouteVsActionRelation"].ToString().Trim(),
                                    AllowStartFromMidway = x["AllowStartFromMidway"].ToString().Trim(),
                                    Path = x["Path"].ToString().Trim(),
                                    Adjoins = x["Adjoins"].ToString().Trim(),
                                    Group = x["Group"].ToString().Trim(),
                                    PathOnlyStarts = x["PathOnlyStarts"].ToString().Trim(),
                                    PathOnlyEnds = x["PathOnlyEnds"].ToString().Trim()
                                }).ToArray();

                var _heads = routes.Select(x =>
                                new RouteHead()
                                {
                                    HeadID = Convert.ToInt32(x.HeadId),
                                    ProjectCode = x.ProjectCode,
                                    ProjectName = x.ProjectName,
                                    Id = Convert.ToInt32(x.HeadId),
                                    No = Convert.ToInt32(x.No),
                                    Direction = x.Direction,
                                    Device = x.Device,
                                    RouteVsActionRelation = (RouteVsActionRelations)Convert.ToInt32(x.RouteVsActionRelation),
                                    AllowStartFromMidway = Convert.ToBoolean(x.AllowStartFromMidway),
                                    Group = x.Group,
                                    PathOnlyStarts = x.PathOnlyStarts,
                                    PathOnlyEnds = x.PathOnlyEnds
                                });
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _heads)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }

                List<RouteDetail> _details = new List<RouteDetail>();
                List<RouteRelation> _relations = new List<RouteRelation>();

                Int32 _relationId = 0, _detailId = 0;
                foreach (var item in routes)
                {
                    if (!String.IsNullOrWhiteSpace(item.Path))
                    {
                        foreach (var loc in item.Path.Split(','))
                        {
                            _details.Add(new RouteDetail() { DetailID = _detailId++, Device = item.Device, Id = Convert.ToInt32(item.HeadId), Path = loc.Trim() });
                        }
                    }

                    if (!String.IsNullOrWhiteSpace(item.Adjoins))
                    {
                        foreach (var _id in item.Adjoins.Split(','))
                        {
                            _relations.Add(new RouteRelation() { RelationID = _relationId++, Id = Convert.ToInt32(item.HeadId), Adjoins = Convert.ToInt32(_id) });
                        }
                    }
                }

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _relations)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }

                    foreach (var item in _details)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }

                var _deviceErrors = ExcelToDataTable(file, "DeviceErrorType", true).AsEnumerable().Select(x =>
                                    new DeviceErrorType()
                                    {
                                        Id = Convert.ToInt32(x["Id"]),
                                        AlarmCategory = Convert.ToInt32(x["AlarmCategory"]),
                                        DeviceType = x["DeviceType"].ToString().Trim(),
                                        DeviceErrorCode = x["DeviceErrorCode"].ToString().Trim(),
                                        ErrorName = x["ErrorName"].ToString().Trim(),
                                        Description = x["Description"].ToString().Trim(),
                                        Levle = Convert.ToInt16(x["Levle"]),
                                        Solution = x["Solution"].ToString().Trim(),
                                        IsFault = Convert.ToBoolean(x["IsFault"])
                                    });

                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in _deviceErrors)
                    {
                        unitOfWork.session.SaveOrUpdate(item);
                    }
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static DataTable WorksheetToTable(ExcelWorksheet worksheet)
        {
            //获取worksheet的行数
            int rows = worksheet.Dimension.End.Row;
            //获取worksheet的列数
            int cols = worksheet.Dimension.End.Column;
            DataTable dt = new DataTable(worksheet.Name);
            DataRow dr = null;
            DataColumn dc = null;

            for (int j = 1; j > 0; j++)
            {
                if (worksheet.Cells[1, j].Value != null)
                    dc = dt.Columns.Add(worksheet.Cells[1, j].Value.ToString());
                else
                {
                    cols = j;
                    break;
                }
            }

            for (int i = 2; i > 0; i++)
            {
                if (worksheet.Cells[i, 1].Value != null)
                {
                    dr = dt.Rows.Add();
                    for (int j = 1; j < cols; j++)
                    {
                        try
                        {
                            if (worksheet.Cells[i, j].Value != null)
                                dr[j - 1] = worksheet.Cells[i, j].Value.ToString();
                            else
                                dr[j - 1] = "";
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                else
                    break;
            }
            return dt;
        }

        static List<RouteDetail> _routeDetails;
        public static List<RouteDetail> RouteDetails
        {
            get
            {
                if (_routeDetails == null || _routeDetails.Count() == 0)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        _routeDetails = unitOfWork.session.Query<RouteDetail>().ToList();
                        unitOfWork.Commit();
                    }
                }
                return _routeDetails;
            }
        }

        static List<RouteRelation> _routeRelations;
        public static List<RouteRelation> RouteRelations
        {
            get
            {
                if (_routeRelations == null || _routeRelations.Count() == 0)
                {
                    using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                    {
                        _routeRelations = unitOfWork.session.Query<RouteRelation>().ToList();
                        unitOfWork.Commit();
                    }
                    _routeRelations = _routeRelations.Where(x => RouteHeadIds.Contains(x.Id) && RouteHeadIds.Contains(x.Adjoins)).ToList();
                }
                return _routeRelations;
            }
        }

        static Int32 routeHeadId_AbleArrived = 0;
        static Stack<Int32> stack_AbleArrived;
        static List<Int32> outRouteRelecatons_AbleArrived;
        /// <summary>
        /// 判断两个Location是否联通
        /// </summary>
        /// <param name="start">起点位置</param>
        /// <param name="end">终点位置</param>
        /// <returns>true联通 false不联通</returns>
        public static Boolean AbleArrived(Location start, Location end)
        {
            List<RouteDetail> startRouteDetails = GetRouteStartDetails(start);
            List<RouteDetail> endRouteDetails = GetRouteEndDetails(end);
            List<Int32> startRouteDetailIds = startRouteDetails.Select(x => x.Id).ToList();
            List<Int32> endRouteDetailIds = endRouteDetails.Select(x => x.Id).ToList();

            List<RouteRelation> currentRouteRelations = RouteRelations.ToList();
            if (DisableRouteIds != null && DisableRouteIds.Count() > 0)
                currentRouteRelations = currentRouteRelations.Where(x => !DisableRouteIds.Contains(x.Id) && !DisableRouteIds.Contains(x.Adjoins)).ToList();

            Boolean result = false;
            foreach (var routeHead in startRouteDetailIds)
            {
                if (result)
                    break;

                var currentRouteHead = RouteHelper.RouteHeads.First(x => x.HeadID == routeHead);
                if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                {
                    if (UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device) && UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].Contains(start.UnifiedCode))
                        continue;
                    if (currentRouteHead.PathOnlyEnds.Contains(start.UnifiedCode))
                        continue;
                }

                if (endRouteDetailIds.Contains(routeHead))
                {
                    if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                    {
                        if ((UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device) && UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].Contains(end.UnifiedCode)) || currentRouteHead.PathOnlyStarts.Contains(end.UnifiedCode))
                        {
                            routeHeadId_AbleArrived = routeHead;
                            stack_AbleArrived = new Stack<int>();
                            outRouteRelecatons_AbleArrived = new List<int>();
                            result = FindRoute(start, end, currentRouteRelations, routeHeadId_AbleArrived, outRouteRelecatons_AbleArrived, endRouteDetailIds, ref stack_AbleArrived);
                        }
                        else
                        {
                            RouteDetail startRouteDetail = startRouteDetails.First(x => x.Id == routeHead);
                            RouteDetail endRouteDetail = endRouteDetails.First(x => x.Id == routeHead);
                            if (startRouteDetail.DetailID <= endRouteDetail.DetailID)
                                return true;
                        }
                    }
                    else
                    {
                        RouteDetail startRouteDetail = startRouteDetails.First(x => x.Id == routeHead);
                        RouteDetail endRouteDetail = endRouteDetails.First(x => x.Id == routeHead);
                        if (startRouteDetail.DetailID <= endRouteDetail.DetailID)
                            return true;
                    }
                }
                else
                {
                    routeHeadId_AbleArrived = routeHead;
                    stack_AbleArrived = new Stack<int>();
                    outRouteRelecatons_AbleArrived = new List<int>();
                    result = FindRoute(start, end, currentRouteRelations, routeHeadId_AbleArrived, outRouteRelecatons_AbleArrived, endRouteDetailIds, ref stack_AbleArrived);
                }
            }

            return result;
        }
        /// <summary>
        /// 递归判断是否联通
        /// </summary>
        /// <param name="routeHeadId"></param>
        /// <param name="outRouteRelecatons"></param>
        /// <param name="endRouteDetailIds"></param>
        /// <param name="stack"></param>
        /// <returns></returns>
        private static bool FindRoute(Location start, Location end, List<RouteRelation> currentRouteRelations, int routeHeadId, List<Int32> outRouteRelecatons, List<Int32> endRouteDetailIds, ref Stack<Int32> stack)
        {
            List<RouteRelation> routeRelations = currentRouteRelations.Where(x => x.Id == routeHeadId && !outRouteRelecatons.Contains(x.RelationID)).ToList();
            if (routeRelations.Count() == 0)
                return false;
            else if (routeRelations.Any(x => endRouteDetailIds.Contains(x.Adjoins)))
            {
                //var _adjoins = routeRelations.Where(x => endRouteDetailIds.Contains(x.Adjoins)).Select(x=>x.Adjoins);
                //if (true)
                //{

                //}
                return true;
            }

        CHECK:
            stack.Push(routeHeadId);
            foreach (var item in routeRelations)
            {
                //if (endRouteDetails.Contains(item.Adjoins))
                //    return true;
                routeHeadId_AbleArrived = item.Adjoins;
                outRouteRelecatons.Add(item.RelationID);
                var result = FindRoute(start, end, currentRouteRelations, routeHeadId_AbleArrived, outRouteRelecatons, endRouteDetailIds, ref stack);
                if (result)
                    return true;
            }
            if (stack.Count() == 0)
                return false;

            routeHeadId = stack.Pop();
            return FindRoute(start, end, currentRouteRelations, routeHeadId, outRouteRelecatons, endRouteDetailIds, ref stack);
        }

        static Int32 routeHeadId_GetAllRouteIdSequences = 0;
        static Stack<Int32> stack_GetAllRouteIdSequences;
        static List<Stack<Int32>> statcks;
        static List<Int32> outRouteRelecatons_GetAllRouteIdSequences;
        static object _lockObj = new object();
        /// <summary>
        /// 获取两个Location的所有联通路径
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public static List<Stack<Int32>> GetAllRouteIdSequences(Location start, Location end, Task task = null)
        {
            lock (_lockObj)
            {
                int allowGropMax = 0;

            CalRouteIdSequences:
                List<RouteDetail> startRouteDetails = GetRouteStartDetails(start);
                List<RouteDetail> endRouteDetails = GetRouteEndDetails(end);
                List<Int32> startRouteDetailIds = startRouteDetails.Select(x => x.Id).ToList();
                List<Int32> endRouteDetailIds = endRouteDetails.Select(x => x.Id).ToList();
                List<RouteRelation> currentRouteRelations = RouteRelations.ToList();
                if (DisableRouteIds != null && DisableRouteIds.Count() > 0)
                {
                    startRouteDetailIds = startRouteDetailIds.Where(x => !DisableRouteIds.Contains(x)).ToList();
                    endRouteDetailIds = endRouteDetailIds.Where(x => !DisableRouteIds.Contains(x)).ToList();
                    currentRouteRelations = currentRouteRelations.Where(x => !DisableRouteIds.Contains(x.Id) && !DisableRouteIds.Contains(x.Adjoins)).ToList();
                }
                List<Stack<int>> _statcks = new List<Stack<int>>();

                foreach (var routeHead in startRouteDetailIds)
                {
                    var currentRouteHead = RouteHelper.RouteHeads.First(x => x.HeadID == routeHead);
                    if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                    {
                        if (UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device) && UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].Contains(start.UnifiedCode))
                            continue;
                        if (currentRouteHead.PathOnlyEnds.Contains(start.UnifiedCode))
                            continue;
                    }

                    statcks = new List<Stack<int>>();
                    if (endRouteDetailIds.Contains(routeHead))
                    {
                        if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                        {
                            if ((UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device) && UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].Contains(end.UnifiedCode)) || currentRouteHead.PathOnlyStarts.Contains(end.UnifiedCode))
                            {
                                stack_GetAllRouteIdSequences = new Stack<int>();
                                routeHeadId_GetAllRouteIdSequences = routeHead;
                                stack_GetAllRouteIdSequences.Push(routeHead);
                                outRouteRelecatons_GetAllRouteIdSequences = new List<int>();
                                if (task != null && task.Movements.Count() > 0)
                                {
                                    foreach (var item in task.Movements)
                                    {
                                        if (item.RouteId != null)
                                            outRouteRelecatons_GetAllRouteIdSequences.Add((int)item.RouteId);
                                    }
                                }
                                FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                            }
                            else
                            {
                                RouteDetail startRouteDetail = startRouteDetails.First(x => x.Id == routeHead);
                                RouteDetail endRouteDetail = endRouteDetails.First(x => x.Id == routeHead);
                                if (startRouteDetail.DetailID <= endRouteDetail.DetailID)
                                {
                                    Stack<Int32> _stack = new Stack<int>();
                                    _stack.Push(routeHead);
                                    statcks.Add(_stack);
                                }
                            }
                        }
                        else
                        {
                            RouteDetail startRouteDetail = startRouteDetails.First(x => x.Id == routeHead);
                            RouteDetail endRouteDetail = endRouteDetails.First(x => x.Id == routeHead);
                            if (startRouteDetail.DetailID <= endRouteDetail.DetailID)
                            {
                                Stack<Int32> _stack = new Stack<int>();
                                _stack.Push(routeHead);
                                statcks.Add(_stack);
                            }
                        }
                    }
                    else
                    {
                        stack_GetAllRouteIdSequences = new Stack<int>();
                        routeHeadId_GetAllRouteIdSequences = routeHead;
                        stack_GetAllRouteIdSequences.Push(routeHead);
                        outRouteRelecatons_GetAllRouteIdSequences = new List<int>();
                        if (task != null && task.Movements.Count() > 0)
                        {
                            foreach (var item in task.Movements)
                            {
                                if (item.RouteId != null)
                                    outRouteRelecatons_GetAllRouteIdSequences.Add((int)item.RouteId);
                            }
                        }
                        FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                    }

                    _statcks.AddRange(statcks);
                }

                if (_statcks.Count == 0 && allowGropMax < 3)
                {
                    allowGropMax += 1;
                    goto CalRouteIdSequences;
                }
                if (allowGropMax == 0)
                {
                    allowGropMax += 1;
                    goto CalRouteIdSequences;
                }

                return _statcks;
            }
        }

        private static void FindRoute(Location start, Location end, List<RouteRelation> currentRouteRelations, int routeHeadId, List<int> outRouteRelecatons, List<int> endRouteDetailIds, int allowGropMax, ref Stack<int> stack, ref List<Stack<int>> statcks)
        {
            List<RouteRelation> routeRelations = currentRouteRelations.Where(x => x.Id == routeHeadId && !outRouteRelecatons.Contains(x.RelationID)).ToList();
            if (routeRelations.Count() == 0)
            {
                if (stack.Count() == 0)
                    return;
                else
                    stack.Pop();
            }
            else
            {
                foreach (var item in routeRelations)
                {
                    ///去除循环路径
                    if (!stack.Contains(item.Adjoins))
                    {
                        var currentRouteHead = RouteHeads.FirstOrDefault(x => x.HeadID == routeHeadId);
                        var routeHead = RouteHeads.FirstOrDefault(x => x.HeadID == item.Adjoins);
                        if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                        {
                            if (UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device))
                            {
                                var array = UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].ToArray();
                                if (array.Contains(routeHead.Details.First().Path))
                                {
                                    outRouteRelecatons.Remove(item.RelationID);
                                    continue;
                                }
                            }
                            if (currentRouteHead.PathOnlyStarts.Contains(routeHead.Details.First().Path))
                            {
                                outRouteRelecatons.Remove(item.RelationID);
                                continue;
                            }
                        }
                        if (routeHead.Details.Count() == 1 && routeHead.Details.First().Path == routeHead.Device)
                        {
                            if (UnableSingltTaskDeviceUnableStationsDic.ContainsKey(routeHead.Device))
                            {
                                var array = UnableSingltTaskDeviceUnableStationsDic[routeHead.Device].ToArray();
                                if (array.Contains(currentRouteHead.Details.Last().Path))
                                {
                                    outRouteRelecatons.Remove(item.RelationID);
                                    continue;
                                }
                            }
                            if (routeHead.PathOnlyEnds.Contains(currentRouteHead.Details.Last().Path))
                            {
                                outRouteRelecatons.Remove(item.RelationID);
                                continue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(routeHead.Group) && stack.Count(x => RouteHeads.First(y => y.HeadID == x).Group == routeHead.Group) >= allowGropMax)
                        {
                        }
                        else
                        {
                            stack.Push(item.Adjoins);
                            outRouteRelecatons.Add(item.RelationID);
                            if (endRouteDetailIds.Contains(item.Adjoins))
                            {
                                var adjoinRouteHead = RouteHeads.FirstOrDefault(x => x.HeadID == item.Adjoins);
                                if (currentRouteHead.Details.Count() == 1 && currentRouteHead.Details.First().Path == currentRouteHead.Device)
                                {
                                    if ((UnableSingltTaskDeviceUnableStationsDic.ContainsKey(currentRouteHead.Device) && UnableSingltTaskDeviceUnableStationsDic[currentRouteHead.Device].Contains(end.UnifiedCode)) || currentRouteHead.PathOnlyStarts.Contains(end.UnifiedCode))
                                    {
                                        routeHeadId_GetAllRouteIdSequences = item.Adjoins;
                                        FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                                    }
                                    else
                                    {
                                        Stack<int> _stack = new Stack<int>(stack);
                                        statcks.Add(_stack);
                                        FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                                    }
                                }
                                else
                                {
                                    Stack<int> _stack = new Stack<int>(stack);
                                    statcks.Add(_stack);
                                    FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                                }
                            }
                            else
                            {
                                routeHeadId_GetAllRouteIdSequences = item.Adjoins;
                                FindRoute(start, end, currentRouteRelations, routeHeadId_GetAllRouteIdSequences, outRouteRelecatons_GetAllRouteIdSequences, endRouteDetailIds, allowGropMax, ref stack_GetAllRouteIdSequences, ref statcks);
                            }
                        }
                        outRouteRelecatons.Remove(item.RelationID);
                    }
                }
                stack.Pop();
            }
        }

        /// <summary>
        /// 获取List<Stack<Int32>>所有的位置序列
        /// </summary>
        /// <param name="routeIdsList"></param>
        /// <returns></returns>
        public static Dictionary<Stack<Int32>, List<String>> GetAllRouteLocationSequences(Location start, Location end, List<Stack<Int32>> routeIdsList)
        {
            Dictionary<Stack<Int32>, List<String>> _dics = new Dictionary<Stack<Int32>, List<string>>();
            foreach (var item in routeIdsList)
            {
                Stack<Int32> _stack = new Stack<Int32>(item.Reverse());
                List<String> _list = new List<string>();
                for (int i = 0; i < item.Count; i++)
                {
                    var routeId = _stack.Pop();
                    List<RouteDetail> routeDetails = new List<RouteDetail>();
                    routeDetails = RouteDetails.Where(x => x.Id == routeId).OrderBy(x => x.DetailID).ToList();
                    if (i == 0)
                    {
                        var index = routeDetails.FindIndex(x => (x.Path == start.DeviceCode || x.Path == start.Device.Name) && x.Device == start.Device.Name);
                        for (int j = index; j < routeDetails.Count(); j++)
                        {
                            _list.Add(routeDetails[j].ConvertibleCodeToLcation());
                        }
                    }
                    else if (i + 1 == item.Count)
                    {
                        var index = routeDetails.FindIndex(x => (x.Path == end.DeviceCode || x.Path == end.Device.Name) && x.Device == end.Device.Name);
                        for (int j = 0; j <= index; j++)
                        {
                            _list.Add(routeDetails[j].ConvertibleCodeToLcation());
                        }
                    }
                    else
                    {
                        _list.AddRange(routeDetails.Select(x => x.ConvertibleCodeToLcation()).ToArray());
                    }
                }
                _dics.Add(_stack, _list);
            }
            return _dics;
        }

        /// <summary>
        /// 获取Stack<Int32>所有的位置序列
        /// </summary>
        /// <param name="routeIdsList"></param>
        /// <returns></returns>
        public static List<String> GetOneRouteLocationSequences(Location start, Location end, Stack<Int32> routeIds)
        {
            Stack<Int32> _stack = new Stack<Int32>(routeIds.Reverse());
            List<String> _list = new List<string>();
            for (int i = 0; i < routeIds.Count; i++)
            {
                var routeId = _stack.Pop();
                List<RouteDetail> routeDetails = new List<RouteDetail>();
                routeDetails = RouteDetails.Where(x => x.Id == routeId).OrderBy(x => x.DetailID).ToList();
                if (routeDetails.Count() == 1)
                {
                    _list.Add(routeDetails.First().ConvertibleCodeToLcation());
                    continue;
                }
                if (i == 0)
                {
                    var index = routeDetails.FindIndex(x => (x.Path == start.DeviceCode || x.Path == start.Device.Name) && x.Device == start.Device.Name);
                    if (index == -1)
                    {
                        List<Location> _locations = new List<Location>();
                        _locations.Add(start);
                        _locations.AddRange(start.Synonymous);
                        _locations.AddRange(start.Synonymous.SelectMany(x => x.Synonymous).ToArray());
                        start = _locations.First(x => x.Device.Name == routeDetails.First().Device);
                        index = routeDetails.FindIndex(x => (x.Path == start.DeviceCode || x.Path == start.Device.Name) && x.Device == start.Device.Name);
                    }
                    for (int j = index; j < routeDetails.Count(); j++)
                    {
                        _list.Add(routeDetails[j].ConvertibleCodeToLcation());
                        if (routeDetails[j].Path == end.DeviceCode)
                            break;
                    }
                }
                else if (i + 1 == routeIds.Count)
                {
                    var index = routeDetails.FindIndex(x => (x.Path == end.DeviceCode || x.Path == end.Device.Name) && x.Device == end.Device.Name);
                    if (index == -1)
                    {
                        List<Location> _locations = new List<Location>();
                        _locations.Add(end);
                        _locations.AddRange(end.Synonymous);
                        _locations.AddRange(end.Synonymous.SelectMany(x => x.Synonymous).ToArray());
                        end = _locations.First(x => x.Device.Name == routeDetails.First().Device);
                        index = routeDetails.FindIndex(x => (x.Path == end.DeviceCode || x.Path == end.Device.Name) && x.Device == end.Device.Name);
                    }

                    for (int j = 0; j <= index; j++)
                    {
                        _list.Add(routeDetails[j].ConvertibleCodeToLcation());
                        if (routeDetails[j].Path == end.DeviceCode)
                            break;
                    }
                }
                else
                {
                    _list.AddRange(routeDetails.Select(x => x.ConvertibleCodeToLcation()).ToArray());
                }
            }
            return _list;
        }

        //public static LogicMovement GetLogicMovement(Location start, Location end, Int32 routeId)
        //{
        //    RouteDetail[] routeDetails = RouteDetails.Where(x => x.Id == routeId).OrderBy(x => x.DetailID).ToArray();
        //}

        /// <summary>
        /// 根据位置信息定位到路径明细表中具体的明细
        /// </summary>
        /// <param name="loc">位置信息</param>
        /// 说明：输送线取deviceCode和设备名 堆垛机等单任务多位置设备由于Path采用其设备名称，因此取设备名等于Path的
        /// <returns></returns>
        private static List<RouteDetail> GetRouteStartDetails(Location loc)
        {
            List<RouteDetail> result = new List<RouteDetail>();
            var _routeDetails = RouteDetails.Where(x => (x.Path == loc.DeviceCode || x.Path == loc.Device.Name) && x.Device == loc.Device.Name && !DisableRouteIds.Contains(x.Id)).ToList();
            //兼容区域概念
            var _details = RouteDetails.Where(x => x.Device == loc.Device.Name && x.Path.EndsWith($"@{loc.Device.Name}")).ToList();
            if (_details.Count() > 0)
            {
                foreach (var dtail in _details)
                {
                    try
                    {
                        var _loc = LocationConverter.ConvertibleCodeToLcation(dtail.Path);
                        if (_loc is ILocationWildcard wildcard && wildcard.GetMatchedLocations().Any(x => x.UserCode == loc.UserCode))
                            _routeDetails.Add(dtail);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            //兼容区域概念
            foreach (var item in _routeDetails)
            {
                var _routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == item.Id);
                if (_routeHead.Details.Count() == 1)
                {
                    result.Add(item);
                    continue;
                }
                if (_routeHead.Details.OrderBy(x => x.DetailID).Last().Path != loc.DeviceCode)
                {
                    result.Add(item);
                    continue;
                }
            }

            foreach (var item in loc.Synonymous)
            {
                _routeDetails = RouteDetails.Where(x => (x.Path == item.DeviceCode || x.Path == item.Device.Name) && x.Device == item.Device.Name).ToList();

                //兼容区域概念
                _details = RouteDetails.Where(x => x.Device == item.Device.Name && x.Path.EndsWith($"@{item.Device.Name}")).ToList();
                if (_details.Count() > 0)
                {
                    foreach (var dtail in _details)
                    {
                        try
                        {
                            var _loc = LocationConverter.ConvertibleCodeToLcation(dtail.Path);
                            if (_loc is ILocationWildcard wildcard && wildcard.GetMatchedLocations().Any(x => x.UserCode == item.UserCode))
                                _routeDetails.Add(dtail);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                //兼容区域概念

                foreach (var detail in _routeDetails)
                {
                    var _routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == detail.Id);
                    if (_routeHead.Details.Count() == 1)
                    {
                        result.Add(detail);
                        continue;
                    }
                    if (_routeHead.Details.OrderBy(x => x.DetailID).Last().Path != item.DeviceCode)
                    {
                        result.Add(detail);
                        continue;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 根据位置信息定位到路径明细表中具体的明细
        /// </summary>
        /// <param name="loc">位置信息</param>
        /// 说明：输送线取deviceCode和设备名 堆垛机等单任务多位置设备由于Path采用其设备名称，因此取设备名等于Path的
        /// <returns></returns>
        private static List<RouteDetail> GetRouteEndDetails(Location loc)
        {
            List<RouteDetail> result = new List<RouteDetail>();
            var _routeDetails = RouteDetails.Where(x => (x.Path == loc.DeviceCode || x.Path == loc.Device.Name) && x.Device == loc.Device.Name && !DisableRouteIds.Contains(x.Id)).ToList();
            //兼容区域概念
            var _details = RouteDetails.Where(x => x.Device == loc.Device.Name && x.Path.EndsWith($"@{loc.Device.Name}")).ToList();
            if (_details.Count() > 0)
            {
                foreach (var dtail in _details)
                {
                    try
                    {
                        var _loc = LocationConverter.ConvertibleCodeToLcation(dtail.Path);
                        if (_loc is ILocationWildcard wildcard && wildcard.GetMatchedLocations().Any(x => x.UserCode == loc.UserCode))
                            _routeDetails.Add(dtail);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            //兼容区域概念
            foreach (var item in loc.Synonymous)
            {
                _routeDetails.AddRange(RouteDetails.Where(x => (x.Path == item.DeviceCode || x.Path == item.Device.Name) && x.Device == item.Device.Name));

                //兼容区域概念
                _details = RouteDetails.Where(x => x.Device == item.Device.Name && x.Path.EndsWith($"@{item.Device.Name}")).ToList();
                if (_details.Count() > 0)
                {
                    foreach (var dtail in _details)
                    {
                        try
                        {
                            var _loc = LocationConverter.ConvertibleCodeToLcation(dtail.Path);
                            if (_loc is ILocationWildcard wildcard && wildcard.GetMatchedLocations().Any(x => x.UserCode == item.UserCode))
                                _routeDetails.Add(dtail);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                //兼容区域概念
            }

            foreach (var item in _routeDetails)
            {
                var _routeHead = RouteHelper.RouteHeads.FirstOrDefault(x => x.HeadID == item.Id);
                if (_routeHead.Details.Count() == 1)
                {
                    result.Add(item);
                    continue;
                }
                if (_routeHead.Details.OrderBy(x => x.DetailID).First().Path != loc.DeviceCode)
                {
                    result.Add(item);
                    continue;
                }
            }

            return result;
        }

        public static ExcelAddress ExportToExcel()
        {
            string path = System.Windows.Forms.Application.StartupPath + "\\" + _path;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            var file = directoryInfo.GetFiles().FirstOrDefault(x => x.FullName.Contains(_basicDataExcelName));
            String fileFullName;
            if (file == null)
            {
                var _result = MessageBox.Show(String.Format("未读取到配置文件 {0}.xlsx ,是否新建该文件？", _basicDataExcelName), "未查询到文件", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (_result == DialogResult.No)
                    return null;
                String _newFileName = 输入文件名称.Run(_basicDataExcelName);
                fileFullName = path + "\\" + _newFileName + ".xlsx";
            }
            else
                fileFullName = path + "\\" + _basicDataExcelName + ".xlsx";

            DataTableToExcel(fileFullName);

            return null;
        }

        private static void DataTableToExcel(String fullPath)
        {
            var routes = RouteHeads.Select(x =>
                                new _routes()
                                {
                                    HeadId = x.HeadID.ToString(),
                                    ProjectCode = x.ProjectCode,
                                    ProjectName = x.ProjectName,
                                    No = x.No.ToString(),
                                    Direction = x.Direction,
                                    Device = x.Device,
                                    RouteVsActionRelation = ((int)x.RouteVsActionRelation).ToString(),
                                    AllowStartFromMidway = x.AllowStartFromMidway.ToString(),
                                    Path = String.Join(",", x.Details.OrderBy(y => y.DetailID).Select(y => y.Path)),
                                    Adjoins = x.Relations == null || x.Relations.Count() == 0 ? "" : String.Join(",", x.Relations.Select(y => y.Adjoins).OrderBy(z => z)),
                                    Group = x.Group,
                                    PathOnlyStarts = x.PathOnlyStarts,
                                    PathOnlyEnds = x.PathOnlyEnds
                                }).ToList();

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.Add(_basicXMLName);
                int i = 1;
                ///写入表头
                sheet.Cells[i, 1].Value = "HeadId";
                sheet.Cells[i, 2].Value = "ProjectCode";
                sheet.Cells[i, 3].Value = "ProjectName";
                sheet.Cells[i, 4].Value = "No";
                sheet.Cells[i, 5].Value = "Direction";
                sheet.Cells[i, 6].Value = "Device";
                sheet.Cells[i, 7].Value = "RouteVsActionRelation";
                sheet.Cells[i, 8].Value = "AllowStartFromMidway";
                sheet.Cells[i, 9].Value = "Path";
                sheet.Cells[i, 10].Value = "Adjoins";
                sheet.Cells[i, 11].Value = "Group";
                sheet.Cells[i, 12].Value = "PathOnlyStarts";
                sheet.Cells[i, 13].Value = "PathOnlyEnds";

                foreach (var item in routes)
                {
                    ++i;
                    sheet.Cells[i, 1].Value = item.HeadId;
                    sheet.Cells[i, 2].Value = item.ProjectCode;
                    sheet.Cells[i, 3].Value = item.ProjectName;
                    sheet.Cells[i, 4].Value = item.No;
                    sheet.Cells[i, 5].Value = item.Direction;
                    sheet.Cells[i, 6].Value = item.Device;
                    sheet.Cells[i, 7].Value = item.RouteVsActionRelation;
                    sheet.Cells[i, 8].Value = item.AllowStartFromMidway;
                    sheet.Cells[i, 9].Value = item.Path;
                    sheet.Cells[i, 10].Value = item.Adjoins;
                    sheet.Cells[i, 11].Value = item.Group;
                    sheet.Cells[i, 12].Value = item.PathOnlyStarts;
                    sheet.Cells[i, 13].Value = item.PathOnlyEnds;
                }

                ExcelWorksheet sheetErrorType = package.Workbook.Worksheets.Add(_basicXMLDeviceErrorType);
                int j = 1;
                //写入表头
                sheetErrorType.Cells[j, 1].Value = "Id";
                sheetErrorType.Cells[j, 2].Value = "AlarmCategory";
                sheetErrorType.Cells[j, 3].Value = "DeviceType";
                sheetErrorType.Cells[j, 4].Value = "DeviceErrorCode";
                sheetErrorType.Cells[j, 5].Value = "ErrorName";
                sheetErrorType.Cells[j, 6].Value = "Description";
                sheetErrorType.Cells[j, 7].Value = "Levle";
                sheetErrorType.Cells[j, 8].Value = "Solution";
                sheetErrorType.Cells[j, 9].Value = "IsFault";

                foreach (var item in DeviceErrorHelper.DeviceErrorTypes.OrderBy(x => x.Id))
                {
                    ++j;
                    sheetErrorType.Cells[j, 1].Value = item.Id;
                    sheetErrorType.Cells[j, 2].Value = item.DeviceType;
                    sheetErrorType.Cells[j, 3].Value = item.DeviceErrorCode;
                    sheetErrorType.Cells[j, 4].Value = item.ErrorName;
                    sheetErrorType.Cells[j, 5].Value = item.Levle;
                    sheetErrorType.Cells[j, 6].Value = item.Solution;
                    sheetErrorType.Cells[j, 7].Value = item.IsFault.ToString();
                }

                using (Stream stream = new FileStream(fullPath, FileMode.Create))
                {
                    package.SaveAs(stream);
                }
            }
        }



        static List<string> routeExternalPlugins = null;
        /// <summary>
        /// 路径扩展外挂
        /// </summary>
        public static List<string> RouteExternalPlugins
        {
            get
            {
                if (routeExternalPlugins == null)
                    routeExternalPlugins = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection.GetSetting<string>("RouteExternalPlugin", null).Replace("\r\n", "").Split(';').Select(x => x.Trim()).ToList();

                return routeExternalPlugins;
            }
            set
            { routeExternalPlugins = null; }
        }
    }

    public class _routes
    {
        public String HeadId;
        public String ProjectCode;
        public String ProjectName;
        public String No;
        public String Direction;
        public String Device;
        public String RouteVsActionRelation;
        public String AllowStartFromMidway;
        public String Path;
        public String Adjoins;
        public string Group;
        public string PathOnlyStarts;
        public string PathOnlyEnds;
    }
}
