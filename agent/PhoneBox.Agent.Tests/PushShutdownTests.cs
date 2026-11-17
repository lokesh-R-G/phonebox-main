using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PhoneBox.Agent;
using Xunit;

public sealed class PushShutdownTests
{
    [Fact]
    public void DisposingTheHostFlushesUpdatesStillWaitingForTheTimer()
    {
        var directory = Directory.CreateTempSubdirectory("phonebox-push-shutdown-");
        try
        {
            var path = Path.Combine(directory.FullName, "data.json");
            using (var services = new ServiceCollection()
                .AddSingleton(_ => new PushStore(NullLogger<PushStore>.Instance, path))
                .BuildServiceProvider())
            {
                var store = services.GetRequiredService<PushStore>();
                Assert.Null(store.Set("deployment", PushStore.ParseValue("ready"), null));
            }
            using var restarted = new PushStore(NullLogger<PushStore>.Instance, path);
            Assert.Equal("ready", restarted.Get("deployment")?.Data.GetString());
        }
        finally { directory.Delete(recursive: true); }
    }
}
