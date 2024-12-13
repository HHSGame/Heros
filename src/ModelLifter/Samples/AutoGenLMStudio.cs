using System.ClientModel;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using OpenAI;

namespace ModelLifter.Samples;

public class AutoGenLMStudio {

    public static async Task RunAsync() {
        var endpoint = "http://localhost:1234/v1";
        var openaiClient = new OpenAIClient(new ApiKeyCredential("api-key"), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint),
        });

        var lmAgent = new OpenAIChatAgent(
            chatClient: openaiClient.GetChatClient("mistral-nemo-instruct-2407"),
            name: "assistant")
            .RegisterMessageConnector()
            .RegisterPrintMessage();

        await lmAgent.SendAsync("Can you write a piece of C# code to calculate 100th of fibonacci?");
    }
}