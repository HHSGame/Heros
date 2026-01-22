using HHSGame.Core.Engine;

using Serilog;
using System.Globalization;

Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File("hss.log", formatProvider: CultureInfo.CurrentCulture)
                .CreateLogger();
GameEngineLauncher.Run(
    args,
    loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
