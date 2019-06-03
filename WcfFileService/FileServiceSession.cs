using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Zio;
using Zio.FileSystems;
using FileSystemDB.Models;
using FileSystemDB.DAL;

namespace WcfFileService
{
    //Виртуальная файловая система
    public class VirtualFileSystem
    {
        //Список текущий директорий, подключённых к сервису
        private static Dictionary<string, string> pathList = new Dictionary<string, string>();
        //Список дисков в файловой системе
        private static Dictionary<string, string> drives = new Dictionary<string, string>() {
            {"c:","/c_drive"},
            {"d:","/d_drive"},
        };

        //Блокировщик файловой системы
        private static object locker = new object();
        public static object Lock { get { return locker; } }

        //проверка строки на вхождение символов 
        private static bool Contains(string str, params char[] args)
        {
            bool result = false;
            foreach (char c in args)
            {
                result = result | str.Contains(c);
            }
            return result;
        }
        //Инициализация файловой системы
        private static void InitFileSystem()
        {
            FileSystemContext ctx = new FileSystemContext();

            if (ctx.Directories.Where(d => d.Path == "c:\\").Count() == 0)
            {
                ctx.Directories.Add(new MdDirectory() { Path = "c:\\" });
                ctx.SaveChanges();
            }
            if (ctx.Directories.Where(d => d.Path == "d:\\").Count() == 0)
            {
                ctx.Directories.Add(new MdDirectory() { Path = "d:\\" });
                ctx.SaveChanges();
            }
        }
        //Привидение пути к удобному формату
        private static string formatPath(string path, string sessionId)
        {
            string[] pathComponents = path.Trim('\\').Split('\\');
            if (pathComponents.Count() > 0)
            {
                if (pathComponents[0].Count() == 2 && pathComponents[0][1] == ':')
                {
                    for (int i = 1; i < pathComponents.Count(); i++)
                    {
                        if (Contains(pathComponents[i], '.', '$', ';', ':', '#', '%', '&', '*', '!', '@', '^'))
                        {
                            throw new FileServiceCommandExeption("Wrong path format");
                        }                        
                    }
                    return $"{path.Trim('\\')}\\";
                }
                else if(pathComponents.Count() > 1)
                {
                    throw new FileServiceCommandExeption("Wrong path format");
                } 
                else
                {
                    return $"{pathList[sessionId]}{path.Trim('\\')}\\";
                }
            }
            else
            {
                throw new FileServiceCommandExeption("Wrong path format");
            }
        }
        //Получение пути файла
        private static MdDirectory PathExist(string path)
        {
            string[] pathComponents = path.Trim('\\').Split('\\');
            FileSystemContext ctx = new FileSystemContext();
            string dbPath = $"{path.Trim('\\')}\\";
            if (ctx.Directories.Where(d => d.Path == dbPath).Count() == 0)
            {
                throw new FileServiceCommandExeption($"{path} not exist");
            } else
            {
                return ctx.Directories.Where(d => d.Path == dbPath).FirstOrDefault();
            }
        }
        //Проверка валидности имени файла
        private static bool CheckFilenameFormat(string filename)
        {
            return !Contains(filename, '$', ';', ':', '#', '%', '&', '*', '!', '@', '^') && filename.Contains(".txt");
        }
        //Проверка файла на наличие блокировки
        private static void CheckLock(string sessionId, FileSystemContext ctx, MdFile filename)
        {
            if (ctx.Locks.Where(l => l.IdFile == filename.Id).Count() > 0)
            {
                string user = FileServiceSession.GetUser(sessionId);
                MdDirectory dir = ctx.Directories.Where(d => d.Id == filename.IdDirectory).FirstOrDefault();
                string error = $"{dir.Path}{filename.FileName} locked by ";
                ctx.Locks.Where(l => l.IdFile == filename.Id
                ).ToList().ForEach(l => {
                    if (user == l.User)
                    {
                        error += "Me,";
                    }
                    else
                    {
                        error += $"{l.User},";
                    }
                    
                });
                throw new FileServiceCommandExeption(error.Trim(','));
            }
        }
        //Переместить директорию
        private static void MoveMatches(string sessionId, FileSystemContext ctx, string oldPath, string newPath)
        {
            List<MdDirectory> oldList = ctx.Directories.Where(d => d.Path.Contains(oldPath)).ToList();
            oldList.ForEach(d => {
                List<string> fileSet = new List<string>();
                ctx.Files.Where(f => f.IdDirectory == d.Id).ToList().ForEach(f => {
                    CheckLock(sessionId, ctx, f);
                    fileSet.Add(f.FileName);
                });
                string path = d.Path.Replace(oldPath, newPath);
                MdDirectory newDir = ctx.Directories.Where(nd => nd.Path == path).FirstOrDefault();
                if (newDir != null)
                {
                    List<MdFile> newDirFiles = ctx.Files.Where(f => f.IdDirectory == newDir.Id).ToList();
                    newDirFiles.ForEach(f => {
                        if (fileSet.IndexOf(f.FileName) >= 0)
                        {
                            CheckLock(sessionId, ctx, f);
                            ctx.Files.Remove(f);
                        }
                        else
                        {
                            f.IdDirectory = d.Id;
                        }
                    });
                    ctx.Directories.Remove(newDir);
                }
                d.Path = path;
            });
            ctx.SaveChanges();
        }
        //Копирование директории
        private static void CopyMatches(string sessionId, FileSystemContext ctx, string copyPath, string newPath)
        {
            List<MdDirectory> copyList = ctx.Directories.Where(d => d.Path.Contains(copyPath)).ToList();
            copyList.ForEach(d => {
                List<string> fileSet = new List<string>();
                List<MdFile> fileList = ctx.Files.Where(f => f.IdDirectory == d.Id).ToList();
                fileList.ForEach(f => {
                    fileSet.Add(f.FileName);
                });
                string path = d.Path.Replace(copyPath, newPath);
                MdDirectory newDir = ctx.Directories.Where(nd => nd.Path == path).FirstOrDefault();
                if (newDir != null)
                {
                    List<MdFile> newDirFiles = ctx.Files.Where(f => f.IdDirectory == newDir.Id).ToList();
                    newDirFiles.ForEach(f => {
                        if (fileSet.IndexOf(f.FileName) >= 0)
                        {
                            CheckLock(sessionId, ctx, f);
                        }
                    });
                }
            });
            copyList.ForEach(d => {
                List<string> fileSet = new List<string>();
                List<MdFile> fileList = ctx.Files.Where(f => f.IdDirectory == d.Id).ToList();
                fileList.ForEach(f => {
                    fileSet.Add(f.FileName);
                });
                string path = d.Path.Replace(copyPath, newPath);
                MdDirectory newDir = ctx.Directories.Where(nd => nd.Path == path).FirstOrDefault();
                if (newDir != null)
                {
                    List<MdFile> newDirFiles = ctx.Files.Where(f => f.IdDirectory == newDir.Id).ToList();
                    newDirFiles.ForEach(f => {
                        if (fileSet.IndexOf(f.FileName) >= 0)
                        {
                            MdFile copyFile = fileList.Where(cf => cf.FileName == f.FileName).FirstOrDefault();
                            f.Content = copyFile.Content.ToArray();
                            fileSet.Remove(f.FileName);
                        }
                    });
                    fileList.ForEach(f =>
                    {
                        if (fileSet.IndexOf(f.FileName) >= 0)
                        {
                            ctx.Files.Add(new MdFile() { FileName = f.FileName, IdDirectory = newDir.Id, Content = f.Content.ToArray() });
                        }
                    });
                }
                else
                {
                    MdDirectory copyDir = new MdDirectory() { Path = path };
                    ctx.Directories.Add(copyDir);
                    ctx.SaveChanges();
                    fileList.ForEach(f =>
                    {
                        ctx.Files.Add(new MdFile() { FileName = f.FileName, IdDirectory = copyDir.Id, Content = f.Content.ToArray() });
                    });
                }
            });
            ctx.SaveChanges();
        }
        //Проверка на вхождение одного пути в другой
        private static void IsContainCurDirectory(string sessionId, string path)
        {
            string curPath = GetPath(sessionId);
            if (curPath.Contains(path))
            {
                throw new FileServiceCommandExeption("Yuo can't delete current directory");
            }
        }
        //Директория, в которой находится пользователь
        public static string GetPath(string sessionId)
        {
            InitFileSystem();

            if (pathList.ContainsKey(sessionId))
            {
                return pathList[sessionId];
            }
            else
            {
                throw new FileServiceCommandExeption("User not found");
            }
        }
        //Установить новую директорию, в которой будет находится пользователь
        public static string SetPath(string sessionId, string path)
        {
            InitFileSystem();
            string localPath = formatPath(path.ToLower(), sessionId);
            PathExist(localPath);
            pathList[sessionId] = localPath;
            return localPath;
        }
        //Создание новой директории
        public static string CreateDirectory(string sessionId, string path)
        {
            InitFileSystem();
            string localPath = formatPath(path.ToLower(), sessionId);
            string[] pathComponents = localPath.Trim('\\').Split('\\');
            string dbPath = "";
            FileSystemContext ctx = new FileSystemContext();
            if (pathComponents.Count() > 1 && drives.ContainsKey(pathComponents[0]))
            {
                string tempPath = drives[pathComponents[0]];
                string viewPath = pathComponents[0];
                for (int i = 1; i < pathComponents.Count() - 1; i++)
                {
                    viewPath += $"\\{pathComponents[i]}";
                    dbPath = $"{viewPath}\\";
                    if (ctx.Directories.Where(d => d.Path == dbPath).Count() == 0)
                    {
                        throw new FileServiceCommandExeption($"{viewPath} not exist");
                    }
                }
                dbPath = $"{viewPath}\\{pathComponents[pathComponents.Count() - 1]}\\";
                if (ctx.Directories.Where(d => d.Path == dbPath).Count() > 0)
                {
                    throw new FileServiceCommandExeption($"{viewPath}\\{pathComponents[pathComponents.Count() - 1]} allready created");
                }
                else
                {
                    ctx.Directories.Add(new MdDirectory { Path = dbPath });
                    ctx.SaveChanges();
                }
                return localPath;
            }
            else
            {
                throw new FileServiceCommandExeption("Wrong path format");
            }
        }
        //Удаление директории
        public static string DeleteDirectory(string sessionId, string path)
        {
            string localPath = formatPath(path.ToLower(), sessionId);
            IsContainCurDirectory(sessionId, localPath);
            FileSystemContext ctx = new FileSystemContext();
            if (ctx.Directories.Where(d => d.Path == localPath).Count() == 0)
            {
                throw new FileServiceCommandExeption($"{localPath} not exist");
            }
            if (ctx.Directories.Where(d => d.Path.Contains(localPath)).Count() > 1)
            {
                throw new FileServiceCommandExeption($"{localPath} has subdirectories");
            }
            else
            {
                MdDirectory localDir = ctx.Directories.Where(d => d.Path == localPath).FirstOrDefault();
                MdFile[] fileList = ctx.Files.Where(f => f.IdDirectory  == localDir.Id).ToArray();
                foreach(MdFile f in fileList)
                {
                    CheckLock(sessionId, ctx, f);
                    ctx.Files.Remove(f);
                }
                ctx.Directories.Remove(ctx.Directories.Where(d => d.Path == localPath).FirstOrDefault());
                ctx.SaveChanges();
            }
            return localPath.Trim('\\');
        }
        //удаление директории и всех директорий, входящих в неё
        public static string DeleteTreeDirectory(string sessionId, string path)
        {
            string localPath = formatPath(path.ToLower(), sessionId);
            IsContainCurDirectory(sessionId, localPath);
            FileSystemContext ctx = new FileSystemContext();
            if (ctx.Directories.Where(d => d.Path == localPath).Count() == 0)
            {
                throw new FileServiceCommandExeption($"{localPath} not exist");
            }
            else
            {
                MdDirectory localDir = ctx.Directories.Where(d => d.Path == localPath).FirstOrDefault();
                ctx.Directories.Where(d => d.Path.Contains(localPath)).ToList().ForEach(d => {
                    ctx.Files.Where(f => f.IdDirectory == d.Id).ToList().ForEach(f => {
                        CheckLock(sessionId, ctx, f);
                        ctx.Files.Remove(f);
                    });
                    ctx.Directories.Remove(d);
                });
                ctx.Files.Where(f => f.IdDirectory == localDir.Id).ToList().ForEach(f =>
                {
                    CheckLock(sessionId, ctx, f);
                    ctx.Files.Remove(f);
                });
                ctx.Directories.Remove(ctx.Directories.Where(d => d.Path == localPath).FirstOrDefault());
                ctx.SaveChanges();
            }
            return localPath.Trim('\\');
        }
        //Создание файла
        public static string CreateFile(string sessionId, string path, string filename) 
        {
            FileSystemContext ctx = new FileSystemContext();
            string localPath = path;

            if (path == "")
            {
                localPath = GetPath(sessionId);
            }
            else
            {
                localPath = formatPath(path.ToLower(), sessionId);
            }
            MdDirectory localDir = PathExist(localPath);
            if (!CheckFilenameFormat(filename))
            {
                throw new FileServiceCommandExeption("Bad filename format");
            }
            string fullFilename = $"{localPath}{filename}";
            if (ctx.Files.Where(f => f.IdDirectory == localDir.Id && f.FileName == filename).Count() > 0)
            {
                throw new FileServiceCommandExeption($"{filename} already created in {localPath}");
            }
            else
            {
                FileStream fs = File.Create(Path.GetTempFileName());
                int length = Convert.ToInt32(fs.Length);
                byte[] data = new byte[length];
                fs.Read(data, 0, length);
                fs.Close();
                ctx.Files.Add(new MdFile() { IdDirectory = localDir.Id, FileName = filename, Content = data });
                ctx.SaveChanges();
                return $"{localPath}{filename}";
            }
            
        }
        //Удаление файла
        public static string DeleteFile(string sessionId, string path, string filename)
        {
            string localPath = path;
            FileSystemContext ctx = new FileSystemContext();
            if (path == "")
            {
                localPath = GetPath(sessionId);
            }
            MdDirectory dir = PathExist(localPath);
            string fullFilename = $"{localPath}{filename}";
            if (ctx.Files.Where(f => f.IdDirectory == dir.Id && f.FileName == filename).Count() > 0)
            {
                CheckLock(sessionId, ctx, ctx.Files.Where(f => f.IdDirectory == dir.Id && f.FileName == filename).FirstOrDefault());
                ctx.Files.Remove(ctx.Files.Where(f => f.IdDirectory == dir.Id && f.FileName == filename).FirstOrDefault());
                ctx.SaveChanges();
                return $"{localPath}{filename}";
            }
            else
            {
                throw new FileServiceCommandExeption($"{filename} not exist in {localPath}");
            }
            
        }
        //Блокировка файла
        public static string LockFile(string sessionId, string path, string filename)
        {
            string user = FileServiceSession.GetUser(sessionId);
            string localPath = path;
            FileSystemContext ctx = new FileSystemContext();
            if (path == "")
            {
                localPath = GetPath(sessionId);
            }
            MdDirectory localDir = PathExist(localPath);
            string fullFilename = $"{localPath}{filename}";
            if (ctx.Files.Where(f => f.IdDirectory == localDir.Id && f.FileName == filename).Count() > 0)
            {
                MdFile localFile = ctx.Files.Where(f => f.IdDirectory == localDir.Id && f.FileName == filename).FirstOrDefault();
                if (ctx.Locks.Where(l => l.IdFile == localFile.Id && l.User == user).Count() > 0)
                {
                    throw new FileServiceCommandExeption("You already lock this file");
                } else
                {
                    ctx.Locks.Add(new MdLock() { IdFile = localFile.Id, User = user });
                    ctx.SaveChanges();
                    return $"{localPath}{filename}";
                }
            }
            else
            {
                throw new FileServiceCommandExeption($"{filename} not exist in {localPath}");
            }
        }
        //Разблокировка файла
        public static string UnlockFile(string sessionId, string path, string filename)
        {
            string user = FileServiceSession.GetUser(sessionId);
            string localPath = path;
            FileSystemContext ctx = new FileSystemContext();
            if (path == "")
            {
                localPath = GetPath(sessionId);
            }
            string fullFilename = $"{localPath}{filename}";
            MdDirectory localDir = PathExist(localPath);
            if (ctx.Files.Where(f => f.IdDirectory == localDir.Id && f.FileName == filename).Count() > 0)
            {
                MdFile localFile = ctx.Files.Where(f => f.IdDirectory == localDir.Id && f.FileName == filename).FirstOrDefault();
                if (ctx.Locks.Where(l => l.IdFile == localFile.Id && l.User == user).Count() > 0)
                {
                    ctx.Locks.Remove(ctx.Locks.Where(l => l.IdFile == localFile.Id && l.User == user).FirstOrDefault());
                    ctx.SaveChanges();
                }
                return $"{localPath}{filename}";
            }
            else
            {
                throw new FileServiceCommandExeption($"{filename} not exist in {localPath}");
            }
        }
        //Копирование директории
        public static string CopyDir(string sessionId, string source, string dest)
        {
            FileSystemContext ctx = new FileSystemContext();
            string localSource = formatPath(source.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localSource).Count() == 0)
            {
                throw new FileServiceCommandExeption($" source {localSource} not exist");
            }
            string localDest = formatPath(dest.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localDest).Count() == 0)
            {
                throw new FileServiceCommandExeption($" destination {localDest} not exist");
            }

            string newPath = $"{localDest}{localSource.Trim('\\').Split('\\')[localSource.Trim('\\').Split('\\').Length - 1]}\\";

            CopyMatches(sessionId, ctx, localSource, newPath);
            return $"{localSource} copied to {localDest}";
        }
        //Перемещение директории
        public static string MoveDir(string sessionId, string source, string dest)
        {
            FileSystemContext ctx = new FileSystemContext();
            string localSource = formatPath(source.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localSource).Count() == 0)
            {
                throw new FileServiceCommandExeption($" source {localSource} not exist");
            }
            string localDest = formatPath(dest.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localDest).Count() == 0)
            {
                throw new FileServiceCommandExeption($" destination {localDest} not exist");
            }

            string newPath = $"{localDest}{localSource.Trim('\\').Split('\\')[localSource.Trim('\\').Split('\\').Length - 1]}\\";

            MoveMatches(sessionId, ctx, localSource, newPath);
            return $"{localSource} moved to {localDest}"; 
        }
        //Копирование файла
        public static string CopyFile(string sessionId, string filename, string source, string dest)
        {
            FileSystemContext ctx = new FileSystemContext();
            string localSource = "";
            if (source == "")
            {
                localSource = GetPath(sessionId);
            }
            else
            {
                localSource = formatPath(source.ToLower(), sessionId);
            }
            if (ctx.Directories.Where(d => d.Path == localSource).Count() == 0)
            {
                throw new FileServiceCommandExeption($" source {localSource} not exist");
            }
            string localDest = formatPath(dest.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localDest).Count() == 0)
            {
                throw new FileServiceCommandExeption($" destinationw {localDest} not exist");
            }
            MdDirectory copyDir = ctx.Directories.Where(d => d.Path == localSource).FirstOrDefault();
            MdDirectory newDir = ctx.Directories.Where(d => d.Path == localDest).FirstOrDefault();
            MdFile currentFile = ctx.Files.Where(f => f.IdDirectory == copyDir.Id && f.FileName == filename).FirstOrDefault();
            if (currentFile == null)
            {
                throw new FileServiceCommandExeption($"{filename} not found");
            }
            else
            {
                MdFile dubFile = ctx.Files.Where(f => f.IdDirectory == newDir.Id && f.FileName == filename).FirstOrDefault();
                if (dubFile != null)
                {
                    CheckLock(sessionId, ctx, dubFile);
                    dubFile.Content = currentFile.Content.ToArray();
                }
                else
                {
                    ctx.Files.Add(new MdFile() { FileName = currentFile.FileName, IdDirectory = newDir.Id, Content = currentFile.Content.ToArray() });
                }
                ctx.SaveChanges();
                return $"{filename} copied from {localSource} to {localDest}";
            }

        }
        //Перемещение файла
        public static string MoveFile(string sessionId, string filename, string source, string dest)
        {
            FileSystemContext ctx = new FileSystemContext();
            string localSource = "";
            if (source == "")
            {
                localSource = GetPath(sessionId);
            }
            else
            {
                localSource = formatPath(source.ToLower(), sessionId);
            }
            if (ctx.Directories.Where(d => d.Path == localSource).Count() == 0)
            {
                throw new FileServiceCommandExeption($" source {localSource} not exist");
            }
            string localDest = formatPath(dest.ToLower(), sessionId);
            if (ctx.Directories.Where(d => d.Path == localDest).Count() == 0)
            {
                throw new FileServiceCommandExeption($" destinationw {localDest} not exist");
            }
            MdDirectory oldDir = ctx.Directories.Where(d => d.Path == localSource).FirstOrDefault();
            MdDirectory newDir = ctx.Directories.Where(d => d.Path == localDest).FirstOrDefault();
            MdFile currentFile = ctx.Files.Where(f => f.IdDirectory == oldDir.Id && f.FileName == filename).FirstOrDefault();
            if (currentFile == null)
            {
                throw new FileServiceCommandExeption($"{filename} not found");
            }
            else
            {
                CheckLock(sessionId, ctx, currentFile);
                MdFile dubFile = ctx.Files.Where(f => f.IdDirectory == newDir.Id && f.FileName == filename).FirstOrDefault();
                if (dubFile != null)
                {
                    CheckLock(sessionId, ctx, dubFile);
                    ctx.Files.Remove(dubFile);
                }
                currentFile.IdDirectory = newDir.Id;
                ctx.SaveChanges();
                return $"{filename} moved from {localSource} to {localDest}";
            }
            
        }
        //Получение списка директорий
        public static List<string> Print()
        {
            FileSystemContext ctx = new FileSystemContext();
            return (from d in ctx.Directories orderby d.Path select d.Path).ToList();
        }
        //Получение списка файлов в директории
        public static List<string> PrintFiles(string path, string sessionId)
        {
            FileSystemContext ctx = new FileSystemContext();
            List<string> result = new List<string>();
            MdDirectory dir = PathExist(path);
            ctx.Files.Where(f => f.IdDirectory == dir.Id).ToList().ForEach(f => {
                string lockedBy = "";
                if (ctx.Locks.Where(l => l.IdFile == f.Id).Count() > 0)
                {
                    lockedBy = "locked by ";
                    ctx.Locks.Where(l => l.IdFile == f.Id).ToList().ForEach(l =>
                    {
                        if (l.User == FileServiceSession.GetUser(sessionId))
                        {
                            lockedBy += "Me,";
                        }
                        else
                        {
                            lockedBy += $"{l.User},";
                        }
                        
                    });
                    lockedBy = $"[{lockedBy.Trim(',')}]";
                }
                result.Add($"{dir.Path}{f.FileName}{lockedBy}");
            });
            return result;
        }
    }

    //Класс сессии в сервисе
    public class FileServiceSession
    {
        //Список пользователей
        private static Dictionary<string, string> session = new Dictionary<string, string>();
        //Получение пользователя по Session ID
        public static String GetUser(string sessionId)
        {
            return (from u in session where u.Value.Equals(sessionId) select u.Key).First();
        }
        //Занесение нового пользователя в сессию
        public static void SetUser(string sessionId, string user)
        {
            if ((from u in session where u.Key.Equals(user) select u.Key).Count() == 0)
            {
                session.Add(user, sessionId);
            }
            else
            {
                throw new FileServiceCommandExeption("User already connected");
            }
        }
        //Удаление пользователя из сессии
        public static void DeleteUser(string sessionId)
        {
            String user = (from u in session where u.Value.Equals(sessionId) select u.Key).First();
            session.Remove(user);
        } 
    }
}