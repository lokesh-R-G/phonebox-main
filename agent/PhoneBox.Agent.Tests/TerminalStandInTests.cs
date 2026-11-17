using PhoneBox.Agent;
using Xunit;

// #73: the default Terminal shortcut starts wt.exe (not on Windows 10) or x-terminal-emulator (Debian family only)
public class TerminalStandInTests
{
    [Theory]
    [InlineData("wt.exe")]
    [InlineData("WT")]
    public void WindowsTerminalFallsBackToPowerShell(string cmd) => Assert.Equal(["powershell.exe", "cmd.exe"], Shortcuts.TerminalStandIns(cmd));

    [Fact]
    public void LinuxTriesTheTerminalVariableFirst()
    {
        var alts = Shortcuts.TerminalStandIns("x-terminal-emulator", " alacritty ");
        Assert.Equal("alacritty", alts[0]);
        Assert.Contains("konsole", alts);
        Assert.Equal("xterm", alts[^1]);
    }

    [Fact]
    public void LinuxWithoutTheVariableStartsWithTheDesktopTerminals() =>
        Assert.Equal("kgx", Shortcuts.TerminalStandIns("x-terminal-emulator", null)[0]);

    [Theory]
    [InlineData("notepad.exe")]
    [InlineData("code")]
    [InlineData("C:\\Tools\\wt.exe")] // an explicit path is the user's own choice
    public void OtherCommandsAreLeftAlone(string cmd) => Assert.Empty(Shortcuts.TerminalStandIns(cmd));
}
