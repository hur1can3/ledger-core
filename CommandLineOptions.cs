using LedgerCore.Commands;
using Microsoft.Extensions.CommandLineUtils;

namespace LedgerCore
{
    public class CommandLineOptions
    {

        public static CommandLineOptions Parse(string[] args)
        {
            var options = new CommandLineOptions();

            var app = new CommandLineApplication
            {
                Name = "ledger-core",
                FullName = "Ledger .NET Core tool"
            };

            app.HelpOption("-?|-h|--help");



            var verboseSwitch = app.Option("-v|--verbose",
                                          "Whether the app should be verbose.",
                                          CommandOptionType.NoValue);



            RootCommand.Configure(app, options);

            var result = app.Execute(args);

            if (result != 0)
            {
                return null;
            }

            options.IsVerbose = verboseSwitch.HasValue();

            return options;
        }

        public ICommand Command { get; set; }
        public bool IsVerbose { get; set; }

    }

}