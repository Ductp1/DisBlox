namespace DisBlox.Services;

public interface ICredentialStore
{
    void Save(string key, string value);
    string? Get(string key);
    void Remove(string key);
}
