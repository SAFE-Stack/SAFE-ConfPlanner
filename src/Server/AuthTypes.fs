namespace Server.AuthTypes

// Json web token type.
type JWT = string

// Login credentials.
type Login =
    { UserName : string
      Password : string }
