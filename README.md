# Web Development Master Class

This repo contains the code used in my Web Development Master Class. 

## Getting started

To get started, you will need:

- The [.NET 11 SDK](https://dotnet.microsoft.com/download)
- An IDE (Visual Studio, Rider or VS Code with C# Dev Kit)
- Docker Desktop (or another container runtime), as SQL Server, the Identity Server and the UI all run in containers

The solution is orchestrated using [Aspire](https://aspire.dev), so there is no need to start the individual projects manually.

### HTTPS certificate

The Identity Server container needs an HTTPS certificate that you trust. Aspire automatically provides the ASP.NET Core development certificate to the container, so all you need to do is make sure that it is installed and trusted on your machine:

```bash
dotnet dev-certs https --trust
```

__Note:__ The code for this is in [the AppHost's AppHost.cs file](./src/WebDevMasterClass.AppHost/AppHost.cs)

### Identity Server image

The AppHost runs the Identity Server from a container image called `zerokoll/webdevworkshop-identity-server`, and expects that image to be available locally. Before running the solution for the first time, build it from the source in [_resources/identity-server](./_resources/identity-server) by running the following from the root of the repo

```bash
docker build -t zerokoll/webdevworkshop-identity-server ./_resources/identity-server
```

__Note:__ If you make changes to the Identity Server code (for example, adding clients or users), you need to rebuild the image for the changes to take effect.

### Running the solution

Once this has been done, you should be able to simply press F5 to run the solution, as long as the __WebDevMasterClass.AppHost__ project is set as start-up project. Alternatively, you can use the Aspire CLI and run

```bash
aspire run
```

from the root of the repo.

### Seed data

The first time you run the solution, the products database will be empty. To add some products, open the Aspire dashboard, find the __WebDevMasterClass-Products__ database resource, and run the __Add seed data__ command once the database is healthy.

### Running the tests

The tests use containers as well, so make sure Docker is running, and then run

```bash
dotnet test
```

## Questions

If you have any questions, feel free to reach out to me. I'm available on Twitter...sorry...X...at [@zerokoll](https://twitter.com/zerokoll) and on LinkedIn at [https://www.linkedin.com/in/zerokoll/](https://www.linkedin.com/in/zerokoll/).

## Interested in the Workshop?

If you are interested in the workshop, please reach out to me on Twitter or LinkedIn. I would be more than happy to come and run the workshop for you and your team.