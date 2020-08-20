using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;

namespace UntisCli
{
    internal class UntisUtil
    {
        public static Period GetCurrentPeriod(IEnumerable<Period> periods)
        {
            foreach (var period in periods)
                if (DateTime.Now.TimeOfDay < period.EndTime)
                    return period;

            return null;
        }

        public static UntisClient ConnectUntis(string configPath)
        {
            Program.LogVerbose("Opening Untis Connection");
            // Try to read config
            var configText = File.ReadAllText(configPath);
            if (configText == null)
            {
                Console.Error.WriteLine("Failed to load config");
                return null;
            }

            // Try to parse config
            var config = JsonConvert.DeserializeObject<Config>(configText);

            // Try to connect to untis
            var untisClient = new UntisClient(config.server, config.schoolName);
            if (untisClient.TryLoginAsync(config.user, config.pass).Result) return untisClient;

            Console.Error.WriteLine("Failed to log into untis");
            return null;
        }
    }
}