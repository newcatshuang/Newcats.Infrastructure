using System.Transactions;

namespace Newcats.DataAccess.Core
{
    /// <summary>
    /// TransactionScope构建类
    /// </summary>
    public static class TransactionScopeBuilder
    {
        /// <summary>
        /// 设置事务隔离级别为IsolationLevel.ReadCommitted
        /// </summary>
        /// <param name="enabledAsync">是否启用异步支持</param>
        /// <returns></returns>
        public static TransactionScope CreateReadCommitted(bool enabledAsync = true)
        {
            return Create(IsolationLevel.ReadCommitted, enabledAsync);
        }

        /// <summary>
        /// 创建事务块
        /// </summary>
        /// <param name="isolationLevel">事务隔离级别</param>
        /// <param name="enabledAsync">是否启用异步</param>
        /// <returns>TransactionScope</returns>
        public static TransactionScope Create(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool enabledAsync = true)
        {
            TransactionOptions options = new()
            {
                IsolationLevel = isolationLevel,
                Timeout = TransactionManager.DefaultTimeout
            };
            return new TransactionScope(TransactionScopeOption.Required, options, enabledAsync ? TransactionScopeAsyncFlowOption.Enabled : TransactionScopeAsyncFlowOption.Suppress);
        }
    }
}