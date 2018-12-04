### New in 6.1.0 (Unreleased)

* `Okanshi.Owin` now has an option for which timer to use when monitoring.
* `SlaTimer` has been added. The timer makes it easy to track SLA-breaches.
* `CounterAbsentFilter`, `GaugeAbsentFilter` and `TimerAbsentFilter` has been added. They wrap monitors and only returns values to the poller when changes have been registered.
* `MinMaxAvgGauge` has been added. It allows you to detect extreme values that would otherwise disappear in an average calculation.
* `OkanshiMonitor.DefaultStep` has been deleted since it was forgotten in an earlier clean up process.
* `OkanshiMonitor.PerformanceCounter` bug fix so it now returns an instance.
* Unnecessary calculation has been removed from `Counter`, and locks are no longer used in the counter, instead atomics are used.
* `Counter` now allows increment by negative numbers.
* `ConsoleObserver` has been added to make it easier to get started with Okanshi.
* `OkanshiMonitor.WithAbsentFiltering` has been added. This allows you to instantiate zero filter monitors.
* `MonitorFactory` has been added. It allows you to create multiple monitors with different polling frequencies.
* `AbsentMeasurementsFilterFactory` has been added. It allows you to instantiate absent filter monitors with different polling frequencies.
* `WebApi` middleware has been added. Allows you to do black box monitoring of your WebApi applications.
* `Okanshi.SplunkObserver` has been added. Post Okanshi measurements to Splunk using HTTP.

**BREAKING CHANGES**

* Make IMonitorRegistry generic. This fixes a bug where behaviour is different depending on if the registry is stored as IMonitorRegistry or OkanshiMonitorRegistry
* Instead of Okanshi.Endpoint depending on Newtonsoft.Json to do the JSON serialization, it now accepts a `Func` doing the serialization, which makes it possible for the user to control dependencies and serialization
* `Okanshi.Endpoint` nows takes a poller to be able to easily avoid problems with multiple pollers on the default registry. This also removes `PollingInterval` and `CollectMetricsOnProcessExit` from `EndpointOptions`.

### 6.0.0

The v6.0.0 release has done a major overhaul in the naming of monitors to make it simpler for new users of the API. 

**BREAKING CHANGES**

* `Timer.Register(long)` has been removed. Replace use with `Timer.Register(StopWatch)` or `Timer.Register(TimeSpan)`
* `OkanshiMonitor.HealthCheck()` has been removed. Replace use with a `Gauge`
* `LongTaskTimer` has been removed
* `BasicCounter` has been renamed to `CumulativeCounter` to better explain what it does
* `PeakRateCounter` has been renamed to `Counter`, as it was hard to figure out what PeakRateCounter meant
* `BasicTimer` has been renamed to `Timer`, as it was hard to figure out what BasicTimer meant
* `BasicGauge` has been renamed to `Gauge`, as it was hard to figure out what BasicGauge meant
* `DataPoint` has been deleted as it was no longer used internally
* `DataSourceType` has been removed, instead descriptive names should be used
* The tag statistic has been removed from the Timer, this has been replaced by value names
* `OkanshiMonitor.DefaultTags` has been change to an `ISet<Tag>` to make it clear how duplicates are handled
* `InfluxDB.WriteOnly` dependency has been updated to version 3.1.0

### 6.0.0-alpha2

**BREAKING CHANGES**

* Drop support for submonitors, instead return multiple values. This fixes a race condition caused by treating submonitors as individual monitors.

### 6.0.0-alpha

* Add PollMetrics to IMetricPoller interface, this was not added to the interface by mistake
* Support for waiting for polling and observer sending to complete, this is done by returning Task from observer actions and the pollers PollMetrics method.

### 5.0.0

All fixes in the beta releases, see below

### 5.0.0-beta9

* Fix bug in exception handling in InfluxDBObserver

### 5.0.0-beta8

* Log when sendings metrics to InfluxDB fails

### 5.0.0-beta7

* The method Register on the interface Registry has been removed
* SystemClock and ManualClock has been removed, as this is no longer need after 5.0.0-beta2
* Logging class added

### 5.0.0-beta6

* Fix bug in timer which could make the average zero in cases where the submonitors were read before the average

### 5.0.0-beta5

* Fix bug in average gauge which made the average incorrect

### 5.0.0-beta4

* Add support for override the field types in the InfluxDB observer

### 5.0.0-beta3

* Fix bug in the InfluxDB observer where it couldn't handle 64-bit integers correctly

### 5.0.0-beta2

**BREAKING CHANGES**
* Introduce submonitors to allowing grouping of monitors. This means that submonitors no longer should be registered in the registry, instead submonitors should be returned by ```GetAllMonitors()```. This allows more control in the observers to decide how to process the information. Currently the InfluxDbObserver doesn't support submonitors with submonitors.

### 5.0.0-beta

**BREAKING CHANGES**
* Added GetOrAdd to Registry interface, this enables the caching of monitors to only occur in the registry. The Register have also been made obsolete.

### 4.0.4

* Fix problem where locks weren't released correctly, causing deadlocks.

### 4.0.3

* Update InfluxDB.WriteOnly package, to fix a problem were int64 was not supported.

### 4.0.2

* Fixed bug where disposing the InfluxDB observer, would cause the underlying poller to be stopped.

### 4.0.1

* Fixed bug in the InfluxDB observer, causing all tags converted to fields to only allow floats.

### 4.0.0

* Automatic polling on metrics on process exit, now also supports AppDomain unloads
* Fix dependencies on NuGet package
* Implemented InfluxDB observer for pushing metrics to InfluxDB. It is released in a separate package, Okanshi.InfluxDBObserver.
* Add new NuGet package with support for OWIN.
* Added support for registering a custom timing by providing a number describing the elapsed number of milliseconds.
* Improved performance when using OkanshiMonitor to get monitors
* Ensured thread safety in metric types by using locks where needed
* Added new gauge, AverageGauge, for calculating average value for a specific interval

### 4.0.0-beta

**BREAKING CHANGES**

* Renames DurationTimer to LongTaskTimer
* BasicTimer now has both rate and count. This means the old count value (which was a rate), now is a counter
* Make totalTime value in BasicTimer as counter instead of a rate

**Other**

* Fix bug when multiple tags where used
* Add support for using HealthChecks as a monitor
* Support for monitoring Windows performance counters
* Support for automatically poll of metrics when process is about to exit, the default behaviour is not to poll
* Support for manual timing of operations, using the Start() method on the existing timers, which returns an object that when stopped registers timing to the timer.
* Improved performance of timers and counters using time windows by ~75%
* Fix bug where the PeakRateCounter would not respect maximum value when decremented
* Added Reset method to gauges as resetting gauges was described in the documentation, but not possible

### 3.1.0
Allow setting custom tags on metrics generated through OkanshiMonitor.
For example:

```csharp
OkanshiMonitor.BasicTimer("Test", new[] { new Tag("CustomerTag", "CustomValue") });
```

**Fixed bugs**

* OkanshiMonitor.DurationTimer now correctly returns a DurationTimer
* DurationTimer now also registers the "total duration in seconds" gauge

### 3.0.0
* No longer ILMerge dependencies
* BREAKING: Total rewrite of the metrics structure to make it easy to control and customize

### 2.1.0
* Added endpoint (/packages) which lists the nuget packages used at compile time for this application using the content of the packages.config file, if it is found in the application directory.

### 2.0.0
* Add version information to response from API
	* This is done to allow extending the response in future releases
* ILMerge dependencies into Okanshi assembly

### 1.0.1
* Fix bug where CSharp.Monitor would not be usable in conjuction with the API

### 1.0.0
* Initial project release
