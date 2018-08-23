Tutorial
========================

Metrics
-------

Okanshi has a couple of different monitor types, divided into the following categories:

  * Gauges
  * Counters
  * Timers

All monitors can be instantiated directly or, declared and used through the static `OkanshiMonitor` class.

### Gauges ###

Gauges are monitors that returns the current value of something. It could be the number of files in a director, the number of users currently logged and etc.

#### Gauge ####

The `Gauge` is a monitor that takes a `Func<T>`. Each time the value is polled from the gauge, the `Func<T>` is called and the value returned is the current value.

Example:

```csharp
    OkanshiMonitor.Gauge("Number of users", () => _numberOfUsers);
    // OR
    var gauge = new Gauge(MonitorConfig.Build("Number of users"), () => _numberOfUsers);
```

#### Max/MinGauge ####

The `MaxGauge` is a monitor that tracks the current maximum value. It can be used to track the maximum number of users logged in at the same time or similar. The initial value of the gauge is zero.
The `MinGauge` is a monitor that tracks the current minimum value. The initial value of the gauge is zero, which means zero is treated as no value at all. This has the affect that if the gauge is zero, and a non zero value is posted to the gauge, it would effectively change the minimum value. This is a bug that will be fixed in a future version.

Example:

```csharp
    OkanshiMonitor.MaxGauge("Maximum number of users").Set(1); // New maximum is 1
    OkanshiMonitor.MaxGauge("Maximum number of users").Set(10); // New maximum is 10
    OkanshiMonitor.MaxGauge("Maximum number of users").Set(0); // Maximum is still 10
    // OR
    var gauge = new MaxGauge(MonitorConfig.Build("Maximum number of users"));
    gauge.Set(1);
    gauge.Set(10);
    gauge.Set(0);
    
    OkanshiMonitor.MinGauge("Minimum number of users").Set(1); // New minimum is 1
    OkanshiMonitor.MinGauge("Minimum number of users").Set(10); // Minimum is still 1
    OkanshiMonitor.MinGauge("Minimum number of users").Set(0); // Minimum is 0
    OkanshiMonitor.MinGauge("Minimum number of users").Set(1); // Minimum is 1
    // OR
    var gauge = new MinGauge(MonitorConfig.Build("Minimum number of users"));
    gauge.Set(1);
    gauge.Set(10);
    gauge.Set(0);
    gauge.Set(1);
```

#### AverageGauge ####

The `AverageGauge` monitors the average value over a time interval. This can be for example be used to monitor the average queue length over a time interval. The interval is controlled by the poller.

Example:

```csharp
    OkanshiMonitor.AverageGauge("Average queue length").Set(100); // Average is 100
    OkanshiMonitor.AverageGauge("Average queue length").Set(200); // Average is 150
    // OR
    var gauge = new AverageGauge(MonitorConfig.Build("Maximum number of users"));
    gauge.Set(100);
    gauge.Set(200);
```

#### Long/Double/DecimalGauge ####

The `LongGauge`, `DoubleGauge` and `DecimalGauge` are gauges that handles `long`, `double` and `decimal` values respectively. The value you set is the value you get. Usage of these monitors is the same.

```csharp
    OkanshiMonitor.LongGauge("Maximum number of users").Set(1); // New value is 1
    OkanshiMonitor.LongGauge("Maximum number of users").Set(10); // New value is 10
    OkanshiMonitor.LongGauge("Maximum number of users").Set(0); // New value is 0
    // OR
    var gauge = new LongGauge(MonitorConfig.Build("Maximum number of users"));
    gauge.Set(1);
    gauge.Set(10);
    gauge.Set(0);
```

### Counters ###

Counters are monitors that you can increment as needed. They are thread-safe by default.

#### Counter ####

A `Counter` counts the number of events between polling. The value is a ```int``` and can be incremented using a ```int```.

```csharp
    OkanshiMonitor.Counter("Name").Increment();
    OkanshiMonitor.Counter("Name").Increment(); // The value is 2
    OkanshiMonitor.Counter("Name").GetValuesAndReset();
    OkanshiMonitor.Counter("Name").Increment(); // The value is 1
    // OR
    var counter = new Counter(MonitorConfig.Build("Name"));
    counter.Increment();
    counter.Increment();
    counter.GetValuesAndReset();
    counter.Increment();
```

#### DoubleCounter ####

A `DoubleCounter` counts the number of events between polling. The value is a ```double``` and can be incremented using a ```double```.

```csharp
    OkanshiMonitor.DoubleCounter("Name").Increment();
    OkanshiMonitor.DoubleCounter("Name").Increment();  // The value is 2.0
    OkanshiMonitor.DoubleCounter("Name").GetValuesAndReset();
    OkanshiMonitor.DoubleCounter("Name").Increment(); // The value is 1.0
    // OR
    var counter = new DoubleCounter(MonitorConfig.Build("Name"), TimeSpan.FromSeconds(1));
    counter.Increment();
    counter.Increment();
    counter.GetValuesAndReset();
    counter.Increment();
```

#### CumulativeCounter ####

Is a counter that is never reset at runtime, but retained during the lifetime of the process. Other than that, it works exactly like all other counters. . The value is a ```int``` and can be incremented using a ```int```.

This counter make sense to use then you don't want to take the polling interval into account, and instead post-process the data.

```csharp
    OkanshiMonitor.CumulativeCounter("Name").Increment();
    OkanshiMonitor.CumulativeCounter("Name").Increment(); // The value is 2
    OkanshiMonitor.CumulativeCounter("Name").GetValuesAndReset();
    OkanshiMonitor.CumulativeCounter("Name").Increment(); // The value is 1
    // OR
    var counter = new CumulativeCounter(MonitorConfig.Build("Name"));
    counter.Increment();
    counter.Increment();
    counter.GetValuesAndReset();
    counter.Increment();
```

### Timers ###

Timers measures the time it takes to execute a function.

All timers also support "manual" timing, that are stopped manually instead of passing a Func<T> or Action.
Example:

```csharp
    var timer = OkanshiMonitor.Timer("Query time", TimeSpan.FromSeconds(1)).Start()
    Thread.Sleep(500);
    timer.Stop(); // When stopped the timing is registered
```

#### Timer ####

This is a simple timer that, within a specified interval, measures:

  * The execution time of a function,
  * Tracks the minimum and maximum time of the function call
  * The number of times the function was called
  * The total time of the all calls in the specified step
  * The rate of the calls (the number of calls per second)

Example:

```csharp
    OkanshiMonitor.Timer("Query time", TimeSpan.FromSeconds(1)).Record(() => Thread.Sleep(500)); // Min is 500, Max is 500, Count is 1, TotalTime is 500
    OkanshiMonitor.Timer("Query time", TimeSpan.FromSeconds(1)).Record(() => Thread.Sleep(100)); // Min is 100, Max is 500, Count is 2, TotalTime is 600
    // OR
    var timer = new Timer(MonitorConfig.Build("Query time", TimeSpan.FromSeconds(1)));
    timer.Record(() => Thread.Sleep(500));
    timer.Record(() => Thread.Sleep(100))
```

### Performance counters

As of version 4 it is also possible to monitor Windows performance counters.

```csharp
    OkanshiMonitor.PerformanceCounter(PerformanceCounterConfig.Build("Memory", "Available Bytes"), "AvailableBytes");

    // OR

    var performanceCounterMonitor = new PerformanceCounterMonitor(MonitorConfig.Build("Available Bytes"), PerformanceCounterConfig.Build("Memory", "Available Bytes"));
```

Health checks
-------------

You can also add different health checks to you application. For example the number of files in a directory or something else making sense in you case.
Health checks are added like this:

C#:

```csharp
    HealthChecks.Add("key", () => Directory.GetFiles("C:\\MyData").Length == 0);
```

F#:

```fsharp
    HealthChecks.Add("key", (fun () -> System.IO.Directory.GetFiles("C:\\MyData").Length = 0))
```

The `Func` passed in just have to return a boolean, indicating pass or fail.

As of version 4 health checks can also be registered as a monitor. This is done like this:

```csharp
    OkanshiMonitor.HealthCheck(() => Directory.GetFiles("C:\\MyData").Length == 0, "NoFilesInDirectory")
```

HTTP Endpoint
=============

Prior to version 4, the HTTP endpoint was include in the core package. This is no longer the case, it now exists in a separate package called Okanshi.Endpoint.

Starting the endpoint
---------------------

You start the monitor like this:

```csharp
    var endpoint = new MonitorEndpoint();
    endpoint.Start();
```

You should now be able to access the HTTP endpoint using [http://localhost:13004](http://localhost:13004).
As nothing has been monitored yet, it will return a JSON response with an empty object, like this:

    {}

For custom configuration of the endpoint see the API reference.

Health checks
-------------

To see the current status of all defined healthchecks, go to [http://localhost:13004/healthchecks](http://localhost:13004/healthchecks).

Assembly dependencies
---------------------

The endpoint can show all assemblies currently loaded in the AppDomain.

To see all assembly dependencies for you application just access [http://localhost:13004/dependencies](http://localhost:13004/dependencies). It will provide a list of the names and version of all dependencies.

NuGet dependencies
------------------

If the package.config file is available in the current directory of the process. The endpoint can show all NuGet dependencies and their version. This information can be accessed through [http://localhost:13004/packages](http://localhost:13004/packages).

Observers
=========

Observers are used to store Okanshi metrics, this can be in-memory or by sending it to a database. An observer storing metrics in-memory is included in the Okanshi package.

An observer to send metrics to InfluxDB is provided through another NuGet package called, Okanshi.InfluxDBObserver.

Observer setup
--------------

Setting up an observer is easy:

```csharp
    var observer = new MemoryMetricObserver(new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, pollingInterval, collectMetricsOnProcessExit), numberOfSamplesToStore)
```

This observer stores metrics in-memory using a poller getting data from the default monitor registry.

you can create your own observers easily as shown in the following example that not only prints to the screen but does some processing first

```csharp
    class MyObserver : IMetricObserver
    {
        public MyObserver(IMetricPoller poller)
        {
            poller.RegisterObserver(Update);
        }

        public async Task Update(IEnumerable<Metric> metrics)
        {
            var msg = JsonConvert.SerializeToJson(metrics);
            Console.WriteLine($"sending info to MyLegacySystem<tm> '{msg}'");
        }

        public void Dispose()
        { }
    }
```

To set up this particular observer we can use the following code. It sets up some business logic that is timed.

```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var poller = new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, interval: TimeSpan.FromSeconds(2));
            var observer = new MyObserver(poller);

            BusinessLogic();
        }

        private static void BusinessLogic()
        {
            while (true)
            {
                OkanshiMonitor.Timer("send").Record(() =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));  // simulate business stuff...
                });
            }
        }
    }
```

OWIN
====

Using the package Okanshi.Owin it is possible to measure the request duration grouped by path, HTTP method and optionally the response status code.

To enable use the `AppBuilder` extension method, `UseOkanshi`:

```csharp
    app.UseOkanshi()
```

For configuration see the API reference.

Currently the OWIN integration always uses the default registry.
