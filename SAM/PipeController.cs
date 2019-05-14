using SAM.Interfaces;
using SAM.PlaceHolders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using IOReadMethod = System.Func<string, System.Collections.Generic.IEnumerable<string>>;
using IOReadTestMethod = System.Func<string, System.Collections.Generic.IEnumerable<(string, string)>>;
using Pipe = System.Func<System.Func<string, System.Collections.Generic.IEnumerable<string>>, string, SAM.Tokenizer, SAM.Interfaces.IEvaluator, System.Collections.Generic.IDictionary<SAM.Interfaces.IIdentifier, SAM.Result>>;

namespace SAM
{
    /// <summary>
    /// The PipeController is the central of our railway-programming.
    /// It contains several different types of pipes to make many tasks easier.
    /// </summary>
    static class PipeController
    {

        public static IOReadMethod ReadCSVYield = f => CSVReader.ReadCommentsYield(f);
        //JSon versions of above

        //Takes a pipe(like ThreadedSentenceLevelPipe) and a ReadMethod like ReadCSVYield and produces an ConcurrentDictionary<IIdentifier,Result> (Identifier -> Resultobject)
        public static Func<Pipe, IOReadMethod, string, Tokenizer, IEvaluator, IDictionary<IIdentifier, Result>> Pipe_ReadFile_Tokenize_Evaluate = (pipe, rm, filename, tokenizer, evalr) => pipe(rm, filename, tokenizer, evalr);


        /// <summary>
        /// This method creates an IEnumerable(CResult). It compares the true vales of a sentence to the onces that the program predicts. 
        /// </summary>
        /// <param name="testData">a list of predicted data (result objects)</param>
        /// <param name="evalr">An IEvaluator</param>
        /// <returns>IEnumerable(AResults)</returns>
    
        public static IEnumerable<CResult> Pipe_AccuracyTest(IEnumerable<Result> testData, IEvaluator evalr)
        {
            var aresults = new List<CResult>();

            foreach (Result testResult in testData)
            {
                var original = testResult.identifier.getOriginal();
                var evaluation = evalr.EvaluateSentence(testResult.identifier.getOriginal());
                Result evaluatedResult = new Result(testResult.identifier,evaluation.Item1,evaluation.Item2);

                if (testResult.CompareTo(evaluatedResult) == 1)
                {
                    aresults.Add(new CResult(testResult, true));
                }
                else
                {
                    aresults.Add(new CResult(testResult, false, evaluatedResult.label));
                }
                
            }

            return aresults;
        }

        /// <summary>
        /// The pipe reads from a given readmethod and evaluates it. This is done so threaded. It waits for all threads to complete
        /// </summary>
        /// <param name="rm">A readmethod e.g JSON or .CSV</param>
        /// <param name="filename">Where to read from</param>
        /// <param name="evalr">An IEvaluator</param>
        /// <returns>A dictionary that maps identifiers to their predicted result</returns>

        public static IDictionary<IIdentifier, Result> ThreadedSentenceLevelPipe(IOReadMethod rm, string filename, IEvaluator evalr)
        {
            // idea from https://stackoverflow.com/questions/3720111/threadpool-queueuserworkitem-with-list
            var cBag = new ConcurrentDictionary<IIdentifier, Result>();

            using (var finished = new CountdownEvent(1))
            {

                foreach (string sentence in rm(Program.src_dir + "\\" + filename))
                {

                    var original = sentence;

                    finished.AddCount();
                    ThreadPool.QueueUserWorkItem(
                        (state) =>
                        {
                            try
                            {
                                var ts = evalr.EvaluateSentence(sentence.ToString());
                                var suid = new SUID(Thread.CurrentThread.ManagedThreadId + DateTime.Today.Millisecond, original);
                                var r = new Result(suid, ts.Item1, ts.Item2);
                                if (!cBag.TryAdd(suid, r))
                                    FileWriter.WriteErrorLog("[ThreadedPipe_Dictionary] failed at adding '" + suid + "' and it's result" + Environment.NewLine);
                            }
                            finally
                            {
                                finished.Signal();
                            }
                        }, null);
                }
                finished.Signal();
                finished.Wait();
            }
            return cBag;
        }

        /// <summary>
        /// reads the testdata in and predicts them.
        /// </summary>
        /// <param name="rm">Readmethod e.g JSON or .CSV</param>
        /// <param name="evalr">An IEvaluator</param>
        /// <param name="filename">Where to read from</param>
        /// <returns>IEnumerable of predicted testdata</returns>

        public static IEnumerable<Result> ConvertTestData(IOReadTestMethod rm, IEvaluator evalr, string filename)
        {
            var cBag = new ConcurrentBag<Result>();

            using (var finished = new CountdownEvent(1))
            {
                foreach ((string rating, string original) in rm(filename))
                {
                    finished.AddCount();
                    ThreadPool.QueueUserWorkItem(
                        (state) =>
                        {
                            try
                            {
                                cBag.Add(new Result((new TUID(Thread.CurrentThread.ManagedThreadId+DateTime.Today.Millisecond,original)), Program.InternConcurrentSafe(evalr.Evaluation(float.Parse(rating)).Item1), float.Parse(rating)));
                            }
                            finally
                            {
                                finished.Signal();
                            }
                        }, null);
                }
                finished.Signal();
                finished.Wait();
            }
            return cBag;


        }

        /// <summary>
        /// This is a pipe that evaluates one by one instead of doing so threaded. This is primarily used for debugging or when threaded evalution is not possible e.g Python server
        /// </summary>
        /// <param name="rm">ReadMethod e.g JSON or .CSV</param>
        /// <param name="filename">Where to read from</param>
        /// <param name="evalr">An IEvaluator</param>
        /// <returns>A dictionary that maps identifiers to their predicted result</returns>

        public static IDictionary<IIdentifier, Result> ProcedualSentenceLevelPipe(IOReadMethod rm, string filename, IEvaluator evalr)
        {
            var bag = new Dictionary<IIdentifier,Result>();
            var count = 0;

            foreach (string comment in rm(Program.src_dir + "\\" + filename))
            {
                var res = evalr.EvaluateSentence(comment);
                var r = new Result(new SUID(++count,comment), res.Item1, res.Item2);
                if (!bag.TryAdd(r.identifier, r))
                {
                    FileWriter.WriteErrorLog("[ProcedualPipe_Dictionary] failed at adding '" + r.identifier + "' and it's result" + Environment.NewLine);
                }
            }

            return bag;
        }
    }
}
