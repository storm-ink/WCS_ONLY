using SqlSugar;
using System;


namespace ZHQXC
{
    public class DbContext
    {
        private static string LocalConnStr = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["AGV中间表"];
        private static string RemoteConnStr = Wcs.Framework.Cfg.WcsConfiguration.Instance.SettingCollection._settings["AGV中间表"];
      
        public static SqlSugarScope RemoteDb = new SqlSugarScope(new ConnectionConfig()
        {
            DbType = SqlSugar.DbType.SqlServer,
            ConnectionString = RemoteConnStr,
            IsAutoCloseConnection = true
        },
         db => {
             //单例参数配置，所有上下文生效
             db.Aop.OnLogExecuting = (s, p) =>
             {
                 Console.WriteLine(s);
             };
         });

        public static SqlSugarScope LocalDb = new SqlSugarScope(new ConnectionConfig()
        {
            DbType = SqlSugar.DbType.SqlServer,
            ConnectionString = LocalConnStr,
            IsAutoCloseConnection = true
        },
        db => {
            //单例参数配置，所有上下文生效
            db.Aop.OnLogExecuting = (s, p) =>
            {
                Console.WriteLine(s);
            };
        });
       
      
    }
}
