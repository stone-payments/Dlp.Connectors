using System;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

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

    public class Program {

        public static void Main(string[] args) {

            string connectionString = @"Server=RJ10_DSK006\SQLEXPRESS;Initial Catalog=Balthazar;Persist Security Info=False;Integrated Security=True;Application Name=Dlp.Scow;";

            // Query utilizada para obter as a chave da loja.
            string queryString = "SELECT MerchantId FROM scow.Merchant WHERE MerchantKey = @MerchantKey;";

            Guid merchantKey = new Guid("FFDDC04F-36A6-4285-98CC-06326B4F3BCD");

            // Instancia o conector de acesso ao banco de dados.
            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                // Obtém o id da loja para a chave especificada.
                Nullable<long> merchantId = databaseConnector.ExecuteReader<Nullable<long>>(queryString, new { MerchantKey = merchantKey }).FirstOrDefault();
            }
        }
    }
}
