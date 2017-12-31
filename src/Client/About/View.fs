module Info.View

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma.Layouts
open Fulma.Elements

let view =
  Section.section []
    [
      Container.container [ Container.isFluid ]
        [
          Content.content []
            [
              h1 [] [ str "SAFE-ConfPlanner" ]
              Image.image [ Image.is128x128 ] [ img [ Src "img/safe_logo.png" ] ]
              p []
                [
                  str "This is the sample project that is used in the talk "
                  a [ Href "http://bit.ly/DomainDrivenUi" ] [ str "Domain Driven UI" ]
                  str ". The title is a bit misleading as it is a bad name for 'Reusing your datatypes and behaviour from your CQRS/Event-Sourced models in your Elm-architecture application." ]
              p [] [ str "Given you use F# and Fable, you can actually build simple eventually connected systems and have the exact same model working in back and frontend." ]
              p [] [ str "The application showcases a couple of things:" ]
              p []
                [
                  ul []
                    [
                      li [] [ str "It reuses the complete domain model on the client and server."]
                      li [] [ str "It shows the nice fit of and similarity between CQRS/Event-Sourcing on the backend and the Elm-Architecture on the frontend"]
                      li [] [ str "It reuses projections from the backend in the update function of the elmish app. The backend is sending domain events to the frontend and the (Elm-)model is updated with the help of projections defined in the backend (on all clients that are connected via websockets)"]
                      li [] [ str "It shows an easy way of implementing 'Whatif'-Scenarios, i.e. scenarios that enable the user try out different actions. When the user is happy with the result the system sends a batch of commands to the server. When 'Whatif-Mode' is enabled the client reuses not only the projections but also the domain behaviour defined on the server to create the events needed by the update function. The potential commands are also stored."]
                      li []
                        [
                          str "It uses the awesome "
                          a [ Href "https://mangelmaxime.github.io/Fulma/" ] [ str "Fulma" ]
                          str " library for styling."
                        ]
                      li [] [ str "It has BDD Style tests that show how nice the behaviour of Event-Sourced systems can be tested."]
                      li [] [ str "Websockets with Elmish/Suave"]
                    ]
                ]
            ]
        ]
    ]

