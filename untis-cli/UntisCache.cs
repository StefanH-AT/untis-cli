using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UntisLibrary.Api;
using UntisLibrary.Api.Entities;

namespace UntisCli
{
    public class UntisCache
    {
        
        public string SchoolName { get; set; }
        public string ServerAddress { get; set;  }
        public List<SchoolClass> Classes { get; set;  }
        public List<Teacher> Teachers { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Period> Periods { get; set; }

        private UntisCache()
        {
            
        }

        public static UntisCache ReadCache(string cacheFile)
        {
            return JsonConvert.DeserializeObject<UntisCache>(File.ReadAllText(cacheFile));
        }
        
        public static UntisCache DownloadCache(UntisClient untis)
        {
            UntisCache untisCache = new UntisCache();
            untisCache.RefreshAll(untis);
            return untisCache;
        }

        public void WriteCache(string cacheFile)
        {
            using (StreamWriter writer = File.CreateText(cacheFile))
            {
                writer.Write(JsonConvert.SerializeObject(this));
            }
        }

        public void RefreshAll(UntisClient untis)
        {
            RefreshMeta(untis);
            RefreshClasses(untis);
            RefreshTeachers(untis);
            RefreshRooms(untis);
            RefreshSubjects(untis);
            RefreshPeriods(untis);
        }

        public void RefreshMeta(UntisClient untis)
        {
            SchoolName = untis.School;
            ServerAddress = untis.Server;
        }

        public void RefreshClasses(UntisClient untis)
        {
            Classes = untis.Classes.Result.ToList();
        }
        
        public void RefreshTeachers(UntisClient untis)
        {
            Teachers = untis.Teachers.Result.ToList();
        }
        
        public void RefreshSubjects(UntisClient untis)
        {
            Subjects = untis.Subjects.Result.ToList();
        }
        
        public void RefreshRooms(UntisClient untis)
        {
            Rooms = untis.Rooms.Result.ToList();
        }
        
        public void RefreshPeriods(UntisClient untis)
        {
            Periods = untis.Periods.Result.ToList();
        }
        
    }
}