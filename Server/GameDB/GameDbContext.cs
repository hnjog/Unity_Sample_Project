using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameDB
{
    public class GameDbContext : DbContext
    {
        public DbSet<TestDb> Tests { get; set; }

        // 실행 구문을 로그 찍어주는 용
        static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => { builder.AddConsole(); });

        public static string ConnectionString = "Data Source=(localdb)\\ProjectModels;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public GameDbContext()
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseLoggerFactory(_logger)
                .UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TestDb>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
