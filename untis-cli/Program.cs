using System;
using System.IO;
using Newtonsoft.Json;
using Utility.CommandLine;

namespace UntisCli
{
    internal class Program
    {
        // =========================================
        // CONSTANTS
        // =========================================

        private static readonly string HOME_DIR = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public static string CONFIG_DIR = HOME_DIR + "/.config/untis-cli/";
        public static string CACHE_DIR = HOME_DIR + "/.cache/untis-cli/";

        public static string CONFIG_FILE = CONFIG_DIR + "config.json";
        public static string CACHE_FILE = CACHE_DIR + "cache.json";

        // =========================================
        // CLI PARAMS
        // =========================================
        // Switches
        [Argument('p', "periods", "Lists all periods")]
        private static bool ArgListPeriods { get; set; }

        [Argument('h', "help", "Shows help")] private static bool ArgHelp { get; set; }

        [Argument('r', "remaining", "Prints lesson time remaining")]
        private static bool ArgRemaining { get; set; }

        [Argument('n', "next-lesson", "Prints the upcoming lesson")]
        private static bool ArgNextLesson { get; set; }

        [Argument('f', "refresh-cache", "Refreshes the cache")]
        private static bool ArgRefreshCache { get; set; }

        [Argument('v', "verbose", "Verbose. Log more stuff")]
        public static bool ArgVerbose { get; set; }

        // Arguments
        [Argument(' ', "class", "Set the class to fetch data from")]
        private static string ArgClass { get; set; }

        private static void Main(string[] args)
        {
            Arguments.Populate();

            // Create cache and config dirs & files
            Directory.CreateDirectory(CACHE_DIR);
            Directory.CreateDirectory(CONFIG_DIR);
            if (!File.Exists(CONFIG_FILE))
                using (var writer = File.CreateText(CONFIG_FILE))
                {
                    var templateConfig = new Config();
                    templateConfig.user = "Your username";
                    templateConfig.pass = "Your password";
                    templateConfig.server = "neilo.webuntis.com";
                    templateConfig.schoolName = "Spengergasse";

                    writer.Write(JsonConvert.SerializeObject(templateConfig, Formatting.Indented));
                }
            // ========================================
            // Switch actions that don't require untis
            // ----------------------------------------

            if (ArgHelp) ShowHelp();

            // =============================================
            // Switch actions that require untis connection
            // ---------------------------------------------

            // Read the cache
            UntisCache cache;

            if (ArgRefreshCache)
            {
                var untisClient = UntisUtil.ConnectUntis(CONFIG_FILE);
                cache = UntisCache.DownloadCache(untisClient);
                untisClient.LogoutAsync();
                LogVerbose("Refreshed the cache");
                cache.WriteCache(CACHE_FILE);
                LogVerbose("Wrote cache to disk");
            }
            else
            {
                cache = UntisCache.ReadCache(CACHE_FILE);
            }

            if (ArgRemaining) CliFrontend.ShowRemainingLessonTime(cache);

            if (ArgListPeriods) CliFrontend.ShowPeriodList(cache);

            if (ArgNextLesson) CliFrontend.ShowNextLesson(cache, UntisUtil.ConnectUntis(CONFIG_FILE), ArgClass);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("untis-cli. WebUntis cli tool");
            Console.WriteLine("============================");

            foreach (var info in Arguments.GetArgumentInfo(typeof(Program)))
                //                                               |
                //                Very pretty code right here :) v
                Console.WriteLine(
                    $"  {(info.ShortName == ' ' ? "" : "-" + info.ShortName),-3} | --{info.LongName,-14}| {info.HelpText,-60}");
        }


        // ==============================
        //    UTILS
        // ==============================

        public static void LogVerbose(string message)
        {
            if (ArgVerbose) Console.WriteLine(message);
        }
    }
}