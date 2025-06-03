using Microsoft.SemanticKernel;
using router;

#pragma warning disable SKEXP0070

// ==============================================================================
// ROUTING PATTERN IMPLEMENTATION
// Based on Anthropic's "Building Effective Agents" - Routing Workflow
// 
// The routing pattern classifies an input and directs it to a specialized 
// followup task. This allows for separation of concerns and building more 
// specialized prompts.
// ==============================================================================

var routerKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

// Specialized kernels for different task types
var customerServiceKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var technicalSupportKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var generalInquiryKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

// Initialize the routing system
var specializedKernels = new Dictionary<RouteType, Kernel>
{
    { RouteType.CustomerService, customerServiceKernel },
    { RouteType.TechnicalSupport, technicalSupportKernel },
    { RouteType.GeneralInquiry, generalInquiryKernel },
    { RouteType.Escalation, generalInquiryKernel } // Reuse for escalation
};

var routingAgent = new RoutingAgent(routerKernel, specializedKernels);

// Example usage scenarios demonstrating the routing pattern
var testRequests = new[]
{
    "I was charged twice for my subscription this month and need a refund",
    "The software keeps crashing when I try to export data to CSV format",
    "What are the differences between your Pro and Enterprise plans?",
    "This is completely unacceptable! I've been a customer for 5 years and you're treating me terribly!",
    "How do I reset my password?",
    "Can you help me configure SSL certificates for my deployment?"
};

Console.WriteLine("=== ROUTING PATTERN DEMONSTRATION ===");
Console.WriteLine("Based on Anthropic's 'Building Effective Agents' - Routing Workflow\n");

foreach (var request in testRequests)
{
    Console.WriteLine($"🔍 USER REQUEST: {request}");
    Console.WriteLine("─".PadRight(60, '─'));
    
    try
    {
        // Step 1: Classify the request
        var routeDecision = await routingAgent.ClassifyRequestAsync(request);
        Console.WriteLine($"📋 CLASSIFICATION: {routeDecision.RouteType}");
        Console.WriteLine($"🎯 CONFIDENCE: {routeDecision.Confidence:P1}");
        Console.WriteLine($"💭 REASONING: {routeDecision.Reasoning}");
        Console.WriteLine();
        
        // Step 2: Process with specialized handler
        var response = await routingAgent.ProcessRequestAsync(request, routeDecision);
        Console.WriteLine($"✅ SPECIALIZED RESPONSE:");
        Console.WriteLine($"{response}");
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR: {ex.Message}");
    }
    
    Console.WriteLine();
    Console.WriteLine("═".PadRight(80, '═'));
    Console.WriteLine();
}

Console.WriteLine("\n🎉 Routing pattern demonstration complete!");