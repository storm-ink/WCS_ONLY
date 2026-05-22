using System;
namespace Wcs
{
    /// <summary>
    /// NHibernate 执久化上下文
    /// </summary>
    public interface INHUnitOfWork
    {
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();
        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();
        /// <summary>
        /// 销毁对象
        /// </summary>
        void Dispose();
        /// <summary>
        /// 获取上下文会话对象
        /// </summary>
        NHibernate.ISession session { get; }
    }
}
