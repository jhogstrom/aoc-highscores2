using System.Threading.Tasks;
using Amazon.Runtime.Internal.Util;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class AocRefresherTest: TestBase
    {
        private AocRefresher _sut = null!;
        private BoardConfig _boardConfig = null!;
        

        [SetUp]
        public void Setup()
        {
            _sut = new AocRefresher(Logger, AwsHelpers.InternalBucket);
        }

        [Test]
        public async Task TestRefresh()
        {
            var year = 2017;
            _boardConfig = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, year, Logger);
            await _sut.EnsureFresh(_boardConfig);
        }
    }
}
