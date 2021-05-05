# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.166.1/containers/dotnet/.devcontainer/base.Dockerfile

FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-3.1
RUN mkdir /scripts
COPY ./scripts /scripts
RUN /scripts/azurefunctions.sh
CMD func start --build