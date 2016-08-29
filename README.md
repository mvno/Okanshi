Okanshi
=======

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

Documentation
-------------

Okanshi is a library providing in-process monitoring of an application, it is create by Telenor MVNO to help transitioning to a microservice architecture.

The monitoring information can be accessed using the provided HTTP endpoint (JSON output) or by another custom implemented way.

It is designed to be as unobtrusive as possible, to achieve this the all statistics are calculated asynchronous, and won't impact the application performane that much. The memory footprint is also minimized as statistics are calculated on-the-fly, meaning the individual measurements aren't kept in memory longer than absolutely needed. 

Samples & documentation
-----------------------

The library comes with comprehensible documentation. 

 * [Tutorial](http://mvno.github.io/Okanshi/tutorial.html) contains a further explanation of this library.

 * [API Reference](http://mvno.github.io/Okanshi/reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library.
 
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
