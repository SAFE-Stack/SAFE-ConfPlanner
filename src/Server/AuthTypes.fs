namespace Server.AuthTypes

open System

// Json web token type.
type JWT = string

type Username =
  Username of string

type Password =
  Password of string

type Credentials =
  Username * Password

// Login credentials.
type Login =
  {
    UserName : Username
    Password : Password
    PasswordId : Guid
  }
