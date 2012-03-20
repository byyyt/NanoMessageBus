﻿namespace NanoMessageBus
{
	using System;
	using Logging;

	public class DefaultHandlerContext : IHandlerContext
	{
		public virtual ChannelMessage CurrentMessage
		{
			get { return this.delivery.CurrentMessage; }
		}
		public virtual IChannelTransaction CurrentTransaction
		{
			get { return this.delivery.CurrentTransaction; }
		}
		public virtual IChannelGroupConfiguration CurrentConfiguration
		{
			get { return this.delivery.CurrentConfiguration; }
		}
		public virtual IDependencyResolver CurrentResolver
		{
			get { return this.delivery.CurrentResolver; }
		}
		public virtual IDispatchContext PrepareDispatch(object message = null)
		{
			this.ThrowWhenDisposed();
			return this.delivery.PrepareDispatch(message);
		}

		public virtual bool ContinueHandling
		{
			get { return this.continueHandling; }
		}
		public virtual void DropMessage()
		{
			this.ThrowWhenDisposed();
			this.continueHandling = false;
		}
		public virtual void DeferMessage()
		{
			this.DropMessage();

			this.delivery.PrepareDispatch()
				.WithMessage(this.delivery.CurrentMessage)
				.WithRecipient(ChannelEnvelope.LoopbackAddress)
				.Send();
		}

		protected virtual void ThrowWhenDisposed()
		{
			if (!this.disposed)
				return;

			Log.Warn("The handler context has already been disposed.");
			throw new ObjectDisposedException(typeof(DefaultHandlerContext).Name);
		}

		public DefaultHandlerContext(IDeliveryContext delivery)
		{
			if (delivery == null)
				throw new ArgumentNullException("delivery");

			this.delivery = delivery;
			this.continueHandling = true;
		}
		~DefaultHandlerContext()
		{
			this.Dispose(false);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			this.disposed = true;
			this.continueHandling = false;
		}

		private static readonly ILog Log = LogFactory.Build(typeof(DefaultHandlerContext));
		private readonly IDeliveryContext delivery;
		private bool continueHandling;
		private bool disposed;
	}
}