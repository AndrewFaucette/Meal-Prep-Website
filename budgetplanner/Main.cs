using System;
using System.IO;

/// <summary>Provides the console application's entry point.</summary>
/// <remarks>C# prohibits a Main method in a class named Main, so the Java Main class becomes Program.</remarks>
public static class Program
{
    /// <summary>Runs the menu until the user exits or standard input ends; arguments are unused.</summary>
    public static void Main(string[] args)
    {
        try
        {
            new Options().HandleOptions();
        }
        catch (EndOfStreamException)
        {
            // End redirected input cleanly instead of endlessly retrying failed reads.
        }
    }
}
