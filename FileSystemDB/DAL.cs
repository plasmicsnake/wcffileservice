using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using FileSystemDB.Models;

namespace FileSystemDB.DAL
{
    //Класс для управления БД
    public class FileSystemContext : DbContext
    {
        public FileSystemContext() : base("FileSystemContext")
        {
        }
        //Директории
        public DbSet<MdDirectory> Directories { get; set; }
        //Файлы
        public DbSet<MdFile> Files { get; set; }
        //Блокировки
        public DbSet<MdLock> Locks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Entity<MdFile>().HasRequired(f => f.Directory).WithMany(d => d.Files).HasForeignKey(f => f.IdDirectory);
            modelBuilder.Entity<MdLock>().HasRequired(l => l.File).WithMany(f => f.Locks).HasForeignKey(l => l.IdFile);
        }
    }
}
