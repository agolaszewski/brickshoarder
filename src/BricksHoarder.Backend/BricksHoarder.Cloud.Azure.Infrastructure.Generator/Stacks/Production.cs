using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Production : Stack
    {
        public Production()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-brickshoarder-prd"
            });

            var serviceBusNamespace = new StandardServiceBusNamespace("Default", "prd", resourceGroup);
            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint;
            SharedAccessKey = serviceBusNamespace.SharedAccessKey;
            SharedAccessKeyName = serviceBusNamespace.SharedAccessKeyName;
            ServiceBusConnectionString = serviceBusNamespace.ServiceBusConnectionString;

            #region PostgreSQL

            var dBForPostgreSqlAdministratorLoginPassword = new Pulumi.Random.RandomPassword("DBforPostgreSQL.AdministratorLoginPassword", new()
            {
                Length = 20,
            });
            DbForPostgreSqlAdminPassword = Output.Unsecret(dBForPostgreSqlAdministratorLoginPassword.Result);

            var dBForPostgreSqlServer = new Pulumi.AzureNative.DBforPostgreSQL.V20221201.Server("DBforPostgreSQL.Server", new()
            {
                ResourceGroupName = resourceGroup.Name,
                ServerName = "psql-brickshoarder-prd",
                AdministratorLogin = "brickshoarder_admin",
                AdministratorLoginPassword = dBForPostgreSqlAdministratorLoginPassword.Result,
                CreateMode = Pulumi.AzureNative.DBforPostgreSQL.V20221201.CreateMode.Default,
                Version = Pulumi.AzureNative.DBforPostgreSQL.V20221201.ServerVersion.ServerVersion_14,
                Sku = new Pulumi.AzureNative.DBforPostgreSQL.V20221201.Inputs.SkuArgs()
                {
                    Name = "Standard_B1ms",
                    Tier = Pulumi.AzureNative.DBforPostgreSQL.V20221201.SkuTier.Burstable
                },
                Storage = new Pulumi.AzureNative.DBforPostgreSQL.V20221201.Inputs.StorageArgs()
                {
                    StorageSizeGB = 32
                }
            });

            new Pulumi.AzureNative.DBforPostgreSQL.V20221201.FirewallRule("DBforPostgreSQL.Server.FirewallRule", new Pulumi.AzureNative.DBforPostgreSQL.V20221201.FirewallRuleArgs
            {
                EndIpAddress = "255.255.255.255",
                ResourceGroupName = resourceGroup.Name,
                ServerName = dBForPostgreSqlServer.Name,
                StartIpAddress = "0.0.0.0",
                FirewallRuleName = "DevAllAccess"
            });

            var dBForPostgreSqlDatabase = new Pulumi.AzureNative.DBforPostgreSQL.V20221201.Database("DBforPostgreSQL.Server.Database", new()
            {
                ServerName = dBForPostgreSqlServer.Name,
                ResourceGroupName = resourceGroup.Name,
                DatabaseName = "brickshoarder"
            });

            #endregion PostgreSQL

            #region Storage Account

            var storageAccountFunctions = new StorageAccount("StorageAccount", new Pulumi.AzureNative.Storage.StorageAccountArgs
            {
                AccountName = "stbrickshoarderdev",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
                {
                    Name = "Standard_LRS"
                },
                Kind = Pulumi.AzureNative.Storage.Kind.StorageV2
            });

            Output<string> listStorageAccountKeysOutput = Output.All(storageAccountFunctions.Name).Apply(async x =>
            {
                var keys = await ListStorageAccountKeys.InvokeAsync(new ListStorageAccountKeysArgs()
                {
                    ResourceGroupName = resourceGroup.Name.Convert(),
                    AccountName = storageAccountFunctions.Name.Convert(),
                });
                return keys.Keys[0].Value;
            });

            StorageAccountConnectionString = Output.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccountFunctions.Name};AccountKey={listStorageAccountKeysOutput.Apply(x => x)};EndpointSuffix=core.windows.net");

            #endregion Storage Account

            #region Application Insight

            var workspace = new Pulumi.AzureNative.OperationalInsights.Workspace("Workspace", new()
            {
                WorkspaceName = "log-appi-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                Sku = new WorkspaceSkuArgs
                {
                    Name = Pulumi.AzureNative.OperationalInsights.WorkspaceSkuNameEnum.PerGB2018
                },
                WorkspaceCapping = new WorkspaceCappingArgs()
                {
                    DailyQuotaGb = 0.5
                }
            });

            var appInsights = new Component("AppInsights", new ComponentArgs
            {
                ResourceName = "appi-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                ApplicationType = "web",
                FlowType = "Bluefield",
                Kind = "web",
                RequestSource = "rest",
                WorkspaceResourceId = workspace.Id
            });

            #endregion Application Insight

            #region Functions Linux

            var containerAppEnv = new ManagedEnvironment("ContainerAppsEnvironment", new ManagedEnvironmentArgs
            {
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                EnvironmentName = "cae-brickshoarder-prd",
                Sku = new EnvironmentSkuPropertiesArgs()
                {
                    Name = "Consumption"
                },
                DaprAIInstrumentationKey = appInsights.InstrumentationKey
            });

            var containerImage = $"{config["DockerHub:Registry"]}/{config["DockerHub:Username"]}/brickshoarder:latest";

            var functionApp = new WebApp("WebApp.Functions.Linux", new WebAppArgs
            {
                Name = "func-cea-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                ManagedEnvironmentId = containerAppEnv.Id,
                SiteConfig = new SiteConfigArgs
                {
                    LinuxFxVersion = $"DOCKER|{containerImage}",
                    NetFrameworkVersion = null,
                    AppSettings = new[]
                    {
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_URL",
                            Value = config["DockerHub:Registry"]
                        },
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_USERNAME",
                            Value = config["DockerHub:Username"]
                        },
                        new NameValuePairArgs
                        {
                            Name = "DOCKER_REGISTRY_SERVER_PASSWORD",
                            Value = config["DockerHub:Password"]
                        },
                        new NameValuePairArgs
                        {
                            Name = "AzureWebJobsStorage",
                            Value = StorageAccountConnectionString
                        },
                        new NameValuePairArgs
                        {
                            Name = "FUNCTIONS_EXTENSION_VERSION",
                            Value = "~4"
                        },
                        new NameValuePairArgs()
                        {
                            Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                            Value = appInsights.ConnectionString
                        },
                        new NameValuePairArgs()
                        {
                            Name = "PLAYWRIGHT_BROWSERS_PATH",
                            Value = "/home/site/wwwroot/.playwright/ms-playwright"
                        },
                        new NameValuePairArgs()
                        {
                            Name = "ServiceBusConnectionString",
                            Value = ServiceBusConnectionString
                        },
                        new NameValuePairArgs()
                        {
                            Name = "Rebrickable__Url",
                            Value = "https://rebrickable.com"
                        },
                        new NameValuePairArgs()
                        {
                            Name = "Rebrickable__Key",
                            Value = config["Rebrickable:Key"]
                        },
                        new NameValuePairArgs()
                        {
                            Name = "MartenAzure__Host",
                            Value = "psql-brickshoarder-prd.postgres.database.azure.com"
                        },
                        new NameValuePairArgs()
                        {
                            Name = "MartenAzure__Database",
                            Value = dBForPostgreSqlDatabase.Name
                        },
                        new NameValuePairArgs()
                        {
                            Name = "MartenAzure__Username",
                            Value = "brickshoarder_admin"
                        },
                        new NameValuePairArgs()
                        {
                            Name = "MartenAzure__Password",
                            Value = DbForPostgreSqlAdminPassword
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__Endpoint",
                            Value = serviceBusNamespace.ServiceBusEndpoint
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__SharedAccessKeyName",
                            Value = serviceBusNamespace.SharedAccessKeyName
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__SharedAccessKey",
                            Value = serviceBusNamespace.SharedAccessKey
                        },
                        new NameValuePairArgs()
                        {
                            Name = "Redis__ConnectionString",
                            Value = config["Redis:ConnectionString"]
                        },
                        new NameValuePairArgs()
                        {
                            Name = "Rev",
                            Value = "123"
                        }
                    }
                },
                Kind = "functionapp",
                HttpsOnly = true
            });

            #endregion Functions Linux
        }

        [Output]
        public Output<string> ServiceBusEndpoint { get; set; }

        [Output]
        public Output<string> SharedAccessKey { get; set; }

        [Output]
        public Output<string> SharedAccessKeyName { get; set; }

        [Output]
        public Output<string> ServiceBusConnectionString { get; set; }

        [Output]
        public Output<string> DbForPostgreSqlAdminPassword { get; set; }

        [Output]
        public Output<string> StorageAccountConnectionString { get; set; }
    }
}