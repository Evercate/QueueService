#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["QueueService.Runner/QueueService.Runner.csproj", "QueueService.Runner/"]
COPY ["QueueService.DAL/QueueService.DAL.csproj", "QueueService.DAL/"]
COPY ["QueueService.Model/QueueService.Model.csproj", "QueueService.Model/"]
RUN dotnet restore "QueueService.Runner/QueueService.Runner.csproj"
COPY . .
WORKDIR "/src/QueueService.Runner"
RUN dotnet build "QueueService.Runner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QueueService.Runner.csproj" -c DockerDevelopment -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueueService.Runner.dll"]