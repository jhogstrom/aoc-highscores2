using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.VisualBasic;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    internal class AocRefresherTest
    {
        private AocRefresher _sut = null!;
        private ListConfig _listConfig = null!;

        [SetUp]
        public void Setup()
        {
            var logger = Substitute.For<ILambdaLogger>();
            logger.
                When(x=>x.LogLine(default)).
                Do(c=>
                    Console.WriteLine(c.Args()[0]));

            _listConfig = ListConfigHelper.LoadFromFile();
            _sut = new AocRefresher(logger, Constants.InternalBucket);
        }

        [Test]
        public async Task TestRefresh2020()
        {
            await _sut.EnsureFresh(_listConfig, 2020);
        }
    }

}
