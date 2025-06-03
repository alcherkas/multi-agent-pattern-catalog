# Parallelization Pattern Example

This project demonstrates the **Parallelization** pattern from Anthropic's guide on building effective agents. The pattern shows how LLMs can work simultaneously on tasks and have their outputs aggregated programmatically.

## Pattern Overview

The Parallelization pattern manifests in two key variations:

### 1. 🔀 Sectioning
Breaking a task into **independent subtasks** that run in parallel:
- Each subtask focuses on a specific aspect of the problem
- Different LLM instances handle different concerns
- Results are combined for comprehensive analysis

### 2. 🗳️ Voting
Running the **same task multiple times** to get diverse outputs:
- Multiple LLM instances evaluate the same question
- Consensus is reached through majority voting
- Increases confidence and reduces false positives/negatives

## Demo Implementation

This example implements a **Code Review System** that uses both parallelization approaches:

### Sectioning Example
Three specialized agents analyze code in parallel:
- **🔒 Security Agent**: Focuses on vulnerabilities, authentication, data exposure
- **⚡ Performance Agent**: Analyzes efficiency, memory usage, scalability
- **🔧 Maintainability Agent**: Reviews code structure, naming, best practices

### Voting Example
Three security auditors vote on whether code contains critical vulnerabilities:
- Each uses different prompting approaches
- Majority consensus determines final security assessment
- Reduces false positives while catching critical issues

### Final Aggregation
A senior reviewer synthesizes all parallel results into:
- Executive summary
- Prioritized recommendations
- Risk assessment
- Actionable next steps

## Benefits Demonstrated

- **⚡ Speed**: Parallel execution reduces total processing time
- **🎯 Quality**: Multiple focused evaluations improve accuracy
- **🔄 Reliability**: Voting mechanisms increase confidence
- **📊 Comprehensive**: Different perspectives provide complete coverage
- **🧩 Modularity**: Specialized agents can be easily modified or replaced

## When to Use This Pattern

According to Anthropic's research, parallelization is effective when:

- **Divided subtasks can be parallelized for speed**
- **Multiple perspectives are needed for higher confidence**
- **Complex tasks have multiple considerations** that benefit from focused attention
- **Independent aspects** of a problem can be evaluated separately

## Example Use Cases

### Sectioning Applications:
- **Guardrails**: Content screening + response generation
- **Evaluation Systems**: Different aspects evaluated separately
- **Multi-domain Analysis**: Technical, business, and compliance reviews

### Voting Applications:
- **Code Vulnerability Assessment**: Multiple security reviews
- **Content Moderation**: Consensus on appropriateness
- **Decision Validation**: Critical choices verified by multiple agents

## Running the Demo

1. **Prerequisites**:
   - .NET 9.0 SDK
   - Ollama running locally with `gemma3:4b-it-q8_0` model

2. **Setup Ollama**:
   ```bash
   # Install and start Ollama
   ollama pull gemma3:4b-it-q8_0
   ollama serve
   ```

3. **Run the Demo**:
   ```bash
   cd parallelization
   dotnet run
   ```

## Key Implementation Details

### Parallel Task Execution
```csharp
var sectioningTasks = new List<Task<string>>
{
    Task.Run(async () => await securityKernel.InvokePromptAsync(securityPrompt)),
    Task.Run(async () => await performanceKernel.InvokePromptAsync(performancePrompt)),
    Task.Run(async () => await maintainabilityKernel.InvokePromptAsync(maintainabilityPrompt))
};

var results = await Task.WhenAll(sectioningTasks);
```

### Voting Consensus
```csharp
var votes = await Task.WhenAll(votingTasks);
var criticalVotes = votes.Count(v => v);
var consensus = criticalVotes >= (totalVotes / 2.0); // Majority rule
```

### Result Aggregation
```csharp
var aggregationPrompt = BuildComprehensivePrompt(sectioningResults, votingConsensus);
var finalAssessment = await aggregationKernel.InvokePromptAsync(aggregationPrompt);
```

## Architecture Benefits

1. **Separation of Concerns**: Each agent has a single, well-defined responsibility
2. **Fault Tolerance**: Failure of one agent doesn't break the entire system
3. **Scalability**: Easy to add more specialized agents or voting participants
4. **Testability**: Individual agents can be tested and validated independently
5. **Flexibility**: Different models can be used for different aspects

## Related Patterns

- **Orchestrator-Workers**: When subtasks aren't predefined
- **Router**: When different inputs need different handling
- **Evaluator-Optimizer**: When iterative refinement is needed

## References

Based on Anthropic's "Building Effective Agents" guide:
https://www.anthropic.com/engineering/building-effective-agents
