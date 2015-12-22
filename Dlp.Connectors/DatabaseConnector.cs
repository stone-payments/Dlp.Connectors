using Dlp.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dlp.Connectors {

	public delegate void OutputEventHandler(object sender, OutputEventArgs e);

	/// <summary>
	/// Connector for database operations.
	/// </summary>
	public class DatabaseConnector : IDisposable {

		/// <summary>
		/// Initializes an instance of the DatabaseConnector. A SqlTransaction must be running.
		/// </summary>
		/// <param name="commandTimeoutInSeconds">Limit time, in seconds, that an operation may take to be completed.</param>
		/// <exception cref="System.InvalidOperationException">Start a database transaction with DatabaseConnector.BeginTransaction, prior using this constructor.</exception>
		public DatabaseConnector(int commandTimeoutInSeconds = 90) {

			// Verifica se existe uma transação de banco de dados em andamento.
			if (_sqlTransaction == null || _sqlTransaction.Connection == null) { throw new InvalidOperationException("This constructor requires a sqlTransaction to be running. Start one with DatabaseConnector.BeginTransaction() method."); }

			// Utiliza a conexão da transação em andamento.
			this.Connection = _sqlTransaction.Connection;

			// Utiliza a transação de banco global.
			this.Transaction = _sqlTransaction;

			// Define o timeout da operação.
			this.CommandTimeoutInSeconds = commandTimeoutInSeconds;
		}

		/// <summary>
		/// Initializes an instance of the DatabaseConnector.
		/// </summary>
		/// <param name="connectionString">Database ConnectionString to be used by the connector. Ignored if a DatabaseTransaction is already running.</param>
		/// <param name="commandTimeoutInSeconds">Limit time, in seconds, that an operation may take to be completed.</param>
		/// <exception cref="System.ArgumentNullException">The connectionString parameter must have a value.</exception>
		public DatabaseConnector(string connectionString, int commandTimeoutInSeconds = 90) {

			// Só inicializa os dados caso não exista uma transação global em andamento.
			if (_sqlTransaction == null || _sqlTransaction.Connection == null) {

				// Dispara uma exceção caso a connection string não tenha sido especificada.
				if (string.IsNullOrEmpty(connectionString) == true) { throw new ArgumentNullException("connectionString"); }

				// Cria o objeto de conexão com o banco de dados.
				this.Connection = new SqlConnection(connectionString);
			}
			else {
				// Utiliza a conexão da transação em andamento.
				this.Connection = _sqlTransaction.Connection;

				// Utiliza a transação de banco global.
				this.Transaction = _sqlTransaction;
			}

			// Define o timeout da operação.
			this.CommandTimeoutInSeconds = commandTimeoutInSeconds;
		}

		/// <summary>
		/// Destrutor que fechará os recursos utilizados, caso o usuário não tenha fechado explicitamente.
		/// </summary>
		~DatabaseConnector() { this.Dispose(false); }

		public event OutputEventHandler OnOutput;

		/// <summary>
		/// Gets or sets the database connection used by this connector.
		/// </summary>
		private SqlConnection Connection { get; set; }

		/// <summary>
		/// Gets or sets the current database transaction.
		/// </summary>
		private SqlTransaction Transaction { get; set; }

		[ThreadStatic]
		private static SqlTransaction _sqlTransaction;

		/// <summary>
		/// Gets the database operation timeout. Default: 90 seconds.
		/// </summary>
		public int CommandTimeoutInSeconds { get; private set; }

		/// <summary>
		/// Abre a conexão com o banco de dados.
		/// </summary>
		private void OpenConnection() {

			this.WriteOutput("OpenConnection", string.Format("Estado atual da conexão: {0}.", this.Connection.State));

			// Abre a conexão com o banco de dados, caso esteja fechada.
			if (this.Connection.State == ConnectionState.Closed) {

				this.WriteOutput("OpenConnection", "Abrindo conexão com o servidor de banco de dados.");
				this.Connection.Open();
			}

			this.WriteOutput("OpenConnection", string.Format("ConnectionId: {0}", this.Connection.ClientConnectionId));
			this.WriteOutput("OpenConnection", string.Format("ConnectionString: {0}", this.Connection.ConnectionString));
			this.WriteOutput("OpenConnection", string.Format("ConnectionTimeout: {0}", this.Connection.ConnectionTimeout));
			this.WriteOutput("OpenConnection", string.Format("CommandTimeout: {0}", this.CommandTimeoutInSeconds));
		}

		/// <summary>
		/// Cria um objeto IDbCommand apropriado para o tipo de conexão especificada.
		/// </summary>
		/// <param name="query">Query a ser executada no banco de dados.</param>
		/// <param name="connection">Objeto de conexão com o banco de dados.</param>
		/// <param name="transaction">Transação de banco de dados.</param>
		/// <param name="commandTimeoutInSeconds">Timeout da operação do banco de dados.</param>
		/// <returns>Retorna um objeto IDbCommand apropriado para o tipo de conexão especificada.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private static SqlCommand SqlCommandFactory(string query, SqlConnection connection, SqlTransaction transaction, int commandTimeoutInSeconds) {

			// Cria o objeto SqlCommand.
			SqlCommand command = new SqlCommand(query, connection, transaction);

			// Define o timeout da operação.
			command.CommandTimeout = commandTimeoutInSeconds;

			return command;
		}

		/// <summary>
		/// Remove dados desnecessários da query, para que a quantidade de informações enviadas para o servidor seja reduzida.
		/// </summary>
		/// <param name="query">Query a ser tratada.</param>
		/// <returns>Retorna a query tratada.</returns>
		private string CleanQuery(string query) {

			this.WriteOutput("CleanQuery", "Removendo espaços em branco e comentários da query.");

			// Remove os comentários da query.
			query = Regex.Replace(query, @"--(.*)", "", RegexOptions.Multiline);

			// Remove espaços em branco desnecessários.
			query = Regex.Replace(query, @"\s+", " ");

			return query;
		}

		/// <summary>
		/// Executes the query and returns the first column from the first row returned by the query. Additional columns or rows are ignored.
		/// </summary>
		/// <typeparam name="T">Type of the object instance to be returned.</typeparam>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns a typed object with the found value. If no data is found, default(T) is returned.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the query parameter.</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.Data.SqlClient.SqlException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteScalar"]/*'/>
		public T ExecuteScalar<T>(string query, dynamic parameters = null) {

			this.WriteOutput("ExecuteScalar", "Iniciando ExecuteScalar.");

			// Verifica se a query foi especificada.
			if (string.IsNullOrWhiteSpace(query) == true) { throw new ArgumentNullException("query"); }

			// Limpa a query, removendo espaços e informações desnecessárias.
			query = this.CleanQuery(query);

			try {
				// Abre a conexão com o banco de dados.
				this.OpenConnection();

				// Instancia o objeto command que será executado no banco de dados.
				using (SqlCommand command = SqlCommandFactory(query, this.Connection, this.Transaction, this.CommandTimeoutInSeconds)) {

					// Adiciona os parâmetros.
					this.AddParameters(query, command, parameters as object);

					// Execura a query e retorna o objeto.
					object result = command.ExecuteScalar();

					// Verifica se algum valor foi encontrado.
					if (result == null) { return default(T); }

					// Obtém o tipo a ser retornado.
					Type returnType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

					// Retorna o objeto no formato específicado.
					return (T)Convert.ChangeType(result, returnType);
				}
			}
			catch (Exception) {

				this.Close();

				throw;
			}
		}

		/// <summary>
		/// Executes the specified query and returns the number of rows affected.
		/// </summary>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns the number of rows affected.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the query parameter.</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.Data.SqlClient.SqlException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteNonQuery"]/*'/>
		public int ExecuteNonQuery(string query, dynamic parameters = null) {

			this.WriteOutput("ExecuteNonQuery", "Iniciando ExecuteNonQuery.");

			// Verifica se a query foi especificada.
			if (string.IsNullOrWhiteSpace(query) == true) { throw new ArgumentNullException("query"); }

			// Limpa a query, removendo espaços e informações desnecessárias.
			query = this.CleanQuery(query);

			try {
				// Abre a conexão com o banco de dados.
				this.OpenConnection();

				// Instancia o objeto command que será executado no banco de dados.
				using (SqlCommand command = SqlCommandFactory(query, this.Connection, this.Transaction, this.CommandTimeoutInSeconds)) {

					// Adiciona os parâmetros.
					this.AddParameters(query, command, parameters as object);

					// Execura a query e retorna a quantidade de linhas afetadas.
					return command.ExecuteNonQuery();
				}
			}
			catch (Exception) {

				this.Close();

				throw;
			}
		}

		/// <summary>
		/// Sends the query to be executed by the database and return a KeyValuePair with the number of available rows and the retrieved data for the specified page.
		/// </summary>
		/// <typeparam name="T">Type of the object instance to be returned.</typeparam>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="pageNumber">Page to be returned.</param>
		/// <param name="pageSize">Number of rows per page.</param>
		/// <param name="orderByColumnName">Column name to ot used to order the retrieved data.</param>
		/// <param name="sortDirection">Sorting direction.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns a KeyValuePair where Key = Number of rows available in database and Value = Data of type T returned for current page number.</returns>
		/// <exception cref="ArgumentNullException">Missing the query or the orderByColumnName parameter.</exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteReaderPaged"]/*'/>
		public KeyValuePair<int, IEnumerable<T>> ExecuteReader<T>(string query, int pageNumber, int pageSize, string orderByColumnName, SortDirection sortDirection, dynamic parameters = null) where T : new() {

			this.WriteOutput("ExecuteReader", "Iniciando ExecuteReader.");

			// Verifica se a query foi especificada.
			if (string.IsNullOrWhiteSpace(query) == true) { throw new ArgumentNullException("query"); }

			// Verifica se a coluna pela qual os dados serão ordenados foi especificada.
			if (string.IsNullOrWhiteSpace(orderByColumnName) == true) { throw new ArgumentNullException("orderByColumnName"); }

			// Limpa a query, removendo espaços e informações desnecessárias.
			query = this.CleanQuery(query);

			// Verifica se o número da página é válido.
			if (pageNumber < 1) { pageNumber = 1; }

			// Verifica se o tamanho da página é válido.
			if (pageSize < 1) { pageSize = 1; }

			this.WriteOutput("ExecuteReader", "Preparando query para paginação.");

			// Separa a query em colunas a serem retornadas e em filtros a serem aplicados.
			string[] queryParts = query.Split(new[] { "SELECT", "FROM" }, 3, StringSplitOptions.None);

			// Monta a query para obter a quantidade total de registros, utilizando o filtro de busca original.
			string countQuery = string.Format("{0} SELECT COUNT(1) AS TotalRows FROM {1}", queryParts[0], queryParts[2]);

			// Obtém o número de linhas disponíveis no banco de dados.
			int countResult = this.ExecuteScalar<int>(countQuery, parameters);

			// Obtém uma coleção com o nome da cada coluna a ser pesquisada.
			string[] fields = queryParts[1].Split(new[] { "," }, StringSplitOptions.None);

			string rawFields = string.Empty;

			// Extrai o nome de cada coluna, sem o nome da tabela a qual pertence.
			for (int i = 0; i < fields.Length; i++) {

				// Separa o nome da tabela do nome da coluna.
				string[] field = fields[i].Split(new[] { "." }, StringSplitOptions.None);

				// Concatena a string com o nome das colunas sem nomes de tabelas.
				rawFields = string.Format("{0}, {1}", rawFields, (field.Length > 1) ? field[1] : field[0]);
			}

			// Remove qualquer vírgula extra.
			rawFields = rawFields.Trim(',');

			// Monta a query que retornará apenas os dados da página solicitada.
			string paginationQuery = string.Format("{0} WITH SourceTable AS (SELECT TOP {1} ROW_NUMBER() OVER(ORDER BY {2} {3}) AS RowNumber, {4} FROM {5}) SELECT {6} FROM SourceTable WHERE RowNumber BETWEEN {7} AND {8};",
				queryParts[0],
				pageNumber * pageSize,
				orderByColumnName,
				sortDirection.ToString(),
				queryParts[1],
				queryParts[2].TrimEnd(';'),
				rawFields,
				(pageNumber - 1) * pageSize + 1,
				pageNumber * pageSize).Trim();

			// Obtém os dados da página solicitada.
			object paginationResult = this.ExecuteReaderFetchAll<T>(paginationQuery, parameters);

			return new KeyValuePair<int, IEnumerable<T>>(countResult, paginationResult as IEnumerable<T>);
		}

		/// <summary>
		/// Sends the query to be executed by the database and return a collection of type T containing all records. Has the same effect as ExecuteReaderFetchAll.
		/// </summary>
		/// <typeparam name="T">Type of the object instance to be returned.</typeparam>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns a list of type T with the retrieved data.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the query parameter.</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.Data.SqlClient.SqlException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteReader"]/*'/>
		public IEnumerable<T> ExecuteReader<T>(string query, dynamic parameters = null) {

			// Não remover este cast.
			return ExecuteReaderFetchAll<T>(query, parameters) as IEnumerable<T>;
		}

		/// <summary>
		/// Sends the query to be executed by the database and return a collection of type T containing all records
		/// </summary>
		/// <typeparam name="T">Type of the object instance to be returned.</typeparam>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns a list of type T with the retrieved data.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the query parameter.</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.Data.SqlClient.SqlException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteReader"]/*'/>
		public IEnumerable<T> ExecuteReaderFetchAll<T>(string query, dynamic parameters = null) {

			this.WriteOutput("ExecuteReader", "Iniciando ExecuteReader.");

			// Verifica se a query foi especificada.
			if (string.IsNullOrWhiteSpace(query) == true) { throw new ArgumentNullException("query"); }

			// Limpa a query, removendo espaços e informações desnecessárias.
			query = this.CleanQuery(query);

			try {
				// Abre a conexão com o banco de dados.
				this.OpenConnection();

				this.WriteOutput("ExecuteReader", "Criando o objeto SqlCommand.");

				// Instancia o objeto command que será executado no banco de dados.
				using (SqlCommand command = SqlCommandFactory(query, this.Connection, this.Transaction, this.CommandTimeoutInSeconds)) {

					// Adiciona os parâmetros.
					this.AddParameters(query, command, parameters as object);

					this.WriteOutput("ExecuteReader", "Executando o comando ExecuteReader.");

					// Instancia o reader responsável pela leitura dos dados.
					using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo)) {

						// Mapeia e armazena todos os registros encontrados.
						IEnumerable<T> result = this.InternalReaderFetchAll<T>(reader);

						this.WriteOutput("ExecuteReader", "Operação concluída.");

						return result;
					}
				}
			}
			catch (Exception ex) {

				this.Close();

				this.WriteOutput("ExecuteReader", string.Format("Exceção no processamento: {0}.", ex));

				throw;
			}
		}

		private sealed class SqlEnumerableHelper<T> : IDisposableEnumerable<T> {

			private readonly SqlDataReader reader;
			private readonly IEnumerable<T> enumerable;
			private readonly IDisposable[] disposables;

			public SqlEnumerableHelper(IEnumerable<T> enumerable, SqlDataReader reader, IDisposable[] disposables) {
				this.enumerable = enumerable;
				this.reader = reader;
				this.disposables = disposables;
			}

			~SqlEnumerableHelper() { this.Dispose(); }

			public void Dispose() {
				((IDisposable)this.reader).Dispose();
				foreach (var disposable in this.disposables) {
					disposable.Dispose();
				}
			}

			public IEnumerator<T> GetEnumerator() {
				return this.enumerable.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				return this.enumerable.GetEnumerator();
			}
		}

		/// <summary>
		/// Sends the query to be executed by the database and return a disposable enumerable of type T
		/// </summary>
		/// <typeparam name="T">Type of the object instance to be returned.</typeparam>
		/// <param name="query">Query to be executed. The parameter is mandatory.</param>
		/// <param name="parameters">Query parameters, as a dynamic object, following the format: new {Param1 = value1, Param2 = value2, ...}.</param>
		/// <returns>Returns a list of type T with the retrieved data.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the query parameter.</exception>
		/// <exception cref="System.InvalidCastException"></exception>
		/// <exception cref="System.Data.SqlClient.SqlException"></exception>
		/// <exception cref="System.IO.IOException"></exception>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <exception cref="System.ObjectDisposedException"></exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="ExecuteReader"]/*'/>
		public IDisposableEnumerable<T> ExecuteReaderAsEnumerable<T>(string query, dynamic parameters = null, bool closeOnDispose = true) {

			this.WriteOutput("ExecuteReader", "Iniciando ExecuteReader.");

			// Verifica se a query foi especificada.
			if (string.IsNullOrWhiteSpace(query)) {
				throw new ArgumentNullException("query");
			}

			// Limpa a query, removendo espaços e informações desnecessárias.
			query = this.CleanQuery(query);

			try {
				// Abre a conexão com o banco de dados.
				this.OpenConnection();

				this.WriteOutput("ExecuteReader", "Criando o objeto SqlCommand.");

				// Instancia o objeto command que será executado no banco de dados.
				SqlCommand command = SqlCommandFactory(query, this.Connection, this.Transaction, this.CommandTimeoutInSeconds);

				// Adiciona os parâmetros.
				this.AddParameters(query, command, parameters as object);

				this.WriteOutput("ExecuteReader", "Executando o comando ExecuteReader.");

				// Instancia o reader responsável pela leitura dos dados.
				SqlDataReader reader = command.ExecuteReader(CommandBehavior.KeyInfo);
				return new SqlEnumerableHelper<T>(this.InternalReader<T>(reader), reader,
					closeOnDispose ? new IDisposable[] { command, this } : new IDisposable[] { command });

			}
			catch (Exception ex) {

				this.Close();

				this.WriteOutput("ExecuteReader", string.Format("Exceção no processamento: {0}.", ex));

				throw;
			}
		}

		/// <summary>
		/// Copys all elements of a collection to a destination table.
		/// </summary>
		/// <param name="tableName">Target table name.</param>
		/// <param name="collection">Collection containing the data to be pushed to the database. The elements properties must have the same name as the destination columns.</param>
		/// <param name="sqlBulkCopyOptions">Rule to be used by Sql Server before insert the data.</param>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="BulkInsert"]/*'/>
		public void BulkInsert(string tableName, IEnumerable collection, SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.Default) {

			this.WriteOutput("BulkInsert", "Iniciando BulkInsert.");

			// O método não pode prosseguir caso não tenha sido definido o nome da tabela.
			if (string.IsNullOrEmpty(tableName) == true) { throw new ArgumentNullException("tableName"); }

			// Sai do método caso não existam dados a serem inseridos.
			if (collection == null) { return; }

			// Obtém um enumerador para a coleção recebida.
			IEnumerator enumerator = collection.GetEnumerator();

			// Acessa o primeiro item da coleção.
			enumerator.MoveNext();

			this.WriteOutput("BulkInsert", "Obtendo as propriedades públicas do objeto a ser inserido.");

			// Obtém as informações sobre as propriedades de cada item da coleção.
			PropertyInfo[] propertyInfoCollection = enumerator.Current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			this.WriteOutput("BulkInsert", string.Format("Criando o DataTable para a tabela '{0}'.", tableName));

			// Cria o DataTable que será usado para copiar as informações para o banco de dados.
			DataTable dataTable = new DataTable(tableName);

			this.WriteOutput("BulkInsert", "Iniciando o mapeando das propriedades dos objetos a serem inseridos.");

			// Dicionário que conterá o nome das colunas no banco de dados e a propriedade que será salva nesta coluna.
			Dictionary<string, PropertyInfo> propertyTableDictionary = new Dictionary<string, PropertyInfo>(propertyInfoCollection.Length);

			// Mapeia cada propriedade da coleção, para criar as colunas do DataTable.
			foreach (PropertyInfo propertyInfo in propertyInfoCollection) {

				Type propertyType = propertyInfo.PropertyType;

				// Verifica se a propriedade é Nullable. Caso seja, obtém o tipo genérico.
				if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) { propertyType = propertyType.GetGenericArguments()[0]; }

				this.WriteOutput("BulkInsert", string.Format("Mapeando a coluna '{0}' para o tipo '{1}'.", propertyInfo.Name, propertyType.Name));

				// Obtém os atributos da propriedade.
				Attribute[] attributes = Attribute.GetCustomAttributes(propertyInfo);

				ColumnMapperAttribute columnNameAttribute = null;

				// Obtém todos os atributos da propriedade.
				foreach (System.Attribute attribute in attributes) {

					// Verifica se o atributo é do tipo ColumnNameAttribute.
					if (attribute is ColumnMapperAttribute) { columnNameAttribute = attribute as ColumnMapperAttribute; }
				}

				string columnName = null;

				// Caso o atributo exista e possua um valor definido, utilizamos este valor como nome da coluna. Caso contrário, utiliza o nome da propriedade.
				if (columnNameAttribute != null && string.IsNullOrEmpty(columnNameAttribute.ColumnName) == false) { columnName = columnNameAttribute.ColumnName; }
				else { columnName = propertyInfo.Name; }

				// Adiciona a nova coluna no DataTable com o nome do atributo.
				dataTable.Columns.Add(new DataColumn(columnName, propertyType));

				// Adiciona a informação da coluna e propriedade no dicionário de dados a serem inseridos na tabela.
				propertyTableDictionary.Add(columnName, propertyInfo);
			}

			this.WriteOutput("BulkInsert", "Iniciando o preenchimento DataTable com os dados recebidos.");

			do {
				// Cria uma nova linha que será adicionada ao Datatable.
				DataRow dataRow = dataTable.NewRow();

				// Obtém os valores de cada propriedade do item da coleção e adiciona na coluna correspondente.
				foreach (KeyValuePair<string, PropertyInfo> kvPair in propertyTableDictionary) {
					dataRow[kvPair.Key] = kvPair.Value.GetValue(enumerator.Current, null) ?? DBNull.Value;
				}

				// Adiciona a linha ao DataTable.
				dataTable.Rows.Add(dataRow);

			} while (enumerator.MoveNext() == true);

			try {
				// Cria um novo SqlBulkCopy que será utilizado para inserir todas as informações da coleção de uma vez.
				using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(this.Connection, sqlBulkCopyOptions, this.Transaction)) {

					// Define o timeout da operação.
					sqlBulkCopy.BulkCopyTimeout = this.CommandTimeoutInSeconds;

					this.WriteOutput("BulkInsert", "Abrindo a conexão com o banco de dados.");

					// Abre a conexão com o banco de dados.
					this.OpenConnection();

					this.WriteOutput("BulkInsert", string.Format("Transaction Isolation Level: {0}", (this.Transaction != null) ? this.Transaction.IsolationLevel.ToString() : "null"));
					this.WriteOutput("BulkInsert", string.Format("SqlBulkCopyOptions: {0}", sqlBulkCopyOptions));

					this.WriteOutput("BulkInsert", "Mapeando as colunas do DataTable com as colunas do banco de dados.");

					// Mapeia cada coluna do DataTable com sua respectiva coluna no banco de dados. Por isso é necessário que as propriedades tenham o mesmo nome das colunas no banco de dados.
					for (int i = 0; i < dataTable.Columns.Count; i++) { sqlBulkCopy.ColumnMappings.Add(dataTable.Columns[i].Caption, dataTable.Columns[i].Caption); }

					// Define o nome da tabela que receberá os dados.
					sqlBulkCopy.DestinationTableName = dataTable.TableName;

					// Define a quantidade máxima de registros a serem enviados por vez para o servidor. Utilizar entre 5000 e 10000 para evitar timeouts.
					sqlBulkCopy.BatchSize = 5000;

					this.WriteOutput("BulkInsert", "Enviando as instruções para serem executadas no servidor.");

					// Insere as informações no banco de dados.
					sqlBulkCopy.WriteToServer(dataTable);

					this.WriteOutput("BulkInsert", "Operação concluída.");
				}
			}
			catch (Exception ex) {

				this.Close();

				this.WriteOutput("BulkInsert", string.Format("Exceção no processamento: {0}.", ex));

				throw;
			}
		}

		/// <summary>
		/// Adiciona os parâmetros para a execução da query.
		/// </summary>
		/// <param name="query">Query a ser enviada para execução.</param>
		/// <param name="command">Command responsável pela execução da query.</param>
		/// <param name="parameters">Objeto contendo as propriedades a serem mepeadas para os parâmetros.</param>
		private void AddParameters(string query, SqlCommand command, object parameters) {

			this.WriteOutput("AddParameters", "Adicionando os parâmetros da query.");

			// Verifica se existem parâmetros a serem adicionados.
			if (parameters == null) { return; }

			// Obtém todas as propriedades do tipo anônimo.
			PropertyInfo[] propertyInfoCollection = parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			// Limpa quaisquer parâmetros pré-existentes.
			command.Parameters.Clear();

			// Mapeia o valor de cada propriedade para um novo parâmetro com o mesmo nome da propriedade.
			for (int i = 0; i < propertyInfoCollection.Length; i++) {

				string parameterName = "@" + propertyInfoCollection[i].Name;

				// Verifica se o parâmetro realmente existe na query.
				if (query.IndexOf(parameterName, StringComparison.OrdinalIgnoreCase) < 0) { continue; }

				// Obtém ou valor da propriedade.
				object value = propertyInfoCollection[i].GetValue(parameters, null);

				// Verifica se existe valor para a propriedade.
				if (value != null) {

					// Obtém o tipo da propriedade.
					Type valueType = value.GetType();

					// Deve ser um tipo básico ou Enum, e a única coleção permitida é string (coleção de chars).
					if ((valueType.IsClass == true && valueType.FullName.StartsWith("System") == false && valueType.IsEnum == false)) { continue; }

					// Verifica se é uma coleção que não seja uma string. Caso positivo, transforma os elementos da coleção para uma string.
					if (typeof(IEnumerable).IsAssignableFrom(valueType) && (value is string == false) && (value is byte[] == false)) {

						// Converte a coleção para uma string a ser adicionada na query.
						value = ((IEnumerable)value).AsString(',', "'");

						// Como o Sql não suporta coleções como parâmetros, atualiza a query dinamicamente.
						query = query.Replace("@" + propertyInfoCollection[i].Name, value as string);

						// Como o parâmetro já foi definido, passa para o próximo.
						continue;
					}
				}

				// Cria um novo parâmetro de banco de dados.
				SqlParameter parameter = command.CreateParameter();

				// Cria o nome do parâmetro, assim como no SqlCommand, colocando o sinal de @ na frente do nome.
				parameter.ParameterName = parameterName;

				// Se o valor for uma string nula, define o valor como DBNull. Caso contrário, utiliza o valor do objeto. Se o valor do objeto for nulo, utiliza novamente DBNull.
				parameter.Value = value ?? DBNull.Value;

				this.WriteOutput("AddParameters", string.Format("Adicionando parâmetro '{0}' com o valor '{1}'.", parameter.ParameterName, parameter.Value));

				// Validações adicionais antes de preencher o valor do objeto.
				if (value != null) {

					if (value is string && (value as string).Length == 0) { parameter.Value = DBNull.Value; }
					else if (value.GetType().IsEnum == true) { parameter.Value = value.ToString(); }
					else if (value is DateTime && (DateTime)value == DateTime.MinValue) { parameter.Value = DBNull.Value; }
				}

				// Define o motivo do parâmetro.
				parameter.Direction = ParameterDirection.Input;

				// Adiciona o parâmetro no objeto command a ser executado.
				command.Parameters.Add(parameter);

			}

			// Redefine a query, pois ela pode ter sido modificada.
			command.CommandText = query;
		}

		/// <summary>
		/// Executa a leitura dos dados de um DataReader.
		/// </summary>
		/// <typeparam name="T">Tipo do objeto que será preenchido.</typeparam>
		/// <param name="reader">Reader a ser utilizado para obter as informações do banco de dados.</param>
		/// <returns>Retorna uma coleção com os registros do tipo T encontrados no banco de dados.</returns>
		private IEnumerable<T> InternalReader<T>(SqlDataReader reader) {

			Type returnType = typeof(T);

			this.WriteOutput("InternalReader", string.Format("Obtendo as propriedades do tipo '{0}'.", returnType.Name));

			// Armazena todas as propriedades do objeto. Importante obter a propriedade desta coleção para que a busca possa ser case insensitive, ao contrário do GetProperty do reflection.
			PropertyInfo[] returnTypeProperties = returnType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			// Armazena o schema da tabela que será utilizado para fazer a tipagem e o mapeamento dos dados.
			DataTable schemaTable = reader.GetSchemaTable();

			// Lê cada registro encontrado.
			while (reader.Read()) {


				// Caso seja um tipo primitivo do .Net, apenas obtém o valor.
				if (returnType.FullName.IndexOf("System.") >= 0) {

					// Caso o valor da coluna seja nulo, não é necessário fazer mapeamentos adicionais.
					if (reader.IsDBNull(0)) {
						this.WriteOutput("InternalReader", "Nada encontrado na consulta.");
						continue;
					}

					this.WriteOutput("InternalReader", string.Format("Encontrado o valor '{0}'.", reader[0]));
					yield return (T)Convert.ChangeType(reader[0], returnType);

				}
				else {

					IList<KeyValuePair<string, object>> columns = new List<KeyValuePair<string, object>>();

					// Analisa todas as propriedades encontradas.
					for (int i = 0; i < reader.FieldCount; i++) {
						columns.Add(new KeyValuePair<string, object>(reader.GetName(i), reader[i]));
					}

					this.WriteOutput("InternalReader", string.Format("Criando a instancia do tipo '{0}'.", returnType.Name));

					T returnInstance = Activator.CreateInstance<T>();
					IList<string> mappedProperties = null;
					for (int i = 0; i < columns.Count; i++) {

						// Verifica se a coluna possui algum valor a ser mapeado.
						if (columns[i].Value == DBNull.Value || columns[i].Value == null) { continue; }

						if (mappedProperties == null) { mappedProperties = new List<string>(); }

						// Executa o mapeamento da propriedade encontrada.
						this.ParseProperty(returnType, returnTypeProperties, schemaTable, returnInstance, columns[i].Key, columns[i].Value, i, mappedProperties);
					}

					yield return returnInstance;
				}
			}
		}

		/// <summary>
		/// Executa a leitura dos dados de um DataReader.
		/// </summary>
		/// <typeparam name="T">Tipo do objeto que será preenchido.</typeparam>
		/// <param name="reader">Reader a ser utilizado para obter as informações do banco de dados.</param>
		/// <returns>Retorna uma coleção com os registros do tipo T encontrados no banco de dados.</returns>
		private IEnumerable<T> InternalReaderFetchAll<T>(SqlDataReader reader) {

			Type returnType = typeof(T);

			this.WriteOutput("InternalReader", string.Format("Obtendo as propriedades do tipo '{0}'.", returnType.Name));

			// Armazena todas as propriedades do objeto. Importante obter a propriedade desta coleção para que a busca possa ser case insensitive, ao contrário do GetProperty do reflection.
			PropertyInfo[] returnTypeProperties = returnType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			// Armazena o schema da tabela que será utilizado para fazer a tipagem e o mapeamento dos dados.
			DataTable schemaTable = reader.GetSchemaTable();

			// Coleção que será retornada.
			List<T> returnCollection = new List<T>();

			TaskFactory taskFactory = new TaskFactory();
			List<Task> taskList = new List<Task>();

			// Lê cada registro encontrado.
			while (reader.Read() == true) {

				T returnInstance;

				// Caso seja um tipo primitivo do .Net, apenas obtém o valor.
				if (returnType.FullName.IndexOf("System.") >= 0) {

					// Caso o valor da coluna seja nulo, não é necessário fazer mapeamentos adicionais.
					if (reader.IsDBNull(0) == true) {
						this.WriteOutput("InternalReader", "Nada encontrado na consulta.");
						continue;
					}

					// Obtém o valor encontrado na consulta.
					returnInstance = (T)Convert.ChangeType(reader[0], returnType);

					// Adiciona o registro preenchido na coleção que será retornada.
					returnCollection.Add(returnInstance);

					this.WriteOutput("InternalReader", string.Format("Encontrado o valor '{0}'.", reader[0]));
				}
				else {

					List<string> mappedProperties = new List<string>();

					List<KeyValuePair<string, object>> columns = new List<KeyValuePair<string, object>>();

					// Analisa todas as propriedades encontradas.
					for (int i = 0; i < reader.FieldCount; i++) {
						columns.Add(new KeyValuePair<string, object>(reader.GetName(i), reader[i]));
					}

					this.WriteOutput("InternalReader", string.Format("Criando a instancia do tipo '{0}'.", returnType.Name));

					returnInstance = Activator.CreateInstance<T>();

					// Adiciona o registro preenchido na coleção que será retornada.
					returnCollection.Add(returnInstance);

					Task task = taskFactory.StartNew(() => {

						for (int i = 0; i < columns.Count; i++) {

							// Verifica se a coluna possui algum valor a ser mapeado.
							if (columns[i].Value == DBNull.Value || columns[i].Value == null) { continue; }

							// Executa o mapeamento da propriedade encontrada.
							this.ParseProperty(returnType, returnTypeProperties, schemaTable, returnInstance, columns[i].Key, columns[i].Value, i, mappedProperties);
						}
					});

					taskList.Add(task);
				}

				// Adiciona o registro preenchido na coleção que será retornada.
				//returnCollection.Add(returnInstance);
			}

			Task.WaitAll(taskList.ToArray());

			return returnCollection;
		}

		/// <summary>
		/// Tenta mapear uma coluna encontrada no banco de dados para uma propriedade do objeto especificado.
		/// </summary>
		/// <param name="returnType">Tipo do objeto para onde os dados encontrados no banco serão mapeados.</param>
		/// <param name="schemaTable">Objeto contendo os metadados da tabela a qual a coluna encontrada pertence.</param>
		/// <param name="returnInstance">Instancia do objeto que será retornado.</param>
		/// <param name="columnName">Nome da coluna encontrada no banco de dados.</param>
		/// <param name="databaseValue">Valor encontrado no banco de dados.</param>
		/// <param name="ordinal">Índice da coluna. Usado para identificar a qual tabela pertence um campo quando houver joins.</param>
		/// <returns>Retorna true caso seja encontrada uma propriedade para a coluna, ou false, caso contrário.</returns>
		private bool ParseProperty(Type returnType, PropertyInfo[] returnTypeProperties, DataTable schemaTable, object returnInstance, string columnName, object databaseValue, int ordinal, IList<string> mappedProperties) {

			// Caso o valor encontrado no banco seja nulo, não é necessário obter o seu valor, passa para a próxima propriedade.
			if (databaseValue == null) { return false; }

			this.WriteOutput("ParseProperty", string.Format("Mapeando os dados da coluna '{0}' para o objeto {1}.", columnName, returnType.Name));

			// Armazena todas as propriedades do objeto. Importante obter a propriedade desta coleção para que a busca possa ser case insensitive, ao contrário do GetProperty do reflection.
			//PropertyInfo[] returnTypeProperties = returnType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			string explicitClassName = null;
			string explicitPropertyName = null;

			// Verifica se existe um ponto no nome da coluna. Caso positivo, indica que foi seguido o padrão "Classe.Propriedade" no alias da coluna.
			if (columnName.IndexOf('.') >= 0) {

				string[] aliasData = columnName.Split(new char[] { '.' }, 2);

				explicitClassName = aliasData[0];
				explicitPropertyName = aliasData[1];

				this.WriteOutput("ParseProperty", string.Format("Mapeamento explícito da propriedade '{0}' para o objeto {1}.", aliasData[1], aliasData[0]));
			}

			// Armazena o nome real da propriedade.
			string propertyName = explicitPropertyName ?? columnName;

			// Armazena o nome da propriedade esperada.
			string memberName = explicitClassName ?? propertyName;

			if (string.IsNullOrWhiteSpace(explicitClassName) == true) {
				// Sai do método caso a propriedade já tenha sido mapeada.
				//if (mappedProperties.Contains(returnType.Name + "." + ((string.IsNullOrWhiteSpace(memberName) == false) ? memberName + "." : string.Empty) + propertyName) == true) { return false; }
				if (mappedProperties.Contains(returnType.Name + "." + propertyName) == true) { return false; }
			}

			// Obtém a propriedade que possui o mesmo nome da propriedade encontrada na consulta ao banco.
			PropertyInfo propertyInfo = returnTypeProperties.FirstOrDefault(p => p.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));

			// Extrai os dados da coluna que estamos trabalhando, a partir do schema.
			DataRow dataRow = schemaTable.Select("ColumnName = '" + columnName + "' AND ColumnOrdinal = " + ordinal).FirstOrDefault();

			// Obtém o nome da tabela a qual a coluna pertence. O nome fica armazenado na coluna 11 para Sql ou 8 para SqlCe.
			string tableName = (dataRow != null) ? (explicitClassName ?? dataRow["BaseTableName"].ToString()) : explicitClassName;

			this.WriteOutput("ParseProperty", string.Format("Tabela de origem da coluna '{0}': '{1}'.", columnName, tableName));

			if (propertyInfo == null && string.IsNullOrWhiteSpace(tableName) == true) { return false; }

			if (string.IsNullOrWhiteSpace(tableName) == false) {

				//  Se a propriedade não foi encontrada, não é possível fazer o mapeamento do valor da coluna.
				if ((returnType.GetProperty(tableName) != null || propertyInfo == null || mappedProperties.IndexOf(returnType.Name + "." + propertyInfo.Name) >= 0) && (propertyInfo == null || (propertyInfo.PropertyType.FullName.IndexOf("System.") < 0 && propertyInfo.PropertyType.IsEnum == false))) {

					this.WriteOutput("ParseProperty", string.Format("Nenhuma propriedade encontrada no objeto '{0}' para a coluna '{1}'.", returnType.Name, propertyName));

					// Procura uma propriedade com o mesmo nome da tabela.
					PropertyInfo subPropertyInfo = returnTypeProperties.FirstOrDefault(p => p.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

					// Sai do método caso não exista uma propriedade com o nome da tabela.
					if (subPropertyInfo == null) { return false; }

					// Armazena o tipo da sub-propriedade.
					Type subPropertyType = subPropertyInfo.PropertyType;

					// Sai do método caso a propriedade já tenha sido mapeada.
					if (mappedProperties.Contains(returnType.Name + "." + subPropertyType.Name + "." + propertyName) == true) { return false; }

					// Verifica se a propriedade a ser mapeada é do tipo enum.
					if (subPropertyType.IsEnum == true) {

						object value = Enum.Parse(subPropertyType, databaseValue.ToString(), true);

						// Define o valor da propriedade.
						subPropertyInfo.SetValue(returnInstance, value, null);

						// Adiciona a propriedade na lista de dados já mapeados.
						mappedProperties.Add(returnType.Name + "." + subPropertyType.Name + "." + propertyName);

						return true;
					}

					// Tenta obter uma instancia para o sub-objeto. Caso não exista uma definida, cria uma nova.
					object subPropertyInstance = subPropertyInfo.GetValue(returnInstance, null) ?? Activator.CreateInstance(subPropertyType);

					PropertyInfo[] subPropertyTypeProperties = subPropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

					// Verifica se a propriedade comporta o valor encontrado no banco de dados.
					if (this.ParseProperty(subPropertyType, subPropertyTypeProperties, schemaTable, subPropertyInstance, propertyName ?? columnName, databaseValue, ordinal, mappedProperties) == true) {

						// Define o valor da propriedade encontrada.
						subPropertyInfo.SetValue(returnInstance, subPropertyInstance, null);

						// Adiciona a propriedade na lista de dados já mapeados.
						mappedProperties.Add(returnType.Name + "." + subPropertyType.Name + "." + subPropertyInfo.Name);

						return true;
					}

					return false;
				}
			}

			// Define o valor da propriedade encontrada.
			if (propertyInfo.CanWrite == true) {

				object value = databaseValue;

				// Caso a propriedade seja um enum, converte a string para enum.
				if (propertyInfo.PropertyType.IsEnum == true) { value = Enum.Parse(propertyInfo.PropertyType, databaseValue.ToString(), true); }

				else {
					// Obtém o tipo de destino da propriedade.
					Type propertyType = (propertyInfo.PropertyType.IsGenericType == true) ? propertyInfo.PropertyType.GetGenericArguments()[0] : propertyInfo.PropertyType;

					// Converte o tipo do banco de dados para o tipo correto da propriedade que receberá o valor.
					if (value.GetType() != propertyType) { value = Convert.ChangeType(value, propertyType); }
				}

				this.WriteOutput("ParseProperty", string.Format("Mapeando valor '{0}' para a propriedade '{1}' do objeto '{2}'.", value, propertyInfo.Name, returnType.Name));

				// Define o valor da propriedade.
				propertyInfo.SetValue(returnInstance, value, null);

				// Adiciona a propriedade na lista de dados já mapeados.
				mappedProperties.Add(returnType.Name + "." + propertyName);
				//mappedProperties.Add(returnType.Name + "." + ((string.IsNullOrWhiteSpace(memberName) == false) ? memberName + "." : string.Empty) + propertyName);
			}

			return true;
		}

		/// <summary>
		/// Starts a database transaction.
		/// </summary>
		/// <param name="isolationLevel">Transaction isolation level. Default: ReadUncommitted.</param>
		/// <returns>Returns the SqlTransaction created for this connector.</returns>
		public SqlTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted) {

			// Abre a conexão com o banco de dados.
			this.OpenConnection();

			// Inicializa uma transação de banco de dados.
			this.Transaction = this.Connection.BeginTransaction(isolationLevel);

			return this.Transaction;
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		public void Commit() {

			// Sai do método caso não exista uma transação de banco de dados.
			if (this.Transaction == null || this.Transaction.Connection == null) { throw new InvalidOperationException("Must have a valid database transaction."); }

			SqlConnection connection = this.Transaction.Connection;

			this.Transaction.Commit();

			connection.Close();
			connection = null;
			this.Transaction = null;
		}

		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		public void Rollback() {

			// Sai do método caso não exista uma transação de banco de dados.
			if (this.Transaction == null || this.Transaction.Connection == null) { throw new InvalidOperationException("Must have a valid database transaction."); }

			SqlConnection connection = this.Transaction.Connection;

			this.Transaction.Rollback();

			connection.Close();
			connection = null;
			this.Transaction = null;
		}

		#region Métodos estáticos públicos

		/// <summary>
		/// Starts a database transaction. This static method reaches all the DatabaseConnector instances regardless they expect a database transaction or not.
		/// </summary>
		/// <param name="connectionString">The connection used to open the SQL Server database.</param>
		/// <returns>Returns the created SqlTransaction.</returns>
		/// <exception cref="System.ArgumentNullException">Missing the connection string parameter.</exception>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="SqlTransaction"]/*'/>
		public static SqlTransaction BeginGlobalTransaction(string connectionString) {

			// Verifica se a connection string foi especificada.
			if (string.IsNullOrWhiteSpace(connectionString) == true) { throw new ArgumentNullException("connectionString"); }

			// Cria uma nova conexão com o banco de dados.
			SqlConnection connection = new SqlConnection(connectionString);

			// Abre a conexão, para que a transação possa ser criada.
			connection.Open();

			// Inicia a transação.
			_sqlTransaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted);

			return _sqlTransaction;
		}

		/// <summary>
		/// Commits the database transaction. This static method reaches all the DatabaseConnector instances regardless they expect a database transaction or not.
		/// </summary>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="SqlTransaction"]/*'/>
		public static void CommitGlobalTransaction() {

			// Sai do método caso não exista uma transação em progresso.
			if (_sqlTransaction == null) { return; }

			SqlConnection connection = _sqlTransaction.Connection;

			_sqlTransaction.Commit();

			connection.Close();
			connection = null;
			_sqlTransaction = null;
		}

		/// <summary>
		/// Rolls back a transaction from a pending state. This static method reaches all the DatabaseConnector instances regardless they expect a database transaction or not.
		/// </summary>
		/// <include file='Samples/DatabaseConnector.xml' path='Docs/Members[@name="SqlTransaction"]/*'/>
		public static void RollbackGlobalTransaction() {

			// Sai do método caso não exista uma transação em progresso.
			if (_sqlTransaction == null) { return; }

			SqlConnection connection = _sqlTransaction.Connection;

			_sqlTransaction.Rollback();

			connection.Close();
			connection = null;
			_sqlTransaction = null;
		}

		#endregion

		private void WriteOutput(string operationName, string description) {

			// Caso exista alguém aguardando pela saída das operações, dispara o evento.
			if (this.OnOutput != null) {

				try {
					this.OnOutput(this, OutputEventArgs.Create(operationName, description));
				}
				catch (Exception) {

				}
			}
		}

		/// <summary>
		/// Closes the connection to the database. This is the preferred method of closing any open connection.
		/// </summary>
		public void Close() {

			// Verifica se o acesso ao banco foi feito sem o uso de transações.
			if (this.Transaction == null || this.Transaction.Connection == null) {

				// Verifica se existe uma conexão de banco disponível.
				if (this.Connection != null) {

					// Fecha a conexão, caso esteja aberta.
					if (this.Connection.State != ConnectionState.Closed) { this.Connection.Close(); }

					this.Connection = null;
				}

				if (_sqlTransaction == this.Transaction) { _sqlTransaction = null; }

				this.Transaction = null;
			}
		}

		/// <summary>
		/// Disposes the DatabaseConnector instance.
		/// </summary>
		public void Dispose() {

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes the DatabaseConnector instance.
		/// </summary>
		/// <param name="disposing">Flag indicating the dispose process.</param>
		protected virtual void Dispose(bool disposing) {

			if (disposing == true) { this.Close(); }
		}
	}

	/// <summary>
	/// Enumerates all the available options for sorting queries result.
	/// </summary>
	public enum SortDirection {

		/// <summary>
		/// Organize the result of a query in a descending order.
		/// </summary>
		DESC = 0,

		/// <summary>
		/// Organize the result of a query in as acending order.
		/// </summary>
		ASC = 1,
	}
}
