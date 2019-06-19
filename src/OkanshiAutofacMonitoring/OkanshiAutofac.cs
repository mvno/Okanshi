using System;
using Autofac;
using Autofac.Core;

namespace Okanshi.Autofac
{
    public class OkanshiAutofac : Module
    {
        private readonly OkanshiAutofacOptions options;

        public OkanshiAutofac(OkanshiAutofacOptions options)
        {
            this.options = options;
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            switch (options.MeasurementStyle)
            {
                case MeasurementStyleKind.None:
                    break;

                case MeasurementStyleKind.CountInstantiations:
                    var c = new CountEventHandler(options);
                    registration.Activating += c.CountActivatingFast;
                    break;

                case MeasurementStyleKind.CountAndTimeInstantiations:
                    var t = new CountAndTimingEventHandler(options);
                    registration.Activating += t.TimerActicating;
                    registration.Activated += t.TimerActivated;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(options), options.MeasurementStyle.ToString());
            }

            base.AttachToComponentRegistration(componentRegistry, registration);
        }
    }
}