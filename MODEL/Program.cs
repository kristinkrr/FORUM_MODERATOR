using Microsoft.ML;
using Microsoft.ML.Data;

public class CommentInput
{
    [LoadColumn(0)]
    public string Text { get; set; } = null!;

    [LoadColumn(1)]
    public bool Label { get; set; }
}

public class CommentPrediction
{
    public bool PredictedLabel { get; set; }
    public float Probability { get; set; }
    public float Score { get; set; }
}

public class ModelTrainer
{
    public static void TrainAndSaveModel(string dataPath, string modelPath)
    {
        var mlContext = new MLContext();

        var data = mlContext.Data.LoadFromTextFile<CommentInput>(
            dataPath, hasHeader: true, separatorChar: ',', allowQuoting: true);

        var split = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);

        var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(CommentInput.Text))
            .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(CommentInput.Label), featureColumnName: "Features"));

        var model = pipeline.Fit(split.TrainSet);

        var predictions = model.Transform(split.TestSet);
        var metrics = mlContext.BinaryClassification.Evaluate(predictions, labelColumnName: nameof(CommentInput.Label));

        System.Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");

        mlContext.Model.Save(model, split.TrainSet.Schema, modelPath);
    }
}

class Program
{
    static void Main(string[] args)
    {
        string dataPath = "comments.csv"; 
        string modelPath = "model.zip";  

        ModelTrainer.TrainAndSaveModel(dataPath, modelPath);
    }
}