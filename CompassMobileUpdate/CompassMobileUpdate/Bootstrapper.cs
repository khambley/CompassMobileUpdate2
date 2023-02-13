using System;
using Autofac;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using CompassMobileUpdate.ViewModels;
using CompassMobileUpdate.Pages;
using CompassMobileUpdate.Services;

namespace CompassMobileUpdate
{
	public abstract class Bootstrapper
	{
		protected ContainerBuilder ContainerBuilder { get; private set; }

		public Bootstrapper()
		{
			Initialize();
			FinishInitialization();
		}

		protected virtual void Initialize()
		{
			var currentAssembly = Assembly.GetExecutingAssembly();
			ContainerBuilder = new ContainerBuilder();

			foreach(var type in currentAssembly.DefinedTypes.Where(e => e.IsSubclassOf(typeof(Page)) ||
																	e.IsSubclassOf(typeof(ViewModelBase))))
			{
				ContainerBuilder.RegisterType(type.AsType());
			}
			ContainerBuilder.RegisterType<MeterService>().SingleInstance();
            ContainerBuilder.RegisterType<AuthService>().SingleInstance();
        }

		private void FinishInitialization()
		{
			var container = ContainerBuilder.Build();
			Resolver.Initialize(container);
		}
	}
}

