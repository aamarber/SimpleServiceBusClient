using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.FileExtensions;
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
        static QueueClient queueClient;

        static IMessageReceiver messageReceiver;

        static async Task Main(string[] args)
        {
            try
            {
                await Start(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
                queueClient.CloseAsync();
            }
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

            messageReceiver = new MessageReceiver(connectionString, queueName, ReceiveMode.PeekLock);

            var messages = new List<MessageDto>();

            for (var i = 50; i > 0; i--)
            {
                try
                {
                    var message = await messageReceiver.ReceiveAsync();

                    var body = Encoding.UTF8.GetString(message.Body);

                    var settings = new Newtonsoft.Json.JsonSerializerSettings();
                    settings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;

                    var m = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageDto>(body.Substring(body.IndexOf("{")), settings);

                    messages.Add(m);
                }
                catch { }
            }

            Console.WriteLine(string.Join(",", messages.Select(x => x.metadata.site)));
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
