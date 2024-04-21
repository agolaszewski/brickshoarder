#docker build -t agolaszewski1337/brickshoardertest:latest -f ./Docker/BricksHoarder.Api/Dockerfile ./src/BricksHoarder.Backend
#docker run -it agolaszewski1337/brickshoardertest:latest bash
#docker tag brickshoarder:latest agolaszewski1337/brickshoardertest:latest
#docker push agolaszewski1337/brickshoardertest:latest

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . ./
RUN dotnet restore "BricksHoarder.Api/BricksHoarder.Api.csproj"
WORKDIR "/src/BricksHoarder.Api"
RUN dotnet build --configuration Release --no-restore

FROM build AS publish
WORKDIR "/src/BricksHoarder.Api"
RUN dotnet publish --no-restore --no-build -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV PLAYWRIGHT_BROWSERS_PATH=./.playwright/ms-playwright
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y libgssapi-krb5-2 && \
    wget https://github.com/PowerShell/PowerShell/releases/download/v7.4.1/powershell_7.4.1-1.deb_amd64.deb && \
    dpkg -i powershell_7.4.1-1.deb_amd64.deb && \
    apt-get install powershell -f && \
    pwsh playwright.ps1 install-deps && \
    pwsh playwright.ps1 install chromium && \
    rm powershell_7.4.1-1.deb_amd64.deb && \
    apt-get -y remove powershell 

RUN rm appsettings.Development.json
ENV ASPNETCORE_ENVIRONMENT=Staging 
ENTRYPOINT ["dotnet", "BricksHoarder.Api.dll"]