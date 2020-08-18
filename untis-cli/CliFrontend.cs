using System;
using System.Linq;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;

namespace UntisCli
{
    public class CliFrontend
    {
        public static void ShowRemainingLessonTime(UntisCache cache)
        {
            Period currentPeriod = UntisUtil.GetCurrentPeriod(cache.Periods);
            
            TimeSpan lessonEnd = DateTime.Now.TimeOfDay - currentPeriod.EndTime;
            Console.WriteLine(lessonEnd.Duration().ToString(@"hh\:mm\:ss"));
        }

        public static void ShowPeriodList(UntisCache untis)
        {
            foreach (var period in untis.Periods)
            {
                Console.WriteLine($"{period.Nr, 02:d}: {period.StartTime} - {period.EndTime}");
            }
        }

        public static void ShowNextLesson(UntisCache untis, UntisClient untisClient, string className)
        {
            if (className == null)
            {
                Console.Error.WriteLine("The class for the next lesson is not specified");
                return;
            }
            // Get my class
            SchoolClass untisClass = null;
            foreach(SchoolClass k in untis.Classes)
            {
                if (k.UniqueName.Equals(className, StringComparison.OrdinalIgnoreCase)) untisClass = k;
            }
            
            Period currentPeriod = UntisUtil.GetCurrentPeriod(untis.Periods);
            
            Lesson lesson = untisClient.GetLessons(untisClass).Result
                .Where(l => l.Date.Date == DateTime.Today.Date)
                .FirstOrDefault(l => l.Period.Nr == currentPeriod.Nr + 1);
            
            Console.WriteLine( lesson == null ? "FREE" : lesson.SubjectsString );

            untisClient.LogoutAsync();

        }
    }
}