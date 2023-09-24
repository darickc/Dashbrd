using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using CSharpFunctionalExtensions;
using Dashbrd.Shared.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Dashbrd.Shared.Modules.PhotoprismBackgroundImageSlideshow
{
    public partial class PhotoprismBackgroundImageSlideshow
    {
        private string[] _transitions = new[] {
            "opacity",
            "slideFromRight",
            "slideFromLeft",
            "slideFromTop",
            "slideFromBottom",
            "slideFromTopLeft",
            "slideFromTopRight",
            "slideFromBottomLeft",
            "slideFromBottomRight",
            "flipX",
            "flipY"
        };

        private readonly string _transitionTimingFunction = "cubic-bezier(.17,.67,.35,.96)";
        private readonly string[] _animations = { "slide", "zoomOut", "zoomIn" };
        private string _transiationSpeed = "2s";
        private readonly string _backgroundAnimationDuration = "3s";
        private readonly string _backgroudSize = "contain";
        private readonly string _backgroundPosition = "center";

        private readonly Random _random = new();

        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private MessageService MessageService { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }

        private readonly List<string> _imageFiles = new();
        public ImageData Image1 { get; set; }
        public ImageData Image2 { get; set; }
        public ImageData Image3 { get; set; }

        public double SlideShowSpeed { get; set; } = 10;
        public double TransitionSpeed { get; set; } = 2;
        public string[] Transitions { get; set; }
        public string PhotoprismApiUrl { get; set; }
        public string ApiToken { get; set; }

        private bool _playSlideshow = true;
        private int _index = 0;
        private Timer _timer;
        private int _tick;

        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:PhotoprismBackgroundImageSlideshow").Bind(this);
            MessageService.OnMessage += MessageService_OnMessage;

            var timespan = TimeSpan.FromSeconds(SlideShowSpeed);
            SlideShowSpeed = timespan.TotalMilliseconds;
            timespan = TimeSpan.FromSeconds(TransitionSpeed);
            _transiationSpeed = $"{timespan.TotalSeconds}s";
            if (Transitions?.Length > 0)
            {
                _transitions = Transitions;
            }


            await base.OnInitializedAsync();
        }

        private async Task<Result> GetImagesToDisplay()
        {
            return await Result.Try(async () =>
            {
                var httpClient = HttpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-Session-ID", ApiToken);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var images = await httpClient.GetFromJsonAsync<List<PhotoprismImage>>($"{PhotoprismApiUrl}/api/v1/photos/view?count=1000&q=favorites", options);
                _imageFiles.AddRange(images.Select(i => i.Thumbs.Fit1920?.Src ?? i.Thumbs.Fit1280?.Src ?? i.Thumbs.Fit720?.Src));
                Shuffle(_imageFiles);
                _index = 0;
            });
        }

        private async void MessageService_OnMessage(object obj)
        {
            if (obj is PhotoprismBackgroundImageSlideshowMesage message)
            {
                if (message.DisplayImage)
                {
                    _timer.Stop();
                    _playSlideshow = false;
                    await UpdateImage(message.Image);
                    await UpdateImage();
                }
                else
                {
                    _playSlideshow = true;
                    await UpdateImage();
                    _timer.Start();
                }
                await InvokeAsync(StateHasChanged);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                await GetImagesToDisplay();
                if (_imageFiles.Any())
                {
                    
                    _timer = new Timer(SlideShowSpeed);
                    _timer.Elapsed += Timer_Elapsed;
                    _timer.AutoReset = false;
                    await Task.Delay(2000);
                    await LoadNextImage().Tap(image => Image1 = image).Tap(image => image.Show = true);
                    await LoadNextImage().Tap(image => Image2 = image);
                }
                await InvokeAsync(StateHasChanged);
            }
            if (_imageFiles.Count > 0)
            {
                if (_playSlideshow)
                {
                    _timer.Start();
                }
            }
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            await UpdateImage();
            if(_playSlideshow)
                _timer.Start();
        }

        private async Task UpdateImage(string nextImage = null)
        {

            var mod = _tick % 3;
            switch (mod)
            {
                case 0:
                    Image1.Show = false;
                    Image2.Show = true;
                    await LoadNextImage(nextImage).Tap(image => Image3 = image);
                    _tick++;
                    break;
                case 1:
                    Image2.Show = false;
                    Image3.Show = true;
                    await LoadNextImage(nextImage).Tap(image => Image1 = image);
                    _tick++;
                    break;
                case 2:
                    Image3.Show = false;
                    Image1.Show = true;
                    await LoadNextImage(nextImage).Tap(image => Image2 = image);
                    _tick = 0;
                    break;
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task<Result<ImageData>> LoadNextImage(string nextImage = null)
        {
            return await Result.Success(nextImage)
                .Ensure(image => !string.IsNullOrEmpty(image), "Invalid image")
                .OnFailureCompensate(_ => GetImageData(_imageFiles[_index++]))
                .CheckIf(_ => _index >= _imageFiles.Count, _=> GetImagesToDisplay())
                .Map(CreateImage);
        }

        private async Task<Result<string>> GetImageData(string fileLocation)
        {
            return await Result.Try(async () =>
            {
                var httpClient = HttpClientFactory.CreateClient();
                //httpClient.DefaultRequestHeaders.Add("X-Download-Token", _downloadToken);
                var httpResponseMessage = await httpClient.GetAsync($"{PhotoprismApiUrl}{fileLocation}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    var data = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                    var contentType = "";
                    if (httpResponseMessage.Content.Headers.TryGetValues("Content-Type", out var values))
                    {
                        contentType = values.FirstOrDefault();
                    }
                    return $"data:{contentType};base64,{Convert.ToBase64String(data)}";
                }

                return "";
            });
        }

        private string GetTransition()
        {
            var index = _random.Next(0, _transitions.Length);
            return _transitions[index];
        }

        private string GetAnimation()
        {
            var index = _random.Next(0, _animations.Length - 1);
            return _animations[index];
        }

        private static void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private ImageData CreateImage(string data)
        {
            var transition = GetTransition();
            // var animation = GetAnimation();
            var image = new ImageData
            {
                Transition = $"t{transition}",
                // Animation = animation,
                Style = $"background-image: url({data});background-size:{_backgroudSize}; background-position: {_backgroundPosition};",
                TransitionStyle = $"animation-duration: {_transiationSpeed}; transition-duration: {_transiationSpeed}; animation-timing-function: {_transitionTimingFunction};"
            };
            image.Style += $"animation-duration: {_backgroundAnimationDuration};animation-delay: {_transiationSpeed};";

            return image;
        }

        public class ImageData
        {
            public bool Show { get; set; }
            public string Style { get; set; }
            public string TransitionStyle { get; set; }
            public string Transition { get; set; }
            public string Animation { get; set; }
            public string BackgroundImage { get; set; }

            // public ImageData(string data, string transition, string transitionSpeed, string transitionTimingFunction, string backgroungSize, string backgroundPosition)
            // {
            //     Transition = transition;
            //     Style = $"background-image: url({data});background-size:{backgroungSize}; background-position: {backgroundPosition};";
            //     TransitionStyle = $"animation-duration: {transitionSpeed}; transition-duration: {transitionSpeed}; animation-timing-function: {transitionTimingFunction};";
            //     Style += $"animation-duration: {_backgroundAnimationDuration}";
            // }
        }
    }
}


