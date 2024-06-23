using Pulumi;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.Web.Inputs;
using SecretArgs = Pulumi.AzureNative.KeyVault.SecretArgs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources
{
    public class KeyVaultSecret : ComponentResource
    {
        private readonly Secret _secret;

        public KeyVaultSecret(Output<string> keyVault, Output<string> resourceGroupName, string key, Output<string> value) : base("Custom:KeyVault:Secret:KeyVaultSecret", $"KeyVaultSecret.{key}", null, null)
        {
            _secret = new Secret($"KeyVault.Secret.{key}", new SecretArgs()
            {
                SecretName = key,
                ResourceGroupName = resourceGroupName,
                VaultName = keyVault,
                Properties = new SecretPropertiesArgs
                {
                    Value = value
                }
            });
        }

        public Output<string> KeyVaultReference => Output.Format($"@Microsoft.KeyVault(SecretUri={_secret.Properties.Apply(x => x.SecretUri)})");
    }
}