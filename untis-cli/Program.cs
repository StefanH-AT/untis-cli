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

        private static readonly string HomeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static readonly string ConfigDir = HomeDir + "/.config/untis-cli/";
        private static readonly string CacheDir = HomeDir + "/.cache/untis-cli/";

        private static readonly string ConfigFile = ConfigDir + "config.json";
        private static readonly string CacheFile = CacheDir + "cache.json";

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

        [Argument('t', "table", "Prints the timetable for that class. Requires --class!")]
        public static bool ArgTable { get; set; }

        // Arguments
        [Argument('c', "class", "Set the class to fetch data from")]
        private static string ArgClass { get; set; }

        private static void Main(string[] args)
        {
            Arguments.Populate();

            // Create cache and config dirs & files
            Directory.CreateDirectory(CacheDir);
            Directory.CreateDirectory(ConfigDir);
            if (!File.Exists(ConfigFile))
                using (var writer = File.CreateText(ConfigFile))
                {
                    var templateConfig = new Config();
                    templateConfig.user = "Your username";
                    templateConfig.pass = "Your password";
                    templateConfig.server = "neilo.webuntis.com";
                    templateConfig.schoolName = "Spengergasse";
                    templateConfig.dayOfWeekLabels = new[] {"Mon", "Die", "Mit", "Don", "Fre"};

                    writer.Write(JsonConvert.SerializeObject(templateConfig, Formatting.Indented));
                }
            // ========================================
            // Switch actions that don't require untis
            // ----------------------------------------

            if (ArgHelp) ShowHelp();

            // =============================================
            // Switch actions that require untis connection
            // ---------------------------------------------

            // Try to read config
            var configText = File.ReadAllText(ConfigFile);
            if (configText == null)
            {
                Console.Error.WriteLine("Failed to load config");
                return;
            }

            // Try to parse config
            var config = JsonConvert.DeserializeObject<Config>(configText);

            // Read the cache
            UntisCache cache;

            if (ArgRefreshCache)
            {
                var untisClient = UntisUtil.ConnectUntis(config);
                cache = UntisCache.DownloadCache(untisClient);
                untisClient.LogoutAsync();
                LogVerbose("Refreshed the cache");
                cache.WriteCache(CacheFile);
                LogVerbose("Wrote cache to disk");
            }
            else
            {
                cache = UntisCache.ReadCache(CacheFile);
                LogVerbose("Reading cache");
            }

            if (ArgRemaining) CliFrontend.ShowRemainingLessonTime(cache);

            if (ArgListPeriods) CliFrontend.ShowPeriodList(cache);

            if (ArgNextLesson) CliFrontend.ShowNextLesson(cache, UntisUtil.ConnectUntis(config), ArgClass);

            if (ArgTable)
                CliFrontend.ShowTimeTable(cache, UntisUtil.ConnectUntis(config), ArgClass, config.dayOfWeekLabels);
        }

        // ======================================================

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