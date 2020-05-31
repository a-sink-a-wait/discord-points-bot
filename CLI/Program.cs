﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using PointsBot.Infrastructure.Commands;

namespace PointsBot.CLI
{
    public class Program
    {
        public static IConfiguration _configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddJsonFile("./local.appsettings.json", true, true)
            .Build();

        private static Task Main(string[] args)
        {
            var sender = new CommandSender(new QueueClient(new ServiceBusConnectionStringBuilder(_configuration["CommandServiceBusConnectionString"])));

            ICommand command = null;
            switch (args[0])
            {
                case "add":
                    command = new AddCommand(args[1], args[2], Int32.Parse(args[3]));
                    break;
                case "remove":
                    command = new RemoveCommand(args[1], args[2], Int32.Parse(args[3]));
                    break;
                default: return Task.CompletedTask;
            }

            return sender.SendCommand(command);
        }
    }
}
