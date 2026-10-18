FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StudentsAPI/StudentsAPI.csproj", "StudentsAPI/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "StudentsAPI/StudentsAPI.csproj"
COPY . .
WORKDIR "/src/StudentsAPI"
RUN dotnet build "StudentsAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StudentsAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StudentsAPI.dll"]
