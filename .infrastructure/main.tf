terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=2.84.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "PaceMe.GlobalResources"
    storage_account_name = "pacemeterraform"
    container_name       = "tfstate"
    key                  = "local.terraform.tfstate"
  }
}

variable "environment" {
  type = string
  default = "localdev"
}

variable "openidconfig" {
  type = string
  sensitive = true
}

variable "validaudience" {
  type = string
  sensitive = true
}

variable "validissuer" {
  type = string
  sensitive = true
}

provider "azurerm" {
   features {}
}

resource "azurerm_resource_group" "pacemeapi" {
  name     = join("", ["PaceMe.Api.", var.environment])
  location = "UK South"
}

resource "azurerm_storage_account" "pacemeapi" {
  name                     = join("", ["pacemefnapp",var.environment])
  resource_group_name      = azurerm_resource_group.pacemeapi.name
  location                 = azurerm_resource_group.pacemeapi.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_account" "pacemestorage" {
  name                     = join("", ["pacemestorage",var.environment])
  resource_group_name      = azurerm_resource_group.pacemeapi.name
  location                 = azurerm_resource_group.pacemeapi.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_app_service_plan" "pacemeapi" {
  name                = join("", ["paceme-api-serviceplan-",var.environment])
  location            = azurerm_resource_group.pacemeapi.location
  resource_group_name = azurerm_resource_group.pacemeapi.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_application_insights" "pacemeapi" {
  name                = join("", ["paceme-api-appinsights-",var.environment])
  location            = azurerm_resource_group.pacemeapi.location
  resource_group_name = azurerm_resource_group.pacemeapi.name
  application_type    = "web"
}

resource "azurerm_function_app" "pacemeapi" {
  name                       = join("", ["paceme-api-serviceplan-",var.environment])
  location                   = azurerm_resource_group.pacemeapi.location
  resource_group_name        = azurerm_resource_group.pacemeapi.name
  app_service_plan_id        = azurerm_app_service_plan.pacemeapi.id
  storage_account_name       = azurerm_storage_account.pacemeapi.name
  storage_account_access_key = azurerm_storage_account.pacemeapi.primary_access_key
  app_settings = {
      "FUNCTIONS_EXTENSION_VERSION" = "~3"
      "FUNCTIONS_WORKER_RUNTIME" = "dotnet"
      "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.pacemeapi.instrumentation_key
      "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.pacemeapi.connection_string
      "AzureWebJobsStorage" = azurerm_storage_account.pacemeapi.primary_connection_string #"[concat('DefaultEndpointsProtocol=https;AccountName=',parameters('storageAccountName'),';AccountKey=',listKeys(resourceId('Microsoft.Storage/storageAccounts', parameters('storageAccountName')), '2019-06-01').keys[0].value,';EndpointSuffix=','core.windows.net')]"
      "PaceMeStorageAccount" = azurerm_storage_account.pacemestorage.primary_connection_string
      # TODO: There is almost certainly a way of pulling these values from azure
      "OpenIdConfig" = var.openidconfig
      "ValidAudience" = var.validaudience
      "ValidIssuer" = var.validissuer
  }
}