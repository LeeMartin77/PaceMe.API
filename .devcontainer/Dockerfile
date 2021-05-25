# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.166.1/containers/dotnet/.devcontainer/base.Dockerfile

FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-3.1

# Install Azure CLI & Function App Tools

RUN mkdir /tmp/scripts
COPY ./scripts /tmp/scripts

RUN /tmp/scripts/azcli-debian.sh;
RUN /tmp/scripts/azurefunctions.sh
RUN apt-get clean -y 
RUN rm -rf /var/lib/apt/lists/* /tmp/library-scripts

CMD func start --build