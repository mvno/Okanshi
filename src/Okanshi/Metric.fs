namespace Okanshi

module Metric =

    open Statistics
    open System

    /// Contains measurements for an individual key
    type Metric =
        {
            /// The measurement buckets
            measurements : Statistics seq;
            /// The maximum number of measurement buckets
            maxMeasurements : int;
            /// The window size of the measurement buckets
            windowSize : float;
        }

    /// Determines if a measurement bucket should be added to a metric
    let shouldMeasurementBeAdded metricMeasurements =
        if metricMeasurements.measurements |> Seq.isEmpty then
            false
        else
            let newestBucket = metricMeasurements.measurements |> Seq.head
            match newestBucket.endTime with
                | x when DateTimeOffset.Now > x -> true
                | _ -> false

    /// Get current measurement bucket
    let getMeasurement metricMeasurements =
        if metricMeasurements |> shouldMeasurementBeAdded then
            (TimeSpan.FromMilliseconds(metricMeasurements.windowSize) |> Statistics.createEmptyMeasurement, true)
        else
            if metricMeasurements.measurements |> Seq.isEmpty then (TimeSpan.FromMilliseconds(metricMeasurements.windowSize) |> Statistics.createEmptyMeasurement, true) else (metricMeasurements.measurements |> Seq.head, false)

    /// Add measurement
    let add add metricMeasurements =
        let (oldMeasurement, isNewMeasurement) = metricMeasurements |> getMeasurement
        let newMeasurement = oldMeasurement |> add
        match isNewMeasurement with
            | true ->
                match metricMeasurements.measurements |> Seq.length with
                    | x when x = metricMeasurements.maxMeasurements -> { metricMeasurements with measurements = metricMeasurements.measurements |> Seq.take (metricMeasurements.maxMeasurements - 1) |> Seq.append [ newMeasurement ] |> List.ofSeq }
                    | _ -> { metricMeasurements with measurements = metricMeasurements.measurements |> Seq.append [ newMeasurement ] |> List.ofSeq }
            | false -> { metricMeasurements with measurements = metricMeasurements.measurements |> Seq.skip 1 |> Seq.append [ newMeasurement ] |> List.ofSeq }

    /// Create empty metric with the provided number of maximum measurement buckets and window size
    let createEmpty maxMeasurements windowSize =
        { measurements = List.empty<Statistics>; maxMeasurements = maxMeasurements; windowSize = windowSize }

    /// Add a failure
    let addFailed  metric =
        metric |> add Statistics.addFailed

    /// Add a success
    let addSuccess  metric =
        metric |> add Statistics.addSuccess

    /// Add a timining
    let addTiming timing metric =
        metric |> add (timing |> Statistics.addTiming)
