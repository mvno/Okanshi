using System;
using Xunit;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using Okanshi.Observers;

namespace Okanshi.SplunkObservers.Tests
{
    public class SplunkObserverTest
    {
        private string jsonSentToSplunk = null;

        [Fact]
        public void When_creating_an_event_from_a_measurement_Then_pull_up_tags_and_values_if_no_name_collision()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("tag", "val")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_tagname_is_an_existing_field_Then_place_in_tags_collection()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("name", "val")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""name"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_tagname_is_tags_Then_place_in_tags_collection()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("tags", "val")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""tags"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_tagname_is_values_Then_place_in_tags_collection()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("values", "val")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""values"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void
            When_tagname_is_values_And_valuename_is_existing_field_Then_place_in_tags_collection_And_values_collection()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("values", "name")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""values"":""tagvalue""},""valueNoCollision"":42,""values"":{""name"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_valuename_is_existing_field_Then_place_in_values_collection()
        {
            var observer = new Observers.SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("tag", "name")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""valueNoCollision"":42,""values"":{""name"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_valuename_is_tags_Then_place_in_values_collection()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("tag", "tags")}).GetAwaiter().GetResult();

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""valueNoCollision"":42,""values"":{""tags"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_sending_several_measurements_Then_send_to_splunk_as_one_event()
        {
            var observer = new SplunkObserver(CreateHttpPoster().Object, JsonConvert.SerializeObject);

            observer.Update(new[] {CreateMetrics("someTag", "someValue"), CreateMetrics("someTag", "someValue")}).GetAwaiter()
                .GetResult();

            var expected = @"{""event"":{"
                           + @"""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"",""someTag"":""tagvalue"",""tagNoCollision"":""noCollision"",""someValue"":23,""valueNoCollision"":42}}"
                           + @" "
                           + @"{""event"":{"
                           + @"""name"":""name"",""timeStamp"":""03-02-2022 12:33:44.2340000"",""someTag"":""tagvalue"",""tagNoCollision"":""noCollision"",""someValue"":23,""valueNoCollision"":42}}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public void When_no_measurements_Then_do_not_send_to_splunk()
        {
            var httpPoster = new Mock<IHttpPoster>();
            var observer = new SplunkObserver(httpPoster.Object, null);

            observer.Update(new Metric[0]);

            httpPoster.VerifyAll();
        }

        private Mock<IHttpPoster> CreateHttpPoster()
        {
            var httpPoster = new Mock<IHttpPoster>();
            httpPoster.Setup(x => x.SendToSplunk(It.IsAny<string>())).Returns((string x) =>
            {
                jsonSentToSplunk = x;
                return "";
            });
            return httpPoster;
        }

        internal static Metric CreateMetrics(string tagname, string valueName)
        {
            var date = new DateTimeOffset(new DateTime(2022, 2, 3, 12, 33, 44).AddMilliseconds(234));
            return new Metric("name",
                date,
                new List<Tag>()
                {
                    new Tag(tagname, "tagvalue"),
                    new Tag("tagNoCollision", "noCollision")
                },
                new IMeasurement[]
                {
                    new Measurement<int>(valueName, 23),
                    new Measurement<int>("valueNoCollision", 42)
                });
        }
    }
}
