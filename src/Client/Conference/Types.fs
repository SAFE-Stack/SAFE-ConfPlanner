module Conference.Types

open Global
open Infrastructure.Types
open Server.ServerTypes

type Msg =
  | Received of ServerMsg<Events.Event,Dummy.QueryResult>
  | QueryState


type Model =
  {
    Info : string
    State : RemoteData<Model.Conference>
  }
