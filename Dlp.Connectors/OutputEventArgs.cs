using System;

namespace Dlp.Connectors {

	/// <summary>
	/// Represents the output of the DatabaseConnector. Useful as a verbose log.
	/// </summary>
	public sealed class OutputEventArgs : EventArgs {

		private OutputEventArgs(string operationName, string description)
			: base() {
			this.OperationName = operationName;
			this.Description = description;
			this.EventDateTime = DateTime.Now;
		}

		internal static OutputEventArgs Create(string operationName, string description) {

			OutputEventArgs outputEventArgs = new OutputEventArgs(operationName, description);
			return outputEventArgs;
		}

		/// <summary>
		/// Gets or sets the date and time of the event.
		/// </summary>
		public DateTime EventDateTime { get; set; }

		/// <summary>
		/// Gets or sets the name of the method that is writing the event.
		/// </summary>
		public string OperationName { get; set; }

		/// <summary>
		/// Gets the description of the event.
		/// </summary>
		public string Description { get; set; }

		public override string ToString() {
			return string.Format("[{0}]: {1} - {2}", this.EventDateTime, this.OperationName, this.Description);
		}
	}
}
