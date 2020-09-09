using System;
using System.Linq;
using UntisLibrary.Api;

namespace UntisCli
{
    public static class CliFrontend
    {
        public static void ShowRemainingLessonTime(UntisCache cache)
        {
            var currentPeriod = UntisUtil.GetCurrentPeriod(cache.Periods);

            var lessonEnd = DateTime.Now.TimeOfDay - currentPeriod.EndTime;
            Console.WriteLine(lessonEnd.Duration().ToString(@"hh\:mm\:ss"));
        }

        public static void ShowPeriodList(UntisCache untis)
        {
            foreach (var period in untis.Periods)
                Console.WriteLine($"{period.Nr,02:d}: {period.StartTime} - {period.EndTime}");
        }

        public static void ShowNextLesson(UntisCache cache, UntisClient untisClient, string className)
        {
            if (className == null)
            {
                Console.Error.WriteLine("The class for the next lesson is not specified");
                return;
            }

            // Get my class
            var untisClass = UntisUtil.GetSchoolClass(cache.Classes, className);

            var currentPeriod = UntisUtil.GetCurrentPeriod(cache.Periods);

            var lesson = untisClient.GetLessons(untisClass).Result
                .Where(l => l.Date.Date == DateTime.Today.Date)
                .FirstOrDefault(l => l.Period.Nr == currentPeriod.Nr + 1);

            Console.WriteLine(lesson == null ? "FREE" : lesson.SubjectsString);

            untisClient.LogoutAsync();
        }

        public static void ShowTimeTable(UntisCache cache, UntisClient untisClient, string className,
            string[] daysOfWeek)
        {
            if (className == null)
            {
                Console.Error.WriteLine("The class for the next lesson is not specified");
                return;
            }

            const int sidebarColumnWidth = 16;
            const int mainColumnWidth = 10;

            var untisClass = UntisUtil.GetSchoolClass(cache.Classes, className);
            var lessons = untisClient.GetLessons(untisClass).Result.ToList();

            // Print table head
            Console.Write($" {untisClass.UniqueName,-sidebarColumnWidth}|");
            for (var d = 0; d < 5; d++) // Loop through days of the week (columns)
                Console.Write($" {daysOfWeek[d],-mainColumnWidth}|");
            //Console.Write(lessons.Count);

            Console.WriteLine();

            // Print table body
            foreach (var period in cache.Periods) // Loop through periods (Rows)
            {
                Console.Write(
                    $"{period.Nr,02} {period.StartTime.Duration():hh\\:mm} - {period.StartTime.Duration():hh\\:mm} | ");
                foreach (var lesson in lessons.Where(l => l.Period.Nr == period.Nr))
                    // TODO: Complete timetable printout. Can't test until untis is fixed
                    Console.Write($" {lesson.Subject.DisplayName,-mainColumnWidth}|");

                Console.WriteLine();
            }
        }
    }
}