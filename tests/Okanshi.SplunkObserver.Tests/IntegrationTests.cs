﻿using System;
using Newtonsoft.Json;
using Okanshi.SplunkObservers;
using Xunit;

namespace Okanshi.SplunkObservers.Tests
{
    public class IntegrationTests
    {
        /// <summary>
        /// Manual check data is found in Splunk. Assume local splunk is running.
        /// In order to run these tests set up a running splunk
        /// * > docker pull splunk/splunk
        /// * > docker run -d -p 8000:8000 -p 8088:8088 -e 'SPLUNK_START_ARGS=--accept-license' -e 'SPLUNK_PASSWORD=3100RoskildeBank' splunk/splunk:latest
        /// * log onto splunk with user: "admin" and pw: "3100RoskildeBank"
        /// * Add a http event collector (more information on github)
        /// * Run test
        /// * check for splunk data by searching "index=*"
        /// </summary>
        [Fact]
        public async void When_sending_data_to_splunk_Then_see_data_in_splunk()
        {
            var eventCollectorToken = "f519216f-6641-4468-ba98-52bb614711f5"; // token configured in splunk when setting up the collector
            var observer = new SplunkObserver(new HttpPoster(new Uri("http://localhost:8088/"), eventCollectorToken), JsonConvert.SerializeObject);

            await observer.Update(new[] {SplunkObserverTest.CreateMetrics("foo", "bar")});
        }
    }
}
