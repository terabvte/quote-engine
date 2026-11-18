// Snippet 2 — CreaTivo manual quote creation (C#)
// Adapted from AutoPreventivo.Application/Features/Quotes/CreateManual/CreateManualQuoteHandler.cs
// Shortened for the case study: the PDF try/catch and the explicit null branch were removed.
// Creation behaviour for a non-draft quote is unchanged.

using AutoPreventivo.Domain.Entities;
using AutoPreventivo.Domain.Interfaces;
using Edilvos.Domain.Abstractions;
using MediatR;

public class CreateManualQuoteHandler(
    IQuoteRepository quoteRepository,
    IPdfGeneratorService pdfGenerator,
    IBlobStorageService blobStorage)
    : IRequestHandler<CreateManualQuoteCommand, Result<Quote>>
{
    public async Task<Result<Quote>> Handle(
        CreateManualQuoteCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. The operator supplies every line item and unit price.
        var quote = new Quote
        {
            Title = request.Title,
            Origin = "Manual",
            IsDraft = request.IsDraft,
            ClientName = request.ClientName,
            ClientEmail = request.ClientEmail,
            ClientPhone = request.ClientPhone,
            ClientAddress = request.ClientAddress,
            VatRate = request.VatRate,
            InternalNotes = request.InternalNotes,
            PublicNotes = request.PublicNotes,
            Items = request.Items
                .Select((item, index) => new QuoteItem
                {
                    Position = index,
                    Category = item.Category,
                    Description = item.Description,
                    Unit = item.Unit,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Code = "MANUAL_ENTRY"
                })
                .ToList()
        };

        // 2. Drafts stop here: no PDF, no blob write, PdfUrl stays null.
        if (!quote.IsDraft)
        {
            // 3. Published quotes reuse the AI path's PDF and storage services.
            var pdfBytes = pdfGenerator.GenerateQuotePdf(quote);
            quote.PdfUrl = await blobStorage.UploadAsync(
                pdfBytes,
                $"manual-{quote.Id}.pdf",
                "application/pdf");
        }

        // 4. Same repository as the AI path.
        await quoteRepository.AddAsync(quote, cancellationToken);

        return Result.Success(quote);
    }
}
