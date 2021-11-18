using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class AocGeneratorTests : TestBase
    {
        private static AocGenerator _sut = null!;

        [SetUp]
        public void Setup()
        {
            _sut = new AocGenerator(Logger);
        }

        [Test]
        public void TestDeserialization()
        {
            var json = File.ReadAllText("sampleaoc.json");
            var list = _sut.DeserializeAocJson(json);
        }


        [Test]
        public async Task TestGenerate()
        {
            var year = 2020;
            var config = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, year, Logger);

            await _sut.Generate(config, year);
        }
    }
}
