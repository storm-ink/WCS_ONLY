using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using System.Linq.Expressions;
using NHibernate.Cfg;
using System.Runtime.CompilerServices;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using System.Text.RegularExpressions;

namespace Wcs.Framework
{
    public class DatabaseHelper
    {
        /// <summary>
        /// 创建数据库
        /// </summary>
        public static void CreateDatabase()
        {
            using (System.Transactions.TransactionScope tsc = new System.Transactions.TransactionScope())
            {
                SchemaExport schemaExport = new SchemaExport(NHUnitOfWork._configuration);
                string sqlFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Wcs.Framework.Cfg.WcsConfiguration).Assembly.Location), "create_tables.txt");
                schemaExport.SetOutputFile(sqlFile);
                schemaExport.Drop(false, false);
                schemaExport.Create(false, true);
                schemaExport = null;

                string createTablesSql = System.IO.File.ReadAllText(sqlFile);
                createTirggerAndBackupTables(createTablesSql);
                using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
                {
                    foreach (var item in NHUnitOfWork._configuration.ClassMappings.GroupBy(clazz => clazz.Table.Name).Select(x=>x.Key))
                    {
                        unitOfWork.session
                            .CreateSQLQuery("insert into [dbo].[hilo_values]([next_hi],[clazz]) values (1,'" + item + "')")
                            .ExecuteUpdate();
                    }

                    unitOfWork.Commit();
                }
                tsc.Complete();
            }
        }

        static void createTirggerAndBackupTables(string createDatabaseSql)
        {
            using (NHUnitOfWork unitOfWork = new NHUnitOfWork())
            {
                string backupConnectionString = NHBackupServerUnitOfWork._configuration.Properties["connection.connection_string"];
                string backupDatabaseName = Regex.Match(backupConnectionString, @"Initial Catalog\=(?<database>.*?);", RegexOptions.IgnoreCase).Groups["database"].Value;
                Regex regex = new Regex(@"(?<sql>create table (\[?)(?<tablename>.*?)(\]?) \((?s).*?\)\n\s+\))");
                foreach (Match match in regex.Matches(createDatabaseSql))
                {
                    string createSql = match.Groups["sql"].Value;
                    string tableName = match.Groups["tablename"].Value;
                    string createBakTableSql = Regex
                                                .Replace(createSql, "primary key.*", "");
                    createBakTableSql = Regex.Replace(createBakTableSql, @"\bunique\b", "", RegexOptions.IgnoreCase);

                    string createTirggerSql = string.Format(@"CREATE TRIGGER trigger_{0}_delete
                                               ON  {1}.{0} 
                                               AFTER DELETE
                                            AS 
                                            BEGIN
	                                            SET NOCOUNT ON;

                                                INSERT INTO {2}.{1}.{0} select * FROM deleted

                                            END", tableName, "dbo", backupDatabaseName);

                    using (NHBackupServerUnitOfWork backupServerUnitOfWork = new NHBackupServerUnitOfWork())
                    {
                        //删除备份数据库的表
                        backupServerUnitOfWork.session.CreateSQLQuery(string.Format(@"if exists (select * from dbo.sysobjects where id = object_id(N'[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [{0}]", tableName)).ExecuteUpdate();

                        //创建备份数据库的表
                        backupServerUnitOfWork.session.CreateSQLQuery(createBakTableSql).ExecuteUpdate();


                        backupServerUnitOfWork.Commit();
                    }

                    //删除源数据库的触发器
                    unitOfWork.session
                       .CreateSQLQuery(string.Format(@"if exists (select * from dbo.sysobjects where id = object_id(N'[trigger_{0}_delete]')) drop TRIGGER [trigger_{0}_delete]", tableName))
                       .ExecuteUpdate();
                    //创建源数据库的触发器
                    unitOfWork.session
                        .CreateSQLQuery(createTirggerSql)
                        .ExecuteUpdate();

                }

                unitOfWork.Commit();
            }
        }
    }
}
