namespace SupermarketAPI.Scrapers.Abstractions
{
	public interface IScraper
	{
		string Name { get; }
		Task<IReadOnlyList<ScrapedProduct>> ScrapeAsync(CancellationToken ct = default);
	}

	public record ScrapedProduct(
		string Name,
		string NormalizedName,
		string Brand,
		string CategoryPath,
		string ImageUrl,
		string Url,
		decimal Price,
		string Unit,
		bool IsAvailable,
		bool IsOnSale,
		decimal? OriginalPrice
	);
}
