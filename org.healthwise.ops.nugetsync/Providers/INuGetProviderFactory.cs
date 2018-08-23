namespace org.healthwise.ops.nugetsync.Providers
{
    public interface INuGetProviderFactory
    {
        IPackageProvider LoadProvider(string providerType, string repositoryUrl, string writeToken, string readUsername, string readPassword);
    }
}