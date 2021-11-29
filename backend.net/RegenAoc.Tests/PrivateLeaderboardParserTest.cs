using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RegenAoc.Tests;

internal class PrivateLeaderboardParserTest : TestBase
{
    private PrivateLeaderboardParser _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new PrivateLeaderboardParser(Logger);
    }

    [Test]
    public async Task TestRefreshYear()
    {
        var year = 2020;
        var boardConfig = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, year, Logger);
        var res = await _sut.RefreshLeaderboardData(boardConfig, true);
        Assert.That(res, Is.Not.Null);
        Assert.That(res.Count, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(TestData.Guid1, 2020)]
    public async Task TestRefreshYearAndMatch(string g, int year)
    {
        var boardConfig = await BoardConfigHelper.LoadFromDynamo(g, year, Logger);
        var res = await _sut.RefreshLeaderboardData(boardConfig, true);
        _sut.MatchData(new List<Player>(), new Dictionary<int, LeaderboardPlayer>(), res);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Count, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(TestData.Guid1, 2020)]
    public async Task TestRefreshYearAndMatchWithPlayerData(string g, int year)
    {
        var boardConfig = await BoardConfigHelper.LoadFromDynamo(g, year, Logger);
        var res = await _sut.RefreshLeaderboardData(boardConfig, true);
        _sut.MatchData(new List<Player>(), new Dictionary<int, LeaderboardPlayer>(), res);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Count, Is.GreaterThan(0));
    }

}