using System.Text.Json;
using System.Text.RegularExpressions;

namespace router;

/// <summary>
/// Handles parsing of LLM responses into structured RouteDecision objects.
/// This class exists because LLMs often return JSON embedded in conversational text,
/// requiring robust extraction and fallback parsing strategies.
/// </summary>
public class RouteDecisionParser
{
    private readonly JsonSerializerOptions _jsonOptions;

    public RouteDecisionParser()
    {
        // Configure JSON options for flexible parsing
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, // Handle variations like "routeType" vs "RouteType"
            AllowTrailingCommas = true,         // Be lenient with JSON formatting
            ReadCommentHandling = JsonCommentHandling.Skip // Ignore JSON comments if present
        };
    }

    /// <summary>
    /// Parses an LLM response into a RouteDecision object.
    /// Uses multiple parsing strategies to handle various response formats.
    /// </summary>
    /// <param name="llmResponse">The raw response from the language model</param>
    /// <param name="debugOutput">Optional action to capture debug information</param>
    /// <returns>A parsed RouteDecision or a fallback decision if parsing fails</returns>
    public RouteDecision ParseResponse(string llmResponse, Action<string>? debugOutput = null)
    {
        var content = llmResponse?.Trim() ?? "";
        debugOutput?.Invoke($"Raw model response: {content}");

        // Strategy 1: Try to parse as direct JSON
        var directJsonResult = TryParseDirectJson(content, debugOutput);
        if (directJsonResult != null)
        {
            return directJsonResult;
        }

        // Strategy 2: Extract JSON from conversational text
        var extractedJsonResult = TryParseExtractedJson(content, debugOutput);
        if (extractedJsonResult != null)
        {
            return extractedJsonResult;
        }

        // Strategy 3: Manual parsing using regex patterns
        // This fallback exists because LLMs sometimes return structured data
        // in natural language format rather than strict JSON
        var manualParseResult = TryManualParsing(content, debugOutput);
        if (manualParseResult != null)
        {
            return manualParseResult;
        }

        // Final fallback
        debugOutput?.Invoke("All parsing strategies failed, using fallback");
        return CreateFallbackDecision("All parsing strategies failed");
    }

    /// <summary>
    /// Attempts to parse the response as direct JSON without any text extraction.
    /// This works when the LLM follows instructions perfectly and returns only JSON.
    /// </summary>
    private RouteDecision? TryParseDirectJson(string content, Action<string>? debugOutput)
    {
        try
        {
            var routeDecision = JsonSerializer.Deserialize<RouteDecision>(content, _jsonOptions);
            if (IsValidRouteDecision(routeDecision))
            {
                debugOutput?.Invoke("Successfully parsed as direct JSON");
                return routeDecision;
            }
        }
        catch (JsonException ex)
        {
            debugOutput?.Invoke($"Direct JSON parsing failed: {ex.Message}");
        }
        
        return null;
    }

    /// <summary>
    /// Extracts JSON from conversational text and attempts to parse it.
    /// This handles cases where the LLM includes explanatory text along with the JSON.
    /// Example: "Based on your request, here's the classification: {JSON}"
    /// </summary>
    private RouteDecision? TryParseExtractedJson(string content, Action<string>? debugOutput)
    {
        try
        {
            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                debugOutput?.Invoke($"Extracted JSON: {jsonContent}");
                
                var routeDecision = JsonSerializer.Deserialize<RouteDecision>(jsonContent, _jsonOptions);
                
                if (IsValidRouteDecision(routeDecision))
                {
                    debugOutput?.Invoke("Successfully parsed extracted JSON");
                    return routeDecision;
                }
                else
                {
                    debugOutput?.Invoke($"Invalid RouteType in extracted JSON: {routeDecision?.RouteType}");
                }
            }
        }
        catch (JsonException ex)
        {
            debugOutput?.Invoke($"Extracted JSON parsing failed: {ex.Message}");
        }
        
        return null;
    }

    /// <summary>
    /// Manual parsing using regex patterns to extract structured information.
    /// This fallback strategy exists because LLMs sometimes respond in natural language
    /// even when asked for JSON, especially when they want to be helpful or explanatory.
    /// 
    /// Examples it can handle:
    /// - "This is a CustomerService request with 85% confidence because..."
    /// - "Category: TechnicalSupport, Confidence: 0.9, Reason: software issue"
    /// - "I'm 90% confident this is a GeneralInquiry about product features"
    /// </summary>
    private RouteDecision? TryManualParsing(string content, Action<string>? debugOutput)
    {
        debugOutput?.Invoke("Attempting manual parsing with regex patterns");
        
        // Pattern 1: Look for explicit route type mentions
        var routeType = ExtractRouteTypeFromText(content);
        if (routeType == null)
        {
            debugOutput?.Invoke("Could not extract route type from text");
            return null;
        }

        // Pattern 2: Extract confidence score
        var confidence = ExtractConfidenceFromText(content);
        
        // Pattern 3: Extract reasoning
        var reasoning = ExtractReasoningFromText(content);

        debugOutput?.Invoke($"Manual parsing result - RouteType: {routeType}, Confidence: {confidence}");
        
        return new RouteDecision
        {
            RouteType = routeType.Value,
            Confidence = confidence,
            Reasoning = reasoning
        };
    }

    /// <summary>
    /// Extracts RouteType from natural language text using keyword matching.
    /// </summary>
    private RouteType? ExtractRouteTypeFromText(string content)
    {
        var lowerContent = content.ToLowerInvariant();
        
        // Check for explicit mentions of route types
        if (lowerContent.Contains("customerservice") || lowerContent.Contains("customer service"))
            return RouteType.CustomerService;
        if (lowerContent.Contains("technicalsupport") || lowerContent.Contains("technical support"))
            return RouteType.TechnicalSupport;
        if (lowerContent.Contains("generalinquiry") || lowerContent.Contains("general inquiry"))
            return RouteType.GeneralInquiry;
        if (lowerContent.Contains("escalation"))
            return RouteType.Escalation;

        // Check for contextual keywords
        if (lowerContent.Contains("billing") || lowerContent.Contains("refund") || lowerContent.Contains("subscription"))
            return RouteType.CustomerService;
        if (lowerContent.Contains("bug") || lowerContent.Contains("crash") || lowerContent.Contains("technical"))
            return RouteType.TechnicalSupport;
        if (lowerContent.Contains("angry") || lowerContent.Contains("complaint") || lowerContent.Contains("unacceptable"))
            return RouteType.Escalation;

        return null;
    }

    /// <summary>
    /// Extracts confidence score from text using regex patterns.
    /// Handles various formats like "85%", "0.85", "85% confident", etc.
    /// </summary>
    private double ExtractConfidenceFromText(string content)
    {
        // Pattern for percentage (85%, 85 percent, etc.)
        var percentPattern = @"(\d+(?:\.\d+)?)\s*(?:%|percent)";
        var percentMatch = Regex.Match(content, percentPattern, RegexOptions.IgnoreCase);
        if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var percentValue))
        {
            return percentValue / 100.0; // Convert to decimal
        }

        // Pattern for decimal confidence (0.85, 0.9, etc.)
        var decimalPattern = @"(?:confidence|confident).*?(\d\.\d+)";
        var decimalMatch = Regex.Match(content, decimalPattern, RegexOptions.IgnoreCase);
        if (decimalMatch.Success && double.TryParse(decimalMatch.Groups[1].Value, out var decimalValue))
        {
            return decimalValue;
        }

        // Default confidence when manual parsing is used
        return 0.75; // Indicate moderate confidence for manual parsing
    }

    /// <summary>
    /// Extracts reasoning from text by looking for explanatory phrases.
    /// </summary>
    private string ExtractReasoningFromText(string content)
    {
        // Look for common reasoning patterns
        var reasoningPatterns = new[]
        {
            @"because\s+(.+?)(?:\.|$)",
            @"reason:\s*(.+?)(?:\.|$)",
            @"reasoning:\s*(.+?)(?:\.|$)",
            @"since\s+(.+?)(?:\.|$)"
        };

        foreach (var pattern in reasoningPatterns)
        {
            var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return "Extracted from natural language response using manual parsing";
    }

    /// <summary>
    /// Validates that a RouteDecision object has valid data.
    /// </summary>
    private bool IsValidRouteDecision(RouteDecision? decision)
    {
        return decision != null && 
               Enum.IsDefined(typeof(RouteType), decision.RouteType) &&
               decision.Confidence >= 0 && 
               decision.Confidence <= 1;
    }

    /// <summary>
    /// Creates a fallback RouteDecision when all parsing strategies fail.
    /// </summary>
    private RouteDecision CreateFallbackDecision(string reason)
    {
        return new RouteDecision
        {
            RouteType = RouteType.GeneralInquiry,
            Confidence = 0.3,
            Reasoning = $"Parsing failed: {reason}"
        };
    }
}
