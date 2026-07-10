metadata description = 'MOYO OMS — Azure PaaS deployment: Container Apps, Functions, SQL, Service Bus, Static Web Apps.'

@description('Location for all resources.')
param location string = resourceGroup().location

@description('Static Web Apps Free is offered in only these regions, so the portal is placed independently of everything else.')
@allowed([
  'centralus'
  'eastasia'
  'eastus2'
  'westeurope'
  'westus2'
])
param staticSiteLocation string = 'centralus'

@description('Prefix for resource names. Keep short: some resources cap at 24 characters.')
@minLength(3)
@maxLength(8)
param namePrefix string = 'moyo'

@description('Object id of the Entra principal that becomes the SQL server admin. Get it with: az ad signed-in-user show --query id -o tsv')
param sqlAdminObjectId string

@description('Display name or UPN of the SQL admin principal. Cosmetic; shown in the portal.')
param sqlAdminLogin string

@description('Optional client IP allowed through the SQL firewall so migrations can be run from a laptop.')
param clientIpAddress string = ''

@description('Name of the registry created by registry.bicep.')
param acrName string

@description('Fully qualified API image, including tag.')
param apiImage string

@description('Fully qualified order intake worker image, including tag.')
param intakeImage string

@description('Fully qualified order status publisher worker image, including tag.')
param statusPublisherImage string

@description('ExternalSystemId the intake worker attributes incoming orders to. Matches the seeded system.')
param externalSystemId int = 1

var resourceToken = uniqueString(resourceGroup().id)

var storageName = '${namePrefix}st${resourceToken}'
var sqlServerName = '${namePrefix}-sql-${resourceToken}'
var sqlDatabaseName = 'MoyoOms'
var serviceBusName = '${namePrefix}-sb-${resourceToken}'

var newOrdersTopic = 'orders.new'
var statusTopic = 'orders.status'
var receivedTopic = 'orders.received'
var intakeSubscription = 'oms-intake'
var clientPortalSubscription = 'client-portal'
var allocationSubscription = 'allocation'

var deploymentContainerName = 'deploymentpackage'

// Built-in role definition ids. Scoped per-resource below rather than at the resource group.
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
var serviceBusDataSenderRoleId = '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
var serviceBusDataReceiverRoleId = '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

// ---------------------------------------------------------------------------
// Identity — one user-assigned identity shared by the API, both workers, and the function.
// ---------------------------------------------------------------------------

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${namePrefix}-id-${resourceToken}'
  location: location
}

// ---------------------------------------------------------------------------
// Observability
// ---------------------------------------------------------------------------

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${namePrefix}-log-${resourceToken}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${namePrefix}-ai-${resourceToken}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

// ---------------------------------------------------------------------------
// Container registry — created by registry.bicep so images can be pushed before this runs.
// ---------------------------------------------------------------------------

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' existing = {
  name: acrName
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(containerRegistry.id, identity.id, acrPullRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------------------
// Messaging
// ---------------------------------------------------------------------------

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    disableLocalAuth: true
    minimumTlsVersion: '1.2'
  }
}

resource ordersNew 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: newOrdersTopic
  properties: {
    // Intake dedupes on ClientPortalOrderId in the database, so no broker-side detection here.
    requiresDuplicateDetection: false
  }
}

resource ordersStatus 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: statusTopic
  properties: {
    // The outbox publishes at-least-once and reuses the outbox row id as the message id.
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

resource ordersReceived 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: receivedTopic
  properties: {
    // Intake publishes after CreateOrder and completes afterwards, so a retry can resend.
    requiresDuplicateDetection: true
    duplicateDetectionHistoryTimeWindow: 'PT10M'
  }
}

resource intakeSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: ordersNew
  name: intakeSubscription
  properties: {
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
  }
}

resource clientPortalSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: ordersStatus
  name: clientPortalSubscription
  properties: {
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
  }
}

resource allocationSub 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: ordersReceived
  name: allocationSubscription
  properties: {
    maxDeliveryCount: 5
    deadLetteringOnMessageExpiration: true
  }
}

resource serviceBusSenderAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBus
  name: guid(serviceBus.id, identity.id, serviceBusDataSenderRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataSenderRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusReceiverAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBus
  name: guid(serviceBus.id, identity.id, serviceBusDataReceiverRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', serviceBusDataReceiverRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------------------
// Database — Entra-only auth, so there is no SQL password anywhere in this template.
// ---------------------------------------------------------------------------

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    administrators: {
      administratorType: 'ActiveDirectory'
      principalType: 'User'
      login: sqlAdminLogin
      sid: sqlAdminObjectId
      tenantId: subscription().tenantId
      azureADOnlyAuthentication: true
    }
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  properties: {
    maxSizeBytes: 2147483648
  }
}

// The 0.0.0.0 sentinel means "any Azure service", which is how Container Apps reaches SQL.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource allowClientIp 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = if (!empty(clientIpAddress)) {
  parent: sqlServer
  name: 'AllowDeveloperMachine'
  properties: {
    startIpAddress: clientIpAddress
    endIpAddress: clientIpAddress
  }
}

// "Active Directory Default" delegates to Azure.Identity, which reads the Container Apps
// IDENTITY_ENDPOINT and honours AZURE_CLIENT_ID. "Active Directory Managed Identity" would
// instead query IMDS, which Container Apps does not serve, yielding a token for the wrong
// principal and a "Login failed for user '<token-identified principal>'" from SQL.
var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Encrypt=True;TrustServerCertificate=False;Authentication=Active Directory Default;'

// ---------------------------------------------------------------------------
// Storage — required by the Functions host for deployment packages and leases.
// ---------------------------------------------------------------------------

resource storage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storage
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: deploymentContainerName
}

resource storageBlobAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(storage.id, identity.id, storageBlobDataOwnerRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---------------------------------------------------------------------------
// Static Web App — declared before the API so its hostname can seed the CORS policy.
// ---------------------------------------------------------------------------

resource staticSite 'Microsoft.Web/staticSites@2023-12-01' = {
  name: '${namePrefix}-swa-${resourceToken}'
  location: staticSiteLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// ---------------------------------------------------------------------------
// Container Apps
// ---------------------------------------------------------------------------

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${namePrefix}-cae-${resourceToken}'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

var registryConfiguration = [
  {
    server: containerRegistry.properties.loginServer
    identity: identity.id
  }
]

// AZURE_CLIENT_ID tells DefaultAzureCredential which user-assigned identity to use.
var sharedEnv = [
  {
    name: 'AZURE_CLIENT_ID'
    value: identity.properties.clientId
  }
  {
    name: 'ConnectionStrings__OmsDatabase'
    value: sqlConnectionString
  }
]

var serviceBusEnv = [
  {
    name: 'ServiceBus__FullyQualifiedNamespace'
    value: '${serviceBus.name}.servicebus.windows.net'
  }
]

resource apiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${namePrefix}-api'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        transport: 'auto'
      }
      registries: registryConfiguration
    }
    template: {
      containers: [
        {
          name: 'api'
          image: apiImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: concat(sharedEnv, [
            {
              // Overrides index 0 of the Cors:AllowedOrigins array in appsettings.json.
              name: 'Cors__AllowedOrigins__0'
              value: 'https://${staticSite.properties.defaultHostname}'
            }
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
          ])
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  dependsOn: [
    acrPullAssignment
  ]
}

resource intakeApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${namePrefix}-worker-intake'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      registries: registryConfiguration
    }
    template: {
      containers: [
        {
          name: 'worker-intake'
          image: intakeImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: concat(sharedEnv, serviceBusEnv, [
            {
              name: 'ServiceBus__TopicName'
              value: newOrdersTopic
            }
            {
              name: 'ServiceBus__SubscriptionName'
              value: intakeSubscription
            }
            {
              name: 'ServiceBus__OrderReceivedTopicName'
              value: receivedTopic
            }
            {
              name: 'ServiceBus__ExternalSystemId'
              value: string(externalSystemId)
            }
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Production'
            }
          ])
        }
      ]
      // A message-driven subscriber that must stay resident: no scale to zero.
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  dependsOn: [
    acrPullAssignment
    intakeSub
    serviceBusReceiverAssignment
    serviceBusSenderAssignment
  ]
}

resource statusPublisherApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${namePrefix}-worker-status'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      registries: registryConfiguration
    }
    template: {
      containers: [
        {
          name: 'worker-status'
          image: statusPublisherImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: concat(sharedEnv, serviceBusEnv, [
            {
              name: 'ServiceBus__TopicName'
              value: statusTopic
            }
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Production'
            }
          ])
        }
      ]
      // The outbox poller must be a single replica or rows would be published twice.
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
  dependsOn: [
    acrPullAssignment
    ordersStatus
    serviceBusSenderAssignment
  ]
}

// ---------------------------------------------------------------------------
// Allocation function — Flex Consumption, identity-based connections throughout.
// ---------------------------------------------------------------------------

resource flexPlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${namePrefix}-plan-${resourceToken}'
  location: location
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: '${namePrefix}-func-${resourceToken}'
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    serverFarmId: flexPlan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storage.properties.primaryEndpoints.blob}${deploymentContainerName}'
          authentication: {
            type: 'UserAssignedIdentity'
            userAssignedIdentityResourceId: identity.id
          }
        }
      }
      scaleAndConcurrency: {
        instanceMemoryMB: 2048
        maximumInstanceCount: 40
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '8.0'
      }
    }
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storage.name
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__clientId'
          value: identity.properties.clientId
        }
        {
          // The ServiceBusTrigger's Connection="ServiceBusConnection" resolves to this prefix.
          name: 'ServiceBusConnection__fullyQualifiedNamespace'
          value: '${serviceBus.name}.servicebus.windows.net'
        }
        {
          name: 'ServiceBusConnection__credential'
          value: 'managedidentity'
        }
        {
          name: 'ServiceBusConnection__clientId'
          value: identity.properties.clientId
        }
        {
          name: 'OrderReceivedTopic'
          value: receivedTopic
        }
        {
          name: 'AllocationSubscription'
          value: allocationSubscription
        }
        {
          name: 'ConnectionStrings__OmsDatabase'
          value: sqlConnectionString
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: identity.properties.clientId
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
  }
  dependsOn: [
    deploymentContainer
    storageBlobAssignment
    allocationSub
    serviceBusReceiverAssignment
  ]
}

// ---------------------------------------------------------------------------
// Outputs consumed by the deploy workflow.
// ---------------------------------------------------------------------------

output acrName string = containerRegistry.name
output acrLoginServer string = containerRegistry.properties.loginServer
output apiAppName string = apiApp.name
output apiUrl string = 'https://${apiApp.properties.configuration.ingress.fqdn}'
output intakeAppName string = intakeApp.name
output statusPublisherAppName string = statusPublisherApp.name
output functionAppName string = functionApp.name
output staticSiteName string = staticSite.name
output staticSiteUrl string = 'https://${staticSite.properties.defaultHostname}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output managedIdentityName string = identity.name
output managedIdentityClientId string = identity.properties.clientId
output serviceBusNamespace string = serviceBus.name
