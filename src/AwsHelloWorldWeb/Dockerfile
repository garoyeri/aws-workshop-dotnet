FROM public.ecr.aws/lambda/dotnet:5.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim as build
WORKDIR /src
COPY ["AwsHelloWorldWeb.csproj", "AwsHelloWorldWeb/"]
RUN dotnet restore "AwsHelloWorldWeb/AwsHelloWorldWeb.csproj"

WORKDIR "/src/AwsHelloWorldWeb"
COPY . .
RUN dotnet build "AwsHelloWorldWeb.csproj" --configuration Release --output /app/build

FROM build AS publish
RUN dotnet publish "AwsHelloWorldWeb.csproj" \
            --configuration Release \ 
            --runtime linux-x64 \
            --self-contained false \ 
            --output /app/publish \
            -p:PublishReadyToRun=true  

FROM base AS final
WORKDIR /var/task
COPY --from=publish /app/publish .
