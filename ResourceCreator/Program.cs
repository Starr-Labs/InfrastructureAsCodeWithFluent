using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ResourceCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();


            var client = configuration.GetConnectionString("ClientId");
            var key = configuration.GetConnectionString("ClientKey");
            var tenant = configuration.GetConnectionString("Tenant");
            var subscriptionId = configuration.GetConnectionString("SubscriptionId");

            var storageAccountName = configuration.GetConnectionString("StorageAccountName");
            var region = Region.USEast;
            var resourceGroupName = configuration.GetConnectionString("ResourceGroupName");
            var webAppName = configuration.GetConnectionString("WebAppName");
            var pricingTier = PricingTier.StandardS1;

            var sourceControlRepo = configuration.GetConnectionString("SourceControlRepo");
            var sourceControlBranch = configuration.GetConnectionString("SourceControlBranch");

            var creds = new AzureCredentialsFactory()
                .FromServicePrincipal(client, key, tenant, AzureEnvironment.AzureGlobalCloud);
            var azure = Microsoft.Azure.Management.Fluent.Azure.Authenticate(creds)
                .WithSubscription(subscriptionId);

            //more on creating storage accounts in Fluent here:
            //https://github.com/Azure-Samples/storage-dotnet-manage-storage-accounts
            var storageAccount = azure.StorageAccounts
                .Define(storageAccountName)
                .WithRegion(region)
                .WithNewResourceGroup(resourceGroupName)
                .Create();

            //more on creating app functions here:
            //https://github.com/Azure-Samples/app-service-dotnet-manage-functions
            var appService = azure.AppServices.FunctionApps.
                Define(webAppName).
                WithRegion(region).
                WithExistingResourceGroup(resourceGroupName).
                //more on managing source control here:
                //https://github.com/Azure/azure-libraries-for-net/blob/master/Samples/AppService/ManageWebAppSourceControl.cs
                    DefineSourceControl().
                    WithPublicGitRepository(sourceControlRepo).
                    WithBranch(sourceControlBranch).
                    Attach().
                Create();
            
        }
    }
}
