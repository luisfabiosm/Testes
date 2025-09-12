namespace Domain.Core.Models.Util
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ProcedureAttribute : Attribute
    {
        public string Nome { get; set; }

        public ProcedureAttribute(string nome)
        {
            Nome = nome;
        }
    }
}
