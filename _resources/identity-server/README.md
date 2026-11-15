# IdentityServer4 Sample project

This Identity Server configuration has been cloned from https://github.com/DuendeSoftware/Samples and modified to fit the required purposes.

## Building image

To build the Docker image, just run the following from this directory

```
docker build -t zerokoll/webdevworkshop-identity-server .
```

The image name needs to match the one used in the AppHost ([AppHost.cs](../../src/WebDevMasterClass.AppHost/AppHost.cs)).