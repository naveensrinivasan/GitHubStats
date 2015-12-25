
#I __SOURCE_DIRECTORY__
#I "packages/FSharp.Data/lib/net40"
#I "packages/Xplot.Plotly/lib/net45"
#r "FSharp.Data.dll"
#r "XPlot.Plotly.dll"


#load "packages/FsLab/FsLab.fsx"
#load "GitHub.fsx"

open Foogle
open Deedle
open System.IO
open System.Globalization
open System
open GitHub
open XPlot
open GitHub

type GitHuBPR = {Opened : DateTime;
      Merged:Option<DateTime>;
      Author : string ;
      Comments : int;
      Repo : string;
      Id: int}

let keyvalues =   fun (key,values) -> (key, values |> Seq.length)
let stringkeyvalues =   fun (key,values) -> (key.ToString(), values |> Seq.length)
let PR = "PullRequestEvent"

let fsharpeventsjson = Path.Combine(__SOURCE_DIRECTORY__, "fsharporg2015githubevents.json")


let pullrequests (e)  = 
     GitHubEvent.Parse e |> Seq.filter(fun f ->  try  f.Type = PR with e -> false )
                |> Seq.map(fun f -> f.Payload.Replace("\\",""))  // replacing json escape character
                |> Seq.map(fun f -> try Some(PullRequest.Parse(f)) with e -> None)
                |> Seq.filter(fun f -> f.IsSome)
                |> Seq.map(fun f -> f.Value)
                |> Seq.map(fun f -> {Opened = f.PullRequest.CreatedAt;
                                        Merged = (try Some(DateTime.Parse(f.PullRequest
                                                        .MergedAt.JsonValue.ToString().Replace("\"","")))
                                                    with e -> None);
                                      Author = f.PullRequest.User.Login;
                                      Comments = f.PullRequest.ReviewComments;
                                      Repo = f.PullRequest.Base.Repo.Name;
                                      Id = f.PullRequest.Id})

let top10Repos (githubprs:seq<GitHuBPR>) = githubprs
                                             |> Seq.groupBy (fun x -> x.Repo)
                                             |> Seq.map keyvalues
                                             |> Seq.sortByDescending(fun f -> snd f  )
                                             |> Seq.take(10)

let prsByMonth (githubprs:seq<GitHuBPR>) = githubprs
                                         |> Seq.groupBy (fun f -> DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName( f.Opened.Month))
                                         |> Seq.map keyvalues
                                         |>  Seq.sortByDescending(fun f -> snd f  )

let prsByHour (githubprs:seq<GitHuBPR>) = githubprs
                                             |> Seq.groupBy (fun f -> f.Opened.ToUniversalTime().Hour) 
                                             |> Seq.map stringkeyvalues
                                             |> Seq.sortByDescending(fun f -> snd f)
                                             |> Seq.take(10)

let top10contributorsByPR (githubprs:seq<GitHuBPR>) = 
                githubprs
                     |> Seq.groupBy (fun f -> f.Author)
                     |> Seq.map keyvalues
                     |> Seq.sortByDescending(fun f -> snd f)
                     |> Seq.take(10)


let durationToMergePRByRepo (githubprs:seq<GitHuBPR>) = 
                githubprs
                        |> Seq.filter(fun f -> f.Merged.IsSome)
                        |> Seq.map(fun f ->(f.Repo, 
                                            f.Merged.Value.ToUniversalTime().Subtract(f.Opened.ToUniversalTime()).Hours))
                        |> Seq.groupBy(fun f -> (fst f))
                        |> Seq.map keyvalues
 
let fsharpevents = File.ReadAllText  fsharpeventsjson

let fsharpPRs = pullrequests fsharpevents

fsharpPRs |>  top10Repos  |> Chart.PieChart |> Chart.WithTitle "Top 10 FSharp Repo's in 2015 By PR's"

fsharpPRs |>  top10contributorsByPR  |> Chart.PieChart |> Chart.WithTitle "Top 10 GitHub Contributors in FSharp org in 2015 "

fsharpPRs |>  prsByMonth|> Chart.PieChart |>  Chart.WithTitle "FSharp PR's By Month"
 
fsharpPRs |> prsByHour  |> Chart.PieChart |> Chart.WithTitle "FSharp PR's by the hour of the day"