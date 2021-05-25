az group create --location uksouth --name App.PaceMe.Info
az deployment group create --resource-group App.PaceMe.Info --template-file arm-template.json --parameters @arm-parameters.json