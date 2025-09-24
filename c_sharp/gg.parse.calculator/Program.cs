using gg.parse.instances.calculator;

namespace gg.parse.calculator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("gg.parse.calculator\n");

            var interpreter = new CalculatorInterpreter(
                File.ReadAllText("assets/calculator.tokens"),
                File.ReadAllText("assets/calculator.grammar")
            );

            while (true) 
            {
                try
                {
                    Console.Write(">");

                    var input = Console.ReadLine();

                    if (!string.IsNullOrEmpty(input))
                    {
                        if (input == "x" || input == "exit" || input == "quit" || input == "q")
                        {
                            break;
                        }

                        Console.WriteLine(interpreter.Interpret(input));
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            
        }
    }
}
