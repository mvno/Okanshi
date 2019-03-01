using System;
using Xunit;
using System.Diagnostics;
using Autofac;
using Okanshi.Autofac;


namespace Okanshi.Autofac.Tests.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async void When_sending_data_to_splunk_Then_see_data_in_splunk()
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

        private static void BuildAndRun(int max, MeasurementStyleKind measurementStyleKind, bool register = true)
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

            //Console.WriteLine(okanshiAutofac.ii);
            Console.WriteLine(value: $"{measurementStyleKind,18} {w.ElapsedMilliseconds,-5}      isRegistered: {register}");
        }
    }

    public class A
    {
        public A()
        {
            //Console.WriteLine("aa");
        }
    }

    public class B
    {
        public B(A a)
        {
            //Console.WriteLine("bb");
        }
    }

    public class C
    {
        public C(A a)
        {
            //Console.WriteLine("cc");
        }
    }

    public class D
    {
        public D(A a, C f)
        {
            //Console.WriteLine("cc");
        }
    }

    public class E
    {
        public E(D a, C f, B v)
        {
            //Console.WriteLine("cc");
        }
    }

    public class F
    {
        public F(E a, A f, B v)
        {
            //Console.WriteLine("cc");
        }
    }

    public class Q
    {
        public Q(E a, A f, B v)
        {
            //Console.WriteLine("cc");
        }
    }
    public class R
    {
        public R(E a, A f, B v)
        {
            //Console.WriteLine("cc");
        }
    }

    public class S
    {
        public S(E a, A f, B v)
        {
            //Console.WriteLine("cc");
        }
    }

    public class BigWig
    {
        public BigWig(E a, A f, B v, C c, D d, E e, F fe, Q q, R r, S s)
        {
            //Console.WriteLine("cc");
        }
    }
}

