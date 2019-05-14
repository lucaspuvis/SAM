using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;

namespace SAM
{
    static class FileWriter
    {
        static string dirPath = Program.src_dir + "\\out";
        static string time = (DateTime.UtcNow.ToShortDateString() + "_" + DateTime.UtcNow.ToLongTimeString()).Replace(":", ".");

        static DirectoryInfo dir = Directory.CreateDirectory(dirPath + "\\" + time);
        static string filePath = Program.src_dir + "\\out\\" + time + "\\";

        static ConcurrentDictionary<string, int> nonMarked = new ConcurrentDictionary<string, int>();
        static ConcurrentBag<string> output = new ConcurrentBag<string>();
        static ReaderWriterLock errorLocker = new ReaderWriterLock();

        /// <summary>
        /// Writes to the errorlog
        /// </summary>
        /// <param name="msg">is Object because of callback from threads</param>
        public static void WriteErrorLog(Object msg)
        {
            lock (errorLocker)
            {
                using (StreamWriter writer = new StreamWriter(filePath + "Error.log", true))
                {
                    writer.WriteLine(EncodeToUnicode16((string)msg));
                }
            }
        }
        // unique words + hit counter
        public static void WriteNonMatched(Object msg)
        {
            var smsg = msg.ToString();
            nonMarked.AddOrUpdate(smsg, 1, (id, count) => count + 1);

        }
        /// <summary>
        /// Adds msg to dictionary
        /// </summary>
        /// <param name="msg">is Object, because of callback from threads</param>
        public static void WriteOutput(Object msg)
        {
            var smsg = (string)msg;
            output.Add(EncodeToUnicode16(smsg));

        }
        /// <summary>
        /// Output data is converted to Unicode - else smileys will be lost
        /// </summary>
        /// <param name="input">string to be encoded from UTF8</param>
        /// <returns>Unicode encoded string</returns>
        public static string EncodeToUnicode16(string input)
        {
            var msgA = Encoding.UTF8.GetBytes(input);
            var converted = UnicodeEncoding.Convert(Encoding.UTF8, UnicodeEncoding.Unicode, msgA);
            return Encoding.Unicode.GetString(converted);
        }

        /// <summary>
        /// This method prints everything inside the dictionary
        /// </summary>
        /// <param name="setting"></param>
        public static void flushPrinter(Setting setting)
        {
            if (!nonMarked.IsEmpty && setting.writeNonMatchTokens)
            {

                var dictionary = new List<string>();

                foreach (KeyValuePair<string, int> pair in nonMarked)
                {
                    dictionary.Add(EncodeToUnicode16(pair.Value + "," + pair.Key));
                }

                File.WriteAllLines(filePath + "NonMatched.log", dictionary);
                nonMarked.Clear();
            }

            if (!output.IsEmpty)
            {
                File.WriteAllLines(filePath + "Result.log", output);
            }

        }

        /// <summary>
        /// This method takes a file of e.g 100 lines of testdata. It then splits it up into two lists of length int and int. These lists are randomly shuffled
        /// and then written to disc.
        /// </summary>
        /// <param name="loadedFile"></param>
        /// <param name="wishedTestAmount">How many lines of the data should be for testing</param>
        /// <param name="wishedTrainingAmount">How many lines of the data should be for training</param>

        public static void CreateRandomTrainingTest(IEnumerable<(string, string)> loadedFile, int wishedTestAmount, int wishedTrainingAmount)
        {

            var list = loadedFile.ToList();
            if (list.Count < wishedTestAmount + wishedTrainingAmount){
                WriteErrorLog("[createTraining] TestAmount and TrainingAmount exceeded actual amount");
                new ArgumentException("bad input");
            }

            list.Shuffle();

            var training = list.Take(wishedTrainingAmount);
            var formattedTrain = from res in training select EncodeToUnicode16(res.Item1+",\""+res.Item2+"\"");

            File.WriteAllLines(Program.src_dir + "\\Classifiers\\TrainingData\\training_data.csv", formattedTrain);

            var test = list.GetRange(list.Count-wishedTestAmount,wishedTestAmount);
            var formattedTest = from res in test select EncodeToUnicode16(res.Item1 + ",\"" + res.Item2+"\"");

            File.WriteAllLines(Program.src_dir + "\\Classifiers\\TrainingData\\test_data.csv", formattedTest);
        }


    }
    //https://stackoverflow.com/questions/273313/randomize-a-listt/1262619#1262619
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }

    static class MyExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
    

