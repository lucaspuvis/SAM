using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SAM.IO;

namespace SAM.Classifiers
{
    /// <summary>
    /// The python classifier connects to a server and remotely evaluates. 
    /// </summary>
    class PythonClassifier : Interfaces.IEvaluator
    {
        private readonly TcpClient client;

        public PythonClassifier(TcpClient client)
        {
            this.client = client;
        }

        public (string, float) EvaluateComment(IEnumerable<string> tc)
        {
            throw new NotImplementedException();
        }

        public (string, float) EvaluateSentence(string sentence)
        {
            return RemoteEval(sentence.ToString());
        }
        /// <summary>
        /// Calls the python server to get the prediction
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Label,rating</returns>
        private (string,float) RemoteEval(string msg)
        {
            var stream = client.GetStream();

            var response = RemoteHandler.CallServer(stream, msg);

            if (response == "" || response == null)
                return ("Error",0);

            return Evaluation(float.Parse(response));

        }

        public IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string, float)> Evaluate(IEnumerable<string> sentences)
        {
            var results = new List<(string, float)>();

            throw new NotImplementedException();
        }


        public (string, float) Evaluation(float sentiment)
        {
            if (sentiment > 0) return (Program.InternConcurrentSafe("Positive"), sentiment);
            if (sentiment < 0) return (Program.InternConcurrentSafe("Negative"), sentiment);
            else return (Program.InternConcurrentSafe("Neutral"), sentiment);
        }
    }
}
