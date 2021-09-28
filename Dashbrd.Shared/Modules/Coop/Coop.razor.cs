using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace Dashbrd.Shared.Modules.Coop
{
    public partial class Coop : IDisposable
    {
        [Inject] private IConfiguration Configuration { get; set; }

        [Inject]
        private ILogger<Coop> Logger { get; set; }
        private IManagedMqttClient _client;

        public string MqttUri { get; set; }
        public string MqttUser { get; set; }
        public string MqttPassword { get; set; }
        public int MqttPort { get; set; } = 1883;
        public bool MqttSecure { get; set; }
        public string MqttBaseTopic { get; set; }
        public string MqttClientId { get; set; }
        public bool Connected { get; set; }

        public int WaterLevel { get; set; }
        public int FoodLevel { get; set; }
        public string DoorState { get; set; }
        public bool? WaterHeaterState { get; set; }
        public int WaterTemp { get; set; }
        public int Temp { get; set; }

        private Dictionary<string, Action<string>> Messages = new();

        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:Coop").Bind(this);
            if (string.IsNullOrEmpty(MqttUri))
            {
                Logger.LogInformation("Mqtt Uri is empty");
                return;
            }

            SetupMessageHandlers();
            var messageBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(MqttUri, MqttPort)
                .WithCleanSession();

            if (!string.IsNullOrEmpty(MqttUser) && !string.IsNullOrEmpty(MqttPassword))
            {
                messageBuilder.WithCredentials(MqttUser, MqttPassword);
            }

            if (!string.IsNullOrEmpty(MqttClientId))
            {
                messageBuilder.WithClientId(MqttClientId);
            }
            if (MqttSecure)
            {
                messageBuilder.WithTls();
            }
            var options = messageBuilder.Build();

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(options)
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();
            _client.UseApplicationMessageReceivedHandler(Handler);
            _client.UseConnectedHandler(async e =>
            {
                await InvokeAsync(async () =>
                {
                    Connected = true;
                    Logger.LogInformation("Connected successfully with MQTT Brokers.");
                    await _client.SubscribeAsync(new MqttTopicFilterBuilder()
                        .WithTopic($"{MqttBaseTopic}/#")
                        .WithAtLeastOnceQoS()
                        .Build());
                    StateHasChanged();
                });
            });

            _client.UseDisconnectedHandler(async e =>
            {
                await InvokeAsync(() =>
                {
                    Logger.LogInformation($"Disconnected from MQTT Brokers: {e.Reason}, {e.Exception?.Message}");
                    Connected = false;
                    StateHasChanged();
                });
            });

            await _client.StartAsync(managedOptions);
        }
        

        private void SetupMessageHandlers()
        {
            Messages.Add($"{MqttBaseTopic}/water/temp", (e) => e.ToInt().Tap(l => WaterTemp = l));
            Messages.Add($"{MqttBaseTopic}/water/level", (e) => e.ToInt().Tap(l => WaterLevel = l));
            Messages.Add($"{MqttBaseTopic}/water/heater", (e) => e.ToBool().Tap(l => WaterHeaterState = l));
            Messages.Add($"{MqttBaseTopic}/temp", (e) => e.ToInt().Tap(l => Temp = l));
            Messages.Add($"{MqttBaseTopic}/food/level", (e) => e.ToInt().Tap(l => FoodLevel = l));
            Messages.Add($"{MqttBaseTopic}/door/state", (e) => DoorState = e);
        }

        private async Task Handler(MqttApplicationMessageReceivedEventArgs e)
        {
            if (Messages.TryGetValue(e.ApplicationMessage.Topic, out var action))
            {
                await InvokeAsync(() =>
                {
                    e.ApplicationMessage.Payload.Decode().Tap(action.Invoke);
                    StateHasChanged();
                });
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
