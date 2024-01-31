using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulumi;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;


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

            //#region Functions Linux

            //var appServicePlanFunctionsLinux = new AppServicePlan("AppServicePlan.Functions.Linux", new AppServicePlanArgs
            //{
            //    Name = "asp-func-linux-brickshoarder-prd",
            //    ResourceGroupName = resourceGroup.Name,
            //    Location = resourceGroup.Location,
            //    Kind = "Linux",
            //    Reserved = true,
            //    Sku = new SkuDescriptionArgs
            //    {
            //        Name = "Y1",
            //        Tier = "Dynamic"
            //    },
            //});

            //var functionApp = new WebApp("WebApp.Functions.Linux", new WebAppArgs
            //{
            //    Name = "func-linux-brickshoarder-prd2",
            //    ResourceGroupName = resourceGroup.Name,
            //    ServerFarmId = appServicePlanFunctionsLinux.Id,
            //    SiteConfig = new SiteConfigArgs
            //    {
            //        AppSettings = new[]
            //        {
            //            new NameValuePairArgs
            //            {
            //                Name = "FUNCTIONS_WORKER_RUNTIME",
            //                Value = "dotnet-isolated"
            //            },
            //            new NameValuePairArgs
            //            {
            //                Name = "AzureWebJobsStorage",
            //                Value = StorageAccountConnectionString
            //            },
            //            new NameValuePairArgs
            //            {
            //                Name = "FUNCTIONS_EXTENSION_VERSION",
            //                Value = "~4"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING",
            //                Value = StorageAccountConnectionString
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "WEBSITE_CONTENTSHARE",
            //                Value = "func-linux-brickshoarder-prd"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED",
            //                Value = "1"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
            //                Value = appInsights.ConnectionString
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "PLAYWRIGHT_BROWSERS_PATH",
            //                Value = "/home/site/wwwroot/.playwright/ms-playwright"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "ServiceBusConnectionString",
            //                Value = ServiceBusConnectionString
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "Rebrickable__Url",
            //                Value = "https://rebrickable.com"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "Rebrickable__Key",
            //                Value = rebrickableKeyOutput
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Host",
            //                Value = "psql-brickshoarder-prd.postgres.database.azure.com"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Database",
            //                Value = dBForPostgreSqlDatabase.Name
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Username",
            //                Value = "brickshoarder_admin"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Password",
            //                Value = DbForPostgreSqlAdminPassword
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__Endpoint",
            //                Value = Endpoint
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__SharedAccessKeyName",
            //                Value = SharedAccessKeyName
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__SharedAccessKey",
            //                Value = SharedAccessKey
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "BrickshoarderDb__Url",
            //                Value = "sql-brickshoarder-prd.database.windows.net,1433"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "BrickshoarderDb__Catalog",
            //                Value = "brickshoarder"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "BrickshoarderDb__User",
            //                Value = "brickshoarder_admin"
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "BrickshoarderDb__Password",
            //                Value = DbForMsSqlAdminPassword
            //            },
            //            new NameValuePairArgs()
            //            {
            //                Name = "Redis__ConnectionString",
            //                Value = redisConnectionStringOutput
            //            }
            //        }
            //    },
            //    Kind = "functionapp",
            //    HttpsOnly = true
            //});

            //#endregion Functions Linux
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