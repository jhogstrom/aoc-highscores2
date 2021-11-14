Template for sample files:

boardconfig.json: 
{
  "Guid": "7d3e8718-f15c-41ed-a561-fbba4f3fa37c",
  "AocId": "139489",
  "Name": "test list",
  "Years": [ 2019, 2020, 2021 ],
  "SessionCookie": "<sessioncookie>",
  "SessionCookieExpiration": "2021-12-31T00:00:00",
  "ExcludedDays": [ 6 ]
}

Documentation of the fields in the final json file:

Players are by default sorted on local score as the primary sort criteria. To solve ties, the timestamp of the most recent star is used.

| Field | Example | Description |
| ----- | ------- | ----------- |
| Name | "Jonas Högström" | Name of participant |
| Id | 139489| AoC id|
| LastStar | 1608873428 | unix timestamp of the most recently acquired star. Used to disambiguate order |
| Stars | 50 | Total number of stars solved this year|
| LocalScore | 1433 | AoC style calculation of local score. Should always match what the AoC website says. Higher score is better|
| TobiiScore | 55 | This is the total number of TobiiScore (tm) points that the player has. Lower score is better |
| GlobalScore | 0 | Total number of Global points for the person. Non-zero score indicates super human coding skills |
| CurrentPosition | 1 | This is the position of the player in the sorted list (see above) |
| PendingLocalPoints | 0 | This is the number of local points that the player could get if all unsolved, published stars were solved before anyone else|
| UnixCompletionTime |  | The unix time for each day/star if the star has been solved, -1 for unsolved stars|
| GlobalScoreForDay |  | Currently not supported |
| PositionForStar |  | This is the order in which the star was solved. Ties will have the same position|
| AccumulatedTobiiScore | [] | This is the accumulated TobiiScore (tm). Lower scores are better |
| AccumulatedLocalScore | [] | This is the accumulated Local score for each star (independent of the order in which the stars were actually solved)|
| AccumulatedPosition | [] | For each day/star, this shows the position that the player would have if only the stars until that star were considered |
| TimeToComplete | [] | This is the time in hours/minutes/seconds that it took the player to complete the star from the publishing of the days problem |
| AccumulatedTimeToComplete | [] | Accumulated time spent on solving the stars|
| OffsetFromWinner | [] | How much time passed between the fastest solver and the player for each star (winners will have "00:00:00") |
| TimeToCompleteStar2 | | The time it took to complete *2 after *1 was solved. |  