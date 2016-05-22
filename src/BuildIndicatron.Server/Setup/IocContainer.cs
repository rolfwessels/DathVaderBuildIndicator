﻿using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using BuildIndicatron.Core.Chat;
using BuildIndicatron.Core.Helpers;
using BuildIndicatron.Core.Processes;
using BuildIndicatron.Server.Fakes;
using BuildIndicatron.Server.Properties;
using BuildIndicatron.Shared.Enums;

namespace BuildIndicatron.Server.Setup
{
	public static class IocContainer
	{
		private static bool _isInitialized;
		private static readonly object _locker = new object();
		private static IContainer _container;

		public static IContainer Instance
		{
			get
			{
				if (_container != null) return _container;
				lock (_locker)
				{
					if (!_isInitialized)
					{
						_isInitialized = true;
						var builder = new ContainerBuilder();
						if (PlatformHelper.IsLinux)
						{
							SetupConcrete(builder);
						}
						else
						{
							SetupFakes(builder);
						}
						SetupTools(builder);
						RegisterControllers(builder);

						return _container = builder.Build();
					}
					return _container;
				}
			}
		}

		#region Private Methods

		private static void SetupTools(ContainerBuilder builder)
		{
			builder.RegisterType<DownloadToFile>()
			       .As<IDownloadToFile>()
			       .WithParameter("tempPath", Settings.Default.SpeachTempFileLocation);
			builder.RegisterType<Stage>().As<IStage>().SingleInstance();
			builder.RegisterType<SequencesFactory>();
            builder.RegisterType<ChatBot>().As<IChatBot>();
            builder.Register(context => new AutofacInjector(_container)).As<IInjector>().SingleInstance();
            
			builder.RegisterType<SequencePlayer>().As<ISequencePlayer>();
			builder.Register((t) => new SoundFilePicker(Settings.Default.SoundFileLocation)).As<ISoundFilePicker>();
		}

		private static void RegisterControllers(ContainerBuilder builder)
		{
			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
		}

		private static void SetupConcrete(ContainerBuilder builder)
		{
			builder.RegisterType<Mp3Player>().As<IMp3Player>();
			builder.RegisterType<VoiceRss>().As<ITextToSpeech>();
			builder.Register(t => new VoiceEnhancer(@"resources/text2speach/Star-Wars-1391.mp3", "speed 0.7 echo 0.8 0.88 6.0 0.4"))
			       .As<IVoiceEnhancer>();
			builder.RegisterType<PinManager>()
				.WithParameter("configuration", ConfigurationRobot)
				.As<IPinManager>().SingleInstance();
		}								  

		private static void SetupFakes(ContainerBuilder builder)
		{
			builder.RegisterType<FakePlayer>().As<IMp3Player>();
			builder.RegisterType<FakeTextToSpeech>().As<ITextToSpeech>();
			builder.RegisterType<FakePlayer>().As<IVoiceEnhancer>();
			builder.RegisterType<FakePinManager>()
				.As<IPinManager>();
		}

		public static GpioConfiguration ConfigurationRobot
		{
			get
			{
				return new GpioConfiguration(
					new GpioConfiguration.Target(PinName.SecondaryLightRed, GpIO.GPIO7, true),
					new GpioConfiguration.Target(PinName.SecondaryLightGreen, GpIO.GPIO8, true),
					new GpioConfiguration.Target(PinName.MainLightRed, GpIO.GPIO11, true),
					new GpioConfiguration.Target(PinName.MainLightGreen, GpIO.GPIO25, true),
					new GpioConfiguration.Target(PinName.MainLightBlue, GpIO.GPIO9, true),
					new GpioConfiguration.Target(PinName.Spinner, GpIO.GPIO22),
					new GpioConfiguration.Target(PinName.Fire, GpIO.GPIO10)
					)
					{
						Buttons =
							new List<GpioConfiguration.Button>() {new GpioConfiguration.Button() {Pin = GpIO.GPIO24, IsToggle = false}}
					};
			}
		}

		public static GpioConfiguration ConfigurationDarth2
		{
			get
			{
				return new GpioConfiguration(
					new GpioConfiguration.Target(PinName.SecondaryLightRed, GpIO.GPIO7, true),
					new GpioConfiguration.Target(PinName.SecondaryLightGreen, GpIO.GPIO8, true),
					new GpioConfiguration.Target(PinName.MainLightRed, GpIO.GPIO11, true),
					new GpioConfiguration.Target(PinName.MainLightGreen, GpIO.GPIO25, true),
					new GpioConfiguration.Target(PinName.MainLightBlue, GpIO.GPIO9, true)
					)
				{
					Buttons =
						new List<GpioConfiguration.Button>() { new GpioConfiguration.Button() { Pin = GpIO.GPIO24, IsToggle = false } }
				};
			}
		}

		public static GpioConfiguration ConfigurationDarth
		{
			get
			{


				return new GpioConfiguration(
					new GpioConfiguration.Target(PinName.SecondaryLightRed, GpIO.GPIO7, true),
					new GpioConfiguration.Target(PinName.SecondaryLightGreen, GpIO.GPIO8, true),
					new GpioConfiguration.Target(PinName.MainLightRed, GpIO.GPIO11, true),
					new GpioConfiguration.Target(PinName.MainLightGreen, GpIO.GPIO25, true),
					new GpioConfiguration.Target(PinName.MainLightBlue, GpIO.GPIO9, true),
					new GpioConfiguration.Target(PinName.Spinner, GpIO.GPIO22),
					new GpioConfiguration.Target(PinName.Fire, GpIO.GPIO10)
					)
					{
						Buttons =
							new List<GpioConfiguration.Button>() {new GpioConfiguration.Button() {Pin = GpIO.GPIO24, IsToggle = false}}
					};
			}
		}

		#endregion
	}
}