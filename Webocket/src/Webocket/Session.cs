using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;

namespace Webocket
{
	public class Session
	{
		private WeakReference<WebSocket> socket;

		public int Id { get; }

		public Session(WebSocket socket)
		{
			Id = Counter.Instance.Increment();
			this.socket = new WeakReference<WebSocket>(socket);
		}

		public async Task Deal()
		{
			var buffer = new byte[1024];
			WebSocket s;

			if (!socket.TryGetTarget(out s))
			{
				return;
			}
			var received = await s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

			while (received.MessageType == WebSocketMessageType.Text)
			{
				var data = Encoding.UTF8.GetString(buffer, 0, received.Count);
				var repeatContainer = new ResponseContainer { Data = data, Id = Id };
				var repeatBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(repeatContainer));

				await Broadcast(repeatBytes);

				if (data.StartsWith("bot "))
				{
					var elem = data.Split(' ');
					switch (elem[1])
					{
						case "ping":
							var pingContainer = new ResponseContainer { Data = "pong", Id = Id };
							var pingBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pingContainer));
							await Broadcast(pingBytes);
							break;
						default:
							break;
					}
				}

				if (!socket.TryGetTarget(out s))
				{
					return;
				}
				received = await s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			}

			if (!socket.TryGetTarget(out s))
			{
				return;
			}
			await s.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
		}

		private async Task Broadcast(byte[] bytes)
		{
			await Task.WhenAll(Startup.Sockets.Select(x =>
			{
				WebSocket s;
				if (!x.socket.TryGetTarget(out s))
				{
					return Task.CompletedTask;
				}
				return s.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
			}));
		}

		sealed class Counter
		{
			private static readonly Lazy<Counter> lazy = new Lazy<Counter>(() => new Counter());

			public static Counter Instance { get { return lazy.Value; } }

			public int Increment()
			{
				return Interlocked.Increment(ref value);
			}

			private Counter() { }

			private int value = 0;
		}
	}
}
