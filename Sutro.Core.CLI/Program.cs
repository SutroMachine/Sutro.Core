using gs;
using Sutro.Core.Logging;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace Sutro.Core.CLI
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var logger = new ConsoleLogger();

            var cli = new CommandLineInterface(
                logger: logger,
                printGenerators: new List<IPrintGeneratorManager> {
                    new PrintGeneratorManager<SingleMaterialFFFPrintGenerator, PrintProfileFFF>(
                        new PrintProfileFFF(), "fff", "Basic FFF prints", logger)
                });

            cli.Execute(args);
        }
    }
}