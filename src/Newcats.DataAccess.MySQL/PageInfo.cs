using System.Collections.Generic;

namespace Newcats.DataAccess.MySQL
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <typeparam name="TEntity">数据类型</typeparam>
    public class PageInfo<TEntity> where TEntity : class
    {
        /// <summary>
        /// 页码（从第0页开始）
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页数量（默认20）
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 筛选条件
        /// </summary>
        public List<DbWhere<TEntity>> Where { get; set; }

        /// <summary>
        /// 排序条件
        /// </summary>
        public List<DbOrderBy<TEntity>> OrderBy { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        public PageInfo()
        {
            PageIndex = 0;
            PageSize = 20;
            Where = new List<DbWhere<TEntity>>();
            OrderBy = new List<DbOrderBy<TEntity>>();
        }
    }
}