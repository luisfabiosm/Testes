namespace Domain.Core.SharedKernel
{
    public static class Global
    {

        public const string API_NAME = "api-crud-template";
        public const string API_VERSION = "v1";
        public const string API_TITLE = "API CRUD Template";
        public const string API_DESCRIPTION = "A template for building CRUD APIs with .NET 8, Dapper, and SQL Server.";
        public static string ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

        //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJhcGktY3J1ZC10ZW1wbGF0ZSIsImF1ZCI6ImFwaS1jcnVkLXRlbXBsYXRlIiwiaWF0IjoxNzU4MzExMjk3LCJleHAiOjE3ODk5MzM2OTd9.ekMhaR-lUtS1sNWsERLVDCE1FlAuLuMW9sIZIS4TeKY


        //{
        //"cpf": "123.456.789-00",
        //"nome": "Maria Silva Santos",
        //"nascimento": "1990-05-15T00:00:00.000Z",
        //"email": "maria.santos@email.com",
        //"login": "maria_santos",
        //"password": "MinhaSenh@123!"
        //}
    }

}

