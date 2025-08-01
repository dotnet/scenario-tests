using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("File-based app with arguments");
        Console.WriteLine($"Received {args.Length} arguments:");
        
        for (int i = 0; i < args.Length; i++)
        {
            Console.WriteLine($"  arg[{i}]: {args[i]}");
        }
        
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided");
        }
    }
}