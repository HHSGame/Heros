using Microsoft.ML.OnnxRuntimeGenAI;
using System.Linq;
using System.Runtime.CompilerServices;


namespace ModelLifter.Samples;


public class MultiModal
{

    readonly static string modelPath = Path.Combine(ModelUtils.GetDirectoryInTreeThatContains(Directory.GetCurrentDirectory(), "notebooks")!,
            "phi3.5v", "cpu_and_mobile", "cpu-int4-rtn-block-32-acc-level-4");
    public static void Run()
    {
        using OgaHandle ogaHandle = new();

        using Model model = new(modelPath);
        using MultiModalProcessor processor = new(model);
        using var tokenizerStream = processor.CreateStream();

        List<string> imagePaths = [Path.Combine(ModelUtils.GetDirectoryInTreeThatContains(Directory.GetCurrentDirectory(), "notebooks")!,
            "data", "img", "camera_girl.jpeg")];

        string text = "What is shown in this image?";

        Images images = Images.Load([.. imagePaths]);

        string prompt = "<|user|>\n";
        if (images != null)
        {
            for (int i = 0; i < imagePaths.Count; i++)
            {
                prompt += "<|image_" + (i + 1) + "|>\n";
            }
        }
        prompt += text + "<|end|>\n<|assistant|>\n";

        Console.WriteLine("Processing image and prompt...");
        using var inputTensors = processor.ProcessImages(prompt, images);

        Console.WriteLine("Generating response...");
        using GeneratorParams generatorParams = new(model);
        generatorParams.SetSearchOption("max_length", 7680);
        generatorParams.SetInputs(inputTensors);

        using var generator = new Generator(model, generatorParams);
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!generator.IsDone())
        {
            generator.ComputeLogits();
            generator.GenerateNextToken();
            Console.Write(tokenizerStream.Decode(generator.GetSequence(0)[^1]));
        }
        watch.Stop();
        var runTimeInSeconds = watch.Elapsed.TotalSeconds;
        Console.WriteLine();
        Console.WriteLine($"Total Time: {runTimeInSeconds:0.00}");

        images?.Dispose();

    }
}