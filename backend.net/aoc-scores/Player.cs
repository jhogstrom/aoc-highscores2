using System;
using System.Diagnostics;

[DebuggerDisplay("{Name}: {LocalScore}")]
public class Player
{
    public Player()
    {
        const int dayCount = 25;
        unixCompletionTime = new long[dayCount][];
        TimeToComplete = new TimeSpan?[dayCount][];
        AccumulatedTimeToComplete = new TimeSpan?[dayCount][];
        OffsetFromWinner = new TimeSpan?[dayCount][];
        TimeToCompleteStar2 = new TimeSpan?[dayCount];
        PositionForStar = new int[dayCount][];
        AccumulatedTobiiScore = new int[dayCount][];
        AccumulatedScore = new int[dayCount][];
        AccumulatedPosition = new int[dayCount][];
        GlobalScoreForDay = new int?[dayCount][];
        for (int i = 0; i < dayCount; i++)
        {
            unixCompletionTime[i] = new long[] { -1, -1 };
            PositionForStar[i] = new[] { -1, -1 };
            AccumulatedScore[i] = new[] { -1, -1 };
            AccumulatedTobiiScore[i] = new[] { -1, -1 };
            AccumulatedPosition[i] = new[] { -1, -1 };
            GlobalScoreForDay[i] = new int?[] { null, null };
            TimeToComplete[i] = new TimeSpan?[2];
            AccumulatedTimeToComplete[i] = new TimeSpan?[2];
            TimeToCompleteStar2[i] = null;
            OffsetFromWinner[i] = new TimeSpan?[2];
        }
    }

    public int?[][] GlobalScoreForDay { get; set; }

    public int TotalScore { get; set; }
    public int[][] PositionForStar { get; set; }
    public int[][] AccumulatedTobiiScore { get; set; }
    public int[][] AccumulatedScore { get; set; }
    public int[][] AccumulatedPosition { get; set; }

    public string Name { get; set; }
    public long LastStar { get; set; }
    public int Stars { get; set; }
    public int LocalScore { get; set; }
    public int GlobalScore { get; set; }
    public int Id { get; set; }
    public long[][] unixCompletionTime { get; }
    public TimeSpan?[][] TimeToComplete { get; set; }
    public TimeSpan?[][] AccumulatedTimeToComplete { get; set; }
    public TimeSpan?[][] OffsetFromWinner { get; set; }
    public TimeSpan?[] TimeToCompleteStar2 { get; set; }
    public string Props { get; set; }
    public int CurrentPosition { get; set; }
    public int PendingPoints { get; set; }
    public int AccumulatedTobiiScoreTotal { get; set; }

    public string Flyoverhint(int day)
    {
        return $"Time *1: {TimeToComplete[day][0]}\nTime *2: {TimeToComplete[day][1]}";
    }
}