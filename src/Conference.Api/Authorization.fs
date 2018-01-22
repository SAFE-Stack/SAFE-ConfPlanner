module Conference.Api.Authorization

open Domain.Model
open Infrastructure.Auth


type Permissions =
  {
    User : User
    Permissions : Roles.Container
  }

let private adminIdentity =
  "9c29b202-d4b3-4472-bec8-afcd4ecbb9f2"
  |> System.Guid.Parse
  |> Identity


let private adminUserId =
  "882084d3-bc3a-4865-973f-7b9a27dd475a"
  |> System.Guid.Parse
  |> UserId

let private adminId =
  "4ad17799-6c3c-4e04-b1f0-9d7523cfa172"
  |> System.Guid.Parse
  |> AdminId

let private organizerId =
  "311b9fbd-98a2-401e-b9e9-bab15897dad4"
  |> System.Guid.Parse
  |> OrganizerId

let private adminUser =
  {
    Id = adminUserId
    Firstname = "Roman"
    Lastname = "Sachse"
  }

let private adminIdentityProvider (Username username , Password password) =
  async {
    let identity =
      if (username = "admin" && password = "admin") then
        Some adminIdentity
      else
        None

    return identity
  }

let private adminUserProvider identity =
   async {
    let user =
      match identity with
      | identity when identity = adminIdentity ->
          adminUser |> Some

      | _ ->
          None

    return user
  }

let private adminRoleProvider (user : User) =
  async {
    let roles =
      match user.Id with
      | userId when userId = adminUserId ->
          Roles.empty
          |> Roles.withAdmin adminId

      | _ ->
          Roles.empty

    return roles
  }

let private organizerRoleProvider (user : User) =
  async {
    let roles =
      match user.Id with
      | userId when userId = adminUserId ->
          Roles.empty
          |> Roles.withOrganizer organizerId

      | _ ->
          Roles.empty

    return roles
  }

let private identityProviders : AsyncIdentityProvider list =
  [ adminIdentityProvider ]

let private userProviders : AsyncUserProvider<User> list =
  [ adminUserProvider ]

let private permissionProviders : AsyncPermissionProvider<User,Roles.Container> list =
  [
    adminRoleProvider
    organizerRoleProvider
  ]

let identityProvider credentials =
  credentials
  |> oneIdentityOf identityProviders
  |> Async.RunSynchronously

let userProvider identity =
  identity
  |> oneUserOf userProviders
  |> Async.RunSynchronously

let permissionProvider user =
  user
  |> allPermissions permissionProviders
  |> Async.RunSynchronously
  |> Roles.concat
