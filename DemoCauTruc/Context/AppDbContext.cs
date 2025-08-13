using Microsoft.EntityFrameworkCore;
using DemoCauTruc.Models.M0302;

namespace DemoCauTruc.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<M0302GoiKhamSTO> GoiKhamSTOs { get; set; }
        public DbSet<M0302ThongTinDoanhNghiep> ThongTinDoanhNghieps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M0302GoiKhamSTO>().HasNoKey();
            modelBuilder.Entity<M0302ThongTinDoanhNghiep>().HasNoKey();
        }

        public bool TestConnection()
        {
            try
            {
                return this.Database.CanConnect();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
