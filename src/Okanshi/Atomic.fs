namespace Okanshi

open System
open System.Threading
open Okanshi.Helpers

/// Perform atomic operations on a type
type IAtomic<'T> =
    /// Get the value
    abstract Get: unit -> 'T
    /// Sets the value and returns the old value
    abstract GetAndSet : 'T -> 'T
    /// Compare and set the value. If the value returned isn't equal the specified original value, it means
    /// that the value has changed since the last fetch and the new value hasn't been used
    abstract CompareAndSet: 'T * 'T -> 'T
    /// Set the value
    abstract Set: 'T -> unit
    /// Increment the value by one
    abstract Increment: unit -> 'T
    /// Increment the value by the specified amount
    abstract Increment: 'T -> 'T
    
/// Performs atomic operations on a long
type AtomicLong(intialValue) =
    let value = ref intialValue

    new () = AtomicLong(0L)

    /// Get the value
    member __.Get() = Interlocked.Read(value)
    /// Sets the value and returns the old value
    member __.GetAndSet(newValue) = Interlocked.Exchange(value, newValue)
    /// Compare and set the value. If the value returned isn't equal the specified original value, it means
    /// that the value has changed since the last fetch and the new value hasn't been used
    member __.CompareAndSet(newValue, originalValue) = Interlocked.CompareExchange(value, newValue, originalValue)
    /// Set the value
    member __.Set(newValue) = Interlocked.Exchange(value, newValue) |> ignore
    /// Increment the value by one
    member __.Increment() = Interlocked.Increment(value)
    /// Increment the value by the specified amount
    member self.Increment(amount) =
        let rec increment amount =
            let originalValue = self.Get()
            let result = self.CompareAndSet(originalValue + amount, originalValue)
            if result <> originalValue then increment amount
            else originalValue + amount
        increment amount

    interface IAtomic<int64> with
        member self.Get() = self.Get()
        member self.GetAndSet(newValue) = self.GetAndSet(newValue)
        member self.CompareAndSet(newValue, originalValue) = self.CompareAndSet(newValue, originalValue)
        member self.Set(newValue) = self.Set(newValue)
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)

/// Performs atomic operations on a double
type AtomicDouble(intialValue) =
    let value = new AtomicLong(intialValue |> BitConverter.DoubleToInt64Bits)

    new () = AtomicDouble(0.0)

    /// Get the value
    member __.Get() = value.Get() |> BitConverter.Int64BitsToDouble
    /// Sets the value and returns the old value
    member __.GetAndSet(newValue) = newValue |> BitConverter.DoubleToInt64Bits |> value.GetAndSet |> BitConverter.Int64BitsToDouble
    /// Compare and set the value. If the value returned isn't equal the specified original value, it means
    /// that the value has changed since the last fetch and the new value hasn't been used
    member __.CompareAndSet(newValue, originalValue) =
        let originalLong = BitConverter.DoubleToInt64Bits(originalValue)
        let newLong = BitConverter.DoubleToInt64Bits(newValue)
        value.CompareAndSet(newLong, originalLong) |> BitConverter.Int64BitsToDouble
    /// Set the value
    member __.Set(newValue) = newValue |> BitConverter.DoubleToInt64Bits |> value.Set |> ignore
    /// Increment the value by the specified amount
    member __.Increment(amount) =
        let rec loop () =
            let originalLong = value.Get()
            let originalDouble = originalLong |> BitConverter.Int64BitsToDouble
            let newLong = originalDouble + amount |> BitConverter.DoubleToInt64Bits
            if value.CompareAndSet(newLong, originalLong) <> originalLong then loop()
            else originalDouble + 1.0
        loop()
    /// Increment the value by one
    member self.Increment() = self.Increment(1.0)

    interface IAtomic<double> with
        member self.Get() = self.Get()
        member self.GetAndSet(newValue) = self.GetAndSet(newValue)
        member self.CompareAndSet(newValue, originalValue) = self.CompareAndSet(newValue, originalValue)
        member self.Set(newValue) = self.Set(newValue)
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)
    
/// Performs atomic operations on a decimal
type AtomicDecimal(initialValue) =
    let mutable value = initialValue
    let valueLock = new obj()

    new () = AtomicDecimal(0m)

    /// Get the value
    member __.Get() = lock valueLock (fun () -> value)
    /// Sets the value and returns the old value
    member __.GetAndSet(newValue) =
        lock valueLock (fun () ->
            let oldValue = value
            value <- newValue
            oldValue
        )
    /// Compare and set the value. If the value returned isn't equal the specified original value, it means
    /// that the value has changed since the last fetch and the new value hasn't been used
    member __.CompareAndSet(newValue, originalValue) =
        lock valueLock (fun () ->
            if originalValue = value then value <- newValue
            originalValue
        )
    /// Set the value
    member __.Set(newValue) = lock valueLock (fun () -> value <- newValue)
    /// Increment the value by the specified amount
    member __.Increment(amount) = lock valueLock (fun () ->
        value <- value + amount
        value)
    /// Increment the value by one
    member self.Increment() = self.Increment(1m)

    interface IAtomic<decimal> with
        member self.Get() = self.Get()
        member self.GetAndSet(newValue) = self.GetAndSet(newValue)
        member self.CompareAndSet(newValue, originalValue) = self.CompareAndSet(newValue, originalValue)
        member self.Set(newValue) = self.Set(newValue)
        member self.Increment() = self.Increment()
        member self.Increment(amount) = self.Increment(amount)

/// Utility class used to describe step intervals
[<System.Diagnostics.DebuggerDisplay("Timestamp = {Timestamp}; Value = {Value}")>]
type Datapoint = 
    { Timestamp : Nullable<DateTime>
      Value : int64 }
    static member Empty = 
        { Timestamp = new System.Nullable<_>()
          Value = -1L }
