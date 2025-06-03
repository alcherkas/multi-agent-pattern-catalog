using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace router;

public class RoutingAgent
{
    private readonly Kernel _routerKernel;
    private readonly Dictionary<RouteType, Kernel> _specializedKernels;
    private readonly IChatCompletionService _chatService;

    private readonly RouteDecisionParser _parser;

    public RoutingAgent(Kernel routerKernel, Dictionary<RouteType, Kernel> specializedKernels)
    {
        _routerKernel = routerKernel;
        _specializedKernels = specializedKernels;
        _chatService = _routerKernel.GetRequiredService<IChatCompletionService>();
        _parser = new RouteDecisionParser();
    }

    /// <summary>
    /// Classifies a user request into an appropriate route using LLM analysis.
    /// Uses a specialized parser to handle various response formats from the LLM.
    /// </summary>
    /// <param name="userInput">The user's request to classify</param>
    /// <returns>A RouteDecision containing the classification, confidence, and reasoning</returns>
    public async Task<RouteDecision> ClassifyRequestAsync(string userInput)
    {
        var classificationPrompt = $$"""
                                     You are a routing classifier for a customer service system. Your job is to analyze user requests and classify them into the appropriate category.

                                     Categories:
                                     - CustomerService: Billing questions, account issues, refund requests, subscription management
                                     - TechnicalSupport: Bug reports, software issues, configuration problems, troubleshooting
                                     - GeneralInquiry: Product information, general questions, how-to guides
                                     - Escalation: Complex issues requiring human intervention, complaints, legal matters

                                     User Input: "{{userInput}}"

                                     You must respond with ONLY a valid JSON object, no additional text. Use this exact format:
                                     {"RouteType": "CategoryName", "Confidence": 0.95, "Reasoning": "Brief explanation"}

                                     Valid RouteType values: CustomerService, TechnicalSupport, GeneralInquiry, Escalation
                                     """;

        var response = await _chatService.GetChatMessageContentAsync(classificationPrompt);
        
        // Use the specialized parser to handle various response formats
        // The parser includes multiple fallback strategies because LLMs can be unpredictable
        // in their response formatting, even with explicit JSON instructions
        return _parser.ParseResponse(response.Content ?? "", debugMessage => Console.WriteLine($"DEBUG - {debugMessage}"));
    }

    // Step 2: Route to specialized handler based on classification
    public async Task<string> ProcessRequestAsync(string userInput, RouteDecision routeDecision)
    {
        if (!_specializedKernels.ContainsKey(routeDecision.RouteType))
        {
            return "Sorry, I'm unable to process this type of request at the moment.";
        }

        var specializedKernel = _specializedKernels[routeDecision.RouteType];
        var specializedChatService = specializedKernel.GetRequiredService<IChatCompletionService>();

        var specializedPrompt = routeDecision.RouteType switch
        {
            RouteType.CustomerService => BuildCustomerServicePrompt(userInput),
            RouteType.TechnicalSupport => BuildTechnicalSupportPrompt(userInput),
            RouteType.GeneralInquiry => BuildGeneralInquiryPrompt(userInput),
            RouteType.Escalation => BuildEscalationPrompt(userInput),
            _ => BuildGeneralInquiryPrompt(userInput)
        };

        var response = await specializedChatService.GetChatMessageContentAsync(specializedPrompt);
        return response.Content ?? "I apologize, but I'm unable to process your request at this time.";
    }

    private string BuildCustomerServicePrompt(string userInput)
    {
        return $"""
                You are a specialized customer service representative with expertise in billing, accounts, and subscription management.

                Key capabilities:
                - Handle billing inquiries and disputes
                - Process refund requests (up to $500 automatically)
                - Manage subscription changes
                - Access account information
                - Escalate complex billing issues

                Guidelines:
                - Be empathetic and professional
                - Offer concrete solutions
                - Follow company refund policy
                - Document all actions taken

                Customer Request: "{userInput}"

                Provide a helpful response addressing their concern:
                """;
    }

    private string BuildTechnicalSupportPrompt(string userInput)
    {
        return $"""
                You are a technical support specialist with deep knowledge of software troubleshooting and system configuration.

                Key capabilities:
                - Diagnose software bugs and issues
                - Provide step-by-step troubleshooting guides
                - Recommend system configurations
                - Escalate complex technical issues to engineering
                - Document bugs for development team

                Guidelines:
                - Ask clarifying questions about system specs
                - Provide clear, step-by-step instructions
                - Suggest multiple solutions when possible
                - Know when to escalate to engineering

                Technical Issue: "{userInput}"

                Provide technical assistance:
                """;
    }

    private string BuildGeneralInquiryPrompt(string userInput)
    {
        return $"""
                You are a knowledgeable assistant helping with general product information and inquiries.

                Key capabilities:
                - Answer product questions
                - Provide how-to guidance
                - Explain features and benefits
                - Direct users to appropriate resources
                - Handle basic questions

                Guidelines:
                - Be informative and friendly
                - Provide comprehensive answers
                - Include relevant links or resources
                - Know when to route to specialists

                General Inquiry: "{userInput}"

                Provide a helpful and informative response:
                """;
    }

    private string BuildEscalationPrompt(string userInput)
    {
        return $"""
                You are handling an escalated request that requires careful attention and potential human intervention.

                Key capabilities:
                - Acknowledge complex concerns
                - Gather detailed information
                - Document escalation reasons
                - Provide interim solutions
                - Set appropriate expectations

                Guidelines:
                - Show empathy and understanding
                - Collect all relevant details
                - Explain next steps clearly
                - Provide realistic timelines
                - Ensure customer feels heard

                Escalated Issue: "{userInput}"

                Handle this escalated request professionally:
                """;
    }


}