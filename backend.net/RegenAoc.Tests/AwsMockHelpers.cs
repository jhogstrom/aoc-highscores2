using System;
using Amazon.Lambda.Core;
using NSubstitute;

namespace RegenAoc.Tests;

internal class AwsMockHelpers
{
    public static ILambdaLogger CreateMockLogger()
    {
        var logger = Substitute.For<ILambdaLogger>();
        logger.When(x => x.LogLine(Arg.Any<string>())).Do(c =>
            Console.WriteLine(c.Args()[0]));
        return logger;
    }
}