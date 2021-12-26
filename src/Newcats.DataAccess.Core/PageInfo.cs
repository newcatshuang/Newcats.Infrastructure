namespace Newcats.DataAccess.Core
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
        /// 总记录数
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get { return (int)Math.Ceiling(TotalRecords / (double)PageSize); } set { } }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPrevious { get { return PageIndex > 0; } set { } }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNext { get { return PageIndex + 1 < TotalPages; } set { } }

        /// <summary>
        /// 数据
        /// </summary>
        public List<TEntity>? Data { get; set; }

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
            Data = new List<TEntity>();
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="pageIndex">页码（从第0页开始）</param>
        /// <param name="pageSize">每页数量（默认20）</param>
        public PageInfo(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Where = new List<DbWhere<TEntity>>();
            OrderBy = new List<DbOrderBy<TEntity>>();
            Data = new List<TEntity>();
        }
    }
}