using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace AbarimMUD.Data
{
	public class DataContext : DbContext
	{
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

			modelBuilder.Entity<Account>()
				.HasMany<Character>()
				.WithOne(e => e.Account)
				.HasForeignKey(e => e.AccountId)
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany<Room>()
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany<Mobile>()
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany<GameObject>()
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<Area>()
				.HasMany<AreaReset>()
				.WithOne(e => e.Area)
				.HasForeignKey("AreaId")
				.IsRequired();

			modelBuilder.Entity<GameObject>()
				.HasMany<GameObjectEffect>()
				.WithOne(e => e.GameObject)
				.HasForeignKey("GameObjectId")
				.IsRequired();

			modelBuilder.Entity<Mobile>()
				.HasMany<MobileSpecialAttack>()
				.WithOne(e => e.Mobile)
				.HasForeignKey("MobileId")
				.IsRequired();

			modelBuilder.Entity<Mobile>()
				.HasOne<Shop>()
				.WithOne()
				.HasForeignKey<Shop>("MobileId")
				.IsRequired(false);

			modelBuilder.Entity<Room>()
				.HasMany<RoomExit>()
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
