module Conference.Api.Organizers2

open Domain.Model
open Domain.Events
open Infrastructure.Auth

// type OrganizersReadModel =
//   (OrganizerId * Identity) list


// let private apply readModel event =
//   match event with
//     | ConferenceScheduled conference ->
//         (conference.Id, conference.Title) :: readModel

//     | _ ->
//         readModel

// let private evolveState state (streamId : StreamId ,events) : OrganizersReadModel =
//   let conference =
//     state
//     |> Map.tryFind streamId
//     |> Option.defaultValue (emptyConference())

//   state
//   |> Map.add streamId (events |> List.fold apply conference)

// let projection : ProjectionDefinition<ConferenceReadModel, Event>=
//   {
//     InitialState = Map.empty
//     UpdateState = evolveState
//   }
