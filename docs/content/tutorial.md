Tutorial
========================

Starting the API
----------------

After Okanshi has been added to your project, you start the monitor like this:

C#:

    [lang=csharp]
    var api = new MonitorApi();
    api.Start();

F#:

    [lang=fsharp]
    let api = new MonitorApi()
    api.Start()

You should now be able to access the HTTP endpoint using [http://localhost:13004](http://localhost:13004).
As nothing has been monitored yet, it will return an JSON response with an empty object, like this:

    {}

Metrics
-------

Okanshi has a couple of different metrics, you can measure Success, Failure and Time function calls.


The most basic metric is, Success and Failure, used like this:

C#:

    [lang=csharp]
    Monitor.Success("success");
    Monitor.Failed("failure");

F#:

	[lang=fsharp]
	monitor |> Monitor.success "success"
	monitor |> Monitor.failed "failure"

If you access the [API](http://localhost:13004) now, you'll see something like this:

    {
        failure: {
            measurements: [
                {
                average: 0,
                variance: 0,
                numberOfSuccess: 0,
                numberOfFailed: 1,
                minimum: 2147483647,
                maximum: -2147483648,
                numberOfTimedCalls: 0,
                mean: 0,
                sumOfSquares: 0,
                startTime: "2015-06-11T22:08:19.3509214+02:00",
                endTime: "2015-06-11T23:08:19.3509214+02:00",
                standardDeviation: 0
                }
            ],
            maxMeasurements: 100,
            windowSize: 3600000
        },
        success: {
            measurements: [
                {
                average: 0,
                variance: 0,
                numberOfSuccess: 1,
                numberOfFailed: 0,
                minimum: 2147483647,
                maximum: -2147483648,
                numberOfTimedCalls: 0,
                mean: 0,
                sumOfSquares: 0,
                startTime: "2015-06-11T22:07:44.2658471+02:00",
                endTime: "2015-06-11T23:07:44.2658471+02:00",
                standardDeviation: 0
                }
            ],
            maxMeasurements: 100,
            windowSize: 3600000
        }
    }

This contains a lot of information, not introduced yet, but it should be pretty self explanatory. The information will be explained later.
The important information in this case is the two properties, `failure` and `success`, which also is the arguments passed
into `Monitor.Success` and `Monitor.Failed`, these are used as keys to group metrics. You can see that the `success` property contains a measurement
with `numberOfSuccess: 1` and the `failure` metric has `numberOfFailed: 0`, this makes it easy to see how many times the specific metric has
succeeded and failed.

The output also contains a lot of other information mostly related to the timing of function calls. Time a function call like this:

C#: 

    [lang=csharp]
    // Time an Action
    Monitor.Time("action", () => { System.Threading.Thread.Sleep(2000); });
    // Time a Func
    var result = Monitor.Time("func", () => { System.Threading.Thread.Sleep(2000); return true; });

F#:

	[lang=fsharp]
	monitor |> Monitor.time "func" (fun () -> System.Threading.Sleep(2000); true)
	monitor |> Monitor.time "action" (fun () -> System.Threading.Sleep(2000))

After running the statements above twice you should get output something like this:

    {
        func: {
            measurements: [
                {
					average: 1999,
					variance: 0,
					numberOfSuccess: 2,
					numberOfFailed: 0,
					minimum: 1999,
					maximum: 1999,
					numberOfTimedCalls: 2,
					mean: 1999,
					sumOfSquares: 0,
					startTime: "2015-06-12T09:28:01.5582084+02:00",
					endTime: "2015-06-12T10:28:01.5592085+02:00",
					standardDeviation: 0
				}
            ],
            maxMeasurements: 100,
            windowSize: 3600000
        },
        action: {
            measurements: [
                {
					average: 1999,
					variance: 0,
					numberOfSuccess: 2,
					numberOfFailed: 0,
					minimum: 1999,
					maximum: 1999,
					numberOfTimedCalls: 2,
					mean: 1999,
					sumOfSquares: 0,
					startTime: "2015-06-12T09:29:33.043356+02:00",
					endTime: "2015-06-12T10:29:33.043356+02:00",
					standardDeviation: 0
				}
            ],
            maxMeasurements: 100,
            windowSize: 3600000
        }
    }

Now a lot more of the information is populated. As we can see `numberOfSuccess` is incremented as the functions didn't throw an exception, if one of the function calls throws an exception
`numberOfFailed` is incremented instead. All the timing properties should be self explanatory, except `sumOfSquares` and `mean`, this is just values used
internally to calculate the variance and standardDeviation, without saving all timing information for each call.

The last part of the output is `maxMeasurements` and `windowSize`. These indicates the maximum number of measurements to save in memory and the window size
of each measurement in milliseconds. These can be set by passing `MonitorOptions` into `monitorApi.Start`.

Health checks
-------------

You can also add different health checks to you application. For example the number of files in a directory or something else making sense in you case.
Health checks are added like this:

C#:

    [lang=csharp]
    HealthChecks.Add("key", () => Directory.GetFiles("C:\\MyData").Length == 0);

F#:
	[lang=fsharp]
	HealthChecks.Add("key", (fun () -> System.IO.Directory.GetFiles("C:\\MyData").Length = 0))

The `Func` passed in just have to return a boolean, indicating pass or fail. The status of the health checks can be seen using
[http://localhost:13004/healthchecks](http://localhost:13004/healthchecks).

Assembly dependencies
---------------------

To see all assembly dependencies for you application just access [http://localhost:13004/dependencies](http://localhost:13004/dependencies). It will provide
a list of the names and version of all dependencies.