using AutoGen;
using AutoGen.Core;

namespace ModelLifter.Samples;

public class AutoGenConversation {
 
    public static async Task RunAsync() {

        /// Several minor updates needed to be done to run the code
        /// 1. LMStudioConfig.cs should be updated to use new endpoint and model names
        ///     (see https://github.com/microsoft/autogen/pull/4732)
        /// 2. Avoid duplicate messages send to agent by update GroupChatExtension.cs
        ///     (see https://github.com/microsoft/autogen/issues/4731)
        var config = new LMStudioConfig("localhost", 1234);

        var programmer = new AssistantAgent(
            name: "Programmer",
            systemMessage: @"You are a programmer who completes Product Manager's requirement, and
            check the code completeness and correctness, with unit test.",
            llmConfig: new ConversableAgentConfig {
                    Temperature = 0.0f,
                    ConfigList = [config]
            }
        ).RegisterPrintMessage();

        var pm = new AssistantAgent(
            name: "Product Manager",
            systemMessage: @"You are a product manager who specifys requirements and examples for the programmer, then
            verify their implementation by check the ut and code logic. The requirement is simplified but incremental, you should add
            more constraints to the requirement to make sure programmer implements it correctly.",
            llmConfig: new ConversableAgentConfig {
                    Temperature = 0.0f,
                    ConfigList = [config]
            }
        ).RegisterPrintMessage();

        var conversation = await programmer.InitiateChatAsync(
            receiver: pm,
            message: "Hey PM, let me know your requirement about the ecommerce App",
            maxRound: 10);
    }
}