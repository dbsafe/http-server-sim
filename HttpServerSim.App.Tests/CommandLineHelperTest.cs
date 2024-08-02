// Ignore Spelling: App

using FluentAssertions;
using HttpServerSim.App.Config;

namespace HttpServerSim.App.Tests;

[TestClass]
public class CommandLineHelperTest
{
    private readonly static string _argsLine = "--Arg1 Value1 --Arg2 Value2 --Arg2.5 --Arg3 Value3 value --Arg4 Value4";
    private readonly static string[] _args = _argsLine.Split(' ');

    [TestMethod]
    public void Given_existing_arg_Should_return_the_value()
    {
        CommandLineHelper.GetValueFromArgs(_args, "Arg1").Should().Be("Value1");
        CommandLineHelper.GetValueFromArgs(_args, "Arg2").Should().Be("Value2");
        CommandLineHelper.GetValueFromArgs(_args, "Arg4").Should().Be("Value4");
    }

    [TestMethod]
    public void Given_missing_arg_Should_return_null()
    {
        CommandLineHelper.GetValueFromArgs(_args, "Arg10").Should().BeNull();
    }

    [TestMethod]
    public void Given_an_orphan_value_Should_return_value()
    {
        CommandLineHelper.GetValueFromArgs(_args, "value").Should().Be("value");
    }

    [TestMethod]
    public void Given_an_option_without_a_value_Should_return_the_option()
    {
        CommandLineHelper.GetValueFromArgs(_args, "Arg2.5").Should().Be("Arg2.5");
        CommandLineHelper.GetValueFromArgs(_args, "Arg2").Should().Be("Value2");
        CommandLineHelper.GetValueFromArgs(_args, "Arg3").Should().Be("Value3");
    }

    [TestMethod]
    public void Given_a_single_option_without_a_value_Should_return_the_option()
    {
        var args = new[] { "--Arg2.5" };
        CommandLineHelper.GetValueFromArgs(args, "Arg2.5").Should().Be("Arg2.5");
    }

    [TestMethod]
    public void Given_a_single_orphan_value_Should_return_the_value()
    {
        var args = new[] { "value" };
        CommandLineHelper.GetValueFromArgs(args, "value").Should().Be("value");
    }
}
