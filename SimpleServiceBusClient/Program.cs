using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SimpleServiceBusClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine(@"
 _____                     _             ______             
/  ___|                   (_)            | ___ \            
\ `--.   ___  _ __ __   __ _   ___  ___  | |_/ / _   _  ___ 
 `--. \ / _ \| '__|\ \ / /| | / __|/ _ \ | ___ \| | | |/ __|
/\__/ /|  __/| |    \ V / | || (__|  __/ | |_/ /| |_| |\__ \
\____/  \___||_|     \_/  |_| \___|\___| \____/  \__,_||___/
                                                            
                                                            
");

                await Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static void LogConfigRetrieved(string connectionString, string queueName)
        {
            Console.WriteLine($"The configuration is {connectionString} to the queue {queueName}");

            Console.WriteLine("Starting to receive messages...");

            Console.WriteLine();
        }

        private static async Task ReceiveMessages(string connectionString, string queueName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            LogConfigRetrieved(connectionString, queueName);

            var messages = await ReceiveMessages(connectionString, queueName, 50);

            Console.WriteLine(string.Join(",", messages.Select(x => x.metadata.site)));
        }

        private static async Task<IList<MessageDto>> ReceiveMessages(string connectionString, string queueName, int batchSize)
        {
            var messageReceiver = new MessageReceiver(connectionString, queueName, ReceiveMode.PeekLock);

            var messages = new List<MessageDto>();

            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;

            for (var i = batchSize; i > 0; i--)
            {
                try
                {
                    var message = await messageReceiver.ReceiveAsync();

                    var body = Encoding.UTF8.GetString(message.Body);

                    var m = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageDto>(body.Substring(body.IndexOf("{")), settings);

                    messages.Add(m);

                    Console.WriteLine($"Got {messages.Count} until now...");
                }
                catch (Exception e) {
                    Console.WriteLine($"An error happened: {e}");
                }
            }

            return messages;
        }

        static async Task Start(string[] args)
        {
            var config = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json", true, true)
                   .Build();

            var connectionString = config["serviceBusConnectionString"];

            var queueName = config["queueName"];

            if(!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(queueName))
            {
                await ReceiveMessages(connectionString, queueName);
            }

            Parser.Default.ParseArguments<ServiceBusConfiguration>(args)
                .WithParsed(async options =>
                {
                    await ReceiveMessages(options.ServiceBusConnectionString, options.QueueName);
                });
        }
    }
}
