FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

WORKDIR /workdir

COPY ./Contracts ./Contracts/
COPY ./Consumer ./Consumer/

RUN dotnet restore ./Consumer/Consumer.csproj
RUN dotnet publish ./Consumer/Consumer.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=build-env /publish /publish
WORKDIR /publish
ENTRYPOINT ["dotnet", "Consumer.dll"]