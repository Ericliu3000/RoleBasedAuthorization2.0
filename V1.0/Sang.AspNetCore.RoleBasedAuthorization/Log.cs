using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace Sang.AspNetCore.RoleBasedAuthorization
{
    static class Log
    {
        public static string LogsDirectory { get; set; }
        public static string LogsDirectoryPrefix       { get; set; }
        private static readonly Encoding TransferFormat = Encoding.GetEncoding("iso-8859-1");

        #region 定义日志类型枚举
        public enum LogType
        {

            Error = 0,
            Warning = 1,
            Information = 2
        }
        #endregion
        #region 静态构造方法
        static Log()
        {
            //GraphBuffer = new byte[0];
            //IsWriteLog = false;
            LogsDirectoryPrefix = System.AppDomain.CurrentDomain.BaseDirectory;
            LogsDirectory = string.Format("{0}{1}\\{2}", LogsDirectoryPrefix , "logs", DateTime.Now.ToString("yyyyMM")); ;
        }
        #endregion
        public static void WriteLog(string text, LogType logType)
        {
            LogsDirectory = string.Format("{0}{1}\\{2}", LogsDirectoryPrefix, "logs", DateTime.Now.ToString("yyyyMM")); ;

            string startTag = string.Format("\r\n{0}{1}\r\n{2}\r\n", logType, new string('=', 80), DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss"));
            string path = string.Format("{0}\\Log{1}.log", LogsDirectory, DateTime.Now.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(LogsDirectory))
            {
                Directory.CreateDirectory(LogsDirectory);
            }
            File.AppendAllText(path, string.Format("{0}{1}\r\n", startTag, text), Encoding.Default);
           
        }
        public static  void CopyAndSavetoCSVFile(string SourceFile,string Barcode,string Prefix,string FilePath,string[] Paras)
        {
            try
            {
                string path = "data";
                string tempfile = string.Format("{0}\\{1}{2}.csv", "data", Prefix, Barcode);
                /*
                 * 去除 路径的 末尾的  \
                 */
                FilePath = FilePath.Trim();
                if (FilePath.Substring(FilePath.Length - 1, 1) == "\\")
                    FilePath = FilePath.Substring(0, FilePath.Length - 1);
                System.Diagnostics.Debug.Print(FilePath);

                string destfile = string.Format("{0}\\{1}{2}.csv",FilePath, Prefix, Barcode);
                string text = "";
                foreach (string p in Paras)
                {
                    text = string.Format("{0};{1}", text, p);

                }
                text = text.Substring(1, text.Length - 1);
                        
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                //if (File.Exists(tempfile))
                //    File.Delete(tempfile);

                if (File.Exists(SourceFile))
                {
                    File.Copy(SourceFile, tempfile, true);
                    File.AppendAllText(tempfile, string.Format("{0}\r\n",  text), Encoding.Default);
                    File.Copy(tempfile, destfile, true);
                    File.Delete(tempfile);
                }
                else
                {
                    WriteLog(string.Format("{0} {1}\r\n", SourceFile,"File not Exist!"), 0);
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString(), 0);
            }
        }
        
    }
}
