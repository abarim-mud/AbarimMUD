using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace AbarimMUD.Data
{
	public class DataContext : DbContext
	{
		public static Action<string> LogOutput;

		private static void Log(string message)
		{
			if (LogOutput != null)
			{
				LogOutput(message);
			}
		}

		private readonly string _connectionString;

		public DbSet<Area> Areas => Set<Area>();
		public DbSet<Room> Rooms => Set<Room>();
		public DbSet<RoomExit> RoomsExits => Set<RoomExit>();
		public DbSet<Mobile> Mobiles => Set<Mobile>();
		public DbSet<GameObject> Objects => Set<GameObject>();
		public DbSet<GameObjectEffect> ObjectsEffect => Set<GameObjectEffect>();
		public DbSet<AreaReset> AreaResets => Set<AreaReset>();
		public DbSet<Shop> Shops => Set<Shop>();
		public DbSet<MobileSpecialAttack> MobileSpecialAttacks => Set<MobileSpecialAttack>();
		public DbSet<HelpData> Helps => Set<HelpData>();
		public DbSet<Social> Socials => Set<Social>();
		public DbSet<Account> Accounts => Set<Account>();
		public DbSet<Character> Characters => Set<Character>();


		public DataContext(string connectionString)
		{
			_connectionString = connectionString;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			optionsBuilder
				.UseSqlite(_connectionString)
				.LogTo(Log, new[] { RelationalEventId.CommandExecuted });
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Account>()
				.HasMany(e => e.Characters)
				.WithOne(e => e.Account)
				.HasForeignKey("AccountId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany(e => e.Rooms)
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany(e => e.Mobiles)
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany(e => e.Objects)
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany(e => e.Resets)
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<GameObject>()
				.HasMany(e => e.Effects)
				.WithOne(e => e.GameObject)
				.HasForeignKey("GameObjectId")
				.IsRequired();

			modelBuilder.Entity<Mobile>()
				.HasMany(e => e.SpecialAttacks)
				.WithOne(e => e.Mobile)
				.HasForeignKey("MobileId")
				.IsRequired();

			modelBuilder.Entity<Mobile>()
				.HasOne(e => e.Shop)
				.WithOne()
				.HasForeignKey<Shop>("MobileId")
				.IsRequired(false);

			modelBuilder.Entity<Room>()
				.HasMany(e => e.Exits)
				.WithOne(e => e.SourceRoom)
				.HasForeignKey(e => e.SourceRoomId)
				.IsRequired();

			modelBuilder.Entity<Room>()
				.HasMany<RoomExit>()
				.WithOne(e => e.TargetRoom)
				.HasForeignKey(e => e.TargetRoomId)
				.IsRequired(false);

			modelBuilder.Entity<RoomExit>()
				.HasOne<GameObject>()
				.WithMany()
				.HasForeignKey("KeyObjectId")
				.IsRequired(false);
		}
	}
}
