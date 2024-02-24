namespace StorageService.Services;

public interface IStorageService<in T>
{
    Task StoreAsync(T content);
}
