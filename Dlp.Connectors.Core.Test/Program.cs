using Dlp.Connectors.Core;
using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.IO;
using System.Data.SqlClient;

namespace Dlp.Connectors.Core.Test {

    public enum ApplicationType {

        FirstApplication = 2,

        AnotherApplication = 12
    }

    public sealed class Applications {

        public Applications() { }

        public string ApplicationName { get; set; }

        public Guid ApplicationKey { get; set; }

        //public string Username { get; set; }

        //public string Email { get; set; }

        public Users CurrentUser { get; set; }
    }

    public sealed class Users {

        public Users() { }

        public string Name { get; set; }

        public string Email { get; set; }
    }

    internal class MerchantData {

        public MerchantData() { }

        public string Name { get; set; }
        public string Merchant { get; set; }
        public int MerchantId { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }
        public Guid MerchantKey { get; set; }
        public bool IsEnabled { get; set; }
        public string Url { get; set; }
        public StatusType Status { get; set; }
        public Nullable<int> OptionalId { get; set; }
    }

    internal enum StatusType {

        Undefined = 0,
        Created = 1,
        Disabled = 2,
        Suspended = 3
    }

    public class Program {

        private static string connectionString;

        private static bool _tempDirCreated = false;

        private const string _databaseDirectory = @"C:\temp";

        private static void BeforeTests() {

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
            File.Copy("../Dlp.Connectors.Test/UnitTestDatabase.mdf", databaseTempFile, true);

            connectionString = string.Format(@"Data Source=(LocalDB)\mssqllocaldb;AttachDbFilename={0};Integrated Security=True;Connect Timeout=10;", databaseTempFile);
        }

        private static void AfterTests() {

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

                connectionString = @"server=(local);Data Source=(LocalDB)\mssqllocaldb;Integrated Security=True;Connect Timeout=10;";

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

        public static void Main(string[] args) {

            try {
                BeforeTests();

                // Query utilizada para obter as a chave da loja.
                string queryString = "SELECT Merchant.Name, Merchant.MerchantId, Merchant.CreateDate, Merchant.MerchantKey FROM Merchant WHERE MerchantId = 1;";

                Guid merchantKey = new Guid("fee2437e-c810-4c2b-a836-5f619f80bb76");

                // Instancia o conector de acesso ao banco de dados.
                using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                    // Obtém o id da loja para a chave especificada.
                    MerchantData merchant = databaseConnector.ExecuteReader<MerchantData>(queryString, new { MerchantKey = merchantKey }).FirstOrDefault();
                }
            }
            finally {
                AfterTests();
            }
        }
    }
}
