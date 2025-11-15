using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("This will cause a compilation error");
        // Missing semicolon to cause error
        var x = "test"
    }
}