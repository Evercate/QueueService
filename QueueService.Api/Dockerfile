#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["QueueService.Api/QueueService.Api.csproj", "QueueService.Api/"]
COPY ["QueueService.Api.Model/QueueService.Api.Model.csproj", "QueueService.Api.Model/"]
COPY ["QueueService.DAL/QueueService.DAL.csproj", "QueueService.DAL/"]
COPY ["QueueService.Model/QueueService.Model.csproj", "QueueService.Model/"]
RUN dotnet restore "QueueService.Api/QueueService.Api.csproj"
COPY . .
WORKDIR "/src/QueueService.Api"
RUN dotnet build "QueueService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QueueService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueueService.Api.dll"]