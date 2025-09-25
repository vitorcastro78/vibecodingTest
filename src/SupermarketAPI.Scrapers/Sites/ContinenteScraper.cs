using SupermarketAPI.Scrapers.Abstractions;
using SupermarketAPI.Scrapers.Services;

namespace SupermarketAPI.Scrapers.Sites
{
    public class ContinenteScraper : IScraper
    {
        public string Name => "Continente";

        public async Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default)
        {
            // TODO: Implementar scraping real do Continente
            await Task.Delay(1000, ct); // Simular delay de rede
            
            return new List<ScrapedProduct>
            {
                new ScrapedProduct(
                    Name: "Produto Continente Teste",
                    NormalizedName: "produto continente teste",
                    Brand: "Marca Teste",
                    CategoryPath: "Geral",
                    ImageUrl: "",
                    Url: "",
                    Price: 2.00m,
                    Unit: "un",
                    IsAvailable: true,
                    IsOnSale: false,
                    OriginalPrice: null
                )
            }.AsReadOnly();
        }
    }
}