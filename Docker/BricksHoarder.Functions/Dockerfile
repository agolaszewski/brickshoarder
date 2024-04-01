#docker build -t agolaszewski1337/brickshoarder:latest -f ./Docker/BricksHoarder.Functions/Dockerfile ./src/BricksHoarder.Backend
#docker run -it agolaszewski1337/brickshoarder:latest bash
#docker tag brickshoarder:latest agolaszewski1337/brickshoarder:latest
#docker push agolaszewski1337/brickshoarder:latest

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated7.0 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore "BricksHoarder.Functions/BricksHoarder.Functions.csproj"
WORKDIR "/src/BricksHoarder.Functions"
RUN dotnet build --configuration Release --no-restore

FROM build AS publish
WORKDIR "/src/BricksHoarder.Functions"
RUN dotnet publish --no-restore --no-build -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .

ENV PLAYWRIGHT_BROWSERS_PATH=./.playwright/ms-playwright
RUN apt-get update && \
    apt-get install -y wget && \
    wget https://github.com/PowerShell/PowerShell/releases/download/v7.4.1/powershell_7.4.1-1.deb_amd64.deb && \
    dpkg -i powershell_7.4.1-1.deb_amd64.deb && \
    apt-get install -f && \
    pwsh playwright.ps1 install-deps && \
    pwsh playwright.ps1 install chromium && \
    rm powershell_7.4.1-1.deb_amd64.deb && \
    apt-get -y remove powershell 

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true 
