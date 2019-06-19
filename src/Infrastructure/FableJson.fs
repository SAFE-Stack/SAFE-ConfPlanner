/// Functions to serialize and deserialize JSON, with client side support.
module Infrastructure.FableJson

open Thoth.Json.Net

let toJson value =
  Encode.Auto.toString(0, value)

let ofJson<'a> (json:string) : 'a =
  Decode.Auto.unsafeFromString<'a> json
