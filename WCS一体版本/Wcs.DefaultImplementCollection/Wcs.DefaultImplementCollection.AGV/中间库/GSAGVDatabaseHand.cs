using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Wcs.DefaultImplementCollection.AGV
{
    public static class SinevaAGVDatabaseHand
    {
        static Logger _logger = LogManager.CreateNullLogger();

        public static Boolean MiddleDatabaseState = false;

        /// <summary>
        /// 连接字符串
        /// </summary>
        static String SinevaAGVZJBConnectionString
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["AGV中间表"];
            }
           
        }

        public static String getZJBConnection(String device)
        {
            if (device == "AGV调度系统")
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["AGV中间表"];
            }
            else if (device == "YFAGV调度系统")
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["AGV中间表"];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 插入任务到 中间表
        /// </summary>
        /// <returns></returns>
        public static bool InterTaskToAGV(AGV_T_interface model, String device)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSql1 = new StringBuilder();
            StringBuilder strSql2 = new StringBuilder();
            if (model.ID != null)
            {
                strSql1.Append("id,");
                strSql2.Append("" + model.ID + ",");
            }
            if (model.interCode != null)
            {
                strSql1.Append("intercode,");
                strSql2.Append("'" + model.interCode + "',");
            }
            if (model.begionLoc != null)
            {
                strSql1.Append("begionloc,");
                strSql2.Append("'" + model.begionLoc + "',");
            }
            if (model.begionLevel != null)
            {
                strSql1.Append("begionloc_level,");
                strSql2.Append("'" + model.begionLevel + "',");
            }
            if (model.endLoc != null)
            {
                strSql1.Append("endloc,");
                strSql2.Append("'" + model.endLoc + "',");
            }
            if (model.endLevel != null)
            {
                strSql1.Append("endloc_level,");
                strSql2.Append("'" + model.endLevel + "',");
            }
            if (model.interType != null)
            {
                strSql1.Append("intertype,");
                strSql2.Append("'" + model.interType + "',");
            }
            if (model.state != null)
            {
                strSql1.Append("state,");
                strSql2.Append("'" + model.state + "',");
            }
            if (model.createDatetime != null)
            {
                strSql1.Append("createdatetime,");
                strSql2.Append("'" + model.createDatetime + "',");
            }
            if (model.useDatetime != null)
            {
                strSql1.Append("usedatetime,");
                strSql2.Append("'" + model.useDatetime + "',");
            }
            if (model.finishDatetime != null)
            {
                strSql1.Append("finishdatetime,");
                strSql2.Append("'" + model.finishDatetime + "',");
            }
            if (model.remark != null)
            {
                strSql1.Append("remark,");
                strSql2.Append("'" + model.remark + "',");
            }
            if (model.sort != null)
            {
                strSql1.Append("sort,");
                strSql2.Append("'" + model.sort + "',");
            }
            if (model.agvId != null)
            {
                strSql1.Append("agvid,");
                strSql2.Append("" + model.agvId + ",");
            }
            if (model.SalverType != null)
            {
                strSql1.Append("salvertype,");
                strSql2.Append("" + model.SalverType + ",");
            }
            if (model.category != null)
            {
                strSql1.Append("category,");
                strSql2.Append("" + model.category + ",");
            }
            if (model.TrayCode != null)
            {
                strSql1.Append("traycode,");
                strSql2.Append("" + model.TrayCode + ",");
            }
            //if (model.BinterCode != null)
            //{
            //    strSql1.Append("BinterCode,");
            //    strSql2.Append("'" + model.BinterCode + "',");
            //}
            strSql.Append("insert into  [ZJB].[dbo].t_interface(");
            strSql.Append(strSql1.ToString().Remove(strSql1.Length - 1));
            strSql.Append(")");
            strSql.Append(" values (");
            strSql.Append(strSql2.ToString().Remove(strSql2.Length - 1));
            strSql.Append(")");
            bool exists = SinevaAGVDatabaseHand.CheckAgvTaskIdIsExict(model.interCode, "AGV调度系统");
            if (exists)
            {
                return true;
            }
            int rows = ExceuteCommandCount(CommandType.Text, strSql.ToString(), null);
            if (rows > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int delete(string sql ) 
        {

            int rows = ExceuteCommandCount(CommandType.Text, sql, null);
            return rows;
        }
        /// <summary>
        /// 执行一个增、删、改语句或存储过程
        /// </summary>
        /// <param name="cmdtype">执行语句的类型</param>
        /// <param name="cmdText">执行语句</param>
        /// <param name="parm">参数名称</param>
        /// <returns>返回受影响的行数</returns>
        public static int ExceuteCommandCount(CommandType cmdtype, String cmdText, SqlParameter[] parm)
        {
            int count = 0;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = SinevaAGVZJBConnectionString;
            if (con.State != ConnectionState.Open)
            {
                con.Close();
                con.Open();
            }

            try
            {
               

                




                SqlCommand cmd = new SqlCommand(cmdText, con);
                cmd.CommandType = cmdtype;
                if (parm != null && parm.Length > 0)
                {
                    foreach (SqlParameter p in parm)
                    {
                        cmd.Parameters.Add(p);
                    }
                }
                count = cmd.ExecuteNonQuery();
                return count;
            }
            catch (Exception ex)
            {
                count = 0;
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                return count;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
        }

        /// <summary>
        /// 查询已使用任务号
        /// </summary>
        /// <returns></returns>
        public static int[] QueryEquipmentTaskIDs(String device)
        {
            List<int> equipmentTaskIds = new List<int>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = getZJBConnection(device);
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                return null;
            }

            String sqlStr = @"select intercode from [ZJB].[dbo].[t_interface] group by intercode";

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库查询已有任务号失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                return null;
            }

            foreach (DataRow item in dataTables.Rows)
            {
                Int32 _interCode;
                if (Int32.TryParse(item["intercode"].ToString().Trim(),out _interCode))
                    equipmentTaskIds.Add(_interCode);
            }

            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }
            return equipmentTaskIds.Distinct().ToArray();
        }

        /// <summary>
        //  根据任务状态和读写标志位读取任务号
        /// </summary>
        /// <returns></returns>
        public static String[] QueryEquipmentTaskState(EquipmentTaskStatus state, String device)
        {
            List<String> equipmentTaskIds = new List<String>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = getZJBConnection(device);
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false; ;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return null;
            }

            String sqlStr = String.Format(@"select [intercode] from [ZJB].[dbo].[t_interface] where [state] = '{0}' group by [intercode]", Convert.ToInt32(state));

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库查询-->根据任务状态和读写标志位查询任务号失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (adapter != null)
                {
                    adapter.Dispose();
                }

                if (conn != null)
                {
                    conn.Dispose();
                }
                return null;
            }

            foreach (DataRow item in dataTables.Rows)
            {
                equipmentTaskIds.Add(item["intercode"].ToString().Trim());
            }

            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }
            return equipmentTaskIds.ToArray();
        }

        /// <summary>
        /// 更新Synchro标志位
        /// </summary>
        /// <param name="synchro"></param>
        /// <returns></returns>
        public static Boolean UpdateSynchro(String taskNo, EquipmentTaskStatus state, Synchro synchro)
        {
            List<int> equipmentTaskIds = new List<int>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = SinevaAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return true;
            }

            String sqlStr = String.Format(@"update TN_SYS_TASK_RET_STATE
                                            set CN_N_SYNCHRO = '{0}'
                                            where CN_S_TASK_NO = '{1}' and CN_N_STATE = '{2}'", Convert.ToInt32(synchro), taskNo, Convert.ToInt32(state));

            SqlCommand sqlCommand = new SqlCommand(sqlStr, conn);
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
            }

            if (sqlCommand != null)
            {
                sqlCommand.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }

            return true;
        }

        /// <summary>
        /// 检查所生成任务号在SinevaAGV中间表中是否存在
        /// </summary>
        /// <param name="sendAGVTaskId"></param>
        /// <returns></returns>
        public static Boolean CheckAgvTaskIdIsExict(String sendAGVTaskId, String device)
        {
            Boolean isExist = false;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = getZJBConnection(device);
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("检查所生成任务号在SinevaAGV中间表中是否存在时，数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return false;
            }

            //String sqlStr = String.Format(@"select Count(*) [intercode] from [ZJB].[dbo].[t_interface] where [intercode] = '{0}'", sendAGVTaskId);
            String sqlStr = String.Format(@"select Count(*) [intercode] from [ZJB].[dbo].[t_interface] where [intercode] like '%{0}%'", sendAGVTaskId);

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                throw ex;
            }

            foreach (DataRow item in dataTables.Rows)
            {
                isExist = Convert.ToInt32(item["intercode"]) != 0;
            }

            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }

            return isExist;
        }

        /// <summary>
        /// 检查数据库中是否有符合state的数据
        /// </summary>
        /// <param name="agv到提升机入口判断"></param>
        /// <returns></returns>
        public static string CheckAgvStateIsExist(State state)
        {
            Boolean isExist = false;
            string equipmentTaskIds = null;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = SinevaAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                return null;
            }

            String sqlStr = string.Format(@"select CN_N_STATE,CN_GUID from TN_SYS_STN_TRAFFIC_CTRL where CN_N_STATE={0}", Convert.ToInt32(state));
            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库查询已有任务号失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                return null;
            }
            foreach (DataRow item in dataTables.Rows)
            {
                equipmentTaskIds = (item["CN_GUID"].ToString().Trim());
            }
            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }
            return equipmentTaskIds;
        }
        /// <summary>
        /// 修改状态数据
        /// </summary>
        /// <param name="agv到提升机入口判断"></param>
        /// <returns></returns>
        /// 
        public static Boolean UpdateState(string taskid, State state1, State state2)
        {
            SqlConnection conn = new SqlConnection();
            Boolean isExist = false;
            List<int> equipmentTaskIds = new List<int>();
            conn.ConnectionString = SinevaAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return true;
            }

            String sqlStr = String.Format(@"update TN_SYS_STN_TRAFFIC_CTRL set CN_N_STATE = '{0}' where CN_N_STATE = '{1}' and CN_GUID='{2}'", Convert.ToInt32(state2), Convert.ToInt32(state1), taskid);
            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                throw ex;
            }
            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }
            return isExist;
        }

        public static void DeleteCompeletedTask(String taskId)
        {
            String sqlStr = String.Format(@"DELETE FROM [ZJB].[dbo].[t_interface] where State = 3 and Intercode = '{0}'", taskId);
            ExceuteCommandCount(CommandType.Text, sqlStr, null);
        }

        public static DataTable GetAllTaskToZJB(String device)
        {
            string sqlText = @"SELECT TOP 1000[id],
                             [intercode],
                             [begionloc],
                             [begionloc_level],
                             [endloc],[endloc_level],
                             [intertype],
                             case [state] when '1' then '新任务'
                                          when '2' then '执行中'
                                          when '3' then '完成'
                                          when '4' then '已下发AGV系统'
                                          when '5' then '错误' end as state
                            
                             ,[createdatetime],
                             [usedatetime],
                             [finishdatetime],
                             [remark],[grade],
                             [category],[sort],
                             [agvid],
                             [salvertype],
                             [traycode] FROM[ZJB].[dbo].[t_interface]";

            //return ExecuteDataset(CommandType.Text, device, sqlText).Tables[0];
            return ExecuteDataset(CommandType.Text, sqlText, device).Tables[0];
        }


        public static DataTable HisGetAllTaskToZJB(String device)
        {
            string sqlText = @"SELECT TOP 1000[id],
                             [intercode],
                             [begionloc],
                             [begionloc_level],
                             [endloc],[endloc_level],
                             [intertype],
                             case [state] when '1' then '新任务'
                                          when '2' then '执行中'
                                          when '3' then '完成'
                                          when '4' then '已下发AGV系统'
                                          when '5' then '错误' end as state
                            
                             ,[createdatetime],
                             [usedatetime],
                             [finishdatetime],
                             [remark],[grade],
                             [category],[sort],
                             [agvid],
                             [salvertype],
                             [traycode] FROM[ZJB].[dbo].[t_interface_bak]";

          
            return ExecuteDataset(CommandType.Text, sqlText, device).Tables[0];
        }






        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the database specified in the connection string 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(connString, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(CommandType commandType, string commandText, String device, params SqlParameter[] commandParameters)
        {
            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(getZJBConnection(device)))
            {
                connection.Open();

                // Call the overload that takes a connection in place of the connection string
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }
        /// <summary>
        /// Execute a SqlCommand (that returns a resultset) against the specified SqlConnection 
        /// using the provided parameters.
        /// </summary>
        /// <remarks>
        /// e.g.:  
        ///  DataSet ds = ExecuteDataset(conn, CommandType.StoredProcedure, "GetOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <param name="connection">A valid SqlConnection</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParamters used to execute the command</param>
        /// <returns>A dataset containing the resultset generated by the command</returns>
        public static DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            // Create a command and prepare it for execution
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // Create the DataAdapter & DataSet
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();

                // Fill the DataSet using default values for DataTable names, etc
                da.Fill(ds);

                // Detach the SqlParameters from the command object, so they can be used again
                cmd.Parameters.Clear();

                if (mustCloseConnection)
                    connection.Close();

                // Return the dataset
                return ds;
            }
        }
        /// <summary>
        /// This method opens (if necessary) and assigns a connection, transaction, command type and parameters 
        /// to the provided command
        /// </summary>
        /// <param name="command">The SqlCommand to be prepared</param>
        /// <param name="connection">A valid SqlConnection, on which to execute this command</param>
        /// <param name="transaction">A valid SqlTransaction, or 'null'</param>
        /// <param name="commandType">The CommandType (stored procedure, text, etc.)</param>
        /// <param name="commandText">The stored procedure name or T-SQL command</param>
        /// <param name="commandParameters">An array of SqlParameters to be associated with the command or 'null' if no parameters are required</param>
        /// <param name="mustCloseConnection"><c>true</c> if the connection was opened by the method, otherwose is false.</param>
        private static void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");

            // If the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }

            // Associate the connection with the command
            command.Connection = connection;

            // Set the command text (stored procedure name or SQL statement)
            command.CommandText = commandText;

            // If we were provided a transaction, assign it
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }

            // Set the command type
            command.CommandType = commandType;

            // Attach the command parameters if they are provided
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }
        /// <summary>
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        /// 
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.  
        /// 
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        private static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (SqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        static List<AGVHoldSingle> _agvHoldSingles;
        public static List<AGVHoldSingle> AGVHoldSingle
        {
            get
            {
                if (_agvHoldSingles == null || _agvHoldSingles.Count() == 0)
                {
                    var dt = GetAllAGVHoldSingle();
                    _agvHoldSingles = dt.AsEnumerable().Select(x => new AGVHoldSingle() { DeviceCode = x.Field<String>("DeviceCode"), HaveGoods = x.Field<Boolean>("HaveGoods") }).ToList();
                }

                return _agvHoldSingles;
            }
        }

        public static DataTable GetAllAGVHoldSingle()
        {
            List<String> equipmentTaskIds = new List<String>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = SinevaAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false; ;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return null;
            }

            String sqlStr = @"select * from [HoldSignal]";

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("占位信息获取失败", ex.Message), "SinevaAGVDatabaseHand");

                if (adapter != null)
                {
                    adapter.Dispose();
                }

                if (conn != null)
                {
                    conn.Dispose();
                }
                return null;
            }

            if (adapter != null)
            {
                adapter.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }
            return dataTables;
        }

        public static Boolean SaveOrUpdateAGV_HoldSingle(String deviceCode, Boolean haveGoods)
        {
            Boolean result = false;
            SqlConnection conn = new SqlConnection();
            List<int> equipmentTaskIds = new List<int>();
            conn.ConnectionString = SinevaAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return result;
            }

            String sqlStr;
            if (!_agvHoldSingles.Any(x => x.DeviceCode == deviceCode))
                sqlStr = String.Format(@"INSERT INTO [ZJB].[dbo].[HoldSignal] VALUES ('{0}',{1})", deviceCode, Convert.ToInt16(haveGoods));
            else
                sqlStr = String.Format(@"UPDATE [ZJB].[dbo].[HoldSignal] SET HAVEGOODS = {0} WHERE DEVICECODE = '{1}'", Convert.ToInt16(haveGoods), deviceCode);

            SqlCommand command = new SqlCommand(sqlStr, conn);
            try
            {
                command.ExecuteNonQuery();
                result = true;
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "SinevaAGVDatabaseHand");
                throw ex;
            }
            if (command != null)
            {
                command.Dispose();
            }

            if (conn != null)
            {
                conn.Dispose();
            }

            if (result)
            {
                var _agvHoldSingle = _agvHoldSingles.FirstOrDefault(x => x.DeviceCode == deviceCode);
                if (_agvHoldSingles == null)
                    _agvHoldSingles.Add(new Wcs.DefaultImplementCollection.AGV.AGVHoldSingle() { DeviceCode = deviceCode, HaveGoods = haveGoods });
                else if (_agvHoldSingle.HaveGoods != haveGoods)
                    _agvHoldSingle.HaveGoods = haveGoods;
            }

            return result;
        }
    }

    public class AGVHoldSingle
    {
        public string DeviceCode { get; set; }
        public Boolean HaveGoods { get; set; }
    }
}
