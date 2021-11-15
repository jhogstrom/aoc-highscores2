using System;
using System.Collections.Generic;
using System.Linq;

public class LeaderBoard
{
    public LeaderBoard(
        List<Player> players, 
        int highestDay, 
        List<int> excludeDays, 
        List<string> excludedPlayers,
        DateTime aocLastModified)
    {
        Players = players;
        HighestDay = highestDay;
//        TopLocalScore = new int[highestDay][];
        StarsAwarded = new int[highestDay][];
        for (int day = 0; day < highestDay; day++)
        {
//            TopLocalScore[day] = new int[2];
//            TopActiveLocalScore[day] = new int[2];
            StarsAwarded[day] = new int[2];
            foreach (var p in players)
            {
                for (int star = 0; star < 2; star++)
                    if (p.UnixCompletionTime[day][star] != -1)
                        StarsAwarded[day][star]++;
            }
        }

        RetrievedFromAoC = aocLastModified;
        ExcludedDays = new List<int>(excludeDays);
        ExcludedPlayers = new List<string>(excludedPlayers);

    }

    public int HighestDay { get; }
    public List<int> ExcludedDays { get; }
    public List<string> ExcludedPlayers { get; }
    public List<Player> Players { get; }
    public DateTime RetrievedFromAoC { get; set; }

    // public int[][] TopLocalScore { get; }
    // public int[][] TopActiveLocalScore { get; }
    public int[][] StarsAwarded { get; set; }
}