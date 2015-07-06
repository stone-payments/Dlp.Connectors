using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dlp.Connectors;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Text;

namespace Dlp.Sdk.Tests.Connectors {

    #region Classes específicas para testes

    [ExcludeFromCodeCoverage]
    internal class ServiceLogData {

        public ServiceLogData() { }

        public DateTime CreateDate { get; set; }
        public string Category { get; set; }
        public string LogData { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class MerchantData {

        public MerchantData() { }

        public string Name { get; set; }
        public int MerchantId { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid MerchantKey { get; set; }
        public bool IsEnabled { get; set; }
        public string Url { get; set; }
        public StatusType Status { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class MerchantEntity {

        public MerchantEntity() { }

        public string Name { get; set; }
        public int MerchantId { get; set; }
        public MerchantConfigurationEntity MerchantConfiguration { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class MerchantConfigurationEntity {

        public MerchantConfigurationEntity() { }

        public string Url { get; set; }
        public bool IsEnabled { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class ComposedMerchant : AbstractMerchant {

        public ComposedMerchant() { }

        public string Name { get; set; }
        public int MerchantId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class AbstractMerchant {

        public string Url { get; set; }
        public bool IsEnabled { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class MistypedClass {

        public MistypedClass() { }

        public string MistypedName { get; set; }
        public int MistypedMerchantId { get; set; }
        public MistypedProperty SubProperty { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class MistypedProperty {

        public MistypedProperty() { }

        public int SubPropertyId { get; set; }
        public int MistypedClassId { get; set; }
        public bool IsEnabled { get; set; }
        public string Address { get; set; }
        public MistypedProperty AnotherSubProperty { get; set; }
    }

    internal enum StatusType {

        Undefined = 0,
        Created = 1,
        Disabled = 2,
        Suspended = 3
    }

    [ExcludeFromCodeCoverage]
    internal class MerchantBool {

        public MerchantBool() { }

        public bool MerchantId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    internal class BulkData {

        public BulkData() { }

        public string Name { get; set; }
        public string Value { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }
    }

    internal class SimpleTable {

        public SimpleTable() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public SimpleTableRelationship SimpleTableRelationship { get; set; }
    }

    internal class SimpleTableRelationship {

        public SimpleTableRelationship() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public int SimpleTableId { get; set; }
    }

    #endregion

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DatabaseConnectorTest {

        private static string connectionString;

        private static bool _tempDirCreated = false;

        private const string _databaseDirectory = @"C:\temp";

        [ClassInitialize]
        public static void PrepareDatabaseTests(TestContext context) {

            // Verifica se o diretório temporário existe.
            if (Directory.Exists(_databaseDirectory) == false) {

                Directory.CreateDirectory(_databaseDirectory);

                _tempDirCreated = true;
            }

            // Apaga o banco de dados, se por acaso ele estiver carregado no SqlServer.
            ClearTempDatabase();

            // Monta o nome do arquivo temporário.
            string databaseTempFile = string.Format(@"{0}\UnitTestDatabase.mdf", _databaseDirectory);

            // Copia o banco de dados de testes para o diretório temporário.
            File.Copy("UnitTestDatabase.mdf", databaseTempFile, true);

            connectionString = string.Format(@"Data Source=(LocalDB)\v11.0;AttachDbFilename={0};Integrated Security=True;Connect Timeout=10;", databaseTempFile);
        }

        [ClassCleanup]
        public static void FinalizeDatabaseTests() {

            SqlConnection.ClearAllPools();

            // Aguarda a liberação dos pools.
            System.Threading.Thread.Sleep(1000);

            ClearTempDatabase();
        }

        private static void ClearTempDatabase() {

            try {
                // Monta os nomes do arquivo temporário.
                string databaseTempFile = string.Format(@"{0}\UnitTestDatabase.mdf", _databaseDirectory);
                string databaseTempLogFile = string.Format(@"{0}\UnitTestDatabase_log.ldf", _databaseDirectory);

                connectionString = @"server=(local);Data Source=(LocalDB)\v11.0;Integrated Security=True;Connect Timeout=10;";

                string query = string.Format(@"DROP DATABASE [{0}]", databaseTempFile);

                using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) { databaseConnector.ExecuteNonQuery(query); }

                if (_tempDirCreated == true) {

                    // Exclui o diretório temporário, caso a aplicação tenha criado.
                    Directory.Delete(_databaseDirectory, true);

                    _tempDirCreated = false;
                }
                else {
                    if (File.Exists(databaseTempFile) == true) { File.Delete(databaseTempFile); }
                    if (File.Exists(databaseTempLogFile) == true) { File.Delete(databaseTempLogFile); }
                }
            }
            catch (Exception ex) {

                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateDatabaseConnectorWithEmptyConnectionString() {

            DatabaseConnector databaseConnector = new DatabaseConnector(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ExecuteNonQueryWithInvalidParameter() {

            string query = "SELECT Merchant.Name FROM Merchant WHERE Banana = 1;";

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.ExecuteNonQuery(query);
            }
        }

        [TestMethod]
        public void LoadSingleRow() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, Merchant.CreateDate, Merchant.MerchantKey FROM Merchant WHERE MerchantId = 1;";

            MerchantData actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query).FirstOrDefault();
            }

            Assert.IsNotNull(actual);

            Assert.AreEqual("Merchant Number One", actual.Name);
            Assert.AreEqual(1, actual.MerchantId);
            Assert.AreEqual(Guid.Parse("fee2437e-c810-4c2b-a836-5f619f80bb76"), actual.MerchantKey);
            Assert.AreEqual("2014-07-30 13:06:10", actual.CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        public void LoadMultipleRows() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, Merchant.CreateDate, Merchant.MerchantKey FROM Merchant ORDER BY Merchant.MerchantId ASC;";

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            IEnumerable<MerchantData> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query);
            }

            stopwatch.Stop();

            System.Diagnostics.Debug.WriteLine(string.Format("ELAPSED: {0}", stopwatch.ElapsedMilliseconds));

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count());

            Assert.AreEqual("Merchant Number One", actual.ElementAt(0).Name);
            Assert.AreEqual(1, actual.ElementAt(0).MerchantId);
            Assert.AreEqual(Guid.Parse("fee2437e-c810-4c2b-a836-5f619f80bb76"), actual.ElementAt(0).MerchantKey);
            Assert.AreEqual("2014-07-30 13:06:10", actual.ElementAt(0).CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            Assert.AreEqual("Another Merchant", actual.ElementAt(1).Name);
            Assert.AreEqual(2, actual.ElementAt(1).MerchantId);
            Assert.AreEqual(Guid.Parse("eb04aaaa-e8ca-4e14-8068-0f3008a716b9"), actual.ElementAt(1).MerchantKey);
            Assert.AreEqual("2014-07-30 13:06:47", actual.ElementAt(1).CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));

            Assert.AreEqual("Merchant Test", actual.ElementAt(2).Name);
            Assert.AreEqual(3, actual.ElementAt(2).MerchantId);
            Assert.AreEqual(Guid.Parse("c5d66001-8a39-4a04-a22c-d3a190018c46"), actual.ElementAt(2).MerchantKey);
            Assert.AreEqual("2014-07-30 13:07:07", actual.ElementAt(2).CreateDate.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void LoadSingleRowWithParametersFromEntityWithInvalidParameter() {

            string query = @"SELECT Merchant.Name FROM Merchant WHERE Merchant.MerchantId = @MerchantId AND Merchant.Name = @MerchantConfiguration;";

            MerchantData actual = null;

            MerchantEntity merchantEntity = new MerchantEntity();
            merchantEntity.MerchantId = 2;
            merchantEntity.MerchantConfiguration = new MerchantConfigurationEntity();

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, merchantEntity).FirstOrDefault();
            }
        }

        [TestMethod]
        public void LoadSingleRowWithJoin() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, MerchantConfiguration.Url, MerchantConfiguration.IsEnabled
                             FROM Merchant
                             INNER JOIN MerchantConfiguration ON MerchantConfiguration.MerchantId = Merchant.MerchantId
                             WHERE Merchant.MerchantId IN (2, 3)
                             ORDER BY Merchant.MerchantId ASC;";

            IEnumerable<MerchantData> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count());

            Assert.AreEqual("Another Merchant", actual.ElementAt(0).Name);
            Assert.AreEqual(2, actual.ElementAt(0).MerchantId);
            Assert.AreEqual("http://www.anothermerchant.com.br", actual.ElementAt(0).Url);
            Assert.IsFalse(actual.ElementAt(0).IsEnabled);

            Assert.AreEqual("Merchant Test", actual.ElementAt(1).Name);
            Assert.AreEqual(3, actual.ElementAt(1).MerchantId);
            Assert.IsNull(actual.ElementAt(1).Url);
            Assert.IsTrue(actual.ElementAt(1).IsEnabled);
        }

        [TestMethod]
        public void LoadMultipleRowsWithParameters() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId
                             FROM Merchant
                             INNER JOIN MerchantConfiguration ON MerchantConfiguration.MerchantId = Merchant.MerchantId
                             WHERE MerchantConfiguration.IsEnabled = @IsEnabled;";

            IEnumerable<MerchantData> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                // Define o parâmetro IsEnabled da query como true, para retornar apenas as lojas habilitadas.
                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { IsEnabled = true });
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count());

            Assert.AreEqual("Merchant Number One", actual.ElementAt(0).Name);
            Assert.AreEqual(1, actual.ElementAt(0).MerchantId);

            Assert.AreEqual("Merchant Test", actual.ElementAt(1).Name);
            Assert.AreEqual(3, actual.ElementAt(1).MerchantId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadRowWithEmptyQuery() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                databaseConnector.ExecuteReader<int>(string.Empty);
            }
        }

        [TestMethod]
        public void LoadSingleRowWithNestedEntity() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, MerchantConfiguration.IsEnabled, MerchantConfiguration.Url
                             FROM Merchant
                             INNER JOIN MerchantConfiguration ON MerchantConfiguration.MerchantId = Merchant.MerchantId
                             WHERE Merchant.MerchantId = 1";

            MerchantEntity actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteReader<MerchantEntity>(query).FirstOrDefault();
            }

            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.MerchantConfiguration);

            Assert.AreEqual("Merchant Number One", actual.Name);
            Assert.AreEqual(1, actual.MerchantId);

            Assert.IsTrue(actual.MerchantConfiguration.IsEnabled);
            Assert.AreEqual("http://www.merchantnumberone.com.br", actual.MerchantConfiguration.Url);
        }

        [TestMethod]
        public void LoadDatabaseDateTimeWithScalar() {

            string query = @"SELECT GETDATE();";

            object actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteScalar<DateTime>(query);
            }

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(DateTime));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidCastException))]
        public void LoadDatabaseDateTimeWithScalarToInt() {

            string query = @"SELECT GETDATE();";

            object actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteScalar<int>(query);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadDatabaseDateTimeWithoutQuery() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                object actual = databaseConnector.ExecuteScalar<DateTime>(null);
            }
        }

        [TestMethod]
        public void LoadDatabaseTimeWithoutResult() {

            string query = @"SELECT GETDATE() WHERE 1 <> 1;";

            DateTime actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteScalar<DateTime>(query);
            }

            Assert.AreEqual(DateTime.MinValue, actual);
        }

        [TestMethod]
        public void LoadMerchantNameFromId() {

            string query = @"SELECT Name FROM Merchant WHERE MerchantId = @MerchantId;";

            string actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteScalar<string>(query, new { MerchantId = 1 });
            }

            Assert.AreEqual("Merchant Number One", actual);
        }

        [TestMethod]
        public void ExecuteNonQuery() {

            string query = @"UPDATE Merchant SET Merchant.Name = Merchant.Name WHERE Merchant.MerchantId = 1;";

            int actual = 0;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                actual = databaseConnector.ExecuteNonQuery(query);
            }

            Assert.AreEqual(1, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ExecuteNonQueryWithNullQueryString() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {
                databaseConnector.ExecuteNonQuery(null);
            }
        }

        [TestMethod]
        public void InsertMerchantWithDatabaseRollback() {

            string query = @"INSERT INTO Merchant (Name) VALUES ('Temp Merchant');";

            SqlTransaction sqlTransaction = DatabaseConnector.BeginGlobalTransaction(connectionString);

            DatabaseConnector databaseConnector = new DatabaseConnector(connectionString);

            databaseConnector.ExecuteNonQuery(query);

            Nullable<int> merchantId = databaseConnector.ExecuteScalar<Nullable<int>>("SELECT MerchantId FROM Merchant WHERE Name = 'Temp Merchant';");

            sqlTransaction.Rollback();

            merchantId = databaseConnector.ExecuteScalar<Nullable<int>>("SELECT MerchantId FROM Merchant WHERE Name = 'Temp Merchant';");

            databaseConnector.Close();
        }

        [TestMethod]
        public void LoadDateTimeWithDatabaseCommitTransactionAndEmptyConnectionString() {

            string query = @"SELECT GETDATE();";

            DatabaseConnector.BeginGlobalTransaction(connectionString);

            try {
                using (DatabaseConnector databaseConnector = new DatabaseConnector(string.Empty)) {
                    DateTime actual = databaseConnector.ExecuteScalar<DateTime>(query);
                }
            }
            finally {
                DatabaseConnector.CommitGlobalTransaction();
            }
        }

        [TestMethod]
        public void LoadMerchantsAndCommitWithLocalTransaction() {

            string query = @"SELECT Merchant.Name FROM Merchant WHERE MerchantId = @MerchantId;";

            MerchantData actual = null;

            DatabaseConnector databaseConnector = new DatabaseConnector(connectionString);

            try {
                // Inicializa uma transação de banco de dados.
                databaseConnector.BeginTransaction();

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { MerchantId = 1 }).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual("Merchant Number One", actual.Name);

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { MerchantId = 2 }).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual("Another Merchant", actual.Name);

                databaseConnector.Commit();
            }
            catch {
                databaseConnector.Rollback();
            }
            finally {
                databaseConnector.Close();
            }
        }

        [TestMethod]
        public void LoadMerchantsAndRollbackWithLocalTransaction() {

            string query = @"SELECT Merchant.Name FROM Merchant WHERE MerchantId = @MerchantId;";

            MerchantData actual = null;

            DatabaseConnector databaseConnector = new DatabaseConnector(connectionString);

            try {
                // Inicializa uma transação de banco de dados.
                databaseConnector.BeginTransaction();

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { MerchantId = 1 }).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual("Merchant Number One", actual.Name);

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { MerchantId = 2 }).FirstOrDefault();

                Assert.IsNotNull(actual);
                Assert.AreEqual("Another Merchant", actual.Name);
            }
            finally {
                databaseConnector.Rollback();
                databaseConnector.Close();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CommitNullTransaction() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.Commit();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RollbackNullTransaction() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.Rollback();
            }
        }

        [TestMethod]
        public void DatabaseCommitNullTransaction() {

            DatabaseConnector.CommitGlobalTransaction();
        }

        [TestMethod]
        public void DatabaseRollbackNullTransaction() {

            DatabaseConnector.RollbackGlobalTransaction();
        }

        [TestMethod]
        public void LoadDateTimeWithDatabaseRollbackTransaction() {

            string query = @"SELECT GETDATE();";

            DatabaseConnector.BeginGlobalTransaction(connectionString);

            try {
                using (DatabaseConnector databaseConnector = new DatabaseConnector()) {
                    DateTime actual = databaseConnector.ExecuteScalar<DateTime>(query);
                }
            }
            finally {
                DatabaseConnector.RollbackGlobalTransaction();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BeginTransactionWithNullConnectionString() {

            DatabaseConnector.BeginGlobalTransaction(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateConnectorWithNullTransaction() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector()) { }
        }

        [TestMethod]
        public void LoadMultipleRowsWithEnumProperty() {

            string query = @"SELECT Merchant.Name, Merchant.Status FROM Merchant WHERE Merchant.MerchantId IN (1, 2) ORDER BY Merchant.MerchantId;";

            IEnumerable<MerchantData> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count());

            Assert.AreEqual(StatusType.Created, actual.ElementAt(0).Status);
            Assert.AreEqual(StatusType.Disabled, actual.ElementAt(1).Status);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LoadSingleRowWithInvalidEnumProperty() {

            string query = @"SELECT Merchant.Name, Merchant.Status FROM Merchant WHERE Merchant.MerchantId = 3;";

            MerchantData actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query).FirstOrDefault();
            }
        }

        [TestMethod]
        public void LoadSingleRowWithEnumParameter() {

            string query = @"SELECT Merchant.Name FROM Merchant WHERE Merchant.Status = @Status;";

            MerchantData actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { Status = StatusType.Disabled }).FirstOrDefault();
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual("Another Merchant", actual.Name);
        }

        [TestMethod]
        public void ParseIntValueToBooleanProperty() {

            string query = @"SELECT Merchant.MerchantId FROM Merchant;";

            IEnumerable<MerchantBool> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantBool>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count());
            Assert.IsFalse(actual.Any(p => p.MerchantId == false));
        }

        [TestMethod]
        public void SearchWithEmptyStringParameter() {

            string query = @"SELECT Merchant.MerchantId FROM Merchant WHERE Merchant.Name = @Name;";

            IEnumerable<MerchantData> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { Name = string.Empty });
            }

            Assert.IsNotNull(actual);
            Assert.IsFalse(actual.Any());
        }

        [TestMethod]
        public void ParseNullParameter() {

            PrivateType privateType = new PrivateType(typeof(DatabaseConnector));

            object result = privateType.InvokeStatic("ParseProperty", null, null, null, null, null, 0, null);

            bool parsedResult = Convert.ToBoolean(result);

            Assert.IsFalse(parsedResult);
        }

        [TestMethod]
        public void BulkInsert() {

            IList<BulkData> source = new List<BulkData>();

            int dataCount = 100;

            DateTime currentDate = DateTime.UtcNow;

            for (int i = 0; i < dataCount; i++) {

                BulkData bulkData = new BulkData();

                bulkData.Name = string.Format("Name-{0}", i);

                if (i % 2 == 0) { bulkData.Value = i.ToString(); }

                bulkData.CreateDate = currentDate;

                source.Add(bulkData);
            }

            string query = @"DELETE FROM BulkData;";

            int actual = 0;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                // Limpa qualquer informação pré-existente.
                databaseConnector.ExecuteNonQuery(query);

                // Insere os registros.
                databaseConnector.BulkInsert("BulkData", source);

                // Exclui os registros e armazena a quantidade excluída.
                actual = databaseConnector.ExecuteNonQuery(query);
            }

            Assert.AreEqual(dataCount, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BulkInsertMissingMandatoryField() {

            IList<BulkData> source = new List<BulkData>();

            int dataCount = 1;

            DateTime currentDate = DateTime.UtcNow;

            for (int i = 0; i < dataCount; i++) {

                BulkData bulkData = new BulkData();

                bulkData.Name = string.Format("Name-{0}", i);

                if (i % 2 == 0) { bulkData.Value = i.ToString(); }

                source.Add(bulkData);
            }

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                // Insere os registros.
                databaseConnector.BulkInsert("BulkData", source);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BulkInsertEmptyTableName() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.BulkInsert(string.Empty, new List<int>());
            }
        }

        [TestMethod]
        public void BulkInsertNullCollection() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.BulkInsert("BulkData", null);
            }
        }

        [TestMethod]
        public void LoadPagedData() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, Merchant.MerchantKey FROM Merchant;";

            KeyValuePair<int, IEnumerable<MerchantData>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, 1, 2, "MerchantId", SortDirection.DESC);
            }

            Assert.AreEqual(3, actual.Key);
            Assert.IsNotNull(actual.Value);
            Assert.AreEqual(2, actual.Value.Count());

            Assert.AreEqual(3, actual.Value.ElementAt(0).MerchantId);
            Assert.AreEqual(2, actual.Value.ElementAt(1).MerchantId);
        }

        [TestMethod]
        public void LoadPagedDataWithParameters() {

            string query = @"SELECT Name, MerchantId, MerchantKey FROM Merchant WHERE Merchant.Status = @Status;";

            KeyValuePair<int, IEnumerable<MerchantData>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, 1, 2, "MerchantId", SortDirection.DESC, new { Status = StatusType.Disabled });
            }

            Assert.AreEqual(1, actual.Key);
            Assert.IsNotNull(actual.Value);
            Assert.AreEqual(1, actual.Value.Count());

            Assert.AreEqual(2, actual.Value.ElementAt(0).MerchantId);
        }

        [TestMethod]
        public void LoadPageDataWithInvalidPages() {

            string query = @"SELECT Name, MerchantId, MerchantKey FROM Merchant;";

            KeyValuePair<int, IEnumerable<MerchantData>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, 0, 0, "MerchantId", SortDirection.DESC);
            }

            Assert.AreEqual(3, actual.Key);
            Assert.IsNotNull(actual.Value);
            Assert.AreEqual(1, actual.Value.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadPageDataWithEmptyQuery() {

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                databaseConnector.ExecuteReader<MerchantData>(string.Empty, 1, 2, "MerchantId", SortDirection.DESC);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoadPageDataWithNullOrderColumn() {

            string query = @"SELECT Name, MerchantId, MerchantKey FROM Merchant;";

            KeyValuePair<int, IEnumerable<MerchantData>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, 0, 0, null, SortDirection.DESC);
            }
        }

        [TestMethod]
        public void LoadPageDataWithSubquery() {

            string query = @"SELECT Name, MerchantId, MerchantKey FROM Merchant WHERE MerchantId IN (SELECT MerchantId FROM MerchantConfiguration WHERE IsEnabled = 1);";

            KeyValuePair<int, IEnumerable<MerchantData>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, 1, 2, "MerchantId", SortDirection.DESC);
            }

            Assert.AreEqual(2, actual.Key);
            Assert.IsNotNull(actual.Value);

            Assert.AreEqual(3, actual.Value.ElementAt(0).MerchantId);
            Assert.AreEqual(1, actual.Value.ElementAt(1).MerchantId);
        }

        [TestMethod]
        public void LoadPageDataWithJoin() {

            string query = @"SELECT Merchant.Name, Merchant.MerchantId, MerchantConfiguration.Url, MerchantConfiguration.IsEnabled
                             FROM Merchant
                             INNER JOIN MerchantConfiguration ON MerchantConfiguration.MerchantId = Merchant.MerchantId;";

            KeyValuePair<int, IEnumerable<MerchantEntity>> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantEntity>(query, 1, 2, "Merchant.MerchantId", SortDirection.ASC);
            }

            Assert.AreEqual(3, actual.Key);
            Assert.IsNotNull(actual.Value);

            Assert.AreEqual(1, actual.Value.ElementAt(0).MerchantId);
            Assert.AreEqual(2, actual.Value.ElementAt(1).MerchantId);

            Assert.IsNotNull(actual.Value.ElementAt(0).MerchantConfiguration);
            Assert.IsNotNull(actual.Value.ElementAt(1).MerchantConfiguration);

            Assert.IsTrue(actual.Value.ElementAt(0).MerchantConfiguration.IsEnabled);
            Assert.IsFalse(actual.Value.ElementAt(1).MerchantConfiguration.IsEnabled);
        }

        [TestMethod]
        public void LoadSingleRowWithColumnAlias() {

            string query = @"SELECT Merchant.Name, MerchantConfiguration.Url AS 'Value', Merchant.CreateDate
                             FROM Merchant
                             INNER JOIN MerchantConfiguration ON MerchantConfiguration.MerchantId = Merchant.MerchantId
                             WHERE Merchant.MerchantId = 1;";

            BulkData actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<BulkData>(query).FirstOrDefault();
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual("Merchant Number One", actual.Name);
            Assert.AreEqual("http://www.merchantnumberone.com.br", actual.Value);
        }

        [TestMethod]
        public void LoadSingleRowUsingQueryWithComment() {

            string query = @"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
                                
                             -- Campos a serem retornados.
                             SELECT Merchant.Name, Merchant.MerchantId
                             FROM Merchant
                             WHERE Merchant.MerchantId = 1;";

            MerchantData actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query).FirstOrDefault();
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.MerchantId);
        }

        [TestMethod]
        public void LoadSimpleTypeIds() {

            string query = @"SELECT MerchantId FROM Merchant;";

            IEnumerable<int> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<int>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count());
            Assert.IsTrue(actual.Any(p => p == 1));
            Assert.IsTrue(actual.Any(p => p == 2));
            Assert.IsTrue(actual.Any(p => p == 3));
        }

        [TestMethod]
        public void LoadSimpleTypeKeys() {

            string query = @"SELECT MerchantKey FROM Merchant;";

            IEnumerable<Guid> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<Guid>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count());
            Assert.IsTrue(actual.Any(p => p == Guid.Parse("fee2437e-c810-4c2b-a836-5f619f80bb76")));
            Assert.IsTrue(actual.Any(p => p == Guid.Parse("eb04aaaa-e8ca-4e14-8068-0f3008a716b9")));
            Assert.IsTrue(actual.Any(p => p == Guid.Parse("c5d66001-8a39-4a04-a22c-d3a190018c46")));
        }

        [TestMethod]
        public void LoadSimpleDateTimes() {

            string query = @"SELECT CreateDate FROM Merchant;";

            IEnumerable<DateTime> actual = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<DateTime>(query);
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(3, actual.Count());
        }

        [TestMethod]
        public void LoadMerchantsWithInClause() {

            string query = @"SELECT Name FROM Merchant WHERE MerchantKey IN (@MerchantKeyCollection)";

            IEnumerable<Guid> merchantKeyCollection = new Guid[] { Guid.Parse("fee2437e-c810-4c2b-a836-5f619f80bb76"), Guid.Parse("c5d66001-8a39-4a04-a22c-d3a190018c46") };

            IEnumerable<MerchantData> actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<MerchantData>(query, new { MerchantKeyCollection = merchantKeyCollection });
            }

            Assert.IsNotNull(actual);
            Assert.AreEqual(2, actual.Count());

            Assert.IsTrue(actual.Any(p => p.Name.Equals("Merchant Number One")));
            Assert.IsTrue(actual.Any(p => p.Name.Equals("Merchant Test")));
        }

        [TestMethod]
        public void LoadMerchantDataForComposedClass() {

            string query = @"SELECT m.Name,
                             m.MerchantId,
                             mc.Url,
                             mc.IsEnabled
                             FROM Merchant m
                             INNER JOIN MerchantConfiguration mc ON mc.MerchantId = m.MerchantId
                             WHERE m.MerchantId = @MerchantId";

            ComposedMerchant actual;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                actual = databaseConnector.ExecuteReader<ComposedMerchant>(query, new { MerchantId = 1 }).FirstOrDefault();
            }

            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public void MultipleTransactions() {

            DatabaseConnector databaseConnector = new DatabaseConnector(connectionString);

            //DatabaseConnector.BeginTransaction(connectionString);

            try {
                databaseConnector.BeginTransaction();

                databaseConnector.ExecuteNonQuery("SELECT banana FROM Merchant;");

                databaseConnector.Commit();

                //DatabaseConnector.CommitTransaction();
            }
            catch (Exception) {

                //DatabaseConnector.RollbackTransaction();
                databaseConnector.Rollback();
            }

            using (DatabaseConnector dbConnector = new DatabaseConnector(connectionString)) {

                DateTime dateTime = dbConnector.ExecuteScalar<DateTime>("SELECT GETUTCDATE();");
            }
        }

        [TestMethod]
        public void MapNonEntityClass() {

            string query = @"SELECT m.Name AS MistypedName, m.MerchantId AS MistypedMerchantId,
                             mc.Url AS 'SubProperty.Address', mc.IsEnabled AS 'SubProperty.AnotherSubProperty.IsEnabled'
                             FROM Merchant m
                             INNER JOIN MerchantConfiguration mc ON mc.MerchantId = m.MerchantId
                             WHERE m.MerchantId = 1;";

            MistypedClass result = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                result = databaseConnector.ExecuteReader<MistypedClass>(query).FirstOrDefault();
            }

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void LoadMerchantIdWithScalarAndInClause() {

            string query = @"SELECT Merchant.Name FROM Merchant WHERE Merchant.MerchantId IN (@MerchantIds);";

            IEnumerable<int> merchantIds = new int[] { 1 };

            string result = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                result = databaseConnector.ExecuteScalar<string>(query, new { MerchantIds = merchantIds });
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(result, "Merchant Number One");
        }

        [TestMethod]
        public void UntrustedColumnNameWithJoin() {

            string query = @"SELECT sr.Id, sr.Name, sr.SimpleTableId FROM SimpleTableRelationship AS sr INNER JOIN SimpleTable s ON s.Id = sr.SimpleTableId WHERE sr.Name LIKE 'Main configuration'";

            SimpleTableRelationship result = null;

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                result = databaseConnector.ExecuteReader<SimpleTableRelationship>(query).FirstOrDefault();
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, 2);
            Assert.AreEqual(result.SimpleTableId, 1);
            Assert.AreEqual(result.Name, "Main configuration");
        }
    }
}
