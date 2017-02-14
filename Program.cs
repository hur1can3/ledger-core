namespace LedgerCore
{

    public class Program
    {

        public static int Main(string[] args)
        {
            var options = CommandLineOptions.Parse(args);

            if (options?.Command == null)
            {
                // RootCommand will have printed help
                return 1;
            }

            options.Command.Run();

            return 0;

        }
    }
}