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

type AsyncPermissionProvider<'Permission> =
  Identity -> Async<'Permission>

type IdentityProvider =
  (Username * Password) -> Identity option

type PermissionProvider<'Permission> =
  Identity -> 'Permission

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

let allPermissions (permissionProviders : AsyncPermissionProvider<'Permission> list) identity  =
  async {
    let! permissions =
      permissionProviders
      |> List.map (fun provider -> provider identity)
      |> Async.Parallel

    return
      permissions
      |> Array.toList
  }
