using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using AvaloniaSample.Services;
using AvaloniaSample.ViewModels;
using AvaloniaSample.Views;
using Lemon.Hosting.AvaloniauiDesktop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Serilog;

namespace AvaloniaSample
{
    internal sealed class Program
    {
        [STAThread]
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        [RequiresDynamicCode("Calls Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder()")]
        public static int Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: "serilog.json", optional: false, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            try
            {
                var hostBuilder = Host.CreateApplicationBuilder();

                // config IConfiguration
                hostBuilder.Configuration
                    .AddCommandLine(args)
                    .AddEnvironmentVariables()
                    .AddInMemoryCollection();

                // config ILogger
                hostBuilder.Services.AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddSerilog();
                });
                // add some services
                hostBuilder.Services.AddSingleton<ISomeService, SomeService>();

                #region app default (support aot)

                //RunAppDefault(hostBuilder, args);

                #endregion

                #region app without mainwindow (support aot with adding <Assembly Name="AvaloniaSample" Dynamic="Required All"/> to rd.xml)

                //RunAppWithoutMainWindow(hostBuilder, args);

                #endregion

                #region app with serviceprovider (support aot)

                RunAppWithServiceProvider(hostBuilder, args);

                #endregion

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static AppBuilder ConfigAvaloniaAppBuilder(AppBuilder appBuilder)
        {
            return appBuilder
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RunAppDefault(HostApplicationBuilder hostBuilder, string[] args)
        {
            hostBuilder.Services.AddAvaloniauiDesktopApplication<AppDefault>(ConfigAvaloniaAppBuilder);
            // build host
            var appHost = hostBuilder.Build();
            // run app
            appHost.RunAvaloniauiApplication(args);
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RunAppWithoutMainWindow(HostApplicationBuilder hostBuilder, string[] args)
        {
            // add avaloniaui application and config AppBuilder
            hostBuilder.Services.AddAvaloniauiDesktopApplication<AppWithoutMainWindow>(ConfigAvaloniaAppBuilder);
            // add MainWindow & MainWindowViewModelWithParams
            hostBuilder.Services.AddMainWindow<MainWindow, MainWindowViewModelWithParams>();
            // build host
            var appHost = hostBuilder.Build();
            // run app
            appHost.RunAvaloniauiApplication<MainWindow>(args);
        }

        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void RunAppWithServiceProvider(HostApplicationBuilder hostBuilder, string[] args)
        {
            // add avaloniaui application and config AppBuilder
            hostBuilder.Services.AddAvaloniauiDesktopApplication<AppWithServiceProvider>(ConfigAvaloniaAppBuilder);
            // add MainWindowViewModelWithParams
            hostBuilder.Services.AddSingleton<MainWindowViewModelWithParams>();
            // build host
            var appHost = hostBuilder.Build();
            // run app
            appHost.RunAvaloniauiApplication(args);
        }
    }
}