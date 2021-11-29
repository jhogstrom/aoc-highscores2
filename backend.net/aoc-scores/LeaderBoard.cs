using System;
using System.Collections.Generic;

public class LeaderBoard
{
    public LeaderBoard(
        List<Player> players, 
        int highestDay, 
        List<int> excludeDays, 
        List<string> excludedPlayers,
        DateTime aocLastModified, 
        int year, string name)
    {
        Players = players;
        HighestDay = highestDay;
        StarsAwarded = new int[highestDay][];
        for (int day = 0; day < highestDay; day++)
        {
            StarsAwarded[day] = new int[2];
            foreach (var p in players)
            {
                for (int star = 0; star < 2; star++)
                    if (p.UnixCompletionTime[day][star] != -1)
                        StarsAwarded[day][star]++;
            }
        }

        RetrievedFromAoC = aocLastModified;
        Generated = DateTime.UtcNow;
        Year = year;
        Name = name;
        ExcludedDays = new List<int>(excludeDays);
        ExcludedPlayers = new List<string>(excludedPlayers);
    }

    public string Name { get; }
    public int Year { get; }
    public DateTime RetrievedFromAoC { get; set; }
    public DateTime Generated { get; set; }
    public TimeSpan ProcessTime { get; set; }
    public int HighestDay { get; }
    public List<int> ExcludedDays { get; }
    public List<string> ExcludedPlayers { get; }
    public int[][] StarsAwarded { get; set; }
    public List<Player> Players { get; }
}