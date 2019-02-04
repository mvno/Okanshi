using System;
using Xunit;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NSubstitute;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Okanshi.SplunkObservers.Tests
{
    public class SplunkObserverTest
    {
        private string jsonSentToSplunk = null;

        [Fact]
        public async void When_creating_an_event_from_a_measurement_Then_pull_up_tags_and_values_if_no_name_collision()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("tag", "val")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_tagname_is_an_existing_field_Then_place_in_tags_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("name", "val")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""name"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_tagname_is_tags_Then_place_in_tags_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("tags", "val")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""tags"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_tagname_is_values_Then_place_in_tags_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("values", "val")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""values"":""tagvalue""},""val"":23,""valueNoCollision"":42}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_tagname_is_values_And_valuename_is_existing_field_Then_place_in_tags_collection_And_values_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("values", "name")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tagNoCollision"":""noCollision"",""tags"":{""values"":""tagvalue""},""valueNoCollision"":42,""values"":{""name"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_valuename_is_existing_field_Then_place_in_values_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("tag", "name")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""valueNoCollision"":42,""values"":{""name"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_valuename_is_tags_Then_place_in_values_collection()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("tag", "tags")});

            var expected = @"{""event"":"
                           + @"{""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tag"":""tagvalue"",""tagNoCollision"":""noCollision"",""valueNoCollision"":42,""values"":{""tags"":23}}"
                           + @"}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_sending_several_measurements_Then_send_to_splunk_as_one_event()
        {
            var observer = CreateObserver();

            await observer.Update(new[] {CreateMetrics("someTag", "someValue"), CreateMetrics("someTag", "someValue")});

            var expected = @"{""event"":{"
                           + @"""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"",""someTag"":""tagvalue"",""tagNoCollision"":""noCollision"",""someValue"":23,""valueNoCollision"":42}}"
                           + @" "
                           + @"{""event"":{"
                           + @"""name"":""name"",""timeStamp"":""2022-02-03T12:33:44.234Z"",""someTag"":""tagvalue"",""tagNoCollision"":""noCollision"",""someValue"":23,""valueNoCollision"":42}}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void When_no_measurements_Then_do_not_send_to_splunk()
        {
            var httpPoster = Substitute.For<IHttpPoster>();
            var observer = new SplunkObserver(httpPoster, JsonConvert.SerializeObject);

            await observer.Update(new Metric[0]);

            httpPoster.DidNotReceive().SendToSplunk(Arg.Any<string>());
        }

        [Fact]
        public async void When_using_func_Then_output_can_be_controlled()
        {
            var converter = new IsoDateTimeConverter();
            converter.DateTimeStyles = DateTimeStyles.AssumeUniversal;

            var observer = new SplunkObserver(CreateHttpPoster(), 
                metric => new Dictionary<string, object>()
                {
                    {"name", metric.Name},
                    {"timeStamp", metric.Timestamp.LocalDateTime},
                    {"tags", metric.Tags.ToDictionary(x => x.Key, x => x.Value)},
                    {"values", metric.Values.ToDictionary(x => x.Name, x => x.Value)},
                },
                x => JsonConvert.SerializeObject(x, converter));

            await observer.Update(new[] { CreateMetrics("tag", "name") });

            var expected = @"{""event"":"
                           + @"{""name"":""name"","
                           + @"""timeStamp"":""2022-02-03T12:33:44.234Z"","
                           + @"""tags"":{""tag"":""tagvalue"",""tagNoCollision"":""noCollision""}," 
                           + @"""values"":{""name"":23,""valueNoCollision"":42}}"
                           + "}";
            Assert.Equal(expected, jsonSentToSplunk);
        }

        [Fact]
        public async void must_fail()
        {
            Assert.False(true);
        }

        private SplunkObserver CreateObserver()
        {
            var converter = new IsoDateTimeConverter();
            converter.DateTimeStyles = DateTimeStyles.AssumeUniversal;
            var observer = new SplunkObserver(CreateHttpPoster(), x => JsonConvert.SerializeObject(x, converter));
            return observer;
        }


        private IHttpPoster CreateHttpPoster()
        {
            var httpPoster = Substitute.For<IHttpPoster>();
            httpPoster.SendToSplunk(Arg.Any<string>()).Returns("").AndDoes(x => {
                jsonSentToSplunk = (string)x.Args()[0];
            });
            return httpPoster;
        }

        public static Metric CreateMetrics(string tagname, string valueName)
        {
            var date = new DateTimeOffset(2022, 2, 3, 12, 33, 44, 234, TimeSpan.Zero);
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
