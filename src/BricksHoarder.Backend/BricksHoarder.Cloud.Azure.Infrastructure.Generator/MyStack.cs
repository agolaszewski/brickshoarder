using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.ServiceBus;
using Pulumi.AzureNative.ServiceBus.Inputs;

namespace BricksHoarder.Cloud.Azure.Infrastructure.Generator;

internal class MyStack : Stack
{
    public MyStack()
    {
        #region Resource Group

        var resourceGroup = new ResourceGroup("ResourceGroup", new ResourceGroupArgs
        {
            ResourceGroupName = "rg-brickshoarder-dev"
        });

        #endregion Resource Group

        #region Service Bus

        var serviceBusNamespace = new Pulumi.AzureNative.ServiceBus.Namespace("ServiceBus.Namespace", new NamespaceArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Sku = new SBSkuArgs()
            {
                Name = SkuName.Standard,
                Tier = SkuTier.Standard
            },
            NamespaceName = "sb-brickshoarder-dev"
        });

        var serviceBusNamespaceNamespaceAuthorizationRule = new Pulumi.AzureNative.ServiceBus.NamespaceAuthorizationRule("ServiceBus.Namespace.NamespaceAuthorizationRule", new NamespaceAuthorizationRuleArgs()
        {
            NamespaceName = serviceBusNamespace.Name,
            AuthorizationRuleName = "DefaultSharedAccessPolicy",
            ResourceGroupName = resourceGroup.Name,
            Rights = new InputList<AccessRights>()
            {
                AccessRights.Listen,
                AccessRights.Send
            }
        });

        Endpoint = serviceBusNamespace.ServiceBusEndpoint;
        SharedAccessKeyName = serviceBusNamespaceNamespaceAuthorizationRule.Name;

        var output = Output.All(serviceBusNamespaceNamespaceAuthorizationRule.Name).Apply(async x =>
        {
            var namespaceKeys = await ListNamespaceKeys.InvokeAsync(new ListNamespaceKeysArgs()
            {
                AuthorizationRuleName = serviceBusNamespaceNamespaceAuthorizationRule.Name.Convert(),
                NamespaceName = serviceBusNamespace.Name.Convert(),
                ResourceGroupName = resourceGroup.Name.Convert()
            });
            return (namespaceKeys.PrimaryConnectionString, namespaceKeys.PrimaryKey);
        });
        ServiceBusConnectionString = output.Apply(x => x.PrimaryConnectionString);
        SharedAccessKey = output.Apply(x => x.PrimaryKey);

        #endregion Service Bus

        #region PostgreSQL

        var dBForPostgreSqlAdministratorLoginPassword = new Pulumi.Random.RandomPassword("DBforPostgreSQL.AdministratorLoginPassword", new()
        {
            Length = 20,
        });
        DbForPostgreSqlAdminPassword = Output.Unsecret(dBForPostgreSqlAdministratorLoginPassword.Result);

        var dBForPostgreSqlServer = new Pulumi.AzureNative.DBforPostgreSQL.V20221201.Server("DBforPostgreSQL.Server", new()
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = "psql-brickshoarder-dev",
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

        #region Sql Server

        var dBForMsSqlAdministratorLoginPassword = new Pulumi.Random.RandomPassword("DbForMsSQL.AdministratorLoginPassword", new()
        {
            Length = 20,
        });
        DbForMsSqlAdminPassword = Output.Unsecret(dBForMsSqlAdministratorLoginPassword.Result);

        var dbForMsSqlServer = new Pulumi.AzureNative.Sql.Server("DbForMsSqlServer.Server", new Pulumi.AzureNative.Sql.ServerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = "sql-brickshoarder-dev",
            AdministratorLogin = "brickshoarder_admin",
            AdministratorLoginPassword = dBForMsSqlAdministratorLoginPassword.Result
        });

        new Pulumi.AzureNative.Sql.FirewallRule("DBforPostgreSQL.Server.FirewallRule", new Pulumi.AzureNative.Sql.FirewallRuleArgs
        {
            EndIpAddress = "255.255.255.255",
            ResourceGroupName = resourceGroup.Name,
            ServerName = dbForMsSqlServer.Name,
            StartIpAddress = "0.0.0.0",
            FirewallRuleName = "DevAllAccess"
        });

        var sqlDatabase = new Pulumi.AzureNative.Sql.Database("DbForMsSqlServer.Server.Database", new Pulumi.AzureNative.Sql.DatabaseArgs
        {
            ResourceGroupName = resourceGroup.Name,
            ServerName = dbForMsSqlServer.Name,
            DatabaseName = "brickshoarder",
            Sku = new Pulumi.AzureNative.Sql.Inputs.SkuArgs()
            {
                Name = "S0",
                Tier = "Standard"
            }
        });

        #endregion Sql Server
    }

    [Output]
    public Output<string> SharedAccessKey { get; set; }

    [Output]
    public Output<string> SharedAccessKeyName { get; set; }

    [Output]
    public Output<string> ServiceBusConnectionString { get; set; }

    [Output]
    public Output<string> Endpoint { get; set; }

    [Output]
    public Output<string> DbForPostgreSqlAdminPassword { get; set; }

    [Output]
    public Output<string> DbForMsSqlAdminPassword { get; set; }
}