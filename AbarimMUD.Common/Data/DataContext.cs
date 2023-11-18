using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace AbarimMUD.Common.Data
{
	public class DataContext : DbContext
	{
		public DbSet<Area> Areas => Set<Area>();
		public DbSet<Room> Rooms => Set<Room>();
		public DbSet<RoomDirection> RoomsDirections => Set<RoomDirection>();
		public DbSet<Mobile> Mobiles => Set<Mobile>();
		public DbSet<GameObject> Objects => Set<GameObject>();

		public DataContext()
		{
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			optionsBuilder.UseSqlite(connectionString);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Room>()
				.HasMany(r => r.InputDirections)
				.WithOne(rd => rd.TargetRoom)
				.HasForeignKey(rd => rd.TargetRoomId)
				.IsRequired(false);

			modelBuilder.Entity<Room>()
				.HasMany(r => r.OutputDirections)
				.WithOne(rd => rd.SourceRoom)
				.HasForeignKey(rd => rd.SourceRoomId)
				.IsRequired();

			modelBuilder.Entity<RoomDirection>()
				.HasOne(e => e.Area)
				.WithMany()
				.HasForeignKey(e => e.AreaId)
				.IsRequired();
		}
	}
}
