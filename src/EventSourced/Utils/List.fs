module List

    /// Map a Result producing function over a list to get a new Result
    /// ('a -> Result<'b>) -> 'a list -> Result<'b list>
    let traverseResult f list =

      // define the monadic functions
      let (>>=) x f = Result.bind f x
      let retn = Result.Ok

      // right fold over the list
      let initState = retn []
      let folder head tail =
          f head >>= (fun h ->
          tail >>= (fun t ->
          retn (h :: t) ))

      List.foldBack folder list initState