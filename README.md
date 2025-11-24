````markdown
# ContosoCrafts

ContosoCrafts is a small demo product-catalog web application built with ASP.NET Core and Server-side Blazor. It is intended as a tutorial/sample app to demonstrate building web pages, components, controllers, basic dependency injection, and unit/integration testing in .NET.

## Tech stack

- C# and ASP.NET Core (Server-side Blazor + Razor Pages + MVC Controllers)
- Target framework: .NET 8.0 (`net8.0`)
- Hosting model: OutOfProcess
- Product data served from a static JSON file: `wwwroot/data/products.json`

## What this project solves

This repository provides a small product catalog and UI examples for:

- Listing products and viewing details
- Serving product data via a simple service (`JsonFileProductService`)
- Demonstrating server-side Blazor interactivity and Razor Pages
- Showing patterns for unit and integration testing (see `UnitTests/`)

The app is lightweight by design and intended for learning, demos, and small internal deployments.

## Run locally

Prerequisites:

- .NET SDK 8.0 or later installed
- (Optional) Visual Studio 2022/2023 or VS Code with C# extensions

From the repository root, run the web app:

```powershell
cd "f:/Projects/Software Fundamentals/511025FQ-03/src"
dotnet run
```
````

Then open `https://localhost:5001` (or the URL shown in the console).

## Tests

Unit and integration tests are located under `UnitTests/`. Run them from the repository root:

```powershell
cd "f:/Projects/Software Fundamentals/511025FQ-03/UnitTests"
dotnet test
```

## Deployment

This project includes Azure publish profiles and ARM templates for Azure App Service deployments:

- Publish profiles: `src/Properties/PublishProfiles/*.pubxml`
- ARM templates and service dependency metadata: `src/Properties/ServiceDependencies/*.json`

Deployment options:

- Publish to Azure App Service using Visual Studio (use one of the publish profiles) or `dotnet publish` + Web Deploy.
- Containerize the application (add a `Dockerfile`) and push to Azure Container Registry (ACR) then deploy to App Service (Linux containers) or AKS.
- Add a CI pipeline (GitHub Actions or Azure DevOps) to build, test, and publish artifacts.

Example: publish using `dotnet` and Web Deploy (Visual Studio publish profiles are the simplest for this repo):

```powershell
cd "f:/Projects/Software Fundamentals/511025FQ-03/src"
dotnet publish -c Release -o ./publish
# Use your preferred deployment method to push the contents of ./publish to the target host
```

### Containerization (recommended for reproducible deployments)

There is no `Dockerfile` in the repository currently. A minimal Dockerfile for an ASP.NET Core app would look like:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ContosoCrafts.WebSite.csproj", "./"]
RUN dotnet restore "ContosoCrafts.WebSite.csproj"
COPY . ./
WORKDIR "/src"
RUN dotnet publish "ContosoCrafts.WebSite.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ContosoCrafts.WebSite.dll"]
```

If you want, I can add a Dockerfile and a GitHub Actions workflow to build and optionally publish the image to ACR.

## CI/CD

- There are no active GitHub Actions workflows in `.github/workflows/` at the moment. A minimal CI pipeline should build the solution, run tests, and optionally publish artifacts or push a container image.

## Notes & next steps

- For production readiness replace static JSON data with a managed data store (SQL Server, Azure SQL, or Cosmos DB) and add caching.
- Add monitoring, logging, and configuration for secrets (Key Vault, environment variables).

## Contributing

Contributions are welcome — fork the repo, make changes, and open a pull request.

## License

This repository contains a `LICENSE` file at the project root. Review it for license details.

```

## Tutorial

- [ASP.NET Tutorial](https://dotnet.microsoft.com/learn/aspnet/hello-world-tutorial/intro)

## Gists

- [Gist](https://gist.github.com/bradygaster/3d1fcf43d1d1e73ea5d6c1b5aab40130) referenced in the videos

## YouTube Playlist

- [ASP.NET Core 101](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oW8nviYduHq7bmKode-p8Wy)

13 part series
```
