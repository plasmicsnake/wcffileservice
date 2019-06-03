using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace FileSystemDB.Models
{
    //Сущность Директория
    public class MdDirectory
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public List<MdFile> Files { get; set; }
    }
    //Сущность Файл
    public class MdFile
    {
        public int Id { get; set; }
        public int IdDirectory { get; set; }
        public MdDirectory Directory { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public List<MdLock> Locks { get; set; }
    }
    //Сущность Блокировка файла
    public class MdLock
    {
        public int Id { get; set; }
        public int IdFile { get; set; }
        public MdFile File { get; set; }
        public string User { get; set; }
    }
}
