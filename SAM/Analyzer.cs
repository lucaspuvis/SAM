using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SAM
{
    /// <summary>
    /// Holds statics methods to analyze CResults
    /// </summary>
    static class Analyzer
    {
        /// <summary>
        /// Takes the size of the list, counts how many was correctly predicted. Returns the procent of correctly predicted
        /// </summary>
        /// <param name="aresults"></param>
        /// <returns>percentage of correctly predicted</returns>
        public static float GetAccuracy(IEnumerable<CResult> aresults)
        {
            float trueCount = aresults.Count(x => x.hit)*100;

            return trueCount/aresults.ToList().Count;
        }

        /// <summary>
        /// Creates a 3 by 3 confusion matrix of the predicted values vs. was they were supposed to be
        /// </summary>
        /// <param name="aresults"></param>
        /// <returns>a 3 by 3 confusion matrix</returns>

        public static int[,] GetConfusion(IEnumerable<CResult> aresults)
        {
            //true _ predicted
            int pos_positive = 0;
            int pos_neutral = 0;
            int pos_negative = 0;
            int neu_positive = 0;
            int neu_neutral = 0;
            int neu_negative = 0;
            int neg_positive = 0;
            int neg_neutral = 0;
            int neg_negative = 0;


            foreach (CResult r in aresults)
            {

                if (r.label == "Positive")
                {
                    if (r.hit) { ++pos_positive; continue; }
                    if (r.actual == "Negative") { ++pos_negative; continue; }
                    else
                    {
                        ++pos_neutral;
                        continue;
                    }

                }
                if (r.label == "Negative")
                {
                    if (r.hit) { ++neg_negative; continue; }
                    if (r.actual == "Positive") { ++neg_positive; continue; }
                    else
                    {
                        ++neg_neutral;
                        continue;
                    }
                }
                if (r.label == "Neutral")
                {
                    if (r.hit) { ++neu_neutral; continue; }
                    if (r.actual == "Positive") { ++neu_positive; continue; }
                    else
                    {
                        ++neu_negative;
                        continue;
                    }
                }
            }
            var matrix = new int[,] { { neg_negative, neg_neutral, neg_positive},
                                      { neu_negative, neu_neutral, neu_positive},
                                      { pos_negative, pos_neutral, pos_positive} };

            return matrix;
        }

        /// <summary>
        /// Converts the confusion matrix to a nicely formatted string
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>nicely formatted string of confusion matrix</returns>

        public static string MatrixWithNeutralToString(int[,] matrix)
        {

            var sb = new StringBuilder();
            
            sb.Append("T: Annotated/Test, P: Predicted" + Environment.NewLine);
            sb.AppendFormat("             |   P: Negative   |   P: Neutral   |   P: Positive" + Environment.NewLine);
            sb.AppendFormat(" T: Negative |      {0}|     {1}|      {2}" + Environment.NewLine, NormalizeSpaces(matrix[0, 0]), NormalizeSpaces(matrix[0, 1]), NormalizeSpaces(matrix[0, 2]));
            sb.AppendFormat(" T: Neutral  |      {0}|     {1}|      {2}" + Environment.NewLine, NormalizeSpaces(matrix[1, 0]), NormalizeSpaces(matrix[1, 1]), NormalizeSpaces(matrix[1, 2]));
            sb.AppendFormat(" T: Positive |      {0}|     {1}|      {2}" + Environment.NewLine, NormalizeSpaces(matrix[2, 0]), NormalizeSpaces(matrix[2, 1]), NormalizeSpaces(matrix[2, 2]));


            return sb.ToString();
        }

        /// <summary>
        /// Converts the confusion matrix to a nicely formatted string with only neg/pos
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns>nicely formatted string of confusion matrix</returns>

        public static string MatrixNoNeutralToString(int[,] matrix)
        {
            var sb = new StringBuilder();
            sb.Append("T: Annotated/Test, P: Predicted" + Environment.NewLine);
            sb.AppendFormat("             |   P: Negative   |   P: Positive" + Environment.NewLine);
            sb.AppendFormat(" T: Negative |      {0}|      {2}" + Environment.NewLine, NormalizeSpaces(matrix[0, 0]), NormalizeSpaces(matrix[0, 1]), NormalizeSpaces(matrix[0, 2]));
            sb.AppendFormat(" T: Positive |      {0}|      {2}" + Environment.NewLine, NormalizeSpaces(matrix[2, 0]), NormalizeSpaces(matrix[2, 1]), NormalizeSpaces(matrix[2, 2]));

            return sb.ToString();
        }

        /// <summary>
        /// This method makes sure that the string lengths (string length of the integer value) are normalized, so that we can nicely format the confusion matrix. (neg/neu/pos)
        /// </summary>
        /// <param name="s">string to normalize</param>
        /// <returns>a normalized string (10 chars)</returns>

        private static string NormalizeSpaces(int s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i <= 10 - s.ToString().Length; i++) { sb.Append(" "); }
            return s+sb.ToString();
        }

    }
}
