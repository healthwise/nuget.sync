namespace org.healthwise.ops.nugetsync.Providers
{
    public interface IProviderFactory
    {
        IPackageProvider LoadProvider(string providerType, string repositoryUrl, string writeToken, string readUsername, string readPassword);
    }
}