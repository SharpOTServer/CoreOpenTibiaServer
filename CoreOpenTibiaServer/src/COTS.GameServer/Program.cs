﻿using CommandLine;
using COTS.GameServer.CommandLineArgumentsParsing;
using COTS.GameServer.Lua;
using COTS.Infra.CrossCutting.Network;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using COTS.Infra.CrossCutting.Ioc;
using System.IO;

namespace COTS.GameServer {

    public sealed class Program
    {

        private static ServiceProvider _serviceProvider;

        private static void Main(string[] args) {

            var nodes = World.WorldLoader.ParseTree(File.ReadAllBytes(@"C:\Source\forgottenserver-master\data\world\forgotten.otbm"));
            Console.WriteLine(nodes.PropsEnd);

            var serviceCollection = new ServiceCollection();
            BootStrapper.ConfigureGlobalServices(serviceCollection);
            ConfigureLocalServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
            
            var parser = Parser.Default;
            var parseAttempt = parser.ParseArguments<CommandLineArguments>(args: args);

            if (parseAttempt is Parsed<CommandLineArguments> successfullyParsed) {
                RunWithSucessfullyParsedCommandLineArguments(successfullyParsed.Value);
            } else if (parseAttempt is NotParsed<CommandLineArguments> failedAttempt) {
                ReportCommandLineParsingError(failedAttempt);
            } else {
                throw new InvalidOperationException("Fo reals? This line should never be reached.");
            }

            var original = "testiculos";
            var encoded = NetworkMessage.Encode(original);
            Console.WriteLine(encoded.Length);
            var decoded = NetworkMessage.Decode(encoded);
            Console.WriteLine(original == decoded);

            Console.ReadLine();
        }

        private static void RunWithSucessfullyParsedCommandLineArguments(CommandLineArguments commandLineArguments) {           
            _serviceProvider.GetService<LuaManager>().Run();

            var clientConnectionManager = commandLineArguments.GetClientConnectionManager();
            Task.Run(() => clientConnectionManager.StartListening());
        }

        private static void ReportCommandLineParsingError(NotParsed<CommandLineArguments> failedAttempt) {
            throw new NotImplementedException();
        }

        public static void ConfigureLocalServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<LuaManager>();
        }
    }
}