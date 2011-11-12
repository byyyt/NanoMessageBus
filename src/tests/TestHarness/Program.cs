﻿namespace TestHarness
{
	using System;
	using System.Text;
	using Autofac;
	using NanoMessageBus;
	using NanoMessageBus.Handlers;
	using NanoMessageBus.RabbitMQ;
	using NanoMessageBus.Transports;

	internal class Program : IHandleMessages<string>
	{
		private static void Main()
		{
			var connector = new RabbitWireup()
				.UseTransactions()
				.ConnectAnonymouslyToLocalhost()
				.ListenTo("MyQueue")
				.Connect();

			var message = new RabbitMessage
			{
				Body = Encoding.UTF8.GetBytes("Hello, World!")
			};

			using (connector)
			using (var uow = connector.Current.BeginUnitOfWork())
			{
				connector.Current.Send(message, "MyExchange");
				uow.Complete();
			}

			////MessageHandlerTable<IComponentContext>.RegisterHandler(c => new Program());

			////var builder = new ContainerBuilder();
			////builder.RegisterModule(new BusModule());
			////builder.RegisterModule(new TransportModule());

			////using (var container = builder.Build())
			////{
			////    var receiver = container.Resolve<IReceiveMessages>();
			////    receiver.Start();

			////    Console.WriteLine("Press any key to exit...");
			////    Console.ReadLine();
			////}
		}

		public void Handle(string message)
		{
			Console.WriteLine(message);
		}
	}
}