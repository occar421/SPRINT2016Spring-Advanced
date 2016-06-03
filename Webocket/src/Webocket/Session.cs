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
				await Broadcast(repeatContainer.ToBytes());

				if (data.StartsWith("bot "))
				{
					var elem = data.Split(' ');
					switch (elem[1])
					{
						case "ping":
							var pingContainer = new ResponseContainer { Data = "pong", Id = Id, IsBot = true };
							await Broadcast(pingContainer.ToBytes());
							break;

						case "todo":
							if (elem.Length >= 3)
							{
								switch (elem[2])
								{
									case "add":
										if (elem.Length >= 5)
										{
											var name = elem[3];
											var content = string.Join(" ", elem.Skip(4));
											if (Startup.Todos.TryAdd(name, content))
											{
												var addedContainer = new ResponseContainer { Data = "todo added", Id = Id, IsBot = true };
												await Broadcast(addedContainer.ToBytes());
											}
											else
											{
												var errorContainer = new ResponseContainer { Data = "error occurred while adding", Id = Id, IsBot = true };
												await Broadcast(errorContainer.ToBytes());
											}
										}
										else
										{
											var usageContainer = new ResponseContainer { Data = "usage: bot todo add name content", Id = Id, IsBot = true };
											await Broadcast(usageContainer.ToBytes());
										}
										break;

									case "delete":
										if (elem.Length == 4)
										{
											var name = elem[3];
											string undef;
											if (Startup.Todos.TryRemove(name, out undef))
											{
												var deletedContainer = new ResponseContainer { Data = "todo deleted", Id = Id, IsBot = true };
												await Broadcast(deletedContainer.ToBytes());
											}
											else
											{
												var errorContainer = new ResponseContainer { Data = "error occurred while deleting", Id = Id, IsBot = true };
												await Broadcast(errorContainer.ToBytes());
											}
										}
										else
										{
											var usageContainer = new ResponseContainer { Data = "usage: bot todo delete name", Id = Id, IsBot = true };
											await Broadcast(usageContainer.ToBytes());
										}
										break;

									case "list":
										if (elem.Length == 3)
										{
											if (Startup.Todos.Any())
											{
												var allData = string.Join("\n", Startup.Todos.Select(x => $"{x.Key} {x.Value}"));
												var itemContainer = new ResponseContainer { Data = allData, Id = Id, IsBot = true };
												await Broadcast(itemContainer.ToBytes());
											}
											else
											{
												var emptyTodoContainer = new ResponseContainer { Data = "todo empty", Id = Id, IsBot = true };
												await Broadcast(emptyTodoContainer.ToBytes());
											}
										}
										else
										{
											var usageContainer = new ResponseContainer { Data = "usage: bot todo list", Id = Id, IsBot = true };
											await Broadcast(usageContainer.ToBytes());
										}
										break;

									default:
										{
											var usageContainer = new ResponseContainer { Data = "usage: bot todo command [name] [content]", Id = Id, IsBot = true };
											await Broadcast(usageContainer.ToBytes());
										}
										break;
								}
							}
							else
							{
								var usageContainer = new ResponseContainer { Data = "usage: bot todo command [name] [content]", Id = Id, IsBot = true };
								await Broadcast(usageContainer.ToBytes());
							}
							break;

						default:
							{
								var usageContainer = new ResponseContainer { Data = "usage: bot kind [option]", Id = Id, IsBot = true };
								await Broadcast(usageContainer.ToBytes());
							}
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
				if (!x.socket.TryGetTarget(out s) || s.State != WebSocketState.Open)
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
