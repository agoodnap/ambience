using Ambience.Commands;
using Ambience.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal class Program
{
    private CancellationTokenSource? _cts;
    private IConfigurationRoot? _config;
    private DiscordClient? _discord;

    static async Task Main(string[] args) => await new Program().InitBot(args);

    async Task InitBot(string[] args)
    {
        try
        {
            _cts = new CancellationTokenSource();

            Console.WriteLine("[info] Loading configurations..");
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                .AddJsonFile("ambiences.json", optional: false, reloadOnChange: true)
                .Build();

            Console.WriteLine("[info] Creating discord client..");
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = _config.GetValue<string>("discord:token"),
                TokenType = TokenType.Bot
            });

            var endpoint = new ConnectionEndpoint
            {
                Hostname = _config.GetValue<string>("lavalink:host"),
                Port = _config.GetValue<int>("lavalink:port")
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = _config.GetValue<string>("lavalink:password"),
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            var voiceNext = _discord.UseVoiceNext();
            var lavalink = _discord.UseLavalink();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddOptions<AmbienceServiceOptions>();

            var services = serviceCollection
                .AddSingleton<AmbienceService>()
                .Configure<AmbienceServiceOptions>(options =>
                {
                    var children = _config.GetSection("ambiences").GetChildren();
                    var ambiences = new List<Ambience.Domain.Ambience>();
                    foreach (var child in children)
                    {
                        var name = child.GetValue<string>("Name");
                        var links = child.GetSection("Links").GetChildren().Select(x => x.Value);
                        ambiences.Add(new Ambience.Domain.Ambience(name, links.ToArray()));
                    }
                    options.AmbienceList = ambiences.ToArray();
                })
                .BuildServiceProvider();

            var commands = _discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { _config.GetValue<string>("discord:prefix") },
                Services = services
            });

            commands.RegisterCommands<MusicCommands>();

            Console.WriteLine("Connecting..");
            await _discord.ConnectAsync();
            var link = await lavalink.ConnectAsync(lavalinkConfig);
            Console.WriteLine("Connected!");

            while (!_cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromMinutes(1));
        }
        catch (Exception ex)
        {
#if Debug
            Console.Error.WriteLine(ex.ToString());
#else
            Console.Error.WriteLine(ex.Message);
#endif
        }
    }
}