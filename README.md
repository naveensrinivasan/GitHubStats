# GitHubStats

The goal behind GitHUbStats was to identify all the PR's for the give year. [Octokit](https://github.com/octokit/octokit.net) does not provide an API for that.

I used the [githubarchive.org](http://githubarchive.org) and Google [big query](https://bigquery.cloud.google.com/dataset/githubarchive:month) to get the data and exported to json using Google Big Query.

Here is an example of FSharp Github Org [https://github.com/fsharp/](https://github.com/fsharp/) Stats using the `Script.fsx`.


![image](/images/Top10Repos.png)

![image](/images/Top10Contributors.png)

![image](/images/ContributionsByMonth.png)
