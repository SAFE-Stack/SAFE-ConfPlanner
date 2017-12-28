namespace Server.AuthTypes

open System

// Json web token type.
type JWT = string

// Login credentials.
type Login =
  {
    UserName : string
    Password : string
    PasswordId : Guid
  }
