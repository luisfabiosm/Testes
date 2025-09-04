using System.Data;

namespace Domain.Core.Models.Util
{
    public class ParameterAttribute : Attribute
    {
        public string Nome { get; set; }
        public ParameterDirection Direction { get; set; }

        public ParameterAttribute(string nome)
        {
            Nome = nome;
            Direction = ParameterDirection.Input;
        }
    }
}
