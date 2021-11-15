using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;

using NSubstitute;
using NUnit.Framework;

namespace RegenAoc.Tests
{
    public class Tests
    {
        private RegenAocLambda _sut = null!;
        private ILambdaContext _context = null!;

        [SetUp]
        public void Setup()
        {
            _sut = new RegenAocLambda();
            _context = Substitute.For<ILambdaContext>();
            _context.Logger.Returns(Substitute.For<ILambdaLogger>());
        }

        [Test]
        public async Task EmptyEventIsOK()
        {
            var sqsEvent = new SQSEvent() { Records = new List<SQSEvent.SQSMessage>() };
            await _sut.ReceiveEvent(sqsEvent, _context);
        }

        [Test]
        public async Task SimpleEventIsOK()
        {
            var sqsEvent = new SQSEvent() { Records = new List<SQSEvent.SQSMessage>(){new SQSEvent.SQSMessage()
            {
                Body = $"{{\"boardguid\": \"{TestData.Guid1}\", \"year\": 2021}}"
            }} };
            await _sut.ReceiveEvent(sqsEvent, _context);
        }
    }
}