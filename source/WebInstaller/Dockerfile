#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Copy and Build the project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebInstaller.csproj", "."]
RUN dotnet restore "WebInstaller.csproj"
COPY . .
RUN dotnet build "WebInstaller.csproj" -c Release -o /app/build

# Publish the build
FROM build AS publish
RUN dotnet publish "WebInstaller.csproj" -c Release -o /app/publish

# Copy files/
FROM build AS files
WORKDIR /app
COPY ["files", "./files"]

# Copy installer/
FROM files AS installer
WORKDIR /app
COPY ["installer", "./installer"]

# Create the image
FROM installer AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=files /app/files ./files
COPY --from=installer /app/installer ./installer
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
ENTRYPOINT ["dotnet", "/app/WebInstaller.dll"]
