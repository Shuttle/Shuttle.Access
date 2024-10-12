# Shuttle.Access

An Identity and Access Management (IAM) platform.

## Getting Started

The quickest way to get started is to install [Docker](https://www.docker.com/get-started/) and then create a `docker-compose.yml` file that contains the:

```
version: "3.9"
services:
    azurite:
        image: mcr.microsoft.com/azure-storage/azurite
        hostname: azurite
    database:
        image: shuttle/access-sqlserver-linux:latest
        hostname: database
    server:
        image:
            shuttle/access-server
        environment:
            - CONFIGURATION_FOLDER=.
        depends_on:
            - "azurite"
            - "database"
    projection:
        image:
            shuttle/access-projection:latest
        environment:
            - CONFIGURATION_FOLDER=.
        depends_on:
            - "database"
    web-api:
        image:
            shuttle/access-webapi:latest
        environment:
            - ASPNETCORE_URLS=http://*:5599
            - CONFIGURATION_FOLDER=.
        depends_on:
            - "azurite"
            - "server"
        ports:
            - "5599:5599"
    front-end:
        image:
            shuttle/access-vue:latest
        ports:
            - "8080:80"
```

You will now be able to run `docker-compose up` in the folder containing the above file:

```
> docker-compose up
```

The various images will be downloaded and, once all the containers have started up, you can browse to:

```
http://locahost:8080
```

Which should bring you to the sign-in page where `admin` is the identity as well as the password.

To view the web-api endpoints you can browse to:

```
http://locahost:5599/swagger/index.html
```