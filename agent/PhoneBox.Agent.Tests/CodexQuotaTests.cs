using Microsoft.Extensions.Logging.Abstractions;
using PhoneBox.Agent;
using Xunit;

/// <summary>The Codex reading from a session log of the test's own, never the real ~/.codex.</summary>
public sealed class CodexQuotaTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("phonebox-codex-").FullName;

    public void Dispose()
    {
        try { Directory.Delete(_root, recursive: true); } catch (IOException) { }
    }

    private QuotaMonitor Agent() => new(NullLogger<QuotaMonitor>.Instance) { CodexSessionsRoot = _root };

    private static string Reading(int percent) =>
        """{"timestamp":"2026-11-16T10:00:00Z","type":"event_msg","payload":{"type":"token_count","rate_limits":{"primary":{"used_percent":"""
        + percent + ""","window_minutes":300,"resets_at":4102444800},"secondary":{"used_percent":3,"window_minutes":10080,"resets_at":4102444800}}}}""";

    private string Log(params string[] lines)
    {
        var dir = Path.Combine(_root, "2026", "11", "16");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "rollout-2026-11-16T10-00-00-test.jsonl");
        File.WriteAllText(path, string.Join("\n", lines));
        return path;
    }

    private static QuotaProvider Codex(QuotaMonitor agent) => Assert.Single(agent.Snapshot(), p => p.Id == "codex");

    [Fact]
    public void ACompleteLastReadingIsUsed()
    {
        Log("{\"type\":\"session_meta\"}", Reading(12) + "\n");
        var agent = Agent();
        agent.CodexTick();
        Assert.Null(Codex(agent).Error);
        Assert.Equal(12, Codex(agent).Limits[0].Percent);
    }

    [Fact]
    public void ALastLineCutOffMidWriteFallsBackToTheReadingBeforeIt()
    {
        // Codex was stopped, or PhoneBox read the file, while the newest reading was half written
        var torn = Reading(55);
        Log("{\"type\":\"session_meta\"}", Reading(12), torn[..(torn.Length / 2 + 40)]);
        var agent = Agent();
        agent.CodexTick();
        Assert.Null(Codex(agent).Error);
        Assert.Equal(12, Codex(agent).Limits[0].Percent);
    }

    [Fact]
    public void ALockedLogKeepsTheLastReading()
    {
        if (!OperatingSystem.IsWindows()) return; // FileShare.None only locks other readers on Windows
        var path = Log("{\"type\":\"session_meta\"}", Reading(12) + "\n");
        var agent = Agent();
        agent.CodexTick();
        using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            agent.CodexTick();
        Assert.Equal(12, Codex(agent).Limits[0].Percent);
    }
}
