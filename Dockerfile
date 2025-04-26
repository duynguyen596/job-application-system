FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /source
  
COPY *.sln .
COPY src/JobApplicationSystem.Api/*.csproj ./src/JobApplicationSystem.Api/
COPY src/JobApplicationSystem.Application/*.csproj ./src/JobApplicationSystem.Application/
COPY src/JobApplicationSystem.Domain/*.csproj ./src/JobApplicationSystem.Domain/
COPY src/JobApplicationSystem.Infrastructure/*.csproj ./src/JobApplicationSystem.Infrastructure/
COPY test/JobApplicationSystem.Application.UnitTests/*.csproj ./test/JobApplicationSystem.Application.UnitTests/
  

RUN dotnet restore JobApplicationSystem.sln
COPY . .
WORKDIR /source/src/JobApplicationSystem.Api
RUN dotnet publish -c Release -o /app/publish --no-restore
  
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build-env /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
  
ENTRYPOINT ["dotnet", "JobApplicationSystem.Api.dll"]