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

            var config = BoardConfigHelper.LoadFromFile();
            BoardConfigHelper.SaveFile(config);

            await sut.Generate(config, 2020);
        }
    }
}
