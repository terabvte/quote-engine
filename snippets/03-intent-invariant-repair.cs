// Snippet 3 — schema repair after model parsing (C#)
// Adapted from AutoPreventivo.Infrastructure/Services/OpenAiIntentParsingService.cs
// Only the parse-and-repair path is shown. The model call, prompt and routing rules are elided.

public class OpenAiIntentParsingService
{
    public async Task<UserIntent> ParseAsync(string userPrompt)
    {
        // Model call elided; returns the raw JSON string.
        var json = await CallModelAsync(userPrompt);

        // Deserialisation is case-insensitive, tolerates comments and trailing commas,
        // and uses a converter for Italian decimal separators.
        var intent = JsonSerializer.Deserialize<UserIntent>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                Converters = { new ItalianDoubleConverter() }
            });

        // Repair cross-field invariants before returning.
        if (intent is not null)
        {
            // Invariant: full_renovation requires FullRenovation metadata.
            // Missing metadata means the model named a recipe but omitted the structural object.
            // Repair with defaults and attach a warning so the operator sees it.
            if (intent.RecipeName == "full_renovation" && intent.FullRenovation == null)
            {
                intent.FullRenovation = new FullRenovationMeta();
                intent.Warnings.Add(
                    "AI parser returned 'full_renovation' without FullRenovation data. " +
                    "Defaults applied. Operator review recommended.");
            }
        }

        // If parsing throws, the caller receives an empty template with a warning,
        // rather than a partial quote.
        return intent ?? CreateSafeFallback();
    }

    // Defaults are deliberately conservative; the operator must review the result.
    private static UserIntent CreateSafeFallback() => new()
    {
        RecipeName = "bathroom_renovation",
        Dimensions = new RoomDimensions(),
        Scope = new RenovationScopeFlags(),
        Warnings = new List<string>
        {
            "AI parsing failed — operator review required before sending to client."
        }
    };

    // Model call omitted from this excerpt.
    private Task<string> CallModelAsync(string prompt) => throw new NotImplementedException();
}
