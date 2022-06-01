using System.Data.Entity;
using WebCrawler.Entity;

namespace WebCrawler.Context
{
    public class DataContext: DbContext
    {
        public DbSet<TempWorkOrder> TempWorkOrders { get; set; }
        public DbSet<TempWorkOrderLog> TempWorkOrderLogs { get; set; }
        public DbSet<ParserConfig> ParserConfigs { get; set; }

        public DataContext(string cnnStr): base(cnnStr)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DataContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }

    
}
