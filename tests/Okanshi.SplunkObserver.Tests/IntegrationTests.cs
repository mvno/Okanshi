using Newtonsoft.Json;
using Okanshi.Observers;
using Xunit;

namespace Okanshi.SplunkObservers.Tests
{
    public class IntegrationTests
    {
        /// <summary>
        /// Manual check data is found in splunk. Assume local splunk is running.
        /// In order to run these tests set up a running splunk
        /// * docker pull splunk/splunk
        /// * docker run -d -p 8000:8000 -p 8088:8088 -e 'SPLUNK_START_ARGS=--accept-license' -e 'SPLUNK_PASSWORD=3100RoskildeBank' splunk/splunk:latest
        /// * log onto splunk: user: "admin" and pw: "3100RoskildeBank"
        /// * Add a http event collector
        /// * more information on github
        /// </summary>
        [Fact]
        public async void When_sending_data_to_splunk_Then_see_data_in_splunk()
        {
            var eventCollectorToken = "f519216f-6641-4468-ba98-52bb614711f5";
            var observer = new Observers.SplunkObserver(new HttpPoster(Protocol.Http, "localhost", 8088, eventCollectorToken), JsonConvert.SerializeObject);

            await observer.Update(new[] {SplunkObserverTest.CreateMetrics("foo", "bar")});
        }
    }
}
