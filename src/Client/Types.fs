module App.Types

open Global

type Msg =
  | WsMsg of Ws.Msg

type Model = {
    CurrentPage: Page
    WsModel : Ws.Model
  }
