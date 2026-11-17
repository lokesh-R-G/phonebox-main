using PhoneBox.Agent;
using Xunit;

public class KeySenderTests
{
    [Fact]
    public void ParsesCombosAndSequences()
    {
        var combos = KeySender.Parse("Ctrl+Shift+M, cmd+Space,F13, play");
        Assert.Equal([["ctrl", "shift", "m"], ["win", "space"], ["f13"], ["playpause"]], combos);
    }

    [Theory]
    [InlineData("ctrl+a+b")]      // two keys besides modifiers
    [InlineData("ctrl+banana")]   // unknown key
    [InlineData("f25")]
    [InlineData(" , ")]           // nothing
    public void RejectsBadSpecs(string spec) => Assert.Throws<ArgumentException>(() => KeySender.Parse(spec));

    [Fact]
    public void BracketsAreKeys() // Photoshop's brush size
    {
        Assert.Equal([["bracketleft"], ["ctrl", "bracketright"]], KeySender.Parse("[, ctrl+]"));
        Assert.Equal(0xDB, KeySender.WinVk("bracketleft"));
        Assert.Equal(0xDD, KeySender.WinVk("bracketright"));
    }

    [Fact]
    public void ModifiersAloneAreAllowed() => Assert.Equal([["win"]], KeySender.Parse("Win"));

    // #98: the generic VK_SHIFT/VK_CONTROL/VK_MENU left raw-input apps (Discord) guessing which side was pressed
    [Theory]
    [InlineData("shift", 0xA0)]
    [InlineData("ctrl", 0xA2)]
    [InlineData("alt", 0xA4)]
    [InlineData("win", 0x5B)]
    [InlineData("f24", 0x87)]
    [InlineData("m", 0x4D)]
    [InlineData("playpause", 0xB3)]
    public void WindowsKeysAreTheLeftHandOnes(string key, int vk) => Assert.Equal(vk, KeySender.WinVk(key));
}
