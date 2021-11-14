using System.Threading.Tasks;
using Amazon.Lambda.Core;
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
        public async Task TestRefresh2020()
        {
            _boardConfig = await BoardConfigHelper.LoadFromDynamo(TestData.Guid1, 2020);
            await _sut.EnsureFresh(_boardConfig, 2020);
        }
    }

    internal class TestBase
    {
        protected ILambdaLogger Logger { get; } = AwsMockHelpers.CreateMockLogger();
    }
}
