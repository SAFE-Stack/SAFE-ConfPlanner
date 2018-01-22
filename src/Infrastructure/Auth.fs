module Infrastructure.Auth

open System

// Json web token type.
type JWT = string

type Identity =
  Identity of Guid

type Username =
  Username of string

type Password =
  Password of string

type Credentials =
  Username * Password

type AsyncIdentityProvider =
  (Username * Password) -> Async<Identity option>

type AsyncUserProvider<'User> =
  Identity -> Async<'User option>

type AsyncPermissionProvider<'User,'Permission> =
  'User -> Async<'Permission>

type IdentityProvider =
  (Username * Password) -> Identity option

type UserProvider<'User> =
  Identity -> 'User option

type PermissionProvider<'User,'Permission> =
  'User -> 'Permission

// Login credentials.
type Login =
  {
    UserName : Username
    Password : Password
    PasswordId : Guid
  }

let rec oneIdentityOf (identityProviders : AsyncIdentityProvider list) (credentials : Credentials) =
  async {
    match identityProviders with
    | identityProvider :: rest ->
        let! result = identityProvider credentials

        match result with
        | None ->
            return! oneIdentityOf rest credentials

        | Some identity ->
            return Some identity

    | _ -> return None
  }

let rec oneUserOf (userProviders : AsyncUserProvider<'User> list) identity =
  async {
    match userProviders with
    | userProvider :: rest ->
        let! result = userProvider identity

        match result with
        | None ->
            return! oneUserOf rest identity

        | Some user ->
            return Some user

    | _ -> return None
  }

let allPermissions (permissionProviders : AsyncPermissionProvider<'User,'Permission> list) user  =
  async {
    let! permissions =
      permissionProviders
      |> List.map (fun provider -> provider user)
      |> Async.Parallel

    return
      permissions
      |> Array.toList
  }
