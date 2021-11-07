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

resource "azurerm_function_app" "pacemeapi" {
  name                       = join("", ["paceme-api-serviceplan-",var.environment])
  location                   = azurerm_resource_group.pacemeapi.location
  resource_group_name        = azurerm_resource_group.pacemeapi.name
  app_service_plan_id        = azurerm_app_service_plan.pacemeapi.id
  storage_account_name       = azurerm_storage_account.pacemeapi.name
  storage_account_access_key = azurerm_storage_account.pacemeapi.primary_access_key
}