[![Build status](https://ci.appveyor.com/api/projects/status/g9glkc76m1cala6b/branch/master?svg=true)](https://ci.appveyor.com/project/Mvno/okanshi/branch/master) [![Nuget Badge](https://img.shields.io/nuget/v/Okanshi.svg)](https://www.nuget.org/packages/Okanshi/) [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Okanshi.svg)](https://www.nuget.org/packages/Okanshi/)



# Okanshi

Okanshi is a high performance low overhead library for measuring, collecting and transporting in-process application metrics. It also provide a convenient health check mechanism that works out of the box.

To use Okanshi, you must insert into your code, various Okanshi monitors (stopwatches, counters, ...). Okanshi will then collect and transport them to an external service such as InfluxDB, Splunk, or ElasticSearch where reporting, alarms etc. can be set up. The transportation is plugable, so you can integrate with what you want. If you prefer, you can even change the transportation to be self-hosting within your application changing the metric data from a "push" to a "pull" transport.

Each measurement must be named. In addition to names, and you can associate one or more tags detailing e.g. "server name", "the application name", "test/production".


**Maturity**

Okanshi is mature. It has matured over several years, and is now used in production in several large-scale software products across different companies. It was originally conceived by Kim Christensen at [Telenor](https://www.telenor.dk/) "MVNO", now known as "CBB IT DevOps", to help transitioning to a micro service architecture. 

**Configurability**

The monitoring and information metric transportation has good default implementations, and the framework is highly configurable tailoring your needs. 

**Performance**

Okanshi is designed to be as unobtrusive as possible, to achieve this the all statistics are calculated asynchronously, and performance impact is very low. The memory footprint is also minimized as statistics are calculated on-the-fly, meaning the individual measurements aren't kept in memory longer than absolutely needed.


**Platforms**

Okanshi supports a number of platforms
 * [.NET Standard 2.0](https://www.microsoft.com/net) 
 * [.NET 4.5](https://www.microsoft.com/net) and above.

and works well with C# and F# alike.


## Samples & documentation

The library comes with comprehensible documentation. 

 * [Samples](samples) contains some different samples showcasing different parts of Okanshi.

 * [Tutorial](documentation) contains a further explanation of this library.

 * [API Reference](http://mvno.github.io/Okanshi/reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library.

 * [Release notes and upgrade information](https://github.com/mvno/Okanshi/blob/master/RELEASE_NOTES.md) contains information about versions and how to upgrade.


## Nuget

The Okanshi library can be [installed from NuGet](https://nuget.org/packages/Okanshi)

```PM> Install-Package Okanshi```



## Contributing and copyright

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/mvno/Okanshi/tree/master/docs/content
  [gh]: https://github.com/mvno/Okanshi
  [issues]: https://github.com/mvno/Okanshi/issues
  [readme]: https://github.com/mvno/Okanshi/blob/master/README.md
  [license]: https://github.com/mvno/Okanshi/blob/master/LICENSE.txt
