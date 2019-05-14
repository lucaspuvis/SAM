namespace SAM
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Classifiers;
    using CommandLine;
    using static PipeController;
    using static PythonHandler;
    using static IO.RemoteHandler;
    using System.Diagnostics;
    using System.Threading;
    using System.Net.Sockets;
    using System.Reflection;

    public static class Program
    {
        private static string assembly_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string src_path = Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()));
        public static string src_dir { get; } = src_path.Substring(0, (src_path.Length) - 4);  // USE THIS IF RUNNING FROM VISUAL STUDIO!
        //public static string src_dir { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // USE THIS IF RUNNING FROM EXE!
        static void Main(string[] args)
        {
            var setting = JsonHandler.DeserializeSettingsFromFile(src_dir + "/Data/settings.json");
            //var setting = new Setting(threaded: true);
            string method = null;
            string filename = null;
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    switch (o.Classifier)
                    {
                        case "svm":
                            Console.WriteLine("Support Vector Machine on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "rb":
                            Console.WriteLine("Rule Based Classifier on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "rf":
                            Console.WriteLine("Random Forest on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "lstm":
                            Console.WriteLine("LSTM on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "random":
                            Console.WriteLine("Random Values on : " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "bl":
                            Console.WriteLine("Baseline Classifier on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "mml":
                            Console.WriteLine("Microsoft ML on: " + o.InputFile);
                            method = o.Classifier;
                            break;
                        case "data":
                            Console.WriteLine("Creating DataSet");
                            method = o.Classifier;
                            break;
                        default:
                            Console.WriteLine("Please provide a valid classifier");
                            System.Environment.Exit(1);
                            break;
                    }
                    filename = o.InputFile;
                });

            var watch = Stopwatch.StartNew();

            var tokenizer = new Tokenizer(setting);
            //var filename = "/Data/all_data.csv";
            int serverProcessId = -1;

            var testData = src_dir + "/Classifiers/TrainingData/test_data.csv";

            List<Result> threadedResult = new List<Result>();
            List<Result> comparissons = new List<Result>();
            Interfaces.IEvaluator evalr = new BaselineClassifer();

            switch (method)
            {
                case "prediction":
                case "nb":
                case "svr":
                case "svm":
                case "lstm":
                case "rf":
                    Console.WriteLine("Runnning clasifier: " + method);
                    setting.threaded = false; //program cannot run threaded if connected to server (yet)
                    serverProcessId = InitServer(method); // runs the PythonServer with argument parsed
                    evalr = new PythonClassifier(Connect());
                    break;
                case "rb":
                    Console.WriteLine("Runnning clasifier: " + method);
                    var lex = CSVReader.ReadLexicon(src_dir + "\\Data\\AgreedLexicon.csv");// lex should be created like this first and then parsed into a pipe, so that we can evaluate on the fly (like we're tokenizing)
                    var tlex = CSVReader.ReadTriggerLex(src_dir + "\\Data\\triggerwords.csv");
                    evalr = new RuleBasedClassifier(lex, setting, tlex);
                    break;
                case "random":
                    evalr = new RandomClassifier();
                    break;
                case "bl":
                    evalr = new BaselineClassifer();
                    break;
                case "mml":
                    evalr = new SentimentClassifier();
                    break;
                case "data":
                    var list = new List<(string, string)>();
                    foreach (var s in CSVReader.ReadComparrisonsYield(src_dir + "\\" + filename))
                        list.Add(s);
                    double count = list.Count * 0.1;
                    FileWriter.CreateRandomTrainingTest(list, (int)count, (int)(list.Count - count));
                    FileWriter.flushPrinter(setting);
                    Environment.Exit(0);
                    break;
            }

            if (setting.compare)
            {
                Console.WriteLine("Comparing...");
                comparissons = ConvertTestData(CSVReader.ReadComparrisonsYield, evalr, src_dir + filename).ToList();
                var results = Pipe_AccuracyTest(comparissons, evalr).ToList();
                var formatted = from result in results select result.ToString();
                addToOutput(formatted);
                Console.WriteLine(Environment.NewLine + Analyzer.MatrixWithNeutralToString(Analyzer.GetConfusion(results)));
                Console.WriteLine(Analyzer.GetAccuracy(results) + "% Accuracy");
            }
            else
            {
                Console.WriteLine("Predicting...");
                //Procedual evaluation
                if (!setting.threaded)
                {
                    var results = ProcedualSentenceLevelPipe(CSVReader.ReadCommentsYield, filename, evalr).ToList();
                    var formatted = from result in results select result.Value.ToString();
                    addToOutput(formatted);
                }

                //threaded evaluation
                if (setting.threaded)
                {
                    var result = ThreadedSentenceLevelPipe(CSVReader.ReadCommentsYield, filename, evalr).ToList();
                    var formatted = from val in result select val.Value.ToString();
                    addToOutput(formatted);
                }
            }

            FileWriter.flushPrinter(setting);

            watch.Stop();
            var elapsed = watch.ElapsedMilliseconds;
            Console.WriteLine("Completed in: " + elapsed + "Ms with "+method);

            Console.WriteLine("Go to PATH/out/your_time_stamp for the output");

            if (serverProcessId != -1)
            {
                try
                {
                    Process.GetProcessById(serverProcessId).Kill();
                }
                catch (SocketException)
                {
                    // succesfully closed
                }
            }

            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void addToOutput(IEnumerable<string> outputList)
        {
            foreach (string s in outputList)
                FileWriter.WriteOutput(s);
        }

        // https://stackoverflow.com/questions/6983714/locking-on-an-interned-string)
        // apperently String.Intern() is very slow, and it's therefore much faster to use custom dictionary.
        // We want concurrency control aswell, and I'm not sure String.Intern() is that?
        private static readonly ConcurrentDictionary<string, string> concSafe = new ConcurrentDictionary<string, string>();
        public static string InternConcurrentSafe(string s)
        {
            return concSafe.GetOrAdd(s, String.Copy);
        }
    }

    class Options
    {
        [Value(0, MetaName = "Classifier",
                  HelpText = "The classifier used for for predicting",
                  Required= true)]
        public string Classifier { get; set; }
        
        [Option('r', "read", Required = true, HelpText = "Input file to be processed.")]
        public string InputFile { get; set; }

    }
}

