using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Server;
using System.Collections.Generic;
using System.Linq;
using Dashbrd.Shared.Common;
using Dashbrd.Shared.Modules.Coop;
using Newtonsoft.Json;
using System.Net.Http;
using Dashbrd.Shared.Modules.BackgroundImageSlideshow;
using System.Net.Http.Json;
using Dashbrd.Shared.Modules.PhotoprismBackgroundImageSlideshow;

namespace Dashbrd.Shared.Modules.Jellyfin
{
    public partial class Jellyfin : IDisposable
    {
        [Inject] private IConfiguration Configuration { get; set; }

        [Inject] private ILogger<Jellyfin> Logger { get; set; }
        [Inject] private MessageService MessageService { get; set; }
        [Inject] private IHttpClientFactory HttpClientFactory { get; set; }
        private IManagedMqttClient _client;
        public string MqttUri { get; set; }
        public string MqttUser { get; set; }
        public string MqttPassword { get; set; }
        public int MqttPort { get; set; } = 1883;
        public bool MqttSecure { get; set; }
        public string MqttBaseTopic { get; set; }
        public string MqttClientId { get; set; }
        public string ApiKey { get; set; }
        public bool Connected { get; set; }
        private Dictionary<string, Func<JellyfinNotificationData, Task>> _messages = new();
        public JellyfinNotificationData NotificationData { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:Jellyfin").Bind(this);
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
            _client.ApplicationMessageReceivedAsync += MessageReceivedAsync;
            _client.ConnectedAsync += ClientOnConnectedAsync;
            _client.DisconnectedAsync += ClientOnDisconnectedAsync;
            _client.ConnectingFailedAsync += _client_ConnectingFailedAsync;
            await _client.SubscribeAsync($"{MqttBaseTopic}/#");
            await _client.StartAsync(managedOptions);
        }

        private Task _client_ConnectingFailedAsync(ConnectingFailedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task ClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private Task ClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (_messages.TryGetValue(arg.ApplicationMessage.Topic, out var action))
            {
                await InvokeAsync(async () =>
                {
                   await arg.ApplicationMessage.Payload.Decode()
                        .Map(JsonConvert.DeserializeObject<JellyfinNotificationData>)
                        .Tap(action.Invoke);
                    StateHasChanged();
                });
            }
        }

        private void SetupMessageHandlers()
        {
            _messages.Add($"{MqttBaseTopic}/PlaybackStart", OnPlaybackStart);
            _messages.Add($"{MqttBaseTopic}/PlaybackStop", OnPlaybackStop);
            _messages.Add($"{MqttBaseTopic}/PlaybackProgress", OnPlaybackPosition);
        }

        private Task OnPlaybackPosition(JellyfinNotificationData d)
        {
            if (NotificationData?.ItemId == d.ItemId)
            {
                NotificationData!.PlaybackPosition = d.PlaybackPosition;
                NotificationData.IsPaused = d.IsPaused;
            }
            return Task.CompletedTask;
        }

        private Task OnPlaybackStop(JellyfinNotificationData arg)
        {
            NotificationData = null;
            MessageService.SendMessage(new PhotoprismBackgroundImageSlideshowMesage
            {
                DisplayImage = false
            });
            return Task.CompletedTask;
        }

        private async Task OnPlaybackStart(JellyfinNotificationData d)
        {
            await Result.Success(d)
                .Tap(notification => NotificationData = notification)
                .CheckIf(notification => notification.ItemType == "Episode", GetAncestors)
                .Bind(GetBackdropImage)
                .Tap(message=> MessageService.SendMessage(message));
        }

        private async Task<Result<PhotoprismBackgroundImageSlideshowMesage>> GetBackdropImage(JellyfinNotificationData d)
        {
            return await Result.Try(async () =>
            {
                using var client = HttpClientFactory.CreateClient();
                HttpResponseMessage response = await client.GetAsync(d.Backdrop);
                response.EnsureSuccessStatusCode();
                var type = response.Content.Headers.ContentType;
                var data = await response.Content.ReadAsByteArrayAsync();
                return new PhotoprismBackgroundImageSlideshowMesage
                {
                    DisplayImage = true,
                    Image = $"data:{type};base64,{Convert.ToBase64String(data)}"
                };
            });
        }

        private async Task<Result> GetAncestors(JellyfinNotificationData d)
        {
            string error;
            return await Result.Try(async () =>
            {
                using var client = HttpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("X-Emby-Token",ApiKey);
                var items = await client.GetFromJsonAsync<Item[]>($"{d.Server}/Items/{d.ItemId}/Ancestors");
                var series = items?.FirstOrDefault(i => i.Type == "Series");
                d.SeriesId = series?.Id;
            })
                .OnFailure(e=>error = e);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
