using gs;
using Sutro.Core.Logging;
using System;
using System.Collections.Generic;

namespace gsSlicer.CLI
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
                    new PrintGeneratorManager<SingleMaterialFFFPrintGenerator, SingleMaterialFFFSettings>(
                        new GenericRepRapSettings(), "fff", "Basic FFF prints", logger)
                });

            cli.Execute(args);
        }
    }
}