using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SAM
{
    public class JsonHandler
    {
        public static void SerializeCommentsToFile(IEnumerable<JsonComment> comments, string filename)
        {
            using (StreamWriter file = File.CreateText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, comments);
            }
        }

        public static void SerializeSettingsToFile(Setting setting, string filename)
        {
            using (StreamWriter file = File.CreateText(filename))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, setting);
            }
        }

        public static IEnumerable<JsonComment> DeserializeComments(string json)
        {
            return JsonConvert.DeserializeObject<IEnumerable<JsonComment>>(json);
        }

        public static Setting DeserializeSettings(string json)
        {
            return JsonConvert.DeserializeObject<Setting>(json);
        }

        public static Setting DeserializeSettingsFromFile(string filepath)
        {
            return DeserializeSettings(ReadJson(filepath));
        }

        public static IEnumerable<JsonComment> DeserializeCommentsFromFile(string filepath)
        {
            return DeserializeComments(ReadJson(filepath));
        }

        public static string ReadJson(string json)
        {
            return File.ReadAllText(json);
        }
    }
}
