using System;

namespace org.healthwise.ops.nugetsync.Providers
{
    public class NuGetProviderFactory: INuGetProviderFactory
    {
        public IPackageProvider LoadProvider(string providerType, string repositoryUrl, string writeToken, string readUsername,
            string readPassword)
        {
            if (providerType.ToLower().Equals("proget"))
            {
                return new ProGetPackageProvider(repositoryUrl,writeToken,readUsername,readPassword);
            }

            if (providerType.ToLower().Equals("myget"))
            {
                return new MyGetPackageProvider(repositoryUrl, writeToken, readUsername, readPassword);
            }

            throw new Exception($"Unknown NuGet Provider {providerType}");
        }
    }
}