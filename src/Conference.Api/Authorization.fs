module Conference.Api.Authorization

open Domain.Model
open Infrastructure.Auth

let private adminIdentity =
  "9c29b202-d4b3-4472-bec8-afcd4ecbb9f2"
  |> System.Guid.Parse
  |> Identity

let private adminId =
  "4ad17799-6c3c-4e04-b1f0-9d7523cfa172"
  |> System.Guid.Parse
  |> AdminId

let private organizerId =
  "311b9fbd-98a2-401e-b9e9-bab15897dad4"
  |> System.Guid.Parse
  |> OrganizerId

let private adminIdentityProvider (Username username , Password password) =
  async {
    let identity =
      if (username = "admin" && password = "admin") then
        Some adminIdentity
      else
        None

    return identity
  }

let private adminRoleProvider identity =
  async {
    let roles =
      match identity with
      | identity when identity = adminIdentity ->
          Roles.empty
          |> Roles.withAdmin adminId

      | _ ->
          Roles.empty

    return roles
  }

let private organizerRoleProvider identity =
  async {
    let roles =
      match identity with
      | identity when identity = adminIdentity ->
          Roles.empty
          |> Roles.withOrganizer organizerId

      | _ ->
          Roles.empty

    return roles
  }

let private identityProviders : AsyncIdentityProvider list =
  [ adminIdentityProvider ]

let private permissionProviders : AsyncPermissionProvider<Roles.Container> list =
  [
    adminRoleProvider
    organizerRoleProvider
  ]

let identityProvider username password =
  (username,password)
  |> oneIdentityOf identityProviders
  |> Async.RunSynchronously

let permissionProvider identity =
  identity
  |> allPermissions permissionProviders
  |> Async.RunSynchronously
  |> Roles.concat
