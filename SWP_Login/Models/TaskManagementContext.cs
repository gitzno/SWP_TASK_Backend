using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SWP_Login.Models
{
    public partial class TaskManagementContext : DbContext
    {
        public TaskManagementContext()
        {
        }

        public TaskManagementContext(DbContextOptions<TaskManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Emotion> Emotions { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Section> Sections { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserSectionRole> UserSectionRoles { get; set; } = null!;
        public virtual DbSet<UserTaskRole> UserTaskRoles { get; set; } = null!;
        public virtual DbSet<UserWorkSpaceRole> UserWorkSpaceRoles { get; set; } = null!;
        public virtual DbSet<WorkSpace> WorkSpaces { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                IConfigurationRoot configuration = new ConfigurationBuilder()
                           .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                           .AddJsonFile("appsettings.json")
                           .Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("TaskManagement"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.ToTable("Account");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Password).HasMaxLength(16);

                entity.Property(e => e.UserName).HasMaxLength(16);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_Task");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_User");
            });

            modelBuilder.Entity<Emotion>(entity =>
            {
                entity.ToTable("Emotion");

                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .HasColumnName("ID")
                    .IsFixedLength();

                entity.Property(e => e.CommentId).HasColumnName("CommentID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.Emotions)
                    .HasForeignKey(d => d.CommentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Emotion_Comment");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Section>(entity =>
            {
                entity.ToTable("Section");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.WorkSpaceId).HasColumnName("WorkSpaceID");

                entity.HasOne(d => d.WorkSpace)
                    .WithMany(p => p.Sections)
                    .HasForeignKey(d => d.WorkSpaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Section_WorkSpace");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.ToTable("Task");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.SectionId).HasColumnName("SectionID");

                entity.Property(e => e.Tag).HasMaxLength(20);

                entity.Property(e => e.TaskFrom).HasColumnType("datetime");

                entity.Property(e => e.TaskTo).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UserCreatedId).HasColumnName("UserCreatedID");

                entity.HasOne(d => d.Section)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.SectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Task_Section");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Email).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(10);

                entity.Property(e => e.Work).HasMaxLength(50);
            });

            modelBuilder.Entity<UserSectionRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.SectionId, e.RoleId });

                entity.ToTable("UserSectionRole");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.SectionId).HasColumnName("SectionID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserSectionRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSectionRole_Role");

                entity.HasOne(d => d.Section)
                    .WithMany(p => p.UserSectionRoles)
                    .HasForeignKey(d => d.SectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSectionRole_Section");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSectionRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSectionRole_User");
            });

            modelBuilder.Entity<UserTaskRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TaskId, e.RoleId });

                entity.ToTable("UserTaskRole");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.TaskId).HasColumnName("TaskID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserTaskRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserTaskRole_Role");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.UserTaskRoles)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserTaskRole_Task");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserTaskRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserTaskRole_User");
            });

            modelBuilder.Entity<UserWorkSpaceRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.WorkSpaceId, e.RoleId });

                entity.ToTable("UserWorkSpaceRole");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.WorkSpaceId).HasColumnName("WorkSpaceID");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserWorkSpaceRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserWorkSpaceRole_Role");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserWorkSpaceRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserWorkSpaceRole_User");

                entity.HasOne(d => d.WorkSpace)
                    .WithMany(p => p.UserWorkSpaceRoles)
                    .HasForeignKey(d => d.WorkSpaceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserWorkSpaceRole_WorkSpace");
            });

            modelBuilder.Entity<WorkSpace>(entity =>
            {
                entity.ToTable("WorkSpace");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Describe).HasMaxLength(50);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
