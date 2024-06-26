﻿using System.IO;
using System.Threading.Tasks;
using BricksHoarder.Cloud.Azure.Infrastructure.Generator.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Pulumi;
using Pulumi.AzureNative.App;
using Pulumi.AzureNative.App.Inputs;
using Pulumi.AzureNative.Authorization;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using Pulumi.AzureNative.OperationalInsights.Inputs;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using ManagedServiceIdentityArgs = Pulumi.AzureNative.Web.Inputs.ManagedServiceIdentityArgs;
using NameValuePairArgs = Pulumi.AzureNative.Web.Inputs.NameValuePairArgs;
using PreviewNameValuePairArgs = Pulumi.AzureNative.Web.V20230101.Inputs.NameValuePairArgs;
using PreviewSiteConfigArgs = Pulumi.AzureNative.Web.V20230101.Inputs.SiteConfigArgs;
using PreviewWebApp = Pulumi.AzureNative.Web.V20230101.WebApp;
using PreviewWebAppArgs = Pulumi.AzureNative.Web.V20230101.WebAppArgs;
using SecretArgs = Pulumi.AzureNative.KeyVault.SecretArgs;
using SiteConfigArgs = Pulumi.AzureNative.Web.Inputs.SiteConfigArgs;
using SkuDescriptionArgs = Pulumi.AzureNative.Web.Inputs.SkuDescriptionArgs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator.Stacks
{
    internal class Production : Stack
    {
        public Production()
        {
            var clientConfig = Output.Create(Pulumi.AzureNative.Authorization.GetClientConfig.InvokeAsync());
            var subscriptionId = clientConfig.Apply(x => x.SubscriptionId);
            var tenantId = clientConfig.Apply(x => x.TenantId);

            var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
            {
                ResourceGroupName = "rg-brickshoarder-prd"
            });

            var managedIdentity = new Pulumi.AzureNative.ManagedIdentity.UserAssignedIdentity("ManagedIdentity.UserAssignedIdentity", new Pulumi.AzureNative.ManagedIdentity.UserAssignedIdentityArgs
            {
                ResourceName = "mi-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location
            });

            #region Key Vault

            var keyVault = new Vault("KeyVault", new VaultArgs
            {
                VaultName = "kv-as1-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Properties = new VaultPropertiesArgs
                {
                    Sku = new SkuArgs
                    {
                        Family = SkuFamily.A,
                        Name = Pulumi.AzureNative.KeyVault.SkuName.Standard
                    },
                    TenantId = tenantId,
                    EnableRbacAuthorization = true,
                }
            });

            var keyVaultSecretsUserRoleId = "4633458b-17de-408a-b874-0445c86b69e6";
            var keyVaultRoleAssignment = new RoleAssignment("KeyVault.RoleAssignment", new RoleAssignmentArgs
            {
                PrincipalId = managedIdentity.PrincipalId,
                RoleDefinitionId = Output.Format($"/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{keyVaultSecretsUserRoleId}"),
                Scope = Output.Format($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup.Name}/providers/Microsoft.KeyVault/vaults/{keyVault.Name}"),
                PrincipalType = PrincipalType.ServicePrincipal
            });

            #endregion Key Vault


            #region Service Bus

            var serviceBusNamespace = new StandardServiceBusNamespace("Default", "prd", resourceGroup);
            ServiceBusEndpoint = serviceBusNamespace.ServiceBusEndpoint.Apply(x => x.Replace("https://", string.Empty).Replace(":443", string.Empty));
            SharedAccessKey = serviceBusNamespace.SharedAccessKey;
            SharedAccessKeyName = serviceBusNamespace.SharedAccessKeyName;
            ServiceBusConnectionString = serviceBusNamespace.ServiceBusConnectionString;

            var serviceBusEndpointSecret = new KeyVaultSecret(keyVault.Name, resourceGroup.Name, "ServiceBusEndpoint", ServiceBusEndpoint);
            var sharedAccessKeySecret = new KeyVaultSecret(keyVault.Name, resourceGroup.Name, "SharedAccessKey", SharedAccessKey);
            var sharedAccessKeyNameSecret = new KeyVaultSecret(keyVault.Name, resourceGroup.Name, "SharedAccessKeyName", SharedAccessKeyName);

            #endregion Service Bus

            #region PostgreSQL

            var dBForPostgreSqlAdministratorLoginPassword = new Pulumi.Random.RandomPassword("DBforPostgreSQL.AdministratorLoginPassword", new()
            {
                Length = 20,
            });
            DbForPostgreSqlAdminPassword = Output.Unsecret(dBForPostgreSqlAdministratorLoginPassword.Result);

            var dBForPostgreSqlServer = new Pulumi.AzureNative.DBforPostgreSQL.Server("DBforPostgreSQL.Server", new()
            {
                ResourceGroupName = resourceGroup.Name,
                ServerName = "psql-brickshoarder-prd",
                AdministratorLogin = "brickshoarder_admin",
                AdministratorLoginPassword = dBForPostgreSqlAdministratorLoginPassword.Result,
                CreateMode = Pulumi.AzureNative.DBforPostgreSQL.CreateMode.Default,
                Version = Pulumi.AzureNative.DBforPostgreSQL.ServerVersion.ServerVersion_14,
                Sku = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.SkuArgs()
                {
                    Name = "Standard_B1ms",
                    Tier = Pulumi.AzureNative.DBforPostgreSQL.SkuTier.Burstable
                },
                Storage = new Pulumi.AzureNative.DBforPostgreSQL.Inputs.StorageArgs()
                {
                    StorageSizeGB = 32
                }
            });

            new Pulumi.AzureNative.DBforPostgreSQL.FirewallRule("DBforPostgreSQL.Server.FirewallRule", new Pulumi.AzureNative.DBforPostgreSQL.FirewallRuleArgs
            {
                EndIpAddress = "255.255.255.255",
                ResourceGroupName = resourceGroup.Name,
                ServerName = dBForPostgreSqlServer.Name,
                StartIpAddress = "0.0.0.0",
                FirewallRuleName = "DevAllAccess"
            });

            var dBForPostgreSqlDatabase = new Pulumi.AzureNative.DBforPostgreSQL.Database("DBforPostgreSQL.Server.Database", new()
            {
                ServerName = dBForPostgreSqlServer.Name,
                ResourceGroupName = resourceGroup.Name,
                DatabaseName = "brickshoarder"
            });

            #endregion PostgreSQL

            #region Storage Account

            var storageAccountFunctions = new StorageAccount("StorageAccount", new Pulumi.AzureNative.Storage.StorageAccountArgs
            {
                AccountName = "stbrickshoarderprd",
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
                RetentionInDays = 30,
                Sku = new WorkspaceSkuArgs
                {
                    Name = Pulumi.AzureNative.OperationalInsights.WorkspaceSkuNameEnum.PerGB2018
                },
                WorkspaceCapping = new WorkspaceCappingArgs()
                {
                    DailyQuotaGb = 1
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

            //var containerAppEnv = new ManagedEnvironment("ContainerAppsEnvironment", new ManagedEnvironmentArgs
            //{
            //    ResourceGroupName = resourceGroup.Name,
            //    Location = resourceGroup.Location,
            //    EnvironmentName = "cae-brickshoarder-prd",
            //    Sku = new EnvironmentSkuPropertiesArgs()
            //    {
            //        Name = "Consumption"
            //    },
            //    DaprAIInstrumentationKey = appInsights.InstrumentationKey,
            //});

            //var containerImage = $"{config["DockerHub:Registry"]}/{config["DockerHub:Username"]}/brickshoarder:latest";

            //var functionApp = new PreviewWebApp("WebApp.Functions.BricksHoarder.Functions", new PreviewWebAppArgs
            //{
            //    Name = "func-brickshoarder-functions-prd",
            //    ResourceGroupName = resourceGroup.Name,
            //    Location = resourceGroup.Location,
            //    ManagedEnvironmentId = containerAppEnv.Id,
            //    //ResourceConfig = new ResourceConfigArgs()
            //    //{
            //    //    Cpu = 0.5,
            //    //    Memory = "1Gi"
            //    //},
            //    SiteConfig = new PreviewSiteConfigArgs
            //    {
            //        LinuxFxVersion = $"DOCKER|{containerImage}",
            //        FunctionAppScaleLimit = 1,
            //        NetFrameworkVersion = null,
            //        AppSettings = new[]
            //        {
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "DOCKER_REGISTRY_SERVER_URL",
            //                Value = config["DockerHub:Registry"]
            //            },
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "DOCKER_REGISTRY_SERVER_USERNAME",
            //                Value = config["DockerHub:Username"]
            //            },
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "DOCKER_REGISTRY_SERVER_PASSWORD",
            //                Value = config["DockerHub:Password"]
            //            },
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "AzureWebJobsStorage",
            //                Value = StorageAccountConnectionString
            //            },
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "FUNCTIONS_WORKER_RUNTIME",
            //                Value = "dotnet-isolated"
            //            },
            //            new PreviewNameValuePairArgs
            //            {
            //                Name = "FUNCTIONS_EXTENSION_VERSION",
            //                Value = "~4"
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
            //                Value = appInsights.ConnectionString
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "ServiceBusConnectionString",
            //                Value = ServiceBusConnectionString
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "Rebrickable__Url",
            //                Value = "https://rebrickable.com"
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "Rebrickable__Key",
            //                Value = config["Rebrickable:Key"]
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Host",
            //                Value = "psql-brickshoarder-prd.postgres.database.azure.com"
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Database",
            //                Value = dBForPostgreSqlDatabase.Name
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Username",
            //                Value = "brickshoarder_admin"
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "MartenAzure__Password",
            //                Value = DbForPostgreSqlAdminPassword
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__Endpoint",
            //                Value = ServiceBusEndpoint
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__SharedAccessKeyName",
            //                Value = serviceBusNamespace.SharedAccessKeyName
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "AzureServiceBus__SharedAccessKey",
            //                Value = serviceBusNamespace.SharedAccessKey
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "Redis__ConnectionString",
            //                Value = config["Redis:ConnectionString"]
            //            },
            //            new PreviewNameValuePairArgs()
            //            {
            //                Name = "PLAYWRIGHT_BROWSERS_PATH",
            //                Value = "/home/site/wwwroot/.playwright/ms-playwright"
            //            },
            //        }
            //    },
            //    Kind = "functionapp",
            //    HttpsOnly = true,
            //});

            //#endregion Functions Linux

            #region Functions Timers

            var appServicePlanFunctionsLinux = new AppServicePlan("AppServicePlan.Functions.Linux", new AppServicePlanArgs
            {
                Name = "asp-func-linux-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                Location = resourceGroup.Location,
                Kind = "Linux",
                Reserved = true,
                Sku = new SkuDescriptionArgs
                {
                    Name = "Y1",
                    Tier = "Dynamic"
                },
            });

            var functionTimers = new WebApp("WebApp.Functions.BricksHoarder.Functions.Timers", new WebAppArgs
            {
                Name = "func-timers-brickshoarder-prd",
                ResourceGroupName = resourceGroup.Name,
                ServerFarmId = appServicePlanFunctionsLinux.Id,
                SiteConfig = new SiteConfigArgs
                {
                    LinuxFxVersion = "DOTNET-ISOLATED|8.0",
                    FunctionAppScaleLimit = 1,
                    NumberOfWorkers = 1,
                    AppSettings = new[]
                    {
                        new NameValuePairArgs()
                        {
                            Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                            Value = appInsights.ConnectionString
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__Endpoint",
                            Value = serviceBusEndpointSecret.KeyVaultReference
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__SharedAccessKeyName",
                            Value = sharedAccessKeyNameSecret.KeyVaultReference
                        },
                        new NameValuePairArgs()
                        {
                            Name = "AzureServiceBus__SharedAccessKey",
                            Value = sharedAccessKeySecret.KeyVaultReference
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
                        new NameValuePairArgs
                        {
                            Name = "FUNCTIONS_WORKER_RUNTIME",
                            Value = "dotnet-isolated"
                        },
                    }
                },
                Kind = "functionapp",
                HttpsOnly = true,
                KeyVaultReferenceIdentity = managedIdentity.Id,
                Identity = new ManagedServiceIdentityArgs()
                {
                    Type = Pulumi.AzureNative.Web.ManagedServiceIdentityType.UserAssigned,
                    UserAssignedIdentities = { managedIdentity.Id }
                }
            });

            #endregion Functions Linux

            //Output.All(ServiceBusEndpoint, SharedAccessKey, SharedAccessKeyName, ServiceBusConnectionString).Apply(_ =>
            //{
            //    string json = File.ReadAllText("..//BricksHoarder.Functions//production.settings.json");

            //    JObject jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(json) as JObject;

            //    JToken serviceBusConnectionStringToken = jObject!.SelectToken("Values.ServiceBusConnectionString")!;
            //    serviceBusConnectionStringToken.Replace(ServiceBusConnectionString.Convert());

            //    JToken azureServiceBusEndpoint = jObject!.SelectToken("AzureServiceBus.Endpoint")!;
            //    azureServiceBusEndpoint.Replace(ServiceBusEndpoint.Convert().Replace("https://", string.Empty));

            //    JToken azureServiceBusSharedAccessKeyName = jObject!.SelectToken("AzureServiceBus.SharedAccessKeyName")!;
            //    azureServiceBusSharedAccessKeyName.Replace(SharedAccessKeyName.Convert());

            //    JToken azureServiceBusSharedAccessKey = jObject!.SelectToken("AzureServiceBus.SharedAccessKey")!;
            //    azureServiceBusSharedAccessKey.Replace(SharedAccessKey.Convert());

            //    jObject!.SelectToken("MartenAzure.Host")!.Replace("psql-brickshoarder-prd.postgres.database.azure.com");
            //    jObject!.SelectToken("MartenAzure.Database")!.Replace(dBForPostgreSqlDatabase.Name.Convert());
            //    jObject!.SelectToken("MartenAzure.Username")!.Replace("brickshoarder_admin");
            //    jObject!.SelectToken("MartenAzure.Password")!.Replace(DbForPostgreSqlAdminPassword.Convert());

            //    string updatedJsonString = jObject.ToString();
            //    File.WriteAllText("..//BricksHoarder.Functions//production.settings.json", updatedJsonString);

            //    return Task.CompletedTask;
            //});
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