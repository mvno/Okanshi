namespace Okanshi

module Statistics =

    open System

    /// Statistics information
    type Statistics =
        {
            /// The average
            average : decimal;
            /// The variance
            variance : decimal;
            /// The number of successes
            numberOfSuccess : int64;
            /// The number of failures
            numberOfFailed : int64;
            /// The minimum value
            minimum : decimal;
            /// The maximum value
            maximum : decimal;
            /// The number of timed calls
            numberOfTimedCalls : int64;
            /// The mean
            mean : decimal;
            /// The sum of squarces, used in internal calculations
            sumOfSquares : decimal;
            /// The start time of the informations
            startTime : DateTimeOffset;
            /// The end time of the informations
            endTime : DateTimeOffset;
        }
        /// The standard deviation
        member self.standardDeviation =
            self.variance |> float |> sqrt

    /// Add a timinig to the statistic
    let addTiming (timing : int64) measurement =
        let timingAsDecimal = timing |> decimal
        let minimum = Math.Min(measurement.minimum, timingAsDecimal)
        let maximum = Math.Max(measurement.maximum, timingAsDecimal)
        let numberOfTimedCalls = measurement.numberOfTimedCalls + int64 1
        let numberOfTimedCallsAsDecimal = numberOfTimedCalls |> decimal
        let average = ((measurement.average * (numberOfTimedCallsAsDecimal - decimal 1)) + timingAsDecimal) / numberOfTimedCallsAsDecimal
        let delta = timingAsDecimal - measurement.mean
        let mean = measurement.mean + (delta / numberOfTimedCallsAsDecimal)
        let sumOfSquares = measurement.sumOfSquares + (delta * (timingAsDecimal - mean))
        let variance =
            if numberOfTimedCalls = int64 1 then
                sumOfSquares / numberOfTimedCallsAsDecimal
            else
                sumOfSquares / (numberOfTimedCallsAsDecimal - decimal 1)
        { measurement with maximum = maximum; minimum = minimum; average = average; variance = variance; sumOfSquares = sumOfSquares; mean = mean; numberOfTimedCalls = numberOfTimedCalls }

    let private incr (x : int64) =
        x + int64 1

    /// Add a success to the statistic
    let addSuccess measurement =
        { measurement with numberOfSuccess = measurement.numberOfSuccess |> incr }

    /// Add a failure to the statistic
    let addFailed measurement =
        { measurement with numberOfFailed = measurement.numberOfFailed |> incr }

    /// Create an empty statistic with the provided window size
    let createEmptyMeasurement windowSize =
        {
            average = decimal 0;
            variance = decimal 0;
            numberOfSuccess = int64 0;
            numberOfFailed = int64 0;
            minimum = decimal Int32.MaxValue;
            maximum = decimal Int32.MinValue;
            numberOfTimedCalls = int64 0;
            mean = decimal 0;
            sumOfSquares = decimal 0;
            startTime = DateTimeOffset.Now;
            endTime = DateTimeOffset.Now.Add(windowSize);
        }
