#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# LibSassBuilder's compiler tool targets .NET 7; let it run on the newer runtime in this image.
ENV DOTNET_ROLL_FORWARD=Major
WORKDIR /src
COPY ["Dashbrd/Dashbrd.csproj", "Dashbrd/"]
COPY ["Dashbrd.Shared/Dashbrd.Shared.csproj", "Dashbrd.Shared/"]
RUN dotnet restore "Dashbrd/Dashbrd.csproj"
COPY . .
WORKDIR "/src/Dashbrd"
RUN dotnet build "Dashbrd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dashbrd.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dashbrd.dll"]