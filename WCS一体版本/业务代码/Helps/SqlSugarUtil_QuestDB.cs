using SqlSugar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ZHQXC
{
    public class SqlSugarUtil_QuestDB : IDisposable
    {
        static string connstr = null;
        static object objLocker = new object();
        public string ConnStr
        {
            get
            {
                if (connstr == null)
                {
                    lock (objLocker)
                    {
                        if (connstr == null)
                            connstr = GetAppsettingStr("questdb");
                    }
                }
                return connstr;
            }
        }

        public string GetAppsettingStr(string str)
        {
            AppSettingsReader appReader = new AppSettingsReader();
            return appReader.GetValue(str, typeof(string)).ToString();
        }

        public SqlSugarClient Appdb;

        public SqlSugarUtil_QuestDB()
        {
            Appdb = new SqlSugarClient(
                new ConnectionConfig()
                {
                    ConnectionString = ConnStr,
                    DbType = DbType.QuestDB,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute,
                    AopEvents = new AopEvents
                    {
                        OnLogExecuting = (sql, p) =>
                        {
                            Console.WriteLine(sql);
                            Console.WriteLine(string.Join(",", p?.Select(it => it.ParameterName + ":" + it.Value)));
                        }
                    }
                });
        }

        public ISugarQueryable<TEntity> Queryable<TEntity>() where TEntity : class, new()
        {
            return Appdb.Queryable<TEntity>();
        }

        public ISugarQueryable<TEntity> Queryable<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class, new()
        {
            return Appdb.Queryable<TEntity>().Where(expression);
        }

        public TEntity GetById<TEntity>(dynamic id) where TEntity : class, new()
        {
            return Appdb.Queryable<TEntity>().InSingle(id);
        }

        public bool Insert<TEntity>(TEntity entity) where TEntity : class, new()
        {
            return Appdb.Insertable(entity).ExecuteCommand() > 0;
        }

        public bool Update<TEntity>(TEntity entity) where TEntity : class, new()
        {
            return Appdb.Updateable(entity).ExecuteCommand() > 0;
        }

        public bool Delete<TEntity>(TEntity entity) where TEntity : class, new()
        {
            return Appdb.Deleteable(entity).ExecuteCommand() > 0;
        }

        public bool Delete<TEntity>(Expression<Func<TEntity, bool>> expression) where TEntity : class, new()
        {
            return Appdb.Deleteable(expression).ExecuteCommand() > 0;
        }


        //根据连接字符串将数据库表生成实体
        public void GenerateEntity()
        {
            try
            {
                Appdb.Ado.CheckConnection();
                var path = AppDomain.CurrentDomain.BaseDirectory + "\\Entity";//生成的实体存入的文件夹路径
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                Appdb.DbFirst.CreateClassFile(path, "生成的文件的头部的解决方案名称");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Dispose()
        {
            Appdb.Close();
            Appdb.Dispose();
        }
    }
}
