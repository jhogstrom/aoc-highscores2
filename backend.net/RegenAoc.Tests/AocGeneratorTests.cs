﻿using System;
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
        [TestCase(TestData.Guid4, 2018)]
        [TestCase(TestData.Guid1, 2020)]
        public async Task TestGenerate(string id, int year)
        {
            var config = await BoardConfigHelper.LoadFromDynamo(id, year, Logger);

            await _sut.Generate(config);
        }
    }
}
