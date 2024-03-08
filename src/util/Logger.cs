using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MareAssetDetector.Utilities
{
    // CompilePal's Logger, but its more simplified as this is just a CLI application.
    static class Logger
    {
        private static readonly string LOG_FILE = "./asset_detector.log";
        private static StringBuilder lineBuffer = new StringBuilder();
        private static List<string> tempText = new List<string>();

        static Logger()
        {
            File.Delete(LOG_FILE);

            LogLine($"--- Mare's Source Tools BSP Asset Detector ---");
            LogLine($"Runtime: {RuntimeInformation.RuntimeIdentifier}");
            LogLine($"Locale: {CultureInfo.CurrentCulture.Name}");
            LogLine($"OS: {RuntimeInformation.OSDescription}");
        }

        public static string LogColor(string s, ConsoleColor c, params object[] formatStrings)
        {
            string text = s;
            if (formatStrings.Length != 0)
            {
                text = string.Format(s, formatStrings);
            }

            try
            {
                File.AppendAllText(LOG_FILE, text);
            }
            catch {}

            Console.ForegroundColor = c;
            Console.Write(text);
            return text;
        }

        public static string LogLineColor(string s, ConsoleColor c, params object[] formatStrings)
        {
            return LogColor(s + Environment.NewLine, c, formatStrings);
        }

        public static string Log(string s = "", params object[] formatStrings)
        {
            return LogColor(s, ConsoleColor.White, formatStrings);
        }

        public static string LogLine(string s = "", params object[] formatStrings)
        {
            return Log(s + Environment.NewLine, formatStrings);
        }
        public static void LogDebug(string s)
        {
            #if DEBUG
            try
            {
                File.AppendAllText(LOG_FILE, s);
            } catch {}
            #endif
            if (Context.Verbose)
            {
                Console.Write(s);
            }
        }

        public static void LogLineDebug(string s)
        {
            LogDebug(s + Environment.NewLine);
        }

        public static void LogError(string errorText)
        {
            LogColor(errorText, ConsoleColor.Red);
            File.AppendAllText(LOG_FILE, errorText);
        }
        public static void LogLineError(string errorText)
        {
            LogError(errorText + Environment.NewLine);
        }

        public static void LogProgressive(string s)
        {
            lineBuffer.Append(s);

            if (!s.Contains("\n"))
            {
                string? log = Log(s);
                if (log!= null)
                {
                    tempText.Add(log);
                }
            }

            List<string> lines = lineBuffer.ToString().Split("\r\n").ToList();

            string suffixText = lines.Last();
            lineBuffer = new StringBuilder(suffixText);

            for (int i=0; i < lines.Count - 1; i++)
            {
                LogLine(lines[i]);
            }

            if (suffixText.Length > 0)
            {
                string? log = Log(suffixText);
                if (log != null)
                {
                    tempText = new List<string> { log };
                }
            }
        }
    }
}