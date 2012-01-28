namespace NanoMessageBus.Logging
{
	using System;
	using System.Globalization;
	using System.Threading;

	internal static class ExtensionMethods
	{
		public static string FormatMessage(this string message, Type typeToLog, params object[] values)
		{
			message = message ?? string.Empty;

			return string.Format(
				CultureInfo.InvariantCulture,
				MessageFormat,
				DateTime.UtcNow,
				Thread.CurrentThread.GetName(),
				typeToLog.Name,
				string.Format(CultureInfo.InvariantCulture, message, values));
		}
		private static string GetName(this Thread thread)
		{
			return !string.IsNullOrEmpty(thread.Name)
				? thread.Name
				: thread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
		}

		private const string MessageFormat = "{0:yyyy/MM/dd HH:mm:ss.ff} - {1} - {2} - {3}";
	}
}