using PDSimAPI;
using GeTPlanModel;
using Proto;
using System.Diagnostics;
namespace PDSimCLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Test connection first
            var connection = new BackendTestConnectionRequest();
            var testConnection = connection.Connect();
            try
            {
                
                if (testConnection["status"].ToString() == "TO")
                {
                    Console.WriteLine("Could not connect to the backend server. Please make sure the server is running.");
                    Environment.Exit(1);
                }
            }
            catch (NullReferenceException nre)
            {
                Console.WriteLine($"{nre.Message}");
                Console.WriteLine("Error Connecting to Server.");
                Environment.Exit(1);
            }

            var problem = new ProtobufRequest("problem");
            var problemResponse = problem.Connect();

            var plan = new ProtobufRequest("plan");
            var planResponse = plan.Connect();



            var visualisation = new Visualisation(problemResponse, planResponse);
            visualisation.WorldStateChanged += (sender, e) =>
            {
                Console.WriteLine("###################");
                Console.WriteLine(e.AppliedAction);
                Console.WriteLine(e.NewStateVar);
                Console.WriteLine(e.AppliedEffect);
                Console.WriteLine("###################");
                Console.WriteLine();
            };

            visualisation.VisualisationStart += (sender, e) =>
            {
                Console.WriteLine("!--Visualisation Started--!");
            };

            visualisation.VisualisationEnd += (sender, e) =>
            {
                Console.WriteLine("!--Visualisation Ended--!");
            };

            visualisation.VisualisationPaused += (sender, e) =>
            {
                Console.WriteLine("!--Visualisation Paused--!");
            };

            var userInput = string.Empty;
            DisplayMenu();

            while (userInput != "exit")
            {
                Console.Write("> ");
                userInput = Console.ReadLine();
                if (userInput == "next" || userInput == "")
                {
                    visualisation.Advance();
                }
                else if (userInput == "pause")
                {
                    visualisation.Pause();
                }
                else if (userInput == "resume")
                {
                    visualisation.Resume();
                }
                else if (userInput == "reset")
                {
                    visualisation.Reset();
                }
                else if (userInput == "help")
                {
                    DisplayMenu();
                }
            }
        }

        private static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("==========================================");
            Console.WriteLine("||     PDSimCLI Console ~.~             ||");
            Console.WriteLine("==========================================");
            Console.ResetColor();
            Console.WriteLine("Enter 'exit' to quit");
            Console.WriteLine("Type 'next' to advance the visualisation");
            Console.WriteLine("Type 'pause' to pause the visualisation");
            Console.WriteLine("Type 'resume' to resume the visualisation");
            Console.WriteLine("Type 'reset' to reset the visualisation");
            Console.WriteLine("Type 'help' to display this menu again");
            Console.WriteLine("=========================================");
        }
    }
}