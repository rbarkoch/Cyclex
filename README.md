# Cyclex
A simple library for creating safe cyclic executive loops.

## Use Case
We've all done it...
```csharp
bool running = true;
while(running)
{
    Loop();
    Thread.Sleep(1000);
}
```

Amongst many others, the code above has the following two problems:
1. Even if `Loop()` is complete, we still need to wait for `Thread.Sleep(...)` if we want to exit the loop.
1. The loop does not have preditable timing. If `Loop()` requires 250 ms, then the total loop time will be 1250 ms. On the next loop if `Loop()` requires 500ms, then the next loop time will be 1500 ms.

Instead we can now use `CyclicExecutive` to provide safe and consistent cyclic executive loops.

```csharp
CyclicExecutive cyclicExecutive = new CyclicExecutive(Loop)
{
    Interval = TimeSpan.FromMilliseconds(1000)
}

cyclicExecutive.Start();

...

public void Loop()
{
    // Do something.
}
```

The `CyclicExecutive` object fixes the problems above:
1. When the `Stop()` method is called, if the execution of `Loop()` is complete, the loop will exist immediately.
1. Whether `Loop()` takes 250 ms or 500 ms, the `Loop()` method will still be called every ~1000 ms.

## Options
The `CyclicExecutive` object has many properties which allow you to easily monitor and control how the cyclic executive loop behaves.


```csharp
CyclicExecutive cyclicExecutive = new CyclicExecutive(Loop)
{
    Interval = TimeSpan.FromMilliseconds(1000),

    // Rethrows exceptions which occur within the loop.
    PropogateException = false,

    // Stops the loop automatically on an exception.
    StopOnException = false,

    // Stops the loop automatically if the executive time is greater than the target loop time.
    StopOnOverflow = false
}

// Triggered when loop is started.
cyclicExecutive.Started += CyclicExecutive_Started;

// Triggered when execution is complete an loop has stopped.
cyclicExecutive.Stopped += CyclicExecutive_Stopped;

// Triggered at the end of every cycle. Contains metrics about the utilization of the loop.
cyclicExecutive.CycleCompleted += CyclicExecutive_CycleCompleted;

// Triggered if the cycle overflows. Contains metrics about the extent of the overflow.
cyclicExecutive.CycleOverflow += CyclicExecutive_CycleOverflow;

// Triggered if an exception occurs within the loop.
cyclicExecutive.CycleException += CyclicExecutive_CycleException;
...
