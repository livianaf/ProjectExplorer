using System;
using System.IO;
using System.Reflection;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class LogHelper {
        #region private section
        private static string folderName { get; } = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().ManifestModule.Name);
        private static string fullFolderName { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
        private static string mLogFileName { get; } = Path.Combine(fullFolderName, "Logs_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
        private static string mLogFileNameThread2 { get; } = Path.Combine(fullFolderName, "Logs_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_2.log");
        #endregion
        #region public interface
        public static bool Enabled { set; get; } = false;
        //_____________________________________________________________________________________________________________________________________________________________
        public static void CleanOldLogs() {
            var dir = new DirectoryInfo(fullFolderName);
            foreach (var file in dir.EnumerateFiles("Logs_*.log")) {
                if (file.Name.Contains(DateTime.Now.ToString("yyyyMMdd_"))) continue;
                try { file.Delete(); } catch { }
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void Add(string s) {
            if (!Enabled) return;
            string msg = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")};{s}\r\n";
            try { File.AppendAllText(mLogFileName, msg); }
            catch (Exception ex) {
                msg = $"{msg} Add-EXCEPTION: {ex.Message}\r\n";
                foreach (var k in ex.Data.Keys) msg += $" - Data: {k}{ex.Data[k]}\r\n";
                msg += $" - Source: {ex.Source}\r\n";
                msg += $" - StackTrace: {ex.StackTrace}\r\n";
                msg += $" - TargetSite: {ex.TargetSite}\r\n";
                msg += $" - InnerException: {ex.InnerException}\r\n";
                File.AppendAllText(mLogFileNameThread2, msg);
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void AddException(Exception ex, string caller, string details) {
            if (!Enabled) return;
            string msg = $"{DateTime.Now.ToString("yyyyMMdd_HHmmss")};EXCEPTION;{caller};{details};{ex.GetType()};{ex.Message}\r\n";
            foreach (var k in ex.Data.Keys) msg += $" - Data: {k}{ex.Data[k]}\r\n";
            msg += $" - Source: {ex.Source}\r\n";
            msg += $" - StackTrace: {ex.StackTrace}\r\n";
            msg += $" - TargetSite: {ex.TargetSite}\r\n";
            msg += $" - InnerException: {ex.InnerException}\r\n";

            try { File.AppendAllText(mLogFileName, msg); }
            catch (Exception e) {
                msg = $"{msg} AddException-EXCEPTION: {e.Message}\r\n";
                foreach (var k in e.Data.Keys) msg += $" - Data: {k}{e.Data[k]}\r\n";
                msg += $" - Source: {e.Source}\r\n";
                msg += $" - StackTrace: {e.StackTrace}\r\n";
                msg += $" - TargetSite: {e.TargetSite}\r\n";
                msg += $" - InnerException: {e.InnerException}\r\n";
                File.AppendAllText(mLogFileNameThread2, msg);
                }
            }
        #endregion
        }
    }
