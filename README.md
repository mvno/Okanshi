[![Build status](https://ci.appveyor.com/api/projects/status/g9glkc76m1cala6b/branch/master?svg=true)](https://ci.appveyor.com/project/Mvno/okanshi/branch/master) [![Nuget Badge](https://img.shields.io/nuget/v/Okanshi.svg)](https://www.nuget.org/packages/Okanshi/) [![NuGet Pre Release](https://img.shields.io/nuget/vpre/Okanshi.svg)](https://www.nuget.org/packages/Okanshi/)

Okanshi
=======

Okanshi is a library providing in-process monitoring of an application, it was originally created by Telenor MVNO to help transitioning to a microservice architecture. It has matured over several years, and is now used in production in several large-scale software products across different companies.

The monitoring and information retrieval has good standard implementations, and the framework is highly configurable, allowing for custom implementation. An example is the monitoring information, that can be accessed using the provided HTTP endpoint (JSON output) or by custom code.

Okanshi is designed to be as unobtrusive as possible, to achieve this the all statistics are calculated asynchronous, and won't impact the application performane that much. The memory footprint is also minimized as statistics are calculated on-the-fly, meaning the individual measurements aren't kept in memory longer than absolutely needed. 


Samples & documentation
-----------------------

The library comes with comprehensible documentation. 

 * [Tutorial](documentation) contains a further explanation of this library.

 * [API Reference](http://mvno.github.io/Okanshi/reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library.

 * [Release notes and upgrade information](https://github.com/mvno/Okanshi/blob/master/RELEASE_NOTES.md) contains information about versions and how to upgrade.


Nuget
------

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The Okanshi library can be <a href="https://nuget.org/packages/Okanshi">installed from NuGet</a>:
      <pre>PM> Install-Package Okanshi</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>



Contributing and copyright
--------------------------

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
