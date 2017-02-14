using Microsoft.Extensions.CommandLineUtils;

namespace LedgerCore.Commands
{
    public class RootCommand : ICommand
    {

        public static void Configure(CommandLineApplication app, CommandLineOptions options)
        {

            app.Command("import", c => ImportCSVCommand.Configure(c, options));
            app.Command("parse", c => ParseLedgerCommand.Configure(c, options));


            app.OnExecute(() =>
                {
                    options.Command = new RootCommand(app);

                    return 0;
                });

        }

        private readonly CommandLineApplication _app;

        public RootCommand(CommandLineApplication app)
        {
            _app = app;
        }

        public void Run()
        {
            _app.ShowHelp();
        }

    }
}