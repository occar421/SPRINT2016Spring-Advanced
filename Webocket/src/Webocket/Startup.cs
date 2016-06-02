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
using System.Text;
using Newtonsoft.Json;

namespace Webocket
{
	public class Startup
	{
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
					var webSocket = await context.WebSockets.AcceptWebSocketAsync();

					var buffer = new byte[1024];
					var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

					while (received.MessageType == WebSocketMessageType.Text)
					{
						var data = Encoding.UTF8.GetString(buffer, 0, received.Count);
						var container = new ResponseContainer { Data = data };
						var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(container));
						await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), received.MessageType, received.EndOfMessage, CancellationToken.None);

						received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
					}

					await webSocket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);

					webSocket.Dispose();

					return;
				}
				await next();
			});

			app.UseDefaultFiles();
			app.UseStaticFiles();
		}
	}
}
