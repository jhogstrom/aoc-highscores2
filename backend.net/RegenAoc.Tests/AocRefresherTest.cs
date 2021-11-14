using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using NSubstitute;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class AocRefresherTest
    {
        private AocRefresher _sut = null!;
        private BoardConfig _boardConfig = null!;

        [SetUp]
        public void Setup()
        {
            var logger = Substitute.For<ILambdaLogger>();
            logger.
                When(x=>x.LogLine(default)).
                Do(c=>
                    Console.WriteLine(c.Args()[0]));

            _sut = new AocRefresher(logger, AwsHelpers.InternalBucket);
        }

        [Test]
        public async Task TestRefresh2020()
        {
            _boardConfig = await BoardConfigHelper.LoadFromDynamo("7d3e8718-f15c-41ed-a561-fbba4f3fa37c", 2020);
            await _sut.EnsureFresh(_boardConfig, 2020);
        }
    }

}
