module Global

type Page =
  | SSV
  | Home
  | Counter
  | About

let toHash page =
  match page with
  | SSV -> "#ssv"
  | About -> "#about"
  | Counter -> "#counter"
  | Home -> "#home"
