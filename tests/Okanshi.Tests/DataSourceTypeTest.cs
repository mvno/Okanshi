using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class DataSourceTypeTest
	{
		[Fact]
		public void Gauge_type_has_correct_key_and_value()
		{
			DataSourceType.Gauge.Should().Be(new Tag("dataSource", "gauge"));
		}

		[Fact]
		public void Counter_type_has_correct_key_and_value()
		{
			DataSourceType.Counter.Should().Be(new Tag("dataSource", "counter"));
		}

		[Fact]
		public void Rate_type_has_correct_key_and_value()
		{
			DataSourceType.Rate.Should().Be(new Tag("dataSource", "rate"));
		}
	}
}
