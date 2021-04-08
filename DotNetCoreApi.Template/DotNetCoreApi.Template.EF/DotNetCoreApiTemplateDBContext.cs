using DotNetCoreApi.Template.EF.Entity;
using Microsoft.EntityFrameworkCore;
using DotNetCoreApi.Template.EF.Helper;

namespace DotNetCoreApi.Template.EF
{
    public class DotNetCoreApiTemplateDBContext : DbContext
    {
        public DotNetCoreApiTemplateDBContext(DbContextOptions<DotNetCoreApiTemplateDBContext> options) : base(options) { }

        /// <summary>
        /// 取得單一DbSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public DbSet<T> GetDbSet<T>() where T : class
        {
            return this.Set<T>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // sample
            //modelBuilder
            //    .Entity<SystemSetting>()
            //    .AddIndex(x => x.SourceID)
            //    .AddIndex(x => new { x.SourceID, x.Key });

            base.OnModelCreating(modelBuilder);
        }

    }

}
