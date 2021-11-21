FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["src/AwsHelloWorldWeb/AwsHelloWorldWeb.csproj", "AwsHelloWorldWeb/"]
RUN dotnet restore AwsHelloWorldWeb/AwsHelloWorldWeb.csproj

WORKDIR "/work"
COPY . .
RUN dotnet build --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "src/AwsHelloWorldWeb/AwsHelloWorldWeb.csproj" \
            --configuration Release \
            --runtime linux-x64 \
            --self-contained false \
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AwsHelloWorldWeb.dll"]
