### 4.0.0

#### BREAKING CHANGES

* Renames DurationTimer to LongTaskTimer

#### Other

* Fix bug when multiple tags where used
* Add support for using HealthChecks as a monitor
* Support for monitoring Windows performance counters
* Support for automatically poll of metrics when process is about to exit, the default behaviour is not to poll
* Support for manual timing of operations, using the Start() method on the existing timers, which returns an object that when stopped registers timing to the timer.

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
* Intial project release
