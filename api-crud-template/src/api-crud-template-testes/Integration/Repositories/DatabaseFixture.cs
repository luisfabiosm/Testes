using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace api_crud_template_testes.Integration.Repositories
{
    public class DatabaseFixture : IDisposable
    {
        public DatabaseFixture()
        {
            // Here you would initialize shared test database resources
            // For example, using TestContainers to spin up a SQL Server instance
        }

        public void Dispose()
        {
            // Clean up shared resources
        }
    }
}
