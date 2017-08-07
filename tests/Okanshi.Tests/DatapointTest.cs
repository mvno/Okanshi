using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
    public class DatapointTest
    {
        [Fact]
        public void Empty_datapoint_has_timestamp_zero()
        {
            Datapoint.Empty.Timestamp.Should().NotHaveValue();
        }

        [Fact]
        public void Empty_datapoint_has_value_minus_one()
        {
            Datapoint.Empty.Value.Should().Be(-1);
        }
    }
}