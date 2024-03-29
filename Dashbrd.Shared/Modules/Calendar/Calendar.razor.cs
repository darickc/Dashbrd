﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Ical.Net;
using Ical.Net.DataTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dashbrd.Shared.Modules.Calendar
{
    public partial class Calendar
    {
        private const string CalendarCache = "CalendarCache";
        [Inject]
        public IHttpClientFactory ClientFactory { get; set; }
        [Inject] private IConfiguration Configuration { get; set; }
        [Inject] private ILogger<Calendar> Logger { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
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
            try
            {
                if (MemoryCache.TryGetValue(CalendarCache, out List<Occurrence> events))
                {
                    Events = events;
                }
                else
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
                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(UpdateInterval - 5));
                    MemoryCache.Set(CalendarCache, Events, cacheEntryOptions);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e,e.Message);
            }
        }
    }
}
