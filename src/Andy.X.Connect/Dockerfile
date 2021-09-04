# Buildersoft Andy X Connect Docker File

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Andy.X.Connect/Andy.X.Connect.csproj", "src/Andy.X.Connect/"]
COPY ["src/Andy.X.Connect.IO/Andy.X.Connect.IO.csproj", "src/Andy.X.Connect.IO/"]
COPY ["src/Andy.X.Connect.Core/Andy.X.Connect.Core.csproj", "src/Andy.X.Connect.Core/"]
RUN dotnet restore "src/Andy.X.Connect/Andy.X.Connect.csproj"
COPY . .
WORKDIR "/src/src/Andy.X.Connect"
RUN dotnet build "Andy.X.Connect.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Andy.X.Connect.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Andy.X.Connect.dll"]