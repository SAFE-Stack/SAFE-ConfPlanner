# ConfPlanner

Steps to Reproduce

* Clone Repo: `git clone git@github.com:rommsen/ConfPlanner.git`
* Checkout branch `git checkout Suave-Problems`
* go to directory and 
  * run `dotnet restore`
  * run `yarn install`
* go to directory `src/Server` and run `dotnet run`
* go to directory `src/Client` and run `dotnet fable yarn-start`
* open two tabs with `localhost:8080` in Browser 
* click "Command" in one tab => both tabs reveice event
* reload one tab and click "Command" again => everything works fine
* reload tab again and click "Command" => Client reveives event twice (server log shows that events are send three times, twice for the reloaded tab and once for the other tab)
* reload again => event count per click keeps on rising

