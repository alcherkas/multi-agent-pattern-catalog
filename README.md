# Multi-Agent Pattern Catalog

A comprehensive collection of multi-agent patterns implemented in C# using Semantic Kernel and Ollama. These patterns are based on the excellent guidance from [Anthropic's "Building Effective Agents"](https://www.anthropic.com/engineering/building-effective-agents) article.

## 🎯 Overview

This repository demonstrates three fundamental multi-agent patterns that enhance AI agent capabilities through orchestration, specialization, and parallel processing. Each pattern addresses specific challenges in building more effective and reliable AI systems.

## 🚀 Patterns Implemented

### 1. 🔗 [Prompt Chaining](./prompt-chaining/)
**Sequential task execution with quality gates**

The prompt chaining pattern breaks complex tasks into smaller, more manageable steps where the output of one step becomes the input to the next. This approach includes quality gates to ensure each step meets criteria before proceeding.

**Example Implementation:**
- Blog post generation with outline → quality check → full document
- Uses different models for different steps (granite3.2 for generation, gemma3:4b for quality checks)
- Includes gate logic to prevent poor quality outputs from propagating

**Benefits:**
- Better task decomposition and focus
- Quality control at each step
- Easier debugging and refinement
- Reduced complexity per individual prompt

### 2. 🔀 [Parallelization](./parallelization/)
**Concurrent task execution with result aggregation**

The parallelization pattern enables multiple AI agents to work simultaneously on different aspects of a problem, then aggregates their outputs for comprehensive results.

**Two Key Variations:**
- **Sectioning**: Independent subtasks handled by specialized agents
- **Voting**: Same task executed multiple times for consensus

**Example Implementation:**
- Code review system with specialized agents (Security, Performance, Maintainability)
- Voting mechanism for critical vulnerability detection
- Senior reviewer aggregates all parallel results

**Benefits:**
- Faster execution through parallel processing
- Specialized expertise for different domains
- Increased reliability through consensus
- Comprehensive coverage of complex problems

### 3. 🧭 [Routing](./router/)
**Intelligent request classification and specialized handling**

The routing pattern classifies incoming requests and directs them to specialized handlers, enabling separation of concerns and domain-specific optimization.

**Example Implementation:**
- Customer support system with intelligent routing
- Four specialized handlers: Customer Service, Technical Support, General Inquiry, Escalation
- Confidence scoring for routing decisions
- Structured decision parsing and validation

**Benefits:**
- Specialized prompts for different request types
- Better performance through focused optimization
- Scalable architecture for diverse use cases
- Clear separation of concerns

## 🛠 Technical Stack

- **Framework**: .NET 9.0
- **AI Library**: Microsoft Semantic Kernel
- **LLM Provider**: Ollama (local models)
- **Models Used**: 
  - granite3.2 (primary generation)
  - gemma3:4b (quality checks and specialized tasks)

## 🏃‍♂️ Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Ollama](https://ollama.ai/) with granite3.2 and gemma3:4b models

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/multi-agent-pattern-catalog.git
   cd multi-agent-pattern-catalog
   ```

2. Install Ollama models:
   ```bash
   ollama pull granite3.2
   ollama pull gemma3:4b
   ```

3. Run any pattern example:
   ```bash
   cd parallelization
   dotnet run
   ```

## 📚 Learning Resources

This implementation is inspired by and follows the patterns described in:

**[Building Effective Agents](https://www.anthropic.com/engineering/building-effective-agents)** by Anthropic

The article provides excellent theoretical foundation and practical guidance for implementing these patterns. This repository serves as a hands-on companion with working C# implementations.

## 🤝 Contributing

Contributions are welcome! Whether it's:
- Adding new patterns from the Anthropic article
- Improving existing implementations
- Adding support for different LLM providers
- Enhancing documentation and examples

Please feel free to open issues or submit pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Anthropic](https://www.anthropic.com/) for the excellent "Building Effective Agents" article
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) team
- [Ollama](https://ollama.ai/) for making local LLM deployment accessible