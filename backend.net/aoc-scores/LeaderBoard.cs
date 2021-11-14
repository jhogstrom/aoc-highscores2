using System.Collections.Generic;
using System.Linq;

public class LeaderBoard
{
    public LeaderBoard(List<Player> players, int highestDay)
    {
        Players = players;
        HighestDay = highestDay;
        TopScorePerDay = new int[highestDay][];
        StarsAwarded = new int[highestDay][];
        for (int day = 0; day < highestDay; day++)
        {
            TopScorePerDay[day] = new int[2];
            StarsAwarded[day] = new int[2];
            foreach (var p in players)
            {
                for (int star = 0; star < 2; star++)
                    if (p.unixCompletionTime[day][star] != -1)
                        StarsAwarded[day][star]++;
            }
        }
    }

    public List<Player> Players { get; }

//    public IEnumerable<Player> OrderedPlayers => Players.OrderByDescending(p => p.LocalScore).ThenBy(p => p.LastStar).ThenBy(p => p.Id);
    public int HighestDay { get; }
    public int[][] TopScorePerDay { get; }
    public int[][] StarsAwarded { get; set; }
}