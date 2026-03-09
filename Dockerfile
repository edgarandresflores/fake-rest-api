# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# DirtyApi project layout is flat (csproj at repo root)
COPY DirtyApi.csproj ./
RUN dotnet restore DirtyApi.csproj

COPY . .
RUN dotnet publish DirtyApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# AWS Lambda Web Adapter extension
COPY --from=public.ecr.aws/awsguru/aws-lambda-adapter:0.9.1 /lambda-adapter /opt/extensions/lambda-adapter

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV AWS_LWA_PORT=8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DirtyApi.dll"]
