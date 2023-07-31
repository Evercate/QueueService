#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

ARG sentry_auth_token
ARG sentry_organization_name
ARG sentry_project_name
ARG send_to_sentry

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
RUN dotnet publish "QueueService.Api.csproj" -c DockerDevelopment -o /app/publish

RUN curl -sL https://sentry.io/get-cli/ | sh
RUN if [ -z "${send_to_sentry}" ] ; \
	then \
	  echo Not sending debug files to sentry ; \
	else \
	  /usr/local/bin/sentry-cli upload-dif --auth-token ${sentry_auth_token} -o ${sentry_organization_name} -p ${sentry_project_name} /app/publish ; \
	fi

#We don't want to keep the pdb files in the final container after sending them to sentry
RUN find /app/publish -type f -name '*.pdb' -delete

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QueueService.Api.dll"]