terraform {
  backend "azurerm" {
    resource_group_name  = "PaceMe.GlobalResources"
    storage_account_name = "pacemeterraform"
    container_name       = "tfstate"
    key                  = "local.terraform.tfstate"
  }
}
