# Best practices

Here we describe some of the best practices we have gained by introducing and using Okanshi in different businesses. Here, we assume some understanding of Okanshi.

Table of Content

   * [1. How to introduce measurements to a code base](#1-how-to-introduce-measurements-to-a-code-base)
   * [2. Use tags](#2-use-tags)
   * [3. Wrap the okanshi setup in your own type](#3-wrap-the-okanshi-setup-in-your-own-type)
   * [4. Introduce an anti-corruption layer](#4-introduce-an-anti-corruption-layer)
   * [5. Fast fetching of monitors](#5-fast-fetching-of-monitors)
   
   
   
## 1. How to introduce measurements to a code base

Mindset is important when you introduce Okanshi into your code base. It is important not to go into *"analysis paralysis"*. 

We prefer to simply start using Okanshi where you first see a need for measurements. Perhaps in relation to a defect or other. Do not start by agreeing to a long list of cases and situations where monitoring is mandatory. Its a learning process and often case-by-case. No matter how hard you contemplate on where to do measurements, you *will* encounter situations where you are not measuring enough or in the right way.  

Your mindset should be that it is not "the end of the world" when a measurement is missing. It is a "so what? Lets just insert that extra measurement and move on"-kind of thing.



## 2. Use tags

While naming monitors is important, remember you can configure tags that are supplied with all measurements. But what tags to use. The following list may serve you as a list of inspiration. Remember, tags can be used in the consuming system to slice and dice the data. Tags enable to you separate data, but you can always aggregate all data solely based on the name of your measurements.

The list

* **Computer name:** The name/id of the computer making measurement can identify if a particular machine or hardware is experiencing problems.
* **Environment name:** For example "test" and "production" - enable you to see how things are going in various environments.
* **Request path:** In a web-service context the request path enable you to summarize either on individual end point or globally
* **Request method:** In a web-service context, e.g. GET or POST
* **Unit of measure:** When you cannot determine from the measurement name the unit of measure, and its not a standard unit (e.g. days or seconds) consider applying it as a tag.



## 3. Wrap the okanshi setup in your own type
We prefer working and testable code over setup decisions made in word documents. Therefore a perfectly legal and useful way set up Okanshi, is to do it in your own wrapper class. 

By centralizing how the standard setup, you ensure consistency, you enforce mandatory tagging e.g., and you can make it explicit, the considerations that must be made for each service in your setup. 




## 4. Introduce an anti-corruption layer
Consider confining Okanshi types with your own implementation. This makes you less vulnerable when one day Okanshi decides to change names of monitors. It also enable you to define bespoke naming that matches company wide standards and nomenclature. It is a pattern often used in larger code bases and larger companies.



## 5. Fast fetching of monitors
There are two ways to instantiate/get hold of a monitor.  

1.
```
class BusinessLogic
{
    void Foo() 
    {
       OkanshiMonitor.Gauge("xxx").SetValue(...)
``` 

2.
``` 
class BusinessLogic
{
    static IGauge xxx = OkanshiMonitor.Gauge("xxx")

    void Foo() 
    {
      xxx.SetValue(...)    
```

The first way of getting instances is much more lightweigh syntax-wise. And it may be a good way to get up and running. But there is an overhead, in that Okanshi has to do a lookup in its internal cache. Thus the second approach may be preferable if you are fetching the same monitor several times a second.


