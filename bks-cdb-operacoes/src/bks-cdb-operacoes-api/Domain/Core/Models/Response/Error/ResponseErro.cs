namespace Domain.Core.Models.Response.Error
{
    public class ResponseErro
    {
        public List<Erro> Erro { get; set; }
    }
    public class Erro
    {
        public string cod { get; set; }
        public string valor { get; set; }
    }
}
