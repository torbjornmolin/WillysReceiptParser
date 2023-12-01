public class ReceiptPrinter
{
    public static void PrintReceipt(IEnumerable<string> lines, string name)
    {
        Console.WriteLine($"--------Start {name}----------");
        bool flip = false;
        foreach (var l in lines)
        {
            Console.ForegroundColor = flip ? ConsoleColor.Blue : ConsoleColor.Magenta;
            flip = !flip;
            Console.WriteLine(l);
            Console.ResetColor();
        }
        Console.WriteLine();
        Console.WriteLine($"--------End {name}----------");
        Console.WriteLine();
    }

}