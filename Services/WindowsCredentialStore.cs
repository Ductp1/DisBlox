using Windows.Security.Credentials;

namespace DisBlox.Services;

public sealed class WindowsCredentialStore : ICredentialStore
{
    private const string ResourceName = "DisBlox";
    private readonly PasswordVault _vault = new();

    public void Save(string key, string value)
    {
        Remove(key);
        _vault.Add(new PasswordCredential(ResourceName, key, value));
    }

    public string? Get(string key)
    {
        try
        {
            var credential = _vault.Retrieve(ResourceName, key);
            credential.RetrievePassword();
            return credential.Password;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void Remove(string key)
    {
        try
        {
            var credential = _vault.Retrieve(ResourceName, key);
            _vault.Remove(credential);
        }
        catch (Exception)
        {
        }
    }
}
