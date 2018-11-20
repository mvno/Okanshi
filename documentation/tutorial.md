# ![Logo](okanshi_logo_small.png) Okanshi Tutorial

This part of the documentation describes the overall functionality you'll find in Okanshi. It will not cover "best practices", limitations of Okanshi, or how to get started using Okanshi.

<p>

Table of Content

   * [1. Introduction](#1-introduction)
     * [1.1. Metrics and performance measurement](#11-metrics-and-performance-measurement)
     * [1.2. Names and tags](#12-names-and-tags)
     * [1.3. Getting started](#13-getting-started)
     * [1.4. Instantiating monitors](#14-instantiating-monitors)
       * [1.4.1. OkanshiMonitor](#141-okanshimonitor)
       * [1.4.2. Factories](#142-factories)
   * [2. Monitor documentation](#2-monitor-documentation)
     * [2.1. Gauges](#21-gauges)
       * [2.1.1. Gauge](#211-gauge)
       * [2.1.2. Max/MinGauge](#212-maxmingauge)
       * [2.1.3. AverageGauge](#213-averagegauge)
       * [2.1.4. MinMaxAvgGauge](#214-minmaxavggauge)
       * [2.1.5. Long/Double/DecimalGauge](#215-longdoubledecimalgauge)
     * [2.2. Counters](#22-counters)
       * [2.2.1. Counter](#221-counter)
       * [2.2.2. DoubleCounter](#222-doublecounter)
       * [2.2.3. CumulativeCounter](#223-cumulativecounter)
     * [2.3. Timers](#23-timers)
       * [2.3.1. Timer](#231-timer)
       * [2.3.2. SlaTimer](#232-slatimer)
     * [2.4. Performance counters](#24-performance-counters)
   * [3. Filters](#3-filters)
   * [4. Health checks](#4-health-checks)
   * [5. HTTP Endpoint](#5-http-endpoint)
     * [5.1. Starting the endpoint](#51-starting-the-endpoint)
     * [5.2. Health checks](#52-health-checks)
     * [5.3. Assembly dependencies](#53-assembly-dependencies)
     * [5.4. NuGet dependencies](#54-nuget-dependencies)
   * [6. Observers](#6-observers)
     * [6.1. Observer setup](#61-observer-setup)
     * [6.2. ConsoleObserver](#62-consoleobserver)
     * [6.3. Custom observers](#63-custom-observers)
   * [7. OWIN](#7-owin)
 

## 1. Introduction

### 1.1. Metrics and performance measurement

Okanshi has a couple of different monitor types, divided into the following categories:

  * Gauges
  * Counters
  * Timers

Where a "gauge" is something that returns its most present value, a "counter" is something you increment and a "timer", is the time taken for some chunk of code to execute. At its heart, what Okanshi does, is to aggregate data around these three concepts. Data is aggregated in order for the information load to be manageable both in terms of collection, transmission to a central storage and the following indexing.

The three monitors represents the fundamentals to monitoring. And thus it is not uncommon to see these monitors combined into larger and more complex monitors. In fact, Okanshi is shipping such monitors itself. Why do we need to combine monitors? Remember that values from Okanshi are aggregated values. A measurement from a monitor may stem from possibly thousands of measurements. And so, it may be useful to operate on slightly more than just a single aggregated value. 

An example from the trenches: You have a service level agreement to respond to request within 500 ms, and with Okanshi you measure the average time taken to be 400 ms. Are you in the clear? How many times could you have response times above 500 ms and still have an average of 400 ms? The answer is that you can't tell using the simples timer monitor provided in Okanshi. But you can turn to more advanced timers to help you out.

At other times, the simplest readings more than suffice. It depends on a lot of factors. On factor being that your average response time is 10 ms and you maximum response time is 50 ms. Another factor may be that you do not have an explicit SLA with your online users, but you'd like to track long term trends of the user experience and identify releases that particular hurt performance. Do you want automated alarming? Identify critical situations where something is about to go wrong or has gone wrong without the service has crashed? 

We consider the art of measuring a fine art. The relevancy of measurements depends on how what you want to do with your results. Gaining an understanding of what you want to do with you data is often an interactive process:

  1. You measure data, 
  2. evaluate the data and its uses, 
  3. figure out you need to cover new situations or is missing data, 
  4. you extend the monitoring 
  5. goto 1.
  
Also you'll find not all code and not all services needs be treated the same. They have different characteristics, risk profiles etc. that make it relevant to do different kinds of measurements. Hence the iterative process stated above. 

Starting with Okanshi, it's perfectly fine if you are unsure on how exactly you want to measure. Likewise, it is just as OK to have a very thorough understanding of what you want to measure, only to get a lot smarter once you see the results. 


### 1.2. Names and tags

An Okanshi measurement come with both a *name* and a set of *tags*, both of which play a role in distinguishing one measurement from another.
Think of the name of a measurement as the overall distinguishing component, a key if you will. And think of a tag like an *optional* subdivision of that measurement that enable you to "slice and dice" you data.

For example, take the measurement named "Request count" that is a counter counting the number of requests our system receives. We can sum our measurements for a time period and know our load. We can then use the tag "servername" to group our measurements so we know about the distribution of load across our cluster. We can then further split our data using the tag "endpoint path" to see within each server what end points are the most active. 

Recall that we said we can "slide and dice" data with tags. For example, we can choose to only see the request count pr. endpoint to get an overall overview of our end point traffic disregarding the actual machines they hit. Another useful tag could be an "environment" tag describing whether you are in test, pre-production, production, ...

Had we instead incorporated server name and end point names in the measurement name, we would not be able to do the same kind of dynamic grouping of data.

These were just two examples of how you can utilize names and tags. What tags you best fit your situation is difficult to say, but we can provide a list of inspiration for general useful tags. 

* **Computer name:** The name/id of the computer making measurement can identify if a particular machine or hardware is experiencing problems.
* **Environment name:** For example "test" and "production" - enable you to see how things are going in various environments.
* **Request path:** In a web-service context the request path enable you to summarize either on individual end point or globally
* **Request method:** In a web-service context, e.g. GET or POST
* **Unit of measure:** When you cannot determine from the measurement name the unit of measure, and its not a standard unit (e.g. days or seconds) consider applying it as a tag.


### 1.3. Getting started

By now, you are probably excited to see something running. To get an understanding of the "wheels and cogs" of Okanshi, run the following illuminating example. Simply compile and run the in a console:


```csharp
    static void Main(string[] args)
    {
        var registry = DefaultMonitorRegistry.Instance;
        var poller = new MetricMonitorRegistryPoller(registry, TimeSpan.FromSeconds(5), false);
        new ConsoleObserver(poller, x => JsonConvert.SerializeObject(x, Formatting.Indented));
        
        while (true)
        {
            OkanshiMonitor.Counter("hello world").Increment();
        }
    }
```

The main entities of the program is a *registry* which holds a set of monitors. A *poller* which with certain intervals will query the measurements of the monitors, and an *observer* that typically transports measurements to a receiver system that can do data analysis, draw graphs etc. In this case we print to the console to visually show that we are up and running. 

The output when running this program resembles the following:


```
** 06/11/2018 16.56.01
{
  "Name": "test",
  "Timestamp": "2018-11-06T15:56:01.1543922+00:00",
  "Tags": [],
  "Values": [
    {
      "Name": "value",
      "Value": 13275355
    }
  ]
}
```

You'll notice that the `Values.Value` oscillates around the same number rather than growing. This is because every time Okanshi reads the monitors in the poller, the monitor is reset.




### 1.4. Instantiating monitors

#### 1.4.1. OkanshiMonitor

Advantages
  * very easy to get started

#### 1.4.2. Factories

Advantages
  * multiple polling intervals
  * extensionmethods to enhance the factory with your own monitors

  
Plumbing code for using the factories

```
    public static class OkanshiIntegrator
    {
        public static MonitorFactory CreateFactory(TimeSpan pollFrequency, IEnumerable<Tag> defaultTags, bool pollOnExit = false)
        {
            defaultTags = defaultTags ?? new Tag[0];
            var registry = new OkanshiMonitorRegistry();
            new MyObserver(new MetricMonitorRegistryPoller(registry, pollFrequency, pollOnExit));
            var factory = new MonitorFactory(registry, defaultTags);
            return factory;
        }

        public static ZeroFilterFactory CreateZeroFactory(TimeSpan pollFrequency, IEnumerable<Tag> defaultTags, bool pollOnExit = false)
        {
            defaultTags = defaultTags ?? new Tag[0];
            var registry = new OkanshiMonitorRegistry();
            new MyObserver(new MetricMonitorRegistryPoller(registry, pollFrequency, pollOnExit));
            var factory = new ZeroFilterFactory(registry, defaultTags);
            return factory;
        }
    }
```


## 2. Monitor documentation

### 2.1. Gauges

Gauges are monitors that returns the current value of something. It could be the number of files in a director, the number of users currently logged and etc.

All gauges can be instantiated directly or, declared and used through the static `OkanshiMonitor` class.

#### 2.1.1. Gauge 

The `Gauge` is a monitor that takes a `Func<T>`. Each time the value is polled from the gauge, the `Func<T>` is called and the value returned is the current value.

Example:

```csharp
    OkanshiMonitor.Gauge("Number of users", () => _numberOfUsers);
    // OR
    var gauge = new Gauge(MonitorConfig.Build("Number of users"), () => _numberOfUsers);
```

#### 2.1.2. Max/MinGauge 

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

#### 2.1.3. AverageGauge 

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


#### 2.1.4. MinMaxAvgGauge

The `MinMaxAvgGauge` is a combination of three gauges: MinGauge, MaxGauge and AverageGauge. It keeps track of the min, max and average values since last reset.  This gauge is able to detect extreme values that would otherwise disappear in an average calculation. When you are in need of a gauge and you are unsure about what data you are going to get out, this gauge may be the gauge to use.
 
An example from the trenches. As part of a performance measurement we found a suspicious function that looked performance expensive. Initial averages shoved that it contributed only very few milliseconds to the processing time. But almost by accident, we found that sometimes the input payload rose from 1k-3k to 500k, and the execution time likewise rose to seconds! Those spikes would have been averaged away with the `AverageGauge`.
 
Values returned are "min", "max" and "avg"

```csharp
    OkanshiMonitor.MinMaxAvgGauge("Payload").Set(300);
    OkanshiMonitor.MinMaxAvgGauge.Set(900);
    OkanshiMonitor.MinMaxAvgGauge.Set(1200);
    // OR
    var gauge = new MinMaxAvgGauge(MonitorConfig.Build("Maximum number of users"));
    gauge.Set(300);
    gauge.Set(900);
    gauge.Set(1200);
     
    // RESULT is "min:300", "max:1200", "avg:800"
```


#### 2.1.5. Long/Double/DecimalGauge 

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

### 2.2. Counters 

Counters are monitors that you can increment as needed. They are thread-safe by default.

All counters can be instantiated directly or, declared and used through the static `OkanshiMonitor` class.

Counters may be incremented with negative values. For the most part, this makes little sense. If you find yourself wanting to use negative increments, chances are you
should be using a gauge instead. In fact, we cannot think of a single example where this is not the case. :-)

#### 2.2.1. Counter 

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

#### 2.2.2. DoubleCounter 

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

#### 2.2.3. CumulativeCounter 

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

### 2.3. Timers 

Timers measures the time it takes to execute a function.

All timers can be instantiated directly or, declared and used through the static `OkanshiMonitor` class.

All timers also support "manual" timing, that are stopped manually instead of passing a Func<T> or Action.
Example:

```csharp
    var timer = OkanshiMonitor.Timer("Query time", TimeSpan.FromSeconds(1)).Start()
    Thread.Sleep(500);
    timer.Stop(); // When stopped the timing is registered
```

#### 2.3.1. Timer 

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


#### 2.3.2. SlaTimer

A service-level agreement (SLA) is a commitment between a service provider and a client. For example, the service provider promise to respond to a request within an agreed amount of time. 

A SLA-Timer is a special timer, that keeps track of your SLA's and whether they are honored. The SLA-Timer is different than a timer in that it measures strictly against the SLA, whereas the Timer operate on averages. If your performance characteristics are such that you are always doing very good or very bad, a normal timer can be used instead of the SLA-timer, since the average will suffice.

The timer implements two timers one for registrations below the SLA and one above. Each timer provides the following data "average", "total time", "count", "min" and "max". We keep track of both executions below the SLA and above. The reason is, when things are going bad we want to know how bad we are doing. By tracking timings below our SLA we can see if we get dangerously close to our SLA, it also 
enable us to better understand the periods where we break our SLA by knowing how "business as usual" looks like.



### 2.4. Performance counters

As of version 4 it is also possible to monitor Windows performance counters.

```csharp
    OkanshiMonitor.PerformanceCounter(PerformanceCounterConfig.Build("Memory", "Available Bytes"), "AvailableBytes");

    // OR

    var performanceCounterMonitor = new PerformanceCounterMonitor(MonitorConfig.Build("Available Bytes"), PerformanceCounterConfig.Build("Memory", "Available Bytes"));
```


## 3. Filters

Filters wrap monitors such that in the absence of registrations on the monitor, the monitor will not send zero-values when polled. This is useful when you want to reduce the amount of data sent to the receiving system. There can be may reasons for that such as licensing cost, or to reduce the load
on the receiver. Or perhaps you are using a receiving system, where data is stored in a less efficient manner, making you concerned about too much data "with no data in it". 

Using Splunk as the receiving system, you may face many of those reasons, and much less so when using e.g. InfluxDb.

The filter implementations are `CounterZeroFilter`, `GaugeZeroFilter` and `TimerZeroFilter`.


Example:

```csharp
    var counter = new CounterZeroFilter<long>(new Counter(MonitorConfig.Build("Test")));
    counter.GetValues(); // no values are returned
    //
    counter.Increment()
    counter.GetValues(); // value is returned
```

And then we need to register our filter counter.. So a more complete example of a factory method may be

```csharp
    private static CounterZeroFilter<long> FilterCounter(string name, IEnumerable<Tag> tags)
    {
        var config = MonitorConfig.Build(name).WithTags(tags);
        var registry = DefaultMonitorRegistry.Instance;
        var monitor = registry.GetOrAdd(config, x => new CounterZeroFilter<long>(new Counter(x)));
        return monitor;
    }
```

Filters are currently not supported by `OkanshiMonitor`. The following wILL NOT WORK

```csharp
    // don't do this!
    var wrong! = new CounterZeroFilter<long>(OkanshiMonitor.Counter("Foo"));
```

When we use `OkanshiMonitor`, it registers the monitor in a default registry associated with a poller! Hence we will start sending 0-values.


## 4. Health checks

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

As of version 6 health checks can no longer be registered as a monitor.



## 5. HTTP Endpoint

Prior to version 4, the HTTP endpoint was include in the core package. This is no longer the case, it now exists in a separate package called Okanshi.Endpoint.

### 5.1. Starting the endpoint

You start the monitor like this:

```csharp
    var endpoint = new MonitorEndpoint();
    endpoint.Start();
```

You should now be able to access the HTTP endpoint using [http://localhost:13004](http://localhost:13004).
As nothing has been monitored yet, it will return a JSON response with an empty object, like this:

    {}

For custom configuration of the endpoint see the API reference.

Notice that at application startup, the enpoint contains *no data* the first minute. This is because no data have been gathered yet. You can control that period by tweaking the poller interval.

### 5.2. Health checks

To see the current status of all defined healthchecks, go to [http://localhost:13004/healthchecks](http://localhost:13004/healthchecks).

### 5.3. Assembly dependencies

The endpoint can show all assemblies currently loaded in the AppDomain.

To see all assembly dependencies for you application just access [http://localhost:13004/dependencies](http://localhost:13004/dependencies). It will provide a list of the names and version of all dependencies.

### 5.4. NuGet dependencies

If the package.config file is available in the current directory of the process. The endpoint can show all NuGet dependencies and their version. This information can be accessed through [http://localhost:13004/packages](http://localhost:13004/packages).



## 6. Observers

Observers are used to store Okanshi metrics, this can be in-memory or by sending it to a database. An observer storing metrics in-memory is included in the Okanshi package.

An observer to send metrics to InfluxDB is provided through another NuGet package called, Okanshi.InfluxDBObserver.

### 6.1. Observer setup

Setting up an observer is easy:

```csharp
    var observer = new MemoryMetricObserver(new MetricMonitorRegistryPoller(DefaultMonitorRegistry.Instance, pollingInterval, collectMetricsOnProcessExit), numberOfSamplesToStore)
```

This observer stores metrics in-memory using a poller getting data from the default monitor registry.


### 6.2. ConsoleObserver

The `ConsoleObserver` prints measurements to the console, so you can get a quick start seeing that Okanshi works. No need to mess about with a receiver system. 

```csharp
    static void Main(string[] args)
    {
        var pollingInterval = TimeSpan.FromSeconds(5);
        var collectOnExit = true;
        var registry = DefaultMonitorRegistry.Instance;
        var poller = new MetricMonitorRegistryPoller(registry, pollingInterval, collectOnExit);
        new ConsoleObserver(poller, x => JsonConvert.SerializeObject(x, Formatting.Indented));

        while (true)
        {
            OkanshiMonitor.Counter("test").Increment();
        }
    }
```


### 6.3. Custom observers

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
            var msg = JsonConvert.SerializeObject(metrics);
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


## 7. OWIN

Using the package Okanshi.Owin it is possible to measure the request duration grouped by path, HTTP method and optionally the response status code.

To enable use the `AppBuilder` extension method, `UseOkanshi`:

```csharp
    app.UseOkanshi()
```

For configuration see the API reference.

Currently the OWIN integration always uses the default registry.


(document sections maintained by https://github.com/kbilsted/AutonumberMarkdown)
