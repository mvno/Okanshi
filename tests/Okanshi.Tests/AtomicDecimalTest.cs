using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class AtomicDecimalTest
	{
		[Fact]
		public void Default_value_is_zero()
		{
			var atomicDecimal = new AtomicDecimal();

			var value = atomicDecimal.Get();

			value.Should().Be(default(decimal));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Default_value_is_correctly_assigned_when_specified_in_constructor(double expectedValue)
		{
			var atomicDecimal = new AtomicDecimal(new decimal(expectedValue));

			var value = atomicDecimal.Get();

			value.Should().Be(new decimal(expectedValue));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Get_and_set_returns_the_original_value(double originalValue)
		{
			var atomicDecimal = new AtomicDecimal(new decimal(originalValue));

			var value = atomicDecimal.GetAndSet(1000);

			value.Should().Be(new decimal(originalValue));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Get_and_set_updates_the_value_as_specified(double newValue)
		{
			var atomicDecimal = new AtomicDecimal();

			atomicDecimal.GetAndSet(new decimal(newValue));

			var value = atomicDecimal.Get();
			value.Should().Be(new decimal(newValue));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Compare_and_set_updates_the_value_when_original_value_is_correct(double newValue)
		{
			var atomicDecimal = new AtomicDecimal();

			atomicDecimal.CompareAndSet(new decimal(newValue), 0m);

			var value = atomicDecimal.Get();
			value.Should().Be(new decimal(newValue));
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Compare_and_set_does_not_update_the_value_when_original_value_is_incorrect(double newValue)
		{
			var atomicDecimal = new AtomicDecimal();

			atomicDecimal.CompareAndSet(new decimal(newValue), 100m);

			var value = atomicDecimal.Get();
			value.Should().Be(0m);
		}

		[Theory]
		[InlineData(1.0)]
		[InlineData(10.2)]
		[InlineData(17.22)]
		[InlineData(1001.01)]
		[InlineData(-5.1)]
		public void Setting_the_value_updates_the_value_as_expected(double newValue)
		{
			var atomicDecimal = new AtomicDecimal();

			atomicDecimal.Set(new decimal(newValue));

			var value = atomicDecimal.Get();
			value.Should().Be(new decimal(newValue));
		}

		[Theory]
		[InlineData(1.0, 2.0)]
		[InlineData(10.2, 11.2)]
		[InlineData(17.22, 18.22)]
		[InlineData(1001.01, 1002.01)]
		[InlineData(-5.1, -4.1)]
		public void Incrementing_the_value_increments_the_value(double originvalValue, double expectedValue)
		{
			var atomicDecimal = new AtomicDecimal(new decimal(originvalValue));

			atomicDecimal.Increment();

			var value = atomicDecimal.Get();
			value.Should().Be(new decimal(expectedValue));
		}

		[Theory]
		[InlineData(1.0, 2.0, 3.0)]
		[InlineData(10.2, 11.2, 21.4)]
		[InlineData(17.22, 18.22, 35.44)]
		[InlineData(1001.01, 1002.01, 2003.02)]
		[InlineData(-5.1, -1.1, -6.2)]
		public void Incrementing_the_value_by_the_specified_amount_increments_the_value_correctly(double originvalValue, double amount, double expectedValue)
		{
			var atomicDecimal = new AtomicDecimal(new decimal(originvalValue));

			atomicDecimal.Increment(new decimal(amount));

			var value = atomicDecimal.Get();
			value.Should().Be(new decimal(expectedValue));
		}
	}
}
