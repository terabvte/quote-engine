// Snippet 4 — exact-total assertion (C# test)
// Adapted from tests/AutoPreventivo.Tests/Pricing/DeterministicPricingTests.cs on the
// test/autopreventivo-xunit branch. The shape of the test is unchanged; catalogue codes and
// unit prices are synthetic replacements, so the numbers do not match the private price list.

using AutoPreventivo.Domain.Entities;
using AutoPreventivo.Domain.Interfaces;
using AutoPreventivo.Domain.Recipes;
using AutoPreventivo.Infrastructure.Services;
using Moq;

public class DeterministicPricingTests
{
    private static readonly Dictionary<string, PriceListEntry> Catalogue = new()
    {
        ["T-001"] = Entry("T-001", "Scaffolding", "Fixed scaffolding", "mq", 5.00m),
        ["T-002"] = Entry("T-002", "Repair", "Facade repair", "mq", 10.00m),
        ["T-003"] = Entry("T-003", "Insulation", "External insulation", "mq", 40.00m),
        ["T-004"] = Entry("T-004", "Paint", "Exterior paint", "mq", 7.00m),
        ["T-005"] = Entry("T-005", "Openings", "Window reveals", "cad", 50.00m),
        ["T-006"] = Entry("T-006", "Site setup", "Extended site setup", "corpo", 300.00m)
    };

    private readonly Mock<IPriceListService> _priceList = new();

    public DeterministicPricingTests()
    {
        _priceList
            .Setup(s => s.GetItemByCodeAsync(It.IsAny<string>()))
            .ReturnsAsync((string code) =>
                Catalogue.TryGetValue(code, out var entry) ? entry : null);

        // If every line has a known code, the semantic search must never run.
        _priceList
            .Setup(s => s.FindBestMatchAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<List<string>?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException(
                "Semantic search must not run when every item has a known catalogue code"));
    }

    [Fact]
    public async Task KnownIntentAndCatalogue_ProducesExactTotal()
    {
        var intent = new UserIntent
        {
            Dimensions = new RoomDimensions { PerimeterMl = 30, HeightMl = 9 },
            Scope = new RenovationScopeFlags
            {
                ConstMasonryAssist = ScopeItem.Active(),
                ConstPlaster = ScopeItem.Active(),
                FinPainting = ScopeItem.Active(),
                SupplyDoors = ScopeItem.Active(qty: 4)
            }
        };

        var recipe = new FacadeRenovationRecipe(_priceList.Object);
        var generator = new OpenAiQuoteGeneratorService(
            _priceList.Object,
            new IRecipe[] { recipe },
            new VectorSearchTracker());

        var quote = await generator.GenerateFromRecipeAsync("facade_renovation", intent);

        // 30 m perimeter x 9 m height = 270 m2 of facade.
        // 270 x (5 + 10 + 40 + 7) + 4 x 50 = 16,940.00
        // Generator prepends site setup T-006: 1 x 300.00
        // Total: 17,240.00
        Assert.Equal(6, quote.Items.Count);
        Assert.Equal("T-006", quote.Items.First().Code);
        Assert.Equal(17240.00m, quote.TotalPrice);

        _priceList.Verify(
            s => s.FindBestMatchAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<List<string>?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<string?>()),
            Times.Never);
    }

    private static PriceListEntry Entry(
        string code,
        string category,
        string description,
        string unit,
        decimal price) => new()
        {
            Code = code,
            Category = category,
            FullDescription = description,
            Unit = unit,
            Price = price
        };
}
