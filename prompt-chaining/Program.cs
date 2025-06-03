// See https://aka.ms/new-console-template for more information

using Microsoft.SemanticKernel;
using System;
using System.Threading.Tasks;

#pragma warning disable SKEXP0070


var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("granite3.2", new Uri("http://localhost:11434"))
    .Build();

// Create a separate kernel for quality verification
var qualityKernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion("gemma3:4b", new Uri("http://localhost:11434"))
    .Build();

// Step 1: Generate outline
var outline = await kernel.InvokePromptAsync("Generate an outline for a blog post about AI agents.");
Console.WriteLine("Outline:\n" + outline);

// Step 2: Gate - Check outline quality using gemma3:4b
var check = await qualityKernel.InvokePromptAsync($"Does this outline meet the criteria for a good blog post? Please respond with 'Yes' if it has clear structure, logical flow, and covers key topics about AI agents, or 'No' if it needs improvement:\n\n{outline}");
Console.WriteLine("Gate check:\n" + check);

// Step 3: If gate passes, write the document
if (check.ToString().Contains("Yes", StringComparison.OrdinalIgnoreCase))
{
    var document = await kernel.InvokePromptAsync($"Write a blog post based on this outline: {outline}");
    Console.WriteLine("\nGenerated Document:\n" + document);
}
else
{
    Console.WriteLine("Outline did not meet criteria. Please revise.");
}