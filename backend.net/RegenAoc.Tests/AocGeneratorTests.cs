using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class AocGeneratorTests
    {
        [Test]
        public void TestDeserialization()
        {
            var sut = new AocGenerator();
            var json = File.ReadAllText("sampleaoc.json");
            var list = sut.DeserializeAocJson(json);
        }

        [Test]
        public async Task TestGenerate()
        {
            var sut = new AocGenerator();

            var config = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, 2020);

            await sut.Generate(config, 2020);
        }
    }
}
