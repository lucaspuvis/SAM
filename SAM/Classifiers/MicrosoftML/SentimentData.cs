using Microsoft.ML.Data;

namespace SAM.Classifiers
{
    public class SentimentData
    {
        [LoadColumn(0)]
        public string SentimentValue { get; set; }

        [LoadColumn(1)]
        public string SentimentText { get; set; }
    }

    public class SentimentPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }
    }

}
