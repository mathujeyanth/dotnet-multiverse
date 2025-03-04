# Base build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base-build

# Wasm-tools + python is needed for multithreading in blazor wasm
RUN apt-get update -y && apt-get install python3-pip python3 -y
RUN dotnet workload install wasm-tools

# Build stage
FROM base-build AS build
WORKDIR /src

# Copy files
COPY ../MJ.ServerApp.CustomDevServer MJ.ServerApp.CustomDevServer 
COPY * .

# Restore projects
RUN dotnet restore MJ.ServerApp.CustomDevServer/
RUN dotnet restore MJ.PWA.AudioConverter/

# Build and publish projects
RUN dotnet publish MJ.ServerApp.CustomDevServer -c Release -o /publish/MJ.ServerApp.CustomDevServer
RUN dotnet publish MJ.PWA.AudioConverter -c Release -o /publish/MJ.PWA.AudioConverter

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published outputs
COPY --from=build /publish/MJ.ServerApp.CustomDevServer ./MJ.ServerApp.CustomDevServer
COPY --from=build /publish/MJ.PWA.AudioConverter ./MJ.PWA.AudioConverter

# Entry point
ENTRYPOINT ["dotnet","./MJ.ServerApp.CustomDevServer/MJ.ServerApp.CustomDevServer.dll", "--applicationpath", "/app/MJ.PWA.AudioConverter/", "--assemblyname", "MJ.PWA.AudioConverter"]