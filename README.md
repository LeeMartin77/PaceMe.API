# PaceMe Training Planner - Function App API

[![Build Status](https://dev.azure.com/LeeJohnMartin/PaceMe/_apis/build/status/LeeMartin77.PaceMe.API?branchName=master)](https://dev.azure.com/LeeJohnMartin/PaceMe/_build/latest?definitionId=11&branchName=master)

This project is a function app, acting as the server side for a run training planner. It's relatively simple, and has two key goals for me:

- Act as a place to try things out and experiment with C#, Azure and CI/CD
- Be an excersise in running something with as low a cost as possible

## Development Environment

This project is worked on in a linux environment, using a devcontainer in VS Code. You can run the application itself using ```func start --build``` from the command line, once you've started the container up.

Alternatively, you can just run the ```docker-compose.development.yml``` file with docker-compose, if you want to just run a development version without the application.

For authentication, the API expects JWT tokens from AzureADB2C, with the config in ```example.env``` having been populated, and renamed to just ```.env```.