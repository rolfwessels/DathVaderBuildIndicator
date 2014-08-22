﻿using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac.Integration.WebApi;
using Owin;
using log4net;

namespace BuildIndicatron.Server.Setup
{
	public static class BootStrap
	{
		private static bool _isInitialized;
		private static readonly object _locker = new object();

		public static void Initialize(IAppBuilder app)
		{
			lock (_locker)
			{
				if (!_isInitialized)
				{
					_isInitialized = true;
					ConfigureWebApi(app);
					ConfigureIndexResponse(app);
				}
			}
		}

		#region Private Methods

		private static void ConfigureWebApi(IAppBuilder app)
		{
			var config = new HttpConfiguration();
			config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());
			ConfigureRoutes(config);
			ConfigureTheDependencyInjection(config);
			app.UseWebApi(config);
		}

		private static void ConfigureIndexResponse(IAppBuilder app)
		{
			app.Run(context =>
				{
					if (context.Request.Path.Value == "/")
					{
						context.Response.ContentType = "text/html";
						return context.Response.WriteAsync("<html><body>Hello World! <a href=\"api/echo\">echo</a></body></html>");
					}

					context.Response.StatusCode = 404;
					return Task.Delay(0);
				});
		}

		private static void ConfigureTheDependencyInjection(HttpConfiguration config)
		{
			var resolver = new AutofacWebApiDependencyResolver(IocContainer.Initialize);
			config.DependencyResolver = resolver;
		}

		private static void ConfigureRoutes(HttpConfiguration config)
		{
			config.Routes.MapHttpRoute(
				name: "Default api",
				routeTemplate: "api/{controller}",
				defaults: new {id = RouteParameter.Optional}
				);
			
			config.Routes.MapHttpRoute(
				name: "Default api call with id",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional, action = "get" }
				);
			
			config.Routes.MapHttpRoute(
				name: "Default api call with id and action",
				routeTemplate: "api/TextToSpeech/{id}/{action}",
				defaults: new { id = "test", action = "EnhanceSpeech", controller = "TextToSpeech" }
				);

			config.Routes.MapHttpRoute(
				name: "Playmp3file with file name",
				routeTemplate: "api/soundplayer/{folder}/{file}",
				defaults: new { id = RouteParameter.Optional, action = "Get", controller = "SoundPlayer" }
				);
		}

		#endregion

		public class TraceExceptionLogger : ExceptionLogger
		{
			private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
			public override void Log(ExceptionLoggerContext context)
			{
				_log.Error(context.ExceptionContext.Exception.Message, context.ExceptionContext.Exception);
			}
		}
	}

	
}