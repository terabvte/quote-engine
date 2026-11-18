// Snippet 1 — AI quote orchestration (C#)
// Adapted from AutoPreventivo.Application/Features/Quotes/GenerateQuote/GenerateQuoteHandler.cs
// Shortened for the case study: the structured-input branch and the disabled email block are
// elided. No catalogue codes or prices appear here.

using AutoPreventivo.Domain.Entities;
using AutoPreventivo.Domain.Interfaces;
using Edilvos.Domain.Abstractions;
using MediatR;

public class GenerateQuoteHandler(
    IQuoteGeneratorService quoteService,
    ICaptchaService captchaService,
    IQuoteRepository quoteRepository,
    IPdfGeneratorService pdfGenerator,
    IBlobStorageService blobStorage,
    IIntentParsingService intentParsingService)
    : IRequestHandler<GenerateQuoteCommand, Result<Quote>>
{
    public async Task<Result<Quote>> Handle(
        GenerateQuoteCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Security check runs before any model call or database write.
        var captchaResult = await captchaService.VerifyTokenAsync(request.RecaptchaToken);
        if (captchaResult.IsFailure)
            return Result.Failure<Quote>(captchaResult.Error);

        // 2. Turn free text into structured intent. The parser owns its own
        //    schema validation, repair and fallback.
        var intent = await intentParsingService.ParseAsync(request.Prompt);

        // 3. The recipe, not the model, decides quantities and line items.
        var recipeName = string.IsNullOrWhiteSpace(intent.RecipeName)
            ? "bathroom_renovation"
            : intent.RecipeName.ToLower().Trim();

        var quote = await quoteService.GenerateFromRecipeAsync(recipeName, intent);

        // 4. Client details are attached after generation; they never influence pricing.
        quote.ClientName = request.ClientName;
        quote.ClientEmail = request.ClientEmail;
        quote.ClientPhone = request.ClientPhone;
        quote.ClientAddress = request.ClientAddress;

        // 5. Same PDF and storage services the manual path uses.
        var pdfBytes = pdfGenerator.GenerateQuotePdf(quote);
        quote.PdfUrl = await blobStorage.UploadAsync(
            pdfBytes,
            $"{quote.Id}.pdf",
            "application/pdf");

        await quoteRepository.AddAsync(quote, cancellationToken);

        return Result.Success(quote);
    }
}
