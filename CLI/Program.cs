using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using PointsBot.Infrastructure.Commands;

namespace PointsBot.CLI
{
    public class Program
    {
        public static IConfiguration Configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("./local.appsettings.json", true, true)
            .Build();

        private static async Task<int> Main(string[] args)
        {
            var command = args[0];
            Console.WriteLine(args[0]);

            if (command == "postbuild")
            {
                PostBuild.Execute(args[1]);
                return 0;
            }

            var sender = new CommandSender(
                new QueueClient(
                    new ServiceBusConnectionStringBuilder(Configuration["CommandServiceBusConnectionString"])),
                new TopicClient(
                    new ServiceBusConnectionStringBuilder(Configuration["ExtensionsServiceBusConnectionString"])));

            switch (command)
            {
                case "add":
                    await sender.SendAdd(args[1], args[2], Int32.Parse(args[3]), "CLI_home");
                    break;
                case "remove":
                    await sender.SendRemove(args[1], args[2], Int32.Parse(args[3]), "CLI_home");
                    break;
                case "dumpredis":
                    Console.WriteLine("Deprecated;");
                    break;
                default: return 0;
            }

            return 0;
        }
    }

    public class PointsCommand
    {
        public string OriginPlayerId { get; set; }

        public string TargetPlayerId { get; set; }

        public EventParameters EventParameters { get; set; }
    }

    public class EventParameters
    {
        public string Action { get; set; }

        public int Amount { get; set; }
    }
}
