using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal.Util;
using HtmlAgilityPack;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class GlobalMgrTests : TestBase
    {
        private GlobalManager _sut = null!;
        private BoardConfig _boardConfig = null!;


        [SetUp]
        public void Setup()
        {
            _sut = new GlobalManager(Logger);
        }

        [Test]
        public async Task TestRefreshYear()
        {
            var year = 2020;
            var highestDay = 25;
            var boardConfig = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, year, Logger);
            var res = await _sut.GetGlobalScore(boardConfig, highestDay);
            Assert.That(res, Is.Not.Null);
            Assert.That(res.Days.Count, Is.EqualTo(highestDay));
        }


        [Test]
        public async Task TestRefreshDay()
        {
            var year = 2018;
            var res = await _sut.RefreshGlobalData(year, 20);
        }

        [Test]
        public void TestParse()
        {
            var day = 20;
            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText($"{day}.html"));
            _sut.ParseHtml(doc, day);
        }
    }
}
