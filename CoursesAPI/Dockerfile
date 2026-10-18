FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoursesAPI/CoursesAPI.csproj", "CoursesAPI/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "CoursesAPI/CoursesAPI.csproj"
COPY . .
WORKDIR "/src/CoursesAPI"
RUN dotnet build "CoursesAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CoursesAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoursesAPI.dll"]
