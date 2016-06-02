using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Net.WebSockets;
using System.Collections.Concurrent;

namespace Webocket
{
	public class Startup
	{
		private static readonly Lazy<ConcurrentBag<Session>> socketsLazy = new Lazy<ConcurrentBag<Session>>(() => new ConcurrentBag<Session>());
		public static ConcurrentBag<Session> Sockets => socketsLazy.Value;

		private static readonly Lazy<ConcurrentBag<string>> todosLazy = new Lazy<ConcurrentBag<string>>(() => new ConcurrentBag<string>());
		public static ConcurrentBag<string> Todos => todosLazy.Value;

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			app.UseWebSockets();
			app.Use(async (context, next) =>
			{
				if (context.WebSockets.IsWebSocketRequest)
				{
					using (var webSocket = await context.WebSockets.AcceptWebSocketAsync())
					{
						if (webSocket?.State == WebSocketState.Open)
						{
							var session = new Session(webSocket);
							Sockets.Add(session);

							await session.Deal();
							return;
						}
					}
				}
				await next();
			});

			app.UseDefaultFiles();
			app.UseStaticFiles();
		}
	}
}
