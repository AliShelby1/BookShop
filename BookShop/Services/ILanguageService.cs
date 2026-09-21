namespace BookShop.Services
{
    public interface ILanguageService
    {
        string CurrentCulture { get; }
        bool IsRtl { get; }
        string this[string key] { get; }
        string T(string key, string? fallback = null);
        IReadOnlyDictionary<string, string> GetSupportedLanguages();
    }
}
