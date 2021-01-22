using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Ical.Net;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace Dashbrd.Shared.Modules.Calendar
{
    public partial class Calendar
    {
        [Inject]
        public IHttpClientFactory ClientFactory { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        private Timer _timer;

        public int UpdateInterval { get; set; } = 15;
        public List<string> Urls { get; set; } = new();
        public List<Occurrence> Events { get; set; }
        protected override async Task OnInitializedAsync()
        {
            Configuration.GetSection("Settings:Calendar").Bind(this);

            var span = TimeSpan.FromMinutes(UpdateInterval);
            _timer = new Timer(span.TotalMilliseconds);
            _timer.Elapsed += async (sender, args) =>
            {
                await GetCalendars();
                await InvokeAsync(StateHasChanged);
            };
            await GetCalendars();
            if (Urls?.Any() == true)
            {
                _timer.Start();
            }
        }

        private async Task GetCalendars()
        {
            var client = ClientFactory.CreateClient();
            var sb = new StringBuilder();
            foreach (var url in Urls)
            {
                var text = await client.GetStringAsync(url);
                sb.AppendLine(text);
            }
            var calendar = CalendarCollection.Load(sb.ToString());
            Events = calendar.GetOccurrences(DateTime.Now, DateTime.Now.AddMonths(2)).OrderBy(o=>o.Period.StartTime).Take(5).ToList();
            
        }
    }
}
