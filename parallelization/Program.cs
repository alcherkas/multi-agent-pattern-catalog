using Microsoft.SemanticKernel;
using System.Text;

#pragma warning disable SKEXP0070

Console.WriteLine("🔄 Parallelization Pattern Demo");
Console.WriteLine("================================");
Console.WriteLine();

// Initialize multiple kernels for parallel processing
var securityKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var performanceKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var maintainabilityKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var votingKernel1 = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var votingKernel2 = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var votingKernel3 = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

var aggregationKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b-it-q8_0", new Uri("http://localhost:11434"))
    .Build();

// Sample code to review
var sampleCode = @"
public class UserController : ControllerBase
{
    private static List<User> users = new List<User>();
    
    [HttpGet]
    public List<User> GetAllUsers()
    {
        return users;
    }
    
    [HttpPost]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (user.Password.Length < 6)
            return BadRequest(""Password too short"");
            
        users.Add(user);
        return Ok(user);
    }
    
    [HttpGet(""{id}"")]
    public User GetUser(int id)
    {
        return users.FirstOrDefault(u => u.Id == id);
    }
}";

Console.WriteLine("📝 Code to Review:");
Console.WriteLine(sampleCode);
Console.WriteLine("\n" + new string('=', 60) + "\n");

// PATTERN 1: SECTIONING - Different aspects reviewed in parallel
Console.WriteLine("🔀 PATTERN 1: SECTIONING");
Console.WriteLine("Running specialized reviews in parallel...\n");

var sectioningTasks = new List<Task<string>>
{
    // Security analysis
    Task.Run(async () =>
    {
        var prompt = @"You are a security expert. Review this C# code for security vulnerabilities only. 
        Focus on authentication, authorization, input validation, data exposure, and other security concerns.
        Provide a brief summary with specific issues found.
        
        Code to review:
        " + sampleCode;
        
        var result = await securityKernel.InvokePromptAsync(prompt);
        return $"🔒 SECURITY ANALYSIS:\n{result}\n";
    }),
    
    // Performance analysis
    Task.Run(async () =>
    {
        var prompt = @"You are a performance expert. Review this C# code for performance issues only.
        Focus on efficiency, memory usage, database queries, caching, and scalability concerns.
        Provide a brief summary with specific issues found.
        
        Code to review:
        " + sampleCode;
        
        var result = await performanceKernel.InvokePromptAsync(prompt);
        return $"⚡ PERFORMANCE ANALYSIS:\n{result}\n";
    }),
    
    // Maintainability analysis
    Task.Run(async () =>
    {
        var prompt = @"You are a code quality expert. Review this C# code for maintainability issues only.
        Focus on code structure, naming conventions, error handling, best practices, and technical debt.
        Provide a brief summary with specific issues found.
        
        Code to review:
        " + sampleCode;
        
        var result = await maintainabilityKernel.InvokePromptAsync(prompt);
        return $"🔧 MAINTAINABILITY ANALYSIS:\n{result}\n";
    })
};

// Wait for all sectioning tasks to complete
var sectioningResults = await Task.WhenAll(sectioningTasks);

// Display sectioning results
foreach (var result in sectioningResults)
{
    Console.WriteLine(result);
    Console.WriteLine(new string('-', 40));
}

Console.WriteLine("\n" + new string('=', 60) + "\n");

// PATTERN 2: VOTING - Same task run multiple times for consensus
Console.WriteLine("🗳️  PATTERN 2: VOTING");
Console.WriteLine("Running multiple vulnerability assessments for consensus...\n");

var votingTasks = new List<Task<bool>>
{
    // Vote 1: Critical vulnerability check
    Task.Run(async () =>
    {
        var prompt = @"You are a security auditor. Analyze this C# code and determine if it contains 
        CRITICAL security vulnerabilities that could lead to data breaches or system compromise.
        
        Respond with only 'true' if you find critical vulnerabilities, or 'false' if not.
        Consider: SQL injection, authentication bypass, data exposure, etc.
        
        Code to analyze:
        " + sampleCode;
        
        var result = await votingKernel1.InvokePromptAsync(prompt);
        var response = result.ToString().Trim().ToLower();
        return response.Contains("true");
    }),
    
    // Vote 2: Critical vulnerability check (different perspective)
    Task.Run(async () =>
    {
        var prompt = @"As a cybersecurity specialist, evaluate this C# code for severe security flaws 
        that pose immediate risk to application security.
        
        Answer 'true' only if there are serious security issues, 'false' otherwise.
        Focus on: unauthorized access, data leaks, injection attacks, insecure storage.
        
        Code to evaluate:
        " + sampleCode;
        
        var result = await votingKernel2.InvokePromptAsync(prompt);
        var response = result.ToString().Trim().ToLower();
        return response.Contains("true");
    }),
    
    // Vote 3: Critical vulnerability check (third opinion)
    Task.Run(async () =>
    {
        var prompt = @"You're conducting a security review. Does this C# code have major security 
        problems that require immediate attention?
        
        Reply 'true' for critical security issues, 'false' for minor or no issues.
        Look for: improper authentication, exposed sensitive data, vulnerable endpoints.
        
        Code under review:
        " + sampleCode;
        
        var result = await votingKernel3.InvokePromptAsync(prompt);
        var response = result.ToString().Trim().ToLower();
        return response.Contains("true");
    })
};

// Wait for all voting tasks to complete
var votes = await Task.WhenAll(votingTasks);

// Analyze voting results
var criticalVotes = votes.Count(v => v);
var totalVotes = votes.Length;
var consensus = criticalVotes >= (totalVotes / 2.0); // Majority consensus

Console.WriteLine($"🗳️  Voting Results:");
Console.WriteLine($"   Votes for 'Critical Vulnerabilities': {criticalVotes}/{totalVotes}");
Console.WriteLine($"   Consensus: {(consensus ? "⚠️  CRITICAL ISSUES DETECTED" : "✅ NO CRITICAL ISSUES")}");
Console.WriteLine();

Console.WriteLine(new string('-', 40));
Console.WriteLine();

// AGGREGATION: Combine all results
Console.WriteLine("📋 FINAL AGGREGATION");
Console.WriteLine("Synthesizing all parallel analyses...\n");

var aggregationPrompt = $@"You are a senior code reviewer. Based on the following parallel analysis results, 
provide a comprehensive final assessment and prioritized recommendations.

SECTIONING RESULTS:
{string.Join("\n", sectioningResults)}

VOTING CONSENSUS:
Critical Vulnerability Assessment: {(consensus ? "CRITICAL ISSUES FOUND" : "NO CRITICAL ISSUES")} 
({criticalVotes}/{totalVotes} votes for critical)

Provide:
1. Executive Summary
2. Top 3 Priority Issues
3. Recommended Actions
4. Risk Level (Low/Medium/High)

Keep the response concise and actionable.";

var finalAssessment = await aggregationKernel.InvokePromptAsync(aggregationPrompt);

Console.WriteLine("📊 COMPREHENSIVE CODE REVIEW REPORT");
Console.WriteLine(new string('=', 50));
Console.WriteLine(finalAssessment);
Console.WriteLine(new string('=', 50));

Console.WriteLine("\n✨ Parallelization Pattern Demo Complete!");
Console.WriteLine("\nKey Benefits Demonstrated:");
Console.WriteLine("• 🔀 Sectioning: Specialized parallel analysis (security, performance, maintainability)");
Console.WriteLine("• 🗳️  Voting: Multiple perspectives for critical decision consensus");
Console.WriteLine("• ⚡ Speed: Parallel execution reduces total processing time");
Console.WriteLine("• 🎯 Quality: Multiple focused evaluations improve accuracy");
Console.WriteLine("• 📊 Aggregation: Synthesized results provide comprehensive insights");