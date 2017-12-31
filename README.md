# SAFE - A web stack designed for developer happiness

The following document describes a [SAFE-Stack](https://safe-stack.github.io/) sample project that brings together CQRS/Event-Sourcing on the backend and the Elm architecture on the frontend.

SAFE is a technology stack that brings together several technologies into a single, coherent stack for typesafe,
flexible end-to-end web-enabled applications that are written entirely in F#.

![SAFE-Stack](public/img/safe_logo.png "SAFE-Stack")

# SAFE-ConfPlanner

[![Build Status](https://travis-ci.org/rommsen/ConfPlanner.svg?branch=master)](https://travis-ci.org/rommsen/ConfPlanner)

This is the sample project that is used in the talk "Domain Driven UI" ([Slides](http://bit.ly/DomainDrivenUi)). The title is a bit misleading as it is a bad name for "Reusing your datatypes and behaviour from your CQRS/Event-Sourced models in your Elm-architecture application."

Given you use F# and Fable, you can actually build simple eventually connected systems and have the exact same model working in back and frontend.

The application showcases a couple of things:

* The reuse of the complete domain model on the client and server.
* It shows the nice fit of and similarity between CQRS/Event-Sourcing on the backend and the Elm-Architecture on the frontend.
* It reuses projections from the backend in the update function of the elmish app. The backend is sending domain events to the frontend and the (Elm-)model is updated with the help of projections defined in the backend (on all clients that are connected via websockets).
* It shows an easy way of implementing "Whatif"-Scenarios, i.e. scenarios that enable the user try out different actions. When the user is happy with the result the system sends a batch of commands to the server. When "Whatif-Mode" is enabled the client reuses not only the projections but also the domain behaviour defined on the server to create the events needed by the update function. The potential commands are also stored.
* It uses the awesome [Fulma](https://mangelmaxime.github.io/Fulma/) library for styling
* It has BDD Style tests that show how nice the behaviour of Event-Sourced systems can be tested.
* Websockets with Elmish/Suave


## Content
This project consists of 6 dotnetcore subprojects
* `Domain` - Message-based CQRS implementation of the Domain of a ConferencePlanner.
* `Domain.Tests` - BDD-Style Tests for the `Domain`
* `Client` - [Fable](http://fable.io/) Project that uses the Elm-Architecture (with [Fable-Elmish](https://fable-elmish.github.io/elmish/)). It reuses the projections of the `Domain` project. Furthermore it and can also reuse the behaviour of the Domain (when switched to `WhatIf-Mode`)
* `Server` - A Suave Webserver that allows the Client to connect via Websockets.
* `Infrastructure` - This is where all the backend infrastructure is implemented. It contains a simple (file based) event store, command and query handlers and the types that hold everything together. Most of the infrastructure is implemented asynchronously with the help of F# awesome [Mailbox Processors](https://fsharpforfunandprofit.com/posts/concurrency-actor-model/)
* `Support` - A simple project to fill the EventStore with some initial values.

## Requirements

- [Mono](http://www.mono-project.com/) on MacOS/Linux
- [.NET Framework 4.6.2](https://support.microsoft.com/en-us/help/3151800/the--net-framework-4-6-2-offline-installer-for-windows) on Windows
- [node.js](https://nodejs.org/) - JavaScript runtime
- [yarn](https://yarnpkg.com/) - Package manager for npm modules
- [dotnet SDK 2.1.3](https://github.com/dotnet/core/blob/master/release-notes/download-archives/2.0.4-download.md) The .NET Core SDK (will be installed by build script)
- Other tools like [Paket](https://fsprojects.github.io/Paket/) or [FAKE](https://fake.build/) will also be installed by the build script.

## Development mode

This development stack is designed to be used with minimal tooling. An instance of Visual Studio Code together with the excellent [Ionide](http://ionide.io/) plugin should be enough.

Start the development mode with:

    > build.cmd run // on windows
    $ ./build.sh run // on unix

This will start Suave on port 8085 and the webpack-dev-server on port 8080. Then it will open localhost:8080 in your browser.

Enjoy.

**NOTE**
Currently there is a [bug](https://github.com/rommsen/ConfPlanner/issues/30) that might prevent Fable and the server to be started in parallel.

If this is happening:
  * open a terminal
  * go to `src/Server`
  * run `dotnet run`
  * open another terminal
  * go to `src/Client`
  * run `dotnet fable yarn-run`


## Testing

With FAKE (does a full dotnet restore etc.)

    > build.cmd RunTests // on windows
    $ ./build.sh RunTests // on unix

On the CLI (without building everything) in `src/Domain.Tests`:

    > dotnet test

or in watch mode

    > dotnet watch test

You can now edit files in `src/Domain` or `src/Domain.Tests` and recompile + testing will be triggered automatically.

## Demo Data / Fixtures
If you want (and currently you do, because there is no way to add abstracts or organizers) prefill the conference Event-Store with some demo data you can run:

    > build.cmd RunFixtures // on windows
    $ ./build.sh RunFixtures // on unix

The events will be written to `src/Server/conference_eventstore.json`

Currently you do want this, because there is no way to add abstracts or organizers.

## Plans for the future
From the top of my head. If anyone wants to chip in, feel welcome.

### Deployment
* make use of Azure to deploy the application

### Infrastructure
* extract the project into its own repository and make it a bit more production ready :D
* implement at least one different event store implementation (e.g. SQLite or Azure something something)
* implement projections that can send notifications

### Server
* implement a proper autohrization system

### Domain
* build an actual Conference Planner

## Known Issues

### Getting rid of errors in chrome/firefox

- Either comment out the lines in `App.fs`:

```fsharp
#if DEBUG
|> Program.withDebugger
#endif
```

- Or install the [Redux DevTools](http://extension.remotedev.io/) as a Chrome/Firefox Extensions (recommended)
Only one error remains, when visiting the WebApp the first time.

## Maintainer(s)

- [@rommsen](https://github.com/rommsen)
- [@heimeshoff](https://github.com/heimeshoff)
- you?
