using System;
using System.Collections.Generic;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;

namespace UntisCli
{
    internal static class UntisUtil
    {
        public static Period GetCurrentPeriod(IEnumerable<Period> periods)
        {
            foreach (var period in periods)
                if (DateTime.Now.TimeOfDay < period.EndTime)
                    return period;

            return null;
        }

        public static SchoolClass GetSchoolClass(List<SchoolClass> classes, string className)
        {
            return classes.Find(c => c.UniqueName.Equals(className, StringComparison.OrdinalIgnoreCase));
        }

        public static UntisClient ConnectUntis(Config config)
        {
            Program.LogVerbose("Opening Untis Connection");

            // Try to connect to untis
            var untisClient = new UntisClient(config.server, config.schoolName);
            if (untisClient.TryLoginAsync(config.user, config.pass).Result) return untisClient;

            Console.Error.WriteLine("Failed to log into untis");
            return null;
        }
    }
}