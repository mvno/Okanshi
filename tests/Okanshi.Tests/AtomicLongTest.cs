using FluentAssertions;
using Xunit;

namespace Okanshi.Test
{
	public class AtomicLongTest
	{
		[Fact]
		public void Default_value_is_zero()
		{
			var atomicLong = new AtomicLong();

			var value = atomicLong.Get();

			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Default_value_is_correctly_assigned_when_specified_in_constructor(int expectedValue)
		{
			var atomicLong = new AtomicLong(expectedValue);

			var value = atomicLong.Get();

			value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Get_and_set_returns_the_original_value(int originalValue)
		{
			var atomicLong = new AtomicLong(originalValue);

			var value = atomicLong.GetAndSet(1000);

			value.Should().Be(originalValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Get_and_set_updates_the_value_as_specified(int newValue)
		{
			var atomicLong = new AtomicLong();

			atomicLong.GetAndSet(newValue);

			var value = atomicLong.Get();
			value.Should().Be(newValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Compare_and_set_updates_the_value_when_original_value_is_correct(int newValue)
		{
			var atomicLong = new AtomicLong();

			atomicLong.CompareAndSet(newValue, 0);

			var value = atomicLong.Get();
			value.Should().Be(newValue);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Compare_and_set_does_not_update_the_value_when_original_value_is_incorrect(int newValue)
		{
			var atomicLong = new AtomicLong();

			atomicLong.CompareAndSet(newValue, 100);

			var value = atomicLong.Get();
			value.Should().Be(0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(17)]
		[InlineData(1001)]
		[InlineData(-5)]
		public void Setting_the_value_updates_the_value_as_expected(int newValue)
		{
			var atomicLong = new AtomicLong();

			atomicLong.Set(newValue);

			var value = atomicLong.Get();
			value.Should().Be(newValue);
		}

		[Theory]
		[InlineData(1, 2)]
		[InlineData(10, 11)]
		[InlineData(17, 18)]
		[InlineData(1001, 1002)]
		[InlineData(-5)]
		public void Incrementing_the_value_increments_the_value(int originvalValue, int expectedValue)
		{
			var atomicLong = new AtomicLong(originvalValue);

			atomicLong.Increment();

			var value = atomicLong.Get();
			value.Should().Be(expectedValue);
		}

		[Theory]
		[InlineData(1, 2, 3)]
		[InlineData(10, 11, 21)]
		[InlineData(17, 18, 35)]
		[InlineData(1001, 1002, 2003)]
		[InlineData(-5, -1, -6)]
		public void Incrementing_the_value_by_the_specified_amount_increments_the_value_correctly(int originvalValue, int amount, int expectedValue)
		{
			var atomicLong = new AtomicLong(originvalValue);

			atomicLong.Increment(amount);

			var value = atomicLong.Get();
			value.Should().Be(expectedValue);
		}
	}
}
