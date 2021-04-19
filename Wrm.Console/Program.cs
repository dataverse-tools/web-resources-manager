using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using Microsoft.Xrm.Tooling.Connector;
using NLog;
using Wrm.Model;
using Wrm.Services;


namespace Wrm.ConsoleApp
{
    public static class Program
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();


        public static void Main(string[] args)
        {
            var parsedArgs = Parser.Default.ParseArguments<InitOptions, PushOptions, PullOptions, DeleteOptions>(args);
            try
            {
                parsedArgs
                    .WithParsed<Options>(options =>
                    {
                        using (var client = new CrmServiceClient(options.ConnectionString))
                        {
                            if (!client.IsReady)
                            {
                                throw new Exception(client.LastCrmError, client.LastCrmException);
                            }

                            var webResourcesSvc = new WebResourceService(client);

                            parsedArgs
                                .WithParsed<InitOptions>(webResourcesSvc.Init)
                                .WithParsed<PushOptions>(webResourcesSvc.Push)
                                .WithParsed<PullOptions>(webResourcesSvc.Pull)
                                .WithParsed<DeleteOptions>(webResourcesSvc.Delete)
                                .WithNotParsed(ProcessParserErrors);
                        }
                    })
                    .WithNotParsed(ProcessParserErrors);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }
            finally
            {
                parsedArgs
                    .WithParsed<Options>(options =>
                    {
                        if (options.Quiet)
                        {
                            return;
                        }

                        Console.WriteLine("Press ENTER to exit...");
                        Console.ReadLine();
                    });
            }
        }


        private static void ProcessParserErrors(IEnumerable<Error> errors)
        {
            var msg = errors.Aggregate("Failed to parse tool arguments:\n", (current, err) => current + $"\t- {err}\n");
            _logger.Error(msg);
        }
    }
}
