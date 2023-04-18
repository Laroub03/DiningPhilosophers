using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        Random rnd = new Random();
        Fork fork = new Fork();

        // Create 5 philosopher objects with random thinking and eating times
        for (PhilosopherName name = PhilosopherName.Philosopher1; name <= PhilosopherName.Philosopher5; name++)
        {
            int thinkDelay = rnd.Next(1, 5);
            int eatDelay = rnd.Next(1, 5);
            new Philosopher(name, thinkDelay, eatDelay, fork);
        }
    }
}

// enum for storing philosopher names
enum PhilosopherName
{
    Philosopher1,
    Philosopher2,
    Philosopher3,
    Philosopher4,
    Philosopher5
}

class Philosopher
{
    PhilosopherName name;
    int thinkDelay;
    int eatDelay;
    PhilosopherName left;
    PhilosopherName right;
    Fork fork;

    public Philosopher(PhilosopherName name, int thinkDelay, int eatDelay, Fork fork)
    {

        this.name = name;
        this.thinkDelay = thinkDelay;
        this.eatDelay = eatDelay;
        this.fork = fork;

        // Set the left and right fork positions based on the philosopher's name
        switch (name)
        {
            case PhilosopherName.Philosopher1:
                left = PhilosopherName.Philosopher5;
                right = PhilosopherName.Philosopher2;
                break;
            case PhilosopherName.Philosopher2:
                left = PhilosopherName.Philosopher1;
                right = PhilosopherName.Philosopher3;
                break;
            case PhilosopherName.Philosopher3:
                left = PhilosopherName.Philosopher2;
                right = PhilosopherName.Philosopher4;
                break;
            case PhilosopherName.Philosopher4:
                left = PhilosopherName.Philosopher3;
                right = PhilosopherName.Philosopher5;
                break;
            case PhilosopherName.Philosopher5:
                left = PhilosopherName.Philosopher4;
                right = PhilosopherName.Philosopher1;
                break;
        }

        // Start a new thread for the DinnerReady method
        new Thread(new ThreadStart(DinnerReady)).Start();
    }

    // The DinnerReady method that the thread will run
    public void DinnerReady()
    {
        while (true)
        {
            try
            {
                // philosopher thinks for some random time
                Console.WriteLine("Philosopher " + name + " is thinking...");
                Thread.Sleep(TimeSpan.FromSeconds(thinkDelay));

                // philosopher picks up the forks and eats for some random time
                fork.Get(left, right);
                Console.WriteLine("Philosopher " + name + " is eating...");
                Thread.Sleep(TimeSpan.FromSeconds(eatDelay));

                // philosopher puts the forks back down
                fork.Put(left, right);
            }
            catch (Exception e)
            {
                // handle any errors that occur
                Console.WriteLine("Error caught.", e);
            }
        }
    }
}

class Fork
{
    // Create a lock object to use for thread synchronization
    static object _lock = new object();
    // Initialize an array of booleans representing the availability of each fork
    bool[] fork = new bool[5];

    // Method to release the forks
    public void Put(PhilosopherName left, PhilosopherName right)
    {
        // Acquire the lock to ensure exclusive access to shared resources
        Monitor.Enter(_lock);
        try
        {
            // Mark both forks as available
            fork[(int)left] = false;
            fork[(int)right] = false;
            // Signal any waiting threads that the forks are available
            Monitor.PulseAll(_lock);
        }
        finally
        {
            // Release the lock
            Monitor.Exit(_lock);
        }
    }

    // Method to acquire the forks
    public void Get(PhilosopherName left, PhilosopherName right)
    {
        // Acquire the lock to ensure exclusive access to shared resources
        Monitor.Enter(_lock);
        try
        {
            // Wait until both forks are available
            while (fork[(int)left] || fork[(int)right])
            {
                Monitor.Wait(_lock);
            }
            // Mark both forks as unavailable
            fork[(int)left] = true;
            fork[(int)right] = true;
        }
        finally
        {
            // Release the lock
            Monitor.Exit(_lock);
        }
    }
}
