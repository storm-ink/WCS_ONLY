using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace BOE.设备相关.AGV.GSAGV
{
    public static class GSAGVDatabaseHand
    {
        static Logger _logger = LogManager.CreateNullLogger();

        public static Boolean MiddleDatabaseState = false;

        /// <summary>
        /// 连接字符串
        /// </summary>
        static String GSAGVZJBConnectionString
        {
            get
            {
                return Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["GSAGV中间表"];
            }
        }


        /// <summary>
        /// 插入任务到 中间表
        /// </summary>
        /// <returns></returns>
        public static bool InterTaskToAGV(AGV_T_interface model)
        {
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSql1 = new StringBuilder();
            StringBuilder strSql2 = new StringBuilder();
            if (model.ID != null)
            {
                strSql1.Append("ID,");
                strSql2.Append("" + model.ID + ",");
            }
            if (model.interCode != null)
            {
                strSql1.Append("interCode,");
                strSql2.Append("'" + model.interCode + "',");
            }
            if (model.beginLoc != null)
            {
                strSql1.Append("beginLoc,");
                strSql2.Append("'" + model.beginLoc + "',");
            }
            if (model.beginLevel != null)
            {
                strSql1.Append("beginLevel,");
                strSql2.Append("'" + model.beginLevel + "',");
            }
            if (model.endLoc != null)
            {
                strSql1.Append("endLoc,");
                strSql2.Append("'" + model.endLoc + "',");
            }
            if (model.endLevel != null)
            {
                strSql1.Append("endLevel,");
                strSql2.Append("'" + model.endLevel + "',");
            }
            if (model.interType != null)
            {
                strSql1.Append("interType,");
                strSql2.Append("'" + model.interType + "',");
            }
            if (model.state != null)
            {
                strSql1.Append("state,");
                strSql2.Append("'" + model.state + "',");
            }
            if (model.createDatetime != null)
            {
                strSql1.Append("createDatetime,");
                strSql2.Append("'" + model.createDatetime + "',");
            }
            if (model.useDatetime != null)
            {
                strSql1.Append("useDatetime,");
                strSql2.Append("'" + model.useDatetime + "',");
            }
            if (model.finishDatetime != null)
            {
                strSql1.Append("finishDatetime,");
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
                strSql1.Append("agvId,");
                strSql2.Append("" + model.agvId + ",");
            }
            if (model.SalverType != null)
            {
                strSql1.Append("salverType,");
                strSql2.Append("" + model.SalverType + ",");
            }
            if (model.TrayCode != null)
            {
                strSql1.Append("TrayCode,");
                strSql2.Append("" + model.TrayCode + ",");
            }
            strSql.Append("insert into t_interface(");
            strSql.Append(strSql1.ToString().Remove(strSql1.Length - 1));
            strSql.Append(")");
            strSql.Append(" values (");
            strSql.Append(strSql2.ToString().Remove(strSql2.Length - 1));
            strSql.Append(")");
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
            con.ConnectionString = GSAGVZJBConnectionString;
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
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
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
        public static int[] QueryEquipmentTaskIDs()
        {
            List<int> equipmentTaskIds = new List<int>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
                return null;
            }

            String sqlStr = @"select interCode from [t_interface] group by ID";

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库查询已有任务号失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
                return null;
            }

            foreach (DataRow item in dataTables.Rows)
            {
                equipmentTaskIds.Add(Convert.ToInt32(item["interCode"]));
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
        //  根据任务状态和读写标志位读取任务号
        /// </summary>
        /// <returns></returns>
        public static String[] QueryEquipmentTaskState(EquipmentTaskStatus state)
        {
            List<String> equipmentTaskIds = new List<String>();
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false; ;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return null;
            }

            String sqlStr = String.Format(@"select [interCode] from [t_interface] where [state] = '{0}' group by [interCode]", Convert.ToInt32(state));

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库查询-->根据任务状态和读写标志位查询任务号失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");

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
                equipmentTaskIds.Add(item["interCode"].ToString().Trim());
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
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");

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
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
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
        /// 检查所生成任务号在GSAGV中间表中是否存在
        /// </summary>
        /// <param name="sendAGVTaskId"></param>
        /// <returns></returns>
        public static Boolean CheckAgvTaskIdIsExict(String sendAGVTaskId)
        {
            Boolean isExist = false;
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("检查所生成任务号在GSAGV中间表中是否存在时，数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");

                if (conn != null)
                {
                    conn.Dispose();
                }

                return false;
            }

            String sqlStr = String.Format(@"select Count(*) [interCode] from [ZJB].[dbo].[t_interface] where [interCode] = '{0}'", sendAGVTaskId);

            SqlDataAdapter adapter = new SqlDataAdapter(sqlStr, conn);
            DataTable dataTables = new DataTable();
            try
            {
                adapter.Fill(dataTables);
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
                throw ex;
            }

            foreach (DataRow item in dataTables.Rows)
            {
                isExist = Convert.ToInt32(item["interCode"]) != 0;
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
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
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
                _logger.Error(String.Format("数据库查询已有任务号失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
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
            conn.ConnectionString = GSAGVZJBConnectionString;
            try
            {
                conn.Open();
                MiddleDatabaseState = true;
            }
            catch (Exception ex)
            {
                MiddleDatabaseState = false;
                _logger.Error(String.Format("数据库连接失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");

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
                _logger.Error(String.Format("数据库更新失败，异常信息：{0}", ex.Message), "GSAGVDatabaseHand");
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

        public static DataTable GetAllTaskToZJB()
        {
            string sqlText = @"SELECT TOP 1000 [ID]
      ,[interCode]
      ,[beginLoc]
      ,[beginLevel]
      ,[endLoc]
      ,[endLevel]
      ,[interType]
      ,[state]
      ,[createDatetime]
      ,[useDatetime]
      ,[finishDatetime]
      ,[remark]
      ,[category]
      ,[agvId]
      ,[sort]
      ,[SalverType]
      ,[TrayCode]
  FROM t_interface";

            return ExecuteDataset(CommandType.Text, sqlText).Tables[0];
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
        public static DataSet ExecuteDataset(CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            // Create & open a SqlConnection, and dispose of it after we are done
            using (SqlConnection connection = new SqlConnection(GSAGVZJBConnectionString))
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

    }
}
