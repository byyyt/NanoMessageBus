﻿namespace NanoMessageBus.RabbitMQ
{
	using System;
	using Handlers;

	public class RabbitFaultedMessageHandler
	{
		public virtual void ForwardToDeadLetterExchange()
		{
			this.channel.Send(this.message, this.deadLetterExchange.Exchange);
		}

		public virtual void HandleMessageFailure(Exception exception)
		{
			this.unitOfWork.Clear(); // don't perform any registered operations, e.g. publish, send, etc.

			if (++this.message.RetryCount >= this.maxAttempts)
				this.ForwardToPoisonMessageExchange(exception);
			else
				this.ForwardToRetryExchange();

			this.unitOfWork.Complete(); // but still remove the incoming physical message from the queue
		}
		public virtual void ForwardToPoisonMessageExchange(Exception exception)
		{
			this.AppendException(exception, 0);
			this.channel.Send(this.message, this.poisonMessageExchange.Exchange);
		}
		private void AppendException(Exception exception, int depth)
		{
			if (exception == null)
				return;

			this.message.Headers[ExceptionHeader.FormatWith(depth, "type")] = exception.GetType().FullName;
			this.message.Headers[ExceptionHeader.FormatWith(depth, "message")] = exception.Message;
			this.message.Headers[ExceptionHeader.FormatWith(depth, "stack")] = exception.StackTrace;

			this.AppendException(exception.InnerException, depth + 1);
		}
		private void ForwardToRetryExchange()
		{
			this.channel.Send(this.message, null); // TODO;
		}

		public RabbitFaultedMessageHandler(
			RabbitConnector connector,
			RabbitAddress poisonMessageExchange,
			RabbitAddress deadLetterExchange,
			int maxAttempts)
		{
			this.channel = connector.Current;
			this.message = this.channel.CurrentMessage;
			this.unitOfWork = this.channel.UnitOfWork;
			this.poisonMessageExchange = poisonMessageExchange;
			this.deadLetterExchange = deadLetterExchange;
			this.maxAttempts = maxAttempts;
		}

		private const string ExceptionHeader = "x-exception.{0}-{1}";
		private readonly RabbitChannel channel;
		private readonly RabbitMessage message;
		private readonly IHandleUnitOfWork unitOfWork;
		private readonly RabbitAddress poisonMessageExchange;
		private readonly RabbitAddress deadLetterExchange;
		private readonly int maxAttempts;
	}
}