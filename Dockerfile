# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM public.ecr.aws/lambda/dotnet:8 AS runtime
WORKDIR /var/task
COPY --from=build /app/publish .

# Replace with your real assembly and handler.
CMD ["FakeApi::FakeApi.LambdaEntryPoint::FunctionHandlerAsync"]
