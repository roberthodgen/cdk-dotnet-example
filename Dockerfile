FROM microsoft/dotnet:2.2-sdk AS build

COPY *.sln .
COPY Api/Api.csproj Api/Api.csproj
COPY Infrastructure/Infrastructure.csproj Infrastructure/Infrastructure.csproj

RUN dotnet restore

COPY . .

RUN dotnet publish -c Release -o out Api/Api.csproj

FROM microsoft/dotnet:2.2-sdk AS test

COPY . .

RUN dotnet test -c Release

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime

COPY --from=build Api/out .
COPY --from=build Api/appsettings.json .
COPY --from=build Api/appsettings.*.json ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "Api.dll"]
