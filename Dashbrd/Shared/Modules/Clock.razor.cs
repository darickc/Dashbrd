using System;
using System.Timers;

namespace Dashbrd.Shared.Modules
{
    public partial class Clock
    {
        public DateTime DateTime { get; set; } = DateTime.Now;
        private Timer _timer;

        protected override void OnInitialized()
        {
            _timer = new Timer(200);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private async void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime != DateTime.Now)
            {
                DateTime = DateTime.Now;
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
