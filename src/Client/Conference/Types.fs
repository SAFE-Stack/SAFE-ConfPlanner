module Conference.Types


type Model =
  {
    Token : string
  }

type Msg =
  | FinishVotingPeriod
  | PostCommandSuccess of string
  | PostCommandError of exn
