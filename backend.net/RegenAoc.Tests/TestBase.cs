using Amazon.Lambda.Core;

namespace RegenAoc.Tests;

internal class TestBase
{
    protected ILambdaLogger Logger { get; } = AwsMockHelpers.CreateMockLogger();
}