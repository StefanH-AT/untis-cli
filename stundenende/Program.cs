using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;

namespace stundenende
{
    class Program
    {
        static void Main(string[] args)
        {
            string configPath = args.Length > 0 ? args[0] : "config.json";
            
            string configText = File.ReadAllText(configPath);
            Config config = JsonConvert.DeserializeObject<Config>(configText);
            
            // Set up web untis connection
            UntisClient untis = new UntisClient(config.server, config.schoolName);
            if (untis.TryLoginAsync(config.user, config.pass).Result)
            {
                // Get current period
                TimeSpan difference = TimeSpan.Zero;
                List<Period> periods = new List<Period>(untis.Periods.Result);
                int currentPeriodIndex = 0;
                foreach (var period in periods)
                {
                    difference = DateTime.Now.TimeOfDay - period.EndTime;
                    currentPeriodIndex++;
                    if (DateTime.Now.TimeOfDay < period.EndTime) break;
                }
                
                // Exit if last period
                if (periods.Count == currentPeriodIndex + 1)
                {
                    Console.Write("NO PERIOD");
                }
                else
                {
                    // Get my class
                    SchoolClass klasse = null;
                    foreach(SchoolClass k in untis.Classes.Result)
                    {
                        if (k.UniqueName == config.className) klasse = k;
                    }
                    
                    Lesson lesson = untis.GetLessons(klasse).Result
                                    .Where(l => l.Date.Date == DateTime.Today.Date)
                                    .FirstOrDefault(l => l.Period.Nr == currentPeriodIndex + 1);
                    
                    // Love this line!
                    Console.Write( $"{currentPeriodIndex}: {(lesson == null ? "FREE" : lesson.SubjectsString)} in {difference.Duration().ToString(@"mm\:ss")}" );

                }
                
                    
                untis.LogoutAsync();
            }

        }
    }
}