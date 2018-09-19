using System;

namespace org.healthwise.ops.nugetsync.Providers
{
    public class NpmProviderFactory : IProviderFactory
    {
        public IPackageProvider LoadProvider(string providerType, string repositoryUrl, string writeToken, string readUsername,
            string readPassword)
        {
                return new NpmPackageProvider(repositoryUrl, writeToken, readUsername, readPassword, providerType);
        }
    }
}