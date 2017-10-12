module Global

type Page =
  | Websockets

let toHash page =
  match page with
  | Websockets -> "#websockets"
