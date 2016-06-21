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

            string connectionString = @"Data Source=RJ10_DSK006\SQLEXPRESS;Initial Catalog=GlobalIdentity;Persist Security Info=True;User ID=AppDbUser;password=bbnsDoTVGpXIaLvgGsf8;Application Name=Dlp.Scow;";

            string queryString = @"SELECT Applications.ApplicationName, Applications.ApplicationKey, Users.Name as 'CurrentUser.Name', Users.Email as 'CurrentUser.Email'
                                   FROM Applications INNER JOIN Users ON Users.UserId = Applications.ParentUserId WHERE ApplicationId = @ApplicationId";

            using (DatabaseConnector databaseConnector = new DatabaseConnector(connectionString)) {

                Applications name = databaseConnector.ExecuteReader<Applications>(queryString, new { @ApplicationId = 12 }).FirstOrDefault();
            }
        }
    }
}
