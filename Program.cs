using System;
using System.Windows.Forms;

namespace Notes
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // .NET 9.0 uses Application.SetHighDpiMode for DPI awareness
            ApplicationConfiguration.Initialize();
            
            // Set up global exception handling
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            Application.Run(new frmMain());
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowErrorDialog(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowErrorDialog(e.ExceptionObject as Exception);
        }

        private static void ShowErrorDialog(Exception? ex)
        {
            // Log detailed exception information if logging is enabled
            if (Logger.CurrentLogLevel != LogLevel.None)
            {
                if (ex != null)
                {
                    Logger.Error("========== UNHANDLED EXCEPTION ==========");
                    Logger.Error($"Exception Type: {ex.GetType().FullName}");
                    Logger.Error($"Message: {ex.Message}");
                    Logger.Error($"Source: {ex.Source}");
                    Logger.Error($"Stack Trace:\n{ex.StackTrace}");
                    
                    // Log inner exceptions recursively
                    var innerEx = ex.InnerException;
                    int innerCount = 1;
                    while (innerEx != null)
                    {
                        Logger.Error($"--- Inner Exception #{innerCount} ---");
                        Logger.Error($"Type: {innerEx.GetType().FullName}");
                        Logger.Error($"Message: {innerEx.Message}");
                        Logger.Error($"Stack Trace:\n{innerEx.StackTrace}");
                        innerEx = innerEx.InnerException;
                        innerCount++;
                    }
                    
                    Logger.Error("=========================================");
                }
                else
                {
                    Logger.Error("========== UNHANDLED EXCEPTION ==========");
                    Logger.Error("Unknown error - no exception details available");
                    Logger.Error("=========================================");
                }
            }
            
            string message = string.Format("An unexpected error occurred:\n\n{0}\n\nThe application will continue running.", 
                ex != null ? ex.Message : "Unknown error");
            MessageBox.Show(message, NotesLibrary.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

