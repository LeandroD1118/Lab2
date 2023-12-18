using System;
using Xunit;

public class ProgramTests
{

    [Theory]
    [InlineData("48656C6C6F2C20576F726C64", "Hello, World")]
    [InlineData("5465737420537472696E67", "Test String")]
    [InlineData("31", "1")]
    [InlineData("31-32-33", "123")]
    [InlineData("41-42-43", "ABC")]
    [InlineData("776F726C64", "world")]
    [InlineData("4C-65-61-6E-64-72-6F-20-44-6F-6D-69-6E-67-75-65-7A", "Leandro Dominguez")]
    public void HexToAscii_ShouldConvert_WhenValidInput(string input, string expected)
    {
        var calc = Program.DecodeHexString(input);

        Assert.Equal(expected, calc);
    }

    [Fact]
    public void HexToAscii_InvalidHexInput_ShouldThrowException()
    {
    string invalidHexInput = "G123";

    Assert.Throws<FormatException>(() => Program.DecodeHexString(invalidHexInput));
    }

     [Fact]
    public void HexToAscii_WhitespaceInput_ShouldReturnEmptyString()
    {
        string whitespaceHexInput = "   ";

        var result = Program.DecodeHexString(whitespaceHexInput);

        Assert.Equal("", result);
    }

    [Fact]
    public void HexToAscii_nullInput_ShouldReturnEmptyString()
    {
        string nullHexInput = null;

        var result = Program.DecodeHexString(nullHexInput);

        Assert.Equal("", result);
    }
}
