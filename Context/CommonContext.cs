using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCrawler.Entity;

namespace WebCrawler.Context
{
    public class CommonContext: DbContext
    {

        public DbSet<ProductModel> Models { get; set; }
        public DbSet<ProductKind> Products { get; set; }
        public DbSet<ProductName> ProductClasses { get; set; }
        

        public CommonContext(string cnnStr): base(cnnStr)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<CommonContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}
