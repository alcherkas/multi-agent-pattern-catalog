# Routing Pattern Implementation

## Overview

This project demonstrates the **Routing Pattern** as described in Anthropic's ["Building Effective Agents"](https://www.anthropic.com/engineering/building-effective-agents) article. The routing pattern is a workflow that classifies an input and directs it to a specialized follow-up task, allowing for separation of concerns and building more specialized prompts.

## What is the Routing Pattern?

The routing pattern works by:

1. **Classification**: A router analyzes incoming requests and classifies them into predefined categories
2. **Specialized Handling**: Each category is handled by a specialized system with domain-specific prompts and capabilities
3. **Improved Performance**: This separation prevents optimizing for one type of input from hurting performance on other inputs

## Architecture

```
User Input
    ↓
[Router/Classifier]
    ↓
Route Decision (with confidence)
    ↓
┌─────────────────┬─────────────────┬─────────────────┬─────────────────┐
│ Customer Service│ Technical Support│ General Inquiry │   Escalation    │
│   Specialist    │   Specialist    │   Specialist    │   Specialist    │
└─────────────────┴─────────────────┴─────────────────┴─────────────────┘
    ↓
Specialized Response
```

## Implementation Details

### Route Types

The system classifies requests into four main categories:

- **CustomerService**: Billing questions, account issues, refund requests, subscription management
- **TechnicalSupport**: Bug reports, software issues, configuration problems, troubleshooting
- **GeneralInquiry**: Product information, general questions, how-to guides
- **Escalation**: Complex issues requiring human intervention, complaints, legal matters

### Key Components

#### 1. RouteDecision Class
```csharp
public class RouteDecision
{
    public RouteType RouteType { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}
```

#### 2. RoutingAgent Class
- **ClassifyRequestAsync()**: Uses the router kernel to classify incoming requests
- **ProcessRequestAsync()**: Routes classified requests to specialized handlers
- **Specialized Prompt Builders**: Creates domain-specific prompts for each route type

### Specialized Prompts

Each route type has a carefully crafted prompt that:
- Defines the specialist's role and capabilities
- Sets clear guidelines for handling requests
- Provides context-specific instructions
- Includes escalation criteria

## Benefits Demonstrated

✅ **Separation of Concerns**: Each specialist handles only their domain  
✅ **Improved Accuracy**: Focused prompts perform better than generic ones  
✅ **Scalable Architecture**: Easy to add new route types and specialists  
✅ **Confidence Scoring**: Quality control through classification confidence  
✅ **Graceful Fallback**: Handles classification failures elegantly  

## Usage Examples

The implementation includes six test scenarios:

1. **Billing Issue**: "I was charged twice for my subscription this month and need a refund"
   - Routes to: CustomerService
   
2. **Technical Problem**: "The software keeps crashing when I try to export data to CSV format"
   - Routes to: TechnicalSupport
   
3. **Product Inquiry**: "What are the differences between your Pro and Enterprise plans?"
   - Routes to: GeneralInquiry
   
4. **Complaint**: "This is completely unacceptable! I've been a customer for 5 years..."
   - Routes to: Escalation
   
5. **Basic Support**: "How do I reset my password?"
   - Routes to: GeneralInquiry or TechnicalSupport
   
6. **Configuration Help**: "Can you help me configure SSL certificates for my deployment?"
   - Routes to: TechnicalSupport

## Prerequisites

- .NET 9.0 or later
- Ollama running locally on port 11434
- Required models: `gemma3:4b-it-q8_0`

## Installation & Setup

1. **Install Ollama** (if not already installed):
   ```bash
   # macOS
   brew install ollama
   
   # Start Ollama service
   ollama serve
   ```

2. **Pull Required Models**:
   ```bash
   ollama pull gemma3:4b-it-q8_0
   ```

3. **Run the Application**:
   ```bash
   cd router
   dotnet run
   ```

## Project Structure

```
router/
├── Program.cs           # Main routing pattern implementation
├── router.csproj       # Project configuration
└── README.md           # This documentation
```

## Dependencies

- `Microsoft.SemanticKernel.Connectors.Ollama` (v1.54.0-alpha)

## When to Use the Routing Pattern

According to Anthropic's guidance, routing works well for:

✅ **Complex tasks** with distinct categories that are better handled separately  
✅ **Scenarios** where classification can be handled accurately  
✅ **Systems** where one-size-fits-all prompts don't perform well  
✅ **Applications** requiring specialized domain knowledge  

## Extensions and Customizations

You can extend this implementation by:

1. **Adding New Route Types**: Define new enum values and specialized handlers
2. **Implementing Tool Integration**: Add specific tools for each specialist
3. **Adding Confidence Thresholds**: Route low-confidence requests to human review
4. **Implementing Metrics**: Track routing accuracy and specialist performance
5. **Adding Fallback Chains**: Create hierarchical routing for complex scenarios

## Anthropic's Agent Patterns

This routing pattern is one of several patterns described in Anthropic's agent architecture:

- **Prompt Chaining**: Sequential processing through multiple steps
- **Routing**: Classification and specialized handling (this implementation)
- **Parallelization**: Concurrent processing for speed or voting
- **Orchestrator-Workers**: Dynamic task delegation
- **Evaluator-Optimizer**: Iterative improvement loops
- **Autonomous Agents**: Independent operation with tool usage

## Performance Considerations

- **Latency**: Two-step process (classification + specialized handling)
- **Cost**: Multiple model calls per request
- **Accuracy**: Improved through specialization
- **Scalability**: Easy to add new specialists and route types

## Contributing

When extending this implementation:

1. Follow the established patterns for new route types
2. Create comprehensive prompts for new specialists
3. Include test scenarios for new functionality
4. Update documentation and examples

## License

This implementation is part of the multi-agent pattern catalog and follows the same licensing terms as the parent project.
