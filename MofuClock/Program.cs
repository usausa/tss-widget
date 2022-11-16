using MofuClock;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        if (context.HostingEnvironment.IsDevelopment())
        {
            logging.AddConsole();
            logging.AddDebug();
        }
    })
    .ConfigureServices((context, services) =>
    {
        var settings = context.Configuration.GetSection("Screen").Get<Settings>()!;
        services.AddSingleton(settings);

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync().ConfigureAwait(false);
