using System;
using System.Diagnostics;

[DebuggerDisplay("{Name}: {LocalScore}")]
public class Player
{
    public Player()
    {
        const int dayCount = 25;
        UnixCompletionTime = InitArray(dayCount, -1L);
        TimeToComplete = InitArray<TimeSpan?>(dayCount, null); 
        AccumulatedTimeToComplete = InitArray<TimeSpan?>(dayCount, null); 
        OffsetFromWinner = InitArray<TimeSpan?>(dayCount, null);
        PositionForStar = InitArray(dayCount, -1);
        AccumulatedTobiiScore = InitArray(dayCount, -1);
        AccumulatedLocalScore = InitArray(dayCount, -1);
        AccumulatedPosition = InitArray(dayCount, -1);
        GlobalScoreForDay = InitArray<int?>(dayCount, null);

        TimeToCompleteStar2 = new TimeSpan?[dayCount];

        for (int i = 0; i < dayCount; i++)
        {

            TimeToCompleteStar2[i] = null;
        }
    }

    private T[][] InitArray<T>(int size, T def)
    {
        var res = new T[size][];
        for (int i = 0; i < size; i++)
        {
            res[i] = new T[] { def, def };
        }

        return res;
    }

    public string Name { get; set; }
    public int Id { get; set; }
    public long LastStar { get; set; }
    public int Stars { get; set; }
//    public int LocalScore { get; set; }
    public int GlobalScore { get; set; }
    public int LocalScore { get; set; }
    public int TobiiScore { get; set; }
    public int CurrentPosition { get; set; }
    public int PendingLocalPoints { get; set; }

    public string Props { get; set; }

    public long[][] UnixCompletionTime { get; }
    public int?[][] GlobalScoreForDay { get; set; }
    public int[][] PositionForStar { get; set; }
    public int[][] AccumulatedTobiiScore { get; set; }
    public int[][] AccumulatedLocalScore { get; set; }
    public int[][] AccumulatedPosition { get; set; }
    public TimeSpan?[][] TimeToComplete { get; set; }
    public TimeSpan?[][] AccumulatedTimeToComplete { get; set; }
    public TimeSpan?[][] OffsetFromWinner { get; set; }
    public TimeSpan?[] TimeToCompleteStar2 { get; set; }

    public string Flyoverhint(int day)
    {
        return $"Time *1: {TimeToComplete[day][0]}\nTime *2: {TimeToComplete[day][1]}";
    }
}