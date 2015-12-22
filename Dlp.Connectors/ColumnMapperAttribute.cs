using System;

namespace Dlp.Connectors {

	/// <summary>
	/// This class specifies wich column from a table must be mapped to the specified Property. Supported the BulkInsert method only.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ColumnMapperAttribute : Attribute {

		/// <summary>
		/// Name of the column that is going to receive the value from this property.
		/// </summary>
		public string ColumnName { get; set; }

		/// <summary>
		/// Specifies the mapping options for this Property.
		/// </summary>
		public ColumnMapperAttribute() { }

		/// <summary>
		/// Specifies the mapping options for this Property. 
		/// </summary>
		/// <param name="columnName">Name of the column that is going to receive the value from this property.</param>
		public ColumnMapperAttribute(string columnName) {
			this.ColumnName = columnName;
		}
	}
}
