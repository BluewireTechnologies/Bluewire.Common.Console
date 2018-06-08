# Design notes

## Console application

Basic process:
1. Create session object.
2. Add arguments and state.
3. Run: parse arguments into state and run the app.

Example code:
```
var app = new MyApp();
var session = new ConsoleSession();
// Parse command-line switches into app, if it implements IReceiveOptions.
session.Options.AddCollector(app);
// Parse positional command-line arguments into app, if it implements IReceiveArgumentList.
session.ArgumentList.AddCollector(app);
// Configure a logging policy using default command-line switches.
var logging = session.Options.AddCollector(new SimpleConsoleLoggingPolicy());
// Warn if 'app' doesn't claim all the positional arguments.
session.ExcessPositionalArgumentsPolicy = ExcessPositionalArgumentsPolicy.Warn;

return session.Run(args, async () => {
    // Set up logging.
    using (LoggingPolicy.Register(session, logging))
    {
        await app.Run();
        return 0;
    }
});
```

### Logging conventions

* STDOUT is for output, ideally machine-readable.
* STDERR is for human-readable logging, progress messages, etc.
* Log files generally aren't necessary and should not be created unless actually written to.
* There is no configuration file, therefore logging rules should be as simple as possible.

## Daemon application

Basic process:
1. Define a factory for the daemon.
2. Run it.

Example code:
```
// In Main:
DaemonRunner.Run(args, new ServiceDaemonisable());

// Daemon definition:
class ServiceDaemonisable : IDaemonisable, IReceiveOptions, IReceiveArgumentList
{
    public string Name => "MyDaemon";
    public string[] GetDependencies() => new string[0];

    public async Task<IDaemon> Start(CancellationToken token)
    {
        var daemon = new Daemon();
        try
        {
            // Do async initialisation here.
            // You can track instances for automatic disposal, for example:
            var service = daemon.Track(new MyBackgroundService());
            service.StartDoingThings();

            // The caller will dispose this instance when the daemon shuts down.
            return daemon;
        }
        catch (Exception ex)
        {
            // Consider logging the exception!
            // log.Error(ex);
            // Make sure that anything set up so far is disposed if an error occurs.
            daemon.Dispose();
            throw;
        }
    }

    void IReceiveOptions.ReceiveFrom(OptionSet options)
    {
        // Set up options here.
    }

    void IReceiveArgumentList.ReceiveFrom(ArgumentList argumentList);
    {
        // Set up positional arguments here.
    }

    class Daemon : DaemonBase
    {
        protected override void Dispose(bool disposing)
        {
            // Shut down the daemon.
            ...
            // Call the base to dispose all tracked instances...
            base.Dispose(disposing);
            // ... or do it explicitly:
            // CleanUpTrackedInstances();
        }
    }
}
```

### Environments:

* ApplicationEnvironment: Running as a console or Windows Forms application. Potentially interactive and STDOUT/STDERR are available.
* ServiceEnvironment: Running as a Windows service. STDOUT and STDERR are unavailable.
* HostedEnvironment: Running within another application's process, but the environment has not yet been properly initialised.
* InitialisedHostedEnvironment: Running within another application's process and the environment is ready to host us.

### Logging conventions

* In ApplicationEnvironment, STDOUT and STDERR should point to the console.
* In any other environment, STDOUT and STDERR need to be directed to log files, but only if written to.
* Log messages should be written to files, and the logging rules may reside in a configuration file.

### Configuration

By default, all logs are written to a `logs` directory in the application directory. This may be overridden using appSettings
configuration keys:

* `<Application.Name>:LogDirectory` is consulted first.
* `Bluewire.Common.Console:LogDirectory` is used as a fallback.

These are both treated as directory paths, resolved relative to the application directory if not rooted. Beware that the empty
string is significant and means 'the application directory'.

For an application called MyApp.Service, the following has the same effect as the default behaviour:
```
<configuration>
  <appSettings>
    <add key="MyApp.Service:LogDirectory" value="logs" />
    <add key="Bluewire.Common.Console:LogDirectory" value="logs" />
  </appSettings>
</configuration>
```
Changing the second key would have no effect, because the more-specific `MyApp.Service:LogDirectory` takes precedence.

The console logs are placed in the root of the log directory by default but may be configured using
`<Application.Name>:ConsoleLogDirectory` and `Bluewire.Common.Console:ConsoleLogDirectory` similarly. These paths are treated as
relative to `LogDirectory`, so the empty string may be considered their default value.

#### Configuring Loggers

The default logging library is log4net. The log directory configured as above is provided as a `LogDirectory` property and will
always have a trailing directory separator.

For example:
```
<configuration>
  <log4net>
    <root>
      <level value="ERROR" />
      <appender type="log4net.Appender.FileAppender">
        <file type="log4net.Util.PatternString" value="%property{LogDirectory}service" />
      </appender>
    </root>
    <logger name="MyApp.Service.Requests" additivity="false">
      <level value="INFO" />
      <appender type="log4net.Appender.FileAppender">
        <file type="log4net.Util.PatternString" value="L:\SharedLogs\MyApp\Usage\requests" />
        <layout type="log4net.Layout.PatternLayout">
          <conversionPattern value="%date %-5level - %property{RequestType} - %message%newline" />
        </layout>
      </appender>
    </logger>
    <logger name="MyApp.Service.Database" additivity="false">
      <level value="WARN" />
      <appender type="log4net.Appender.FileAppender">
        <file type="log4net.Util.PatternString" value="%property{LogDirectory}database" />
      </appender>
    </logger>
  </log4net>
</configuration>
```
