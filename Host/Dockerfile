#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS vs-containerfastmode

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS builder
WORKDIR /src
COPY ["Host/packages.lock.json", "Host/"]
COPY ["Service/packages.lock.json", "Service/"]
COPY ["Libraries/packages.lock.json", "Libraries/"]
COPY ["Repository/packages.lock.json", "Repository/"]
COPY ["Host/Host.csproj", "Host/"]
COPY ["Service/Service.csproj", "Service/"]
COPY ["Libraries/Libraries.csproj", "Libraries/"]
COPY ["Repository/Repository.csproj", "Repository/"]
RUN dotnet restore "Host/Host.csproj" \
	--locked-mode

COPY . .
RUN dotnet build "Host/Host.csproj" \
	--configuration Release \
	--output /build \
	--no-restore

FROM builder AS publish
RUN dotnet publish "Host/Host.csproj" \
	--configuration Release \
	--output /publish \
	--no-restore

FROM mcr.microsoft.com/dotnet/runtime:7.0
ENV ASPNETCORE_URLS="http://0.0.0.0:8080;http://0.0.0.0:8081"
ENV ASPNETCORE_APIPORT="8080"
ENV ASPNETCORE_MANAGEMENTPORT="8081"
EXPOSE 8080
WORKDIR /app
RUN mkdir ./configmap && chown 1000:1000 ./configmap
RUN mkdir ./secret && chown 1000:1000 ./secret
COPY --from=publish /publish .
ENTRYPOINT ["dotnet", "Host.dll"]