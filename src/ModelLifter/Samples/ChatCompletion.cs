using Microsoft.ML.OnnxRuntimeGenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

using ModelLifter;


namespace ModelLifter.Samples;


public class ChatCompletion
{
    readonly static string modelPath = Path.Combine(ModelUtils.GetDirectoryInTreeThatContains(Directory.GetCurrentDirectory(), "notebooks")!, 
            "phi3mini4k", "cpu_and_mobile", "cpu-int4-rtn-block-32");

    public static async Task RunAsync()
    {

        // create kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOnnxRuntimeGenAIChatCompletion(modelPath: modelPath);
        Kernel kernel = builder.Build();

        // create chat
        var chat = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();

        // run chat
        while (true)
        {
            Console.Write("Q: ");
            var userQ = Console.ReadLine();
            if (string.IsNullOrEmpty(userQ))
            {
                break;
            }
            history.AddUserMessage(userQ);

            Console.Write($"Phi3: ");
            var response = "";
            var result = chat.GetStreamingChatMessageContentsAsync(history);
            await foreach (var message in result)
            {
                Console.Write(message.Content);
                response += message.Content;
            }
            history.AddAssistantMessage(response);
            Console.WriteLine("");
        }
    }
}