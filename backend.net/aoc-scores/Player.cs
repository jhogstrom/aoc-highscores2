using System;
using System.Collections.Generic;
using System.Diagnostics;

[DebuggerDisplay("{Name}: {LocalScore}")]
public class Player
{
    public Player()
    {
        const int dayCount = 25;
        LocalScoreActive = new ScoreRec(dayCount);
        LocalScoreAll = new ScoreRec(dayCount);
        TobiiScore = new ScoreRec(dayCount);
        UnixCompletionTime = InitArray(dayCount, -1L);
        TimeToComplete = InitArray<TimeSpan?>(dayCount, null);
        AccumulatedTimeToComplete = InitArray<TimeSpan?>(dayCount, null);
        OffsetFromWinner = InitArray<TimeSpan?>(dayCount, null);
        PositionForStar = InitArray(dayCount, -1);
        GlobalScoreForDay = InitArray<int?>(dayCount, null);

        TimeToCompleteStar2 = new TimeSpan?[dayCount];

        for (int i = 0; i < dayCount; i++)
        {
            TimeToCompleteStar2[i] = null;
        }
    }

    private static T[][] InitArray<T>(int size, T def)
    {
        var res = new T[size][];
        for (int i = 0; i < size; i++)
        {
            res[i] = new T[] { def, def };
        }

        return res;
    }

    public string Name { get; set; }
    public string AoCName { get; set; }
    public int RaffleTickets { get; set; }
    public int Id { get; set; }
    public long LastStar { get; set; }
    public int Stars { get; set; }
    public int GlobalScore { get; set; }
    public int AoCLocalScore { get; set; }
    public ScoreRec LocalScoreActive { get; }
    public ScoreRec LocalScoreAll { get; }
    public ScoreRec TobiiScore { get; }

    public long[][] UnixCompletionTime { get; }
    public int?[][] GlobalScoreForDay { get; set; }
    public int[][] PositionForStar { get; set; }
    public TimeSpan?[][] TimeToComplete { get; set; }
    public TimeSpan?[][] AccumulatedTimeToComplete { get; set; }
    public TimeSpan?[][] OffsetFromWinner { get; set; }
    public TimeSpan?[] TimeToCompleteStar2 { get; set; }

    public class ScoreRec
    {
        public ScoreRec(int days)
        {
            AccumulatedScore = InitArray(days, -1);
            AccumulatedPosition = InitArray(days, -1);
        }
        public int Score { get; set; }
        public int Position { get; set; }
        public int PendingPoints { get; set; }
        public int[][] AccumulatedScore { get; }
        public int[][] AccumulatedPosition { get; set; }

    }

    internal class LocalScoreComparer : IComparer<Player>
    {
        private readonly Func<Player, ScoreRec> _f;

        public LocalScoreComparer(Func<Player, ScoreRec> f)
        {
            _f = f;
        }

        public int Compare(Player x, Player y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var comp = -1 * _f(x).Score.CompareTo(_f(y).Score);
            if (comp != 0) return comp;

            comp = x.LastStar.CompareTo(y.LastStar);
            if (comp != 0) return comp;

            return x.Id.CompareTo(y.Id);
        }
    }
    internal class TobiiScoreComparer : IComparer<Player>
    {
        public int Compare(Player x, Player y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            var comp = -1 * x.Stars.CompareTo(y.Stars);
            if (comp != 0) return comp;

            comp = x.TobiiScore.Score.CompareTo(y.TobiiScore.Score);
            if (comp != 0) return comp;

            comp = x.LastStar.CompareTo(y.LastStar);
            if (comp != 0) return comp;

            return x.Id.CompareTo(y.Id);
        }
    }
}