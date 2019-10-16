using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ApiWebServer.PBTables
{
    public partial class MaguPBTableContext : DbContext
    {
        public MaguPBTableContext()
        {
        }

        public MaguPBTableContext(DbContextOptions<MaguPBTableContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PB_ACHIEVEMENT> PB_ACHIEVEMENT { get; set; }
        public virtual DbSet<PB_CAREERMODE_CHAINCONTRACT_REWARD> PB_CAREERMODE_CHAINCONTRACT_REWARD { get; set; }
        public virtual DbSet<PB_CAREERMODE_MANAGEMENT_EVENT> PB_CAREERMODE_MANAGEMENT_EVENT { get; set; }
        public virtual DbSet<PB_CAREERMODE_MANAGEMENT_INJURY> PB_CAREERMODE_MANAGEMENT_INJURY { get; set; }
        public virtual DbSet<PB_CAREERMODE_MANAGEMENT_STATIC> PB_CAREERMODE_MANAGEMENT_STATIC { get; set; }
        public virtual DbSet<PB_CAREERMODE_MYTEAM_LINEUP> PB_CAREERMODE_MYTEAM_LINEUP { get; set; }
        public virtual DbSet<PB_CAREERMODE_OWNER_GOAL> PB_CAREERMODE_OWNER_GOAL { get; set; }
        public virtual DbSet<PB_CAREERMODE_RANK_REWARD> PB_CAREERMODE_RANK_REWARD { get; set; }
        public virtual DbSet<PB_CAREERMODE_RECOMMEND_ADVANTAGE> PB_CAREERMODE_RECOMMEND_ADVANTAGE { get; set; }
        public virtual DbSet<PB_CAREERMODE_SEASON_MVP_REWARD> PB_CAREERMODE_SEASON_MVP_REWARD { get; set; }
        public virtual DbSet<PB_CAREERMODE_SPRINGCAMP_ADVICE> PB_CAREERMODE_SPRINGCAMP_ADVICE { get; set; }
        public virtual DbSet<PB_CAREERMODE_SPRINGCAMP_BONUS> PB_CAREERMODE_SPRINGCAMP_BONUS { get; set; }
        public virtual DbSet<PB_CAREERMODE_SPRINGCAMP_GROUP> PB_CAREERMODE_SPRINGCAMP_GROUP { get; set; }
        public virtual DbSet<PB_CAREERMODE_STAGE_REWARD> PB_CAREERMODE_STAGE_REWARD { get; set; }
        public virtual DbSet<PB_CAREERMODE_TEAM_GROUP> PB_CAREERMODE_TEAM_GROUP { get; set; }
        public virtual DbSet<PB_CAREER_SPECIAL_TRAINING> PB_CAREER_SPECIAL_TRAINING { get; set; }
        public virtual DbSet<PB_CAREER_SPRING_RESULT_BONUS> PB_CAREER_SPRING_RESULT_BONUS { get; set; }
        public virtual DbSet<PB_CAREER_SPRING_TRAINING> PB_CAREER_SPRING_TRAINING { get; set; }
        public virtual DbSet<PB_CDN_URL> PB_CDN_URL { get; set; }
        public virtual DbSet<PB_COACH> PB_COACH { get; set; }
        public virtual DbSet<PB_COACH_POSITION> PB_COACH_POSITION { get; set; }
        public virtual DbSet<PB_COACH_REINFORCE_POWER> PB_COACH_REINFORCE_POWER { get; set; }
        public virtual DbSet<PB_COACH_SKILL_RANKUP> PB_COACH_SKILL_RANKUP { get; set; }
        public virtual DbSet<PB_COACH_SLOT_BASE> PB_COACH_SLOT_BASE { get; set; }
        public virtual DbSet<PB_COACH_SLOT_EFFECT> PB_COACH_SLOT_EFFECT { get; set; }
        public virtual DbSet<PB_COMPETITIVE_PLAY> PB_COMPETITIVE_PLAY { get; set; }
        public virtual DbSet<PB_COMPETITIVE_TEAM_GROUP> PB_COMPETITIVE_TEAM_GROUP { get; set; }
        public virtual DbSet<PB_COMPETITIVE_TEAM_LINEUP> PB_COMPETITIVE_TEAM_LINEUP { get; set; }
        public virtual DbSet<PB_CONST> PB_CONST { get; set; }
        public virtual DbSet<PB_GAME_CONSTANT> PB_GAME_CONSTANT { get; set; }
        public virtual DbSet<PB_INVENTORY_LEVEL> PB_INVENTORY_LEVEL { get; set; }
        public virtual DbSet<PB_ITEM> PB_ITEM { get; set; }
        public virtual DbSet<PB_ITEM_CARD> PB_ITEM_CARD { get; set; }
        public virtual DbSet<PB_ITEM_CARD_GACHA> PB_ITEM_CARD_GACHA { get; set; }
        public virtual DbSet<PB_ITEM_CONTENTS> PB_ITEM_CONTENTS { get; set; }
        public virtual DbSet<PB_ITEM_GACHA> PB_ITEM_GACHA { get; set; }
        public virtual DbSet<PB_ITEM_PACKAGE> PB_ITEM_PACKAGE { get; set; }
        public virtual DbSet<PB_ITEM_SINGLE> PB_ITEM_SINGLE { get; set; }
        public virtual DbSet<PB_LIVESEASON_SCHEDULE> PB_LIVESEASON_SCHEDULE { get; set; }
        public virtual DbSet<PB_MANAGER_EXP> PB_MANAGER_EXP { get; set; }
        public virtual DbSet<PB_MARKET_URL> PB_MARKET_URL { get; set; }
        public virtual DbSet<PB_PLAYER_BATTER> PB_PLAYER_BATTER { get; set; }
        public virtual DbSet<PB_PLAYER_PITCHER> PB_PLAYER_PITCHER { get; set; }
        public virtual DbSet<PB_PLAYER_REINFORCE_POWER> PB_PLAYER_REINFORCE_POWER { get; set; }
        public virtual DbSet<PB_PLAYER_SKILL_POTENTIAL> PB_PLAYER_SKILL_POTENTIAL { get; set; }
        public virtual DbSet<PB_PVPCONST> PB_PVPCONST { get; set; }
        public virtual DbSet<PB_REPEAT_MISSION> PB_REPEAT_MISSION { get; set; }
        public virtual DbSet<PB_SCOUT> PB_SCOUT { get; set; }
        public virtual DbSet<PB_SCOUT_BASE> PB_SCOUT_BASE { get; set; }
        public virtual DbSet<PB_SCOUT_BINDER> PB_SCOUT_BINDER { get; set; }
        public virtual DbSet<PB_SCOUT_GACHA> PB_SCOUT_GACHA { get; set; }
        public virtual DbSet<PB_SKILL_COACHING> PB_SKILL_COACHING { get; set; }
        public virtual DbSet<PB_SKILL_LEADERSHIP> PB_SKILL_LEADERSHIP { get; set; }
        public virtual DbSet<PB_SKILL_MASTERY> PB_SKILL_MASTERY { get; set; }
        public virtual DbSet<PB_SKILL_RANKUP> PB_SKILL_RANKUP { get; set; }
        public virtual DbSet<PB_SLANG> PB_SLANG { get; set; }
        public virtual DbSet<PB_STUFF_INFO> PB_STUFF_INFO { get; set; }
        public virtual DbSet<PB_TEAM_COUNTRY_SQUAD> PB_TEAM_COUNTRY_SQUAD { get; set; }
        public virtual DbSet<PB_TEAM_CREATE_GROUP> PB_TEAM_CREATE_GROUP { get; set; }
        public virtual DbSet<PB_TEAM_INFO> PB_TEAM_INFO { get; set; }
        public virtual DbSet<PB_TEAM_LINEUP> PB_TEAM_LINEUP { get; set; }
        public virtual DbSet<PB_TEAM_PAID_PLAYER_LIST> PB_TEAM_PAID_PLAYER_LIST { get; set; }
        public virtual DbSet<PB_TEAM_SELECT_PLAYER> PB_TEAM_SELECT_PLAYER { get; set; }
        public virtual DbSet<PB_VERSION> PB_VERSION { get; set; }

        // Unable to generate entity type for table 'dbo.PB_SLANG_TEST'. Please see the warning messages.

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=183.110.18.142,14333;Database=ma9gb_table;User ID=magu2_admin;Password=vnfms2010gncn!*;TrustServerCertificate=False;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.4-servicing-10062");

            modelBuilder.Entity<PB_ACHIEVEMENT>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.description)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PB_CAREERMODE_CHAINCONTRACT_REWARD>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_MANAGEMENT_EVENT>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_MANAGEMENT_INJURY>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_MANAGEMENT_STATIC>(entity =>
            {
                entity.HasKey(e => e.no);

                entity.Property(e => e.no).ValueGeneratedNever();

                entity.Property(e => e.data)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<PB_CAREERMODE_MYTEAM_LINEUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_OWNER_GOAL>(entity =>
            {
                entity.HasKey(e => e.idx)
                    .HasName("PK_PB_OWNER_GOAL");

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.description)
                    .IsRequired()
                    .HasMaxLength(1024)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PB_CAREERMODE_RANK_REWARD>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_RECOMMEND_ADVANTAGE>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_SEASON_MVP_REWARD>(entity =>
            {
                entity.HasKey(e => new { e.difficulty, e.awards_type })
                    .HasName("PK_PB_CAREERMODE_MVP_REWARD");
            });

            modelBuilder.Entity<PB_CAREERMODE_SPRINGCAMP_ADVICE>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_SPRINGCAMP_BONUS>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_SPRINGCAMP_GROUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREERMODE_STAGE_REWARD>(entity =>
            {
                entity.HasKey(e => new { e.difficulty, e.result_type });
            });

            modelBuilder.Entity<PB_CAREERMODE_TEAM_GROUP>(entity =>
            {
                entity.HasKey(e => e.team_idx);

                entity.Property(e => e.team_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CAREER_SPECIAL_TRAINING>(entity =>
            {
                entity.HasKey(e => e.training_id);
            });

            modelBuilder.Entity<PB_CAREER_SPRING_RESULT_BONUS>(entity =>
            {
                entity.HasKey(e => e.idx)
                    .HasName("PK_PB_CARRER_SPRING_RESULT_BONUS");
            });

            modelBuilder.Entity<PB_CAREER_SPRING_TRAINING>(entity =>
            {
                entity.HasKey(e => e.training_id);
            });

            modelBuilder.Entity<PB_CDN_URL>(entity =>
            {
                entity.HasKey(e => new { e.country_type, e.os_type });

                entity.Property(e => e.url)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<PB_COACH>(entity =>
            {
                entity.HasKey(e => e.coach_idx);

                entity.Property(e => e.coach_idx).ValueGeneratedNever();

                entity.Property(e => e.coach_name)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<PB_COACH_POSITION>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.master_position_num)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PB_COACH_REINFORCE_POWER>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PB_COACH_SKILL_RANKUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.name)
                    .HasMaxLength(1)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PB_COACH_SLOT_BASE>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_COACH_SLOT_EFFECT>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_COMPETITIVE_PLAY>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.rank_name)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<PB_COMPETITIVE_TEAM_GROUP>(entity =>
            {
                entity.HasKey(e => e.team_idx);

                entity.Property(e => e.team_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_COMPETITIVE_TEAM_LINEUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_CONST>(entity =>
            {
                entity.HasKey(e => e.const_key)
                    .HasName("PK_PB_CONST_1");

                entity.Property(e => e.const_key)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_GAME_CONSTANT>(entity =>
            {
                entity.HasKey(e => e.constant_idx);

                entity.Property(e => e.constant_idx).ValueGeneratedNever();

                entity.Property(e => e.constant_eng)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.constant_kor)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<PB_INVENTORY_LEVEL>(entity =>
            {
                entity.HasKey(e => new { e.type, e.extend_level });
            });

            modelBuilder.Entity<PB_ITEM>(entity =>
            {
                entity.HasKey(e => e.item_idx);

                entity.Property(e => e.item_idx).ValueGeneratedNever();

                entity.Property(e => e.memo)
                    .IsRequired()
                    .HasMaxLength(1000);
            });

            modelBuilder.Entity<PB_ITEM_CARD>(entity =>
            {
                entity.HasKey(e => e.item_idx);

                entity.Property(e => e.item_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_ITEM_CARD_GACHA>(entity =>
            {
                entity.HasKey(e => new { e.item_idx, e.rate_idx });
            });

            modelBuilder.Entity<PB_ITEM_CONTENTS>(entity =>
            {
                entity.HasKey(e => e.item_idx);

                entity.Property(e => e.item_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_ITEM_GACHA>(entity =>
            {
                entity.HasKey(e => new { e.item_idx, e.rate_idx });
            });

            modelBuilder.Entity<PB_ITEM_PACKAGE>(entity =>
            {
                entity.HasKey(e => new { e.item_idx, e.sub_idx });
            });

            modelBuilder.Entity<PB_ITEM_SINGLE>(entity =>
            {
                entity.HasKey(e => e.item_idx);

                entity.Property(e => e.item_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_LIVESEASON_SCHEDULE>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_MANAGER_EXP>(entity =>
            {
                entity.HasKey(e => e.level);

                entity.Property(e => e.level).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_MARKET_URL>(entity =>
            {
                entity.HasKey(e => new { e.country_type, e.market_type });

                entity.Property(e => e.url)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<PB_PLAYER_BATTER>(entity =>
            {
                entity.HasKey(e => e.player_idx);

                entity.Property(e => e.player_idx).ValueGeneratedNever();

                entity.Property(e => e.player_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<PB_PLAYER_PITCHER>(entity =>
            {
                entity.HasKey(e => e.player_idx);

                entity.Property(e => e.player_idx).ValueGeneratedNever();

                entity.Property(e => e.player_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<PB_PLAYER_REINFORCE_POWER>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_PLAYER_SKILL_POTENTIAL>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_PVPCONST>(entity =>
            {
                entity.HasKey(e => e.pvpconst_key);

                entity.Property(e => e.pvpconst_key)
                    .HasMaxLength(128)
                    .IsUnicode(false)
                    .ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_REPEAT_MISSION>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.description)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<PB_SCOUT>(entity =>
            {
                entity.HasKey(e => e.scout_idx);

                entity.Property(e => e.scout_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SCOUT_BASE>(entity =>
            {
                entity.HasKey(e => e.scout_idx);

                entity.Property(e => e.scout_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SCOUT_BINDER>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SCOUT_GACHA>(entity =>
            {
                entity.HasKey(e => new { e.scout_idx, e.rate_idx });
            });

            modelBuilder.Entity<PB_SKILL_COACHING>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SKILL_LEADERSHIP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SKILL_MASTERY>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SKILL_RANKUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_SLANG>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();

                entity.Property(e => e.slang)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<PB_STUFF_INFO>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_COUNTRY_SQUAD>(entity =>
            {
                entity.HasKey(e => e.idx)
                    .HasName("PK_PB_TEAM_LEAGUE_SQUAD");

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_CREATE_GROUP>(entity =>
            {
                entity.HasKey(e => e.team_idx);

                entity.Property(e => e.team_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_INFO>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_LINEUP>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_PAID_PLAYER_LIST>(entity =>
            {
                entity.HasKey(e => e.idx);

                entity.Property(e => e.idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_TEAM_SELECT_PLAYER>(entity =>
            {
                entity.HasKey(e => e.team_idx);

                entity.Property(e => e.team_idx).ValueGeneratedNever();
            });

            modelBuilder.Entity<PB_VERSION>(entity =>
            {
                entity.HasKey(e => e.version);

                entity.Property(e => e.version)
                    .HasMaxLength(200)
                    .ValueGeneratedNever();
            });
        }
    }
}
