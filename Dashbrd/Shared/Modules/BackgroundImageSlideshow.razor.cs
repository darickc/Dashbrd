using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Dashbrd.Shared.Modules
{
    public partial class BackgroundImageSlideshow
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

        private string _transitionTimingFunction = "cubic-bezier(.17,.67,.35,.96)";
        private string[] _animations = {"slide", "zoomOut", "zoomIn"};
        private string _transiationSpeed = "2s";
        private string _backgroundAnimationDuration = "3s";
        private string _backgroudSize = "contain";
        private string _backgroundPosition = "center";
        private string[] _fileExtensions = { "jpg","jpeg","bmp","gif","png" };

        private Random _random = new();

        [Inject] private IConfiguration Configuration { get; set; }

        private List<string> _imageFiles = new();
        public ImageData Image1 { get; set; }
        public ImageData Image2 { get; set; }
        public ImageData Image3 { get; set; }

        public double SlideShowSpeed { get; set; } = 10;
        public double TransitionSpeed { get; set; } = 2;
        public string[] Transitions { get; set; }
        public string[] FileExtensions { get; set; }
        public string[] ImagePaths { get; set; }

        private int _index = 0;
        private Timer _timer;
        private int _tick;
        protected override void OnInitialized()
        {
            // await Task.Delay(2000);
            Configuration.GetSection("Settings:BackgroundSlideShow").Bind(this);
            var timespan = TimeSpan.FromSeconds(SlideShowSpeed);
            SlideShowSpeed = timespan.TotalMilliseconds;
            timespan = TimeSpan.FromSeconds(TransitionSpeed);
            _transiationSpeed = $"{timespan.TotalSeconds}s";
            if (Transitions?.Length > 0)
            {
                _transitions = Transitions;
            }

            if (FileExtensions?.Length > 0)
            {
                _fileExtensions = FileExtensions;
            }

            if (ImagePaths?.Any() == true)
            {
                foreach (var imagePath in ImagePaths)
                {
                    foreach (var fileExtension in _fileExtensions)
                    {
                        _imageFiles.AddRange(Directory.GetFiles(imagePath, $"*.{fileExtension}", SearchOption.AllDirectories));
                    }
                }

                Shuffle(_imageFiles);
                _timer = new Timer(SlideShowSpeed);
                _timer.Elapsed += Timer_Elapsed;
                _timer.AutoReset = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            
            if (_imageFiles.Count > 0)
            {
                if (firstRender)
                {
                    await Task.Delay(2000);
                    await LoadNextImage().Tap(image => Image1 = image).Tap(image => image.Show = true);
                    await LoadNextImage().Tap(image => Image2 = image);
                    await InvokeAsync(StateHasChanged);
                }
                _timer.Start();
            }
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await UpdateImage();
        }

        private async Task UpdateImage()
        {
            var mod = _tick % 3;
            switch (mod)
            {
                case 0:
                    Image1.Show = false;
                    Image2.Show = true;
                    await LoadNextImage().Tap(image => Image3 = image);
                    _tick++;
                    break;
                case 1:
                    Image2.Show = false;
                    Image3.Show = true;
                    await LoadNextImage().Tap(image => Image1 = image);
                    _tick++;
                    break;
                case 2:
                    Image3.Show = false;
                    Image1.Show = true;
                    await LoadNextImage().Tap(image => Image2 = image);
                    _tick = 0;
                    break;
            }
            
            await InvokeAsync(StateHasChanged);
        }

        private async Task<Result<ImageData>> LoadNextImage()
        {
            return await GetImageData(_imageFiles[_index++])
                .TapIf(data => _index >= _imageFiles.Count, data => _index = 0)
                .Map(CreateImage);
        }

        private async Task<Result<string>> GetImageData(string fileLocation)
        {
            return await Result.Try(async () =>
            {
                var fileInfo = new FileInfo(fileLocation);
                var data = await File.ReadAllBytesAsync(fileLocation);
                return $"data:image/{fileInfo.Extension};base64,{Convert.ToBase64String(data)}";
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


