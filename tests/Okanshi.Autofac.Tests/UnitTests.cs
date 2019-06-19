using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using System.Linq;
using Autofac;
using Okanshi.Autofac;
using Xunit.Abstractions;

namespace Okanshi.Autofac.Tests
{
    public class UnitTests
    {
        private readonly ITestOutputHelper Console;

        public UnitTests(ITestOutputHelper output)
        {
            Console = output;
        }

        [Fact]
        public void When_instantiating_only_a_Then_only_time_measurements_of_a()
        {
           ConfigureAutofacWithOkanshi(MeasurementStyleKind.CountInstantiations, out var registry, out var container);
            container.Resolve<A>();
            container.Resolve<A>();

            Assert.Equal(new[] {"Okanshi.Autofac.Tests.A"}, registry.GetRegisteredMonitors().Select(x => x.Config.Tags.Single().Value));
            var measurements = registry.GetRegisteredMonitors().Single().GetValues();
            Assert.Equal(2, (long)measurements.Single(x=>x.Name=="value").Value);
        }

        [Fact]
        public void When_instantiating_only_a_Then_only_count_measurements_of_a()
        {
            ConfigureAutofacWithOkanshi(MeasurementStyleKind.CountAndTimeInstantiations, out var registry, out var container);
            container.Resolve<A>();
            container.Resolve<A>();

            Assert.Equal(new[] { "Okanshi.Autofac.Tests.A" }, registry.GetRegisteredMonitors().Select(x => x.Config.Tags.Single().Value));
            var measurements = registry.GetRegisteredMonitors().Single().GetValues();
            Assert.Equal(2L, measurements.Single(x => x.Name == "count").Value);
        }

        [Fact]
        public void When_instantiating_a_and_b_Then_only_count_measurements_of_a_and_b()
        {
            ConfigureAutofacWithOkanshi(MeasurementStyleKind.CountInstantiations, out var registry, out var container);
            container.Resolve<B>();

            Assert.Equal(new[] { "Okanshi.Autofac.Tests.A", "Okanshi.Autofac.Tests.B" }, registry.GetRegisteredMonitors().Select(x => x.Config.Tags.Single().Value));
            var measurements = registry.GetRegisteredMonitors()
                .SelectMany(x => x.GetValues())
                .Where(x => x.Name == "value")
                .Select(x => x.Value)
                .Cast<long>();
            Assert.Equal(new[]{1L,1}, measurements);
        }

        [Fact]
        public void When_instantiating_a_and_b_Then_only_time_measurements_of_a_and_b()
        {
            ConfigureAutofacWithOkanshi(MeasurementStyleKind.CountAndTimeInstantiations, out var registry, out var container);
            container.Resolve<B>();

            Assert.Equal(new[] { "Okanshi.Autofac.Tests.A", "Okanshi.Autofac.Tests.B" }, registry.GetRegisteredMonitors().Select(x => x.Config.Tags.Single().Value));
            var measurements = registry.GetRegisteredMonitors()
                .SelectMany(x => x.GetValues())
                .Where(x => x.Name == "count")
                .Select(x => x.Value)
                .Cast<long>();
            Assert.Equal(new[] { 1L, 1 }, measurements);
        }

        [Fact]
        public void When_using_none_dont_do_anything()
        {
            ConfigureAutofacWithOkanshi(MeasurementStyleKind.None, out var registry, out var container);
            container.Resolve<A>();
            container.Resolve<A>();

            Assert.Equal(0, registry.GetRegisteredMonitors().Length);
        }

        void ConfigureAutofacWithOkanshi(MeasurementStyleKind style, out OkanshiMonitorRegistry registry, out IContainer container)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<A>().AsSelf();
            builder.RegisterType<B>().AsSelf();
            builder.RegisterType<C>().AsSelf();
            builder.RegisterType<D>().AsSelf();
            builder.RegisterType<E>().AsSelf();
            builder.RegisterType<F>().AsSelf();

            builder.RegisterType<Q>().AsSelf();
            builder.RegisterType<R>().AsSelf();
            builder.RegisterType<S>().AsSelf();
            builder.RegisterType<BigWig>().AsSelf();

            registry = new OkanshiMonitorRegistry();
            var fac = new Okanshi.MonitorFactory(registry, new Tag[0]);
            var okanshiAutofac = new OkanshiAutofac(new OkanshiAutofacOptions()
            {
                MeasurementStyle = style,
                CountFactory = (s, t) => fac.Counter(s, t),
                TimerFactory = (s, t) => fac.Timer(s, t),
            });
            builder.RegisterModule(okanshiAutofac);

            container = builder.Build();
        }

        [Fact]
        public  void Measure_performance_overhead_of_okanshi_integration()
        {
            int n = 10_000;
            BuildAndRun(n, MeasurementStyleKind.None, false);

            Console.WriteLine($"n = {n}");
            BuildAndRun(n, MeasurementStyleKind.None, false);
            BuildAndRun(n, MeasurementStyleKind.CountInstantiations);
            BuildAndRun(n, MeasurementStyleKind.CountAndTimeInstantiations);
            Console.WriteLine("");
            BuildAndRun(n, MeasurementStyleKind.None);
            BuildAndRun(n, MeasurementStyleKind.CountInstantiations);
            BuildAndRun(n, MeasurementStyleKind.CountAndTimeInstantiations);
        }

        private void BuildAndRun(int max, MeasurementStyleKind measurementStyleKind, bool register = true)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType<A>().AsSelf();
            builder.RegisterType<B>().AsSelf();
            builder.RegisterType<C>().AsSelf();
            builder.RegisterType<D>().AsSelf();
            builder.RegisterType<E>().AsSelf();
            builder.RegisterType<F>().AsSelf();

            builder.RegisterType<Q>().AsSelf();
            builder.RegisterType<R>().AsSelf();
            builder.RegisterType<S>().AsSelf();
            builder.RegisterType<BigWig>().AsSelf();

            OkanshiAutofac okanshiAutofac = null;
            if (register)
            {
                okanshiAutofac = new OkanshiAutofac(new OkanshiAutofacOptions() { MeasurementStyle = measurementStyleKind });
                builder.RegisterModule(okanshiAutofac);
            }

            IContainer container = builder.Build();

            var w = Stopwatch.StartNew();
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                for (int i = 0; i < max; i++)
                {
                    scope.Resolve<C>();
                    scope.Resolve<B>();
                    scope.Resolve<A>();
                    scope.Resolve<D>();
                    scope.Resolve<E>();
                    scope.Resolve<F>();
                    scope.Resolve<BigWig>();
                }
            }

            Console.WriteLine($"{measurementStyleKind.ToString().PadRight(28)} {w.ElapsedMilliseconds.ToString().PadLeft(5)}      isRegistered: {register}");
        }
    }

    public class A
    {
        public A()
        {
            Console.WriteLine("aa");
        }
    }

    public class B
    {
        public B(A a)
        {
            Console.WriteLine("bb");
        }
    }

    public class C
    {
        public C(A a)
        {
            Console.WriteLine("cc");
        }
    }

    public class D
    {
        public D(A a, C f)
        {
            Console.WriteLine("dd");
        }
    }

    public class E
    {
        public E(D a, C f, B v)
        {
            Console.WriteLine("ee");
        }
    }

    public class F
    {
        public F(E a, A f, B v)
        {
            Console.WriteLine("ff");
        }
    }

    public class Q
    {
        public Q(E a, A f, B v)
        {
            Console.WriteLine("qq");
        }
    }
    public class R
    {
        public R(E a, A f, B v)
        {
            Console.WriteLine("rr");
        }
    }

    public class S
    {
        public S(E a, A f, B v)
        {
            Console.WriteLine("ss");
        }
    }

    public class BigWig
    {
        public BigWig(E a, A f, B v, C c, D d, E e, F fe, Q q, R r, S s)
        {
            Console.WriteLine("bw");
        }
    }
}

