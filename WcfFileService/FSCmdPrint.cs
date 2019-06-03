using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfFileService
{
    //Реализация команды PRINT
    public class FSCmdPrint : IFileServiceCommand
    {
        private string Spaces(string str)
        {
            string result = "";
            for (int i = 0; i < str.Count(); i++)
            {
                result += " ";
            }
            return result;
        }
        private string trimPath(string previos, string current)
        {
            string result = "";
            string[] previosComponents = previos.Trim('\\').Split('\\');
            string[] currentComponents = current.Trim('\\').Split('\\');
            if (currentComponents.Count() > previosComponents.Count())
            {
                int i = 0;
                for (; i < previosComponents.Count(); i++)
                {
                    result += $"{Spaces(currentComponents[i])} ";
                }
                result += $"{currentComponents[i]}\\";
            }
            else
            {
                for (int i = 0; i < currentComponents.Count() - 1; i++)
                {
                    result += $"{Spaces(currentComponents[i])} ";
                }
                result += $"{currentComponents[currentComponents.Count() - 1]}\\";
            }
            return result;
        }
        public string Notify(params string[] args)
        {
            return "";
        }

        public string Run(string sessionId, params string[] args)
        {
            string result = "";
            string previos = "";
            VirtualFileSystem.Print().ForEach(d => {
                result += $"{trimPath(previos, d)}\n";
                VirtualFileSystem.PrintFiles(d, sessionId).ForEach(f =>
                {
                    result += $"{trimPath(d, f).Trim('\\')}\n";
                });
                previos = d;
            });
            return result;
        }
    }
}