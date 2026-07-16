namespace UI.Services.Interfaces
{
    public interface IStorageService
    {
        Task SetItemAsync(string key, string value);
        Task<string?> GetItemAsync(string key);
        Task RemoveItemAsync(string key);
    }
}