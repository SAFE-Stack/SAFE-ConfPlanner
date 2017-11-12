# ConfPlanner
This is the sample project that is used in the talk "Domain Driven UI". The title is a bit misleading as it is a bad name for "Reusing your datatypes and behaviour from your CQRS/Event-Sourced models in your Elm-architecture application."

Given you use F# and Fable, you can actually build simple eventually connected systems and have the exact same model working in back and frontend.

This project uses the [Safe-Stack](https://safe-stack.github.io/). Especially the build script leans heavily on the [Safe-Bookstore Example](https://github.com/SAFE-Stack/SAFE-BookStore).

## Content
This project consists of 6 dotnetcore subprojects
* `Domain` - Message-based CQRS implementation of the Domain of a ConferencePlanner.
* `Domain.Test` - BDD-Style Tests for the `Domain`
* `Client` - [Fable](http://fable.io/) Project that uses the Elm-Architecture (with [Fable-Elmish](https://fable-elmish.github.io/elmish/)). It reuses the projections of the `Domain` project. Furthermore it and can also reuse the behaviour of the Domain (when switched to `WhatIf-Mode`)
* `Server` - A Suave Webserver that allows the Client to connect via Websockets.
* `Infrastructure` - This is where all the backend infrastructure is implemented. It contains a simple (file based) event store, command and query handlers and the types that hold everything together. Most of the infrastructure is implemented asynchronously with the help of F# awesome [Mailbox Processors](https://fsharpforfunandprofit.com/posts/concurrency-actor-model/)
* `Support` - A simple project to fill the EventStore with some initial values.

## Requirements

- [Mono](http://www.mono-project.com/) on MacOS/Linux
- [.NET Framework 4.6.2](https://support.microsoft.com/en-us/help/3151800/the--net-framework-4-6-2-offline-installer-for-windows) on Windows
- [node.js](https://nodejs.org/) - JavaScript runtime
- [yarn](https://yarnpkg.com/) - Package manager for npm modules
- [dotnet SDK 2.0.0](https://www.microsoft.com/net/core) is required but it will be downloaded automatically by the build script if not installed (see below).
- Other tools like [Paket](https://fsprojects.github.io/Paket/) or [FAKE](https://fake.build/) will also be installed by the build script.

## Development mode

This development stack is designed to be used with minimal tooling. An instance of Visual Studio Code together with the excellent [Ionide](http://ionide.io/) plugin should be enough.

Start the development mode with:

    > build.cmd run // on windows
    $ ./build.sh run // on unix

This will start Suave on port 8085 and the webpack-dev-server on port 8080. Then it will open localhost:8080 in your browser.

Enjoy.

## Demo Data / Fixtures
If you want prefill the conference Event-Store with some demo data you can run:

    > build.cmd RunFixtures // on windows
    $ ./build.sh RunFixtures // on unix

The events will be written to `src/Server/conference_eventstore.json`


## Plans for the future
From the top of my head. If anyone wants to chip in, feel welcome.

### Deployment
* make use of Azure to deploy the application

### Infrastructure
* extract the project into its own repository and make it a bit more production ready :D
* implement at least one different event store implementation (e.g. SQLite or Azure something something)
* implement projections that can send notifications

### Server
* implement a proper authentication for websockets


### Client
* use [Fulma](https://mangelmaxime.github.io/Fulma/) for styling
* Learn CSS (and this time for real :D)

### Domain
* build an actual Conference Planner
