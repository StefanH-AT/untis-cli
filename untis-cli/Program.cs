using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;
using Utility.CommandLine;

namespace UntisCli
{
    class Program
    {
        // =========================================
        // CONSTANTS
        // =========================================

        private const string DEFAULT_CONFIG_NAME = "config.json";
        
        // =========================================
        // CLI PARAMS
        // =========================================
        // Switches
        [Argument('p', "periods", "Lists all periods")]
        private static bool ArgListPeriods { get; set; }
        [Argument('h', "help", "Shows help")]
        private static bool ArgHelp { get; set; }
        [Argument('r', "remaining", "Prints lesson time remaining")]
        private static bool ArgRemaining { get; set; }
        [Argument('n', "next-lesson", "Prints the upcoming lesson")]
        private static bool ArgNextLesson { get; set; }
        // Arguments
        [Argument('c', "config", "Set the config file path. Default: ./config.json")]
        private static string ArgConfigPath { get; set; }
        [Argument(' ', "class", "Set the class to fetch data from")]
        private static string ArgClass { get; set; }
        
        // =========================================
        // GLOBAL VARS
        // =========================================
        
        private static DateTime currentTime = DateTime.Now;

        static void Main(string[] args)
        {
            Arguments.Populate();
            
            // ========================================
            // Switch actions that don't require untis
            // ----------------------------------------
            
            if (ArgHelp)
            {
                ShowHelp();
            }

            // =============================================
            // Switch actions that require untis connection
            // ---------------------------------------------

            if (ArgRemaining)
            {
                ShowRemainingLessonTime(ConnectUntis());
            }
            
            if (ArgListPeriods)
            {
                ShowPeriodList(ConnectUntis());
            }

            if (ArgNextLesson)
            {
                ShowNextLesson(ConnectUntis());
            }

        }

        private static void ShowHelp()
        {
            
            Console.WriteLine("untis-cli. WebUntis cli tool");
            Console.WriteLine("============================");
            
            foreach(ArgumentInfo info in Arguments.GetArgumentInfo(typeof(Program)))
            {   //                                               |
                //                Very pretty code right here :) v
                Console.WriteLine($"  {(info.ShortName == ' ' ? "" : "-" + info.ShortName), -3} | --{info.LongName, -14}| {info.HelpText,-60}");
            }
            
        }

        

        private static void ShowRemainingLessonTime(UntisClient untis)
        {
            Period currentPeriod = GetCurrentPeriod(untis.Periods.Result);
            
            TimeSpan lessonEnd = currentTime.TimeOfDay - currentPeriod.EndTime;
            Console.WriteLine(lessonEnd.Duration().ToString(@"hh\:mm\:ss"));

            untis.LogoutAsync();
 
        }

        private static void ShowPeriodList(UntisClient untis)
        {
            foreach (var period in untis.Periods.Result)
            {
                Console.WriteLine($"{period.Nr, 02:d}: {period.StartTime} - {period.EndTime}");
            }

            untis.LogoutAsync();
        }

        private static void ShowNextLesson(UntisClient untis)
        {
            if (ArgClass == null)
            {
                Console.Error.WriteLine("The class for the next lesson is not specified");
                return;
            }
            // Get my class
            SchoolClass untisClass = null;
            foreach(SchoolClass k in untis.Classes.Result)
            {
                if (k.UniqueName.Equals(ArgClass, StringComparison.OrdinalIgnoreCase)) untisClass = k;
            }
            
            Period currentPeriod = GetCurrentPeriod(untis.Periods.Result);
            
            Lesson lesson = untis.GetLessons(untisClass).Result
                .Where(l => l.Date.Date == DateTime.Today.Date)
                .FirstOrDefault(l => l.Period.Nr == currentPeriod.Nr + 1);
            
            Console.WriteLine( lesson == null ? "FREE" : lesson.SubjectsString );

        }
        
        // ==============================
        //    UTILS
        // ==============================
        
        
        private static UntisClient ConnectUntis()
        {
            // Try to read config
            string configText = File.ReadAllText(ArgConfigPath?.Length > 0 ? ArgConfigPath : DEFAULT_CONFIG_NAME);
            if (configText.Length == 0)
            {
                Console.Error.WriteLine("Failed to load config");
                return null;
            }
            // Try to parse config
            Config config = JsonConvert.DeserializeObject<Config>(configText);
            
            // Try to connect to untis
            UntisClient untisClient = new UntisClient(config.server, config.schoolName);
            if (untisClient.TryLoginAsync(config.user, config.pass).Result) return untisClient;
            
            Console.Error.WriteLine("Failed to log into untis");
            return null;

        }

        private static Period GetCurrentPeriod(IEnumerable<Period> periods)
        {
            foreach (Period period in periods)
            {
                if (DateTime.Now.TimeOfDay < period.EndTime)
                {
                    return period;
                }
            }

            return null;
        }
        
    }
}