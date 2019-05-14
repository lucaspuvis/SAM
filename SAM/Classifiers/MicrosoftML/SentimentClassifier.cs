using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Data;
using SAM.Interfaces;

namespace SAM.Classifiers
{
    class SentimentClassifier : IEvaluator
    {
        private static readonly string _trainDataPath = Program.src_dir + "/Classifiers/TrainingData/training_data.csv";
        private static readonly string _testDataPath = Program.src_dir + "/Classifiers/TrainingData/test_data.csv";
        private static readonly string _modelPath = Program.src_dir + "/Classifiers/Model/SentimentModel.zip";

        private static MLContext _mlContext;
        private static PredictionEngine<SentimentData, SentimentPrediction> _predEngine;
        private static ITransformer _trainedModel;
        static IDataView _trainingDataView;


        // Initiate the classifier
        public SentimentClassifier()
        {
            _mlContext = new MLContext();

            // If a trained model exists, load that, otherwise retrain
            if (File.Exists(_modelPath))
            {
                using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _trainedModel = _mlContext.Model.Load(stream);
                }
            }
            else
            {
                _trainingDataView = LoadLabeledData(_trainDataPath);
                var pipeline = MakePipeline();
                _trainedModel = BuildAndTrainModel(_trainingDataView, pipeline);
            }

            _predEngine = _trainedModel.CreatePredictionEngine<SentimentData, SentimentPrediction>(_mlContext);
        }

        // Used to calculate accuracy of the classifier
        private void Evaluate()
        {
            var testDataView = LoadLabeledData(_testDataPath);

            var testMetrics = _mlContext.MulticlassClassification.Evaluate(_trainedModel.Transform(testDataView));

            Console.WriteLine($"*************************************************************************************************************");
            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");
            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");
            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.AccuracyMicro:0.###}");
            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.AccuracyMacro:0.###}");
            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:#.###}");
            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:#.###}");
            Console.WriteLine($"*************************************************************************************************************");
        }

        private ITransformer BuildAndTrainModel(IDataView trainingDataView, IEstimator<ITransformer> pipeline)
        {
            var trainingPipeline = pipeline.Append(_mlContext.MulticlassClassification.Trainers.StochasticDualCoordinateAscent(DefaultColumnNames.Label, DefaultColumnNames.Features))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            _trainedModel = trainingPipeline.Fit(trainingDataView);

            _predEngine = _trainedModel.CreatePredictionEngine<SentimentData, SentimentPrediction>(_mlContext);

            SaveModelAsFile(_mlContext, _trainedModel);

            return _trainedModel;
        }

        private void SaveModelAsFile(MLContext mlContext, ITransformer model)
        {
            using (var fs = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                _mlContext.Model.Save(model, fs);
        }

        private IEstimator<ITransformer> MakePipeline()
        {
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "SentimentValue", outputColumnName: "Label")
                .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "SentimentText", outputColumnName: "SentimentFeaturized"))
                .Append(_mlContext.Transforms.Concatenate("Features", "SentimentFeaturized"))
                .AppendCacheCheckpoint(_mlContext);

            return pipeline;
        }

        private IDataView LoadLabeledData(string path)
        {
            var sentences = CSVReader.ReadComparrisonsYield(path);
            var list = new List<SentimentData>();

            foreach ((string, string) s in sentences)
            {
                var (value, sentiment) = s;

                list.Add(new SentimentData() { SentimentText = sentiment, SentimentValue = value });
            }

            return _mlContext.Data.LoadFromEnumerable(list);
        }

        public (string, float) EvaluateSentence(string sentence)
        {
            var s = sentence.ToString();

            var data = new SentimentData() { SentimentText = s };

            var prediction = _predEngine.Predict(data);

            return Evaluation(float.Parse(prediction.Prediction));
        }

        public IEnumerable<(string, float)> EvaluateSentencesInComment(IEnumerable<string> comment)
        {
            foreach (var sentence in comment)
            {
                yield return EvaluateSentence(sentence);
            }
        }

        public (string, float) Evaluation(float sentiment)
        {
            if (sentiment < 0) return (Program.InternConcurrentSafe("Negative"), sentiment);
            else if (sentiment > 0) return (Program.InternConcurrentSafe("Positive"), sentiment);
            return (Program.InternConcurrentSafe("Neutral"), sentiment);
        }

        public (string, float) EvaluateComment(IEnumerable<string> tc)
        {
            throw new NotImplementedException();
        }

        private (float acc, float fpos, float fneu, float fneg) RunOnce(IDataView traindata, IEnumerable<(string, string)> testdata)
        {
            var pipeline = MakePipeline();
            var model = BuildAndTrainModel(traindata, pipeline);
            var context = new MLContext();
            _predEngine = model.CreatePredictionEngine<SentimentData, SentimentPrediction>(context);

            var pos_pos = 0f; var neu_pos = 0f; var neg_pos = 0f;
            var pos_neu = 0f; var neu_neu = 0f; var neg_neu = 0f;
            var pos_neg = 0f; var neu_neg = 0f; var neg_neg = 0f;

            foreach ((string label, string sentence) in testdata)
            {
                var pred = EvaluateSentence(sentence);

                var pred_lab = pred.Item2;

                if (pred_lab > 0.1)
                {
                    if (label.Equals("1") || label.Equals("2"))             pos_pos++;
                    else if (label.Equals("0"))                             pos_neu++;
                    else if (label.Equals("-1") || label.Equals("-2"))      pos_neg++;
                    else throw new NotImplementedException();
                }
                else if (pred_lab < -0.1)
                {
                    if (label.Equals("1") || label.Equals("2"))             neg_pos++;
                    else if (label.Equals("0"))                             neg_neu++;
                    else if (label.Equals("-1") || label.Equals("-2"))      neg_neg++;
                    else throw new NotImplementedException();
                }
                else
                {
                    if (label.Equals("1") || label.Equals("2"))             neu_pos++;
                    else if (label.Equals("0"))                             neu_neu++;
                    else if (label.Equals("-1") || label.Equals("-2"))      neu_neg++;
                    else throw new NotImplementedException();
                }
            }

            var (fpos, fneu, fneg) = GetFScore(pos_pos, pos_neu, pos_neg, neu_pos, neu_neu, neu_neg, neg_pos, neg_neu, neg_neg);

            var acc = ((pos_pos + neu_neu + neg_neg) / testdata.Count() * 100f);

            return (acc, fpos, fneu, fneg);
        }

        private (float fpos, float fneu, float fneg) GetFScore(float pos_pos, float pos_neu, float pos_neg, float neu_pos, float neu_neu, float neu_neg, float neg_pos, float neg_neu, float neg_neg)
        {
            var fpos = (2f * pos_pos) / (2f * pos_pos + (neu_pos + neg_pos) + (pos_neu + pos_neg));
            var fneu = (2f * neu_neu) / (2f * neu_neu + (pos_neu + neg_neu) + (neu_pos + neu_neg));
            var fneg = (2f * neg_neg) / (2f * neg_neg + (pos_neg + neu_neg) + (neg_pos + neg_neu));

            return (fpos, fneu, fneg);
        } 

        public void RunMany(int runs)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var traindata = LoadLabeledData(_trainDataPath);

            var testdata = CSVReader.ReadComparrisonsYield(_testDataPath);

            var accs = new List<float>();

            var fscores = new List<(float, float, float, float)>();

            for (int i = 0; i < runs; i++)
            {
                var (acc, fpos, fneu, fneg) = RunOnce(traindata, testdata);

                accs.Add(acc);
                fscores.Add((fpos, fneu, fneg, ((fpos + fneu + fneg)/3)));

                Console.WriteLine("Run {0}/{1} complete!", i+1, runs);
            }
            
            var maxFScore = float.NegativeInfinity;
            var minFScore = float.PositiveInfinity;
            foreach ((float, float, float, float) res in fscores)
            {
                var avg = res.Item4;
                if (avg > maxFScore) maxFScore = avg;
                if (avg < minFScore) minFScore = avg;
            }

            var maxAcc = float.NegativeInfinity;
            var minAcc = float.PositiveInfinity;

            foreach (float acc in accs)
            {
                if (acc > maxAcc) maxAcc = acc;
                if (acc < minAcc) minAcc = acc;
            }

            var csv = new StringBuilder();

            var line1 = string.Format("{0} runs:", runs);
            var line2 = string.Format("FScore: Min {0} Max {1}", minFScore, maxFScore);
            var line3 = string.Format("Acc: Min {0} Max {1}", minAcc, maxAcc);
            var line4 = string.Format("Run, Acc, fpos, fneu, fneg, favg");

            csv.AppendLine(line1);
            csv.AppendLine(line2);
            csv.AppendLine(line3);
            csv.AppendLine(line4);

            for (int i = 0; i < accs.Count(); i++)
            {
                var acc = accs[i];
                var fs = fscores[i];
                var line = string.Format("{0},{1:0.0000},{2:0.0000},{3:0.0000},{4:0.0000},{5:0.0000}", i, acc, fs.Item1, fs.Item2, fs.Item3, fs.Item4);

                csv.AppendLine(line);
            }

            File.WriteAllText(Program.src_dir + "/out/mlnetruns.csv", csv.ToString());
        }
    }
}
