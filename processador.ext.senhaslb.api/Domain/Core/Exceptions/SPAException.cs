using Domain.Core.Models.SPA;
using System.Reflection;

namespace Domain.Core.Exceptions
{
    public class SPAException : Exception
    {
        #region variáveis

        private readonly string _source = Assembly.GetExecutingAssembly().GetName().Name!;
        private  string _origem = string.Empty;

        #endregion

        public SPAError Error { get; internal set; }

        public string Origem
        {
            get { return $"{_source} @{_origem}"; }
            set { _origem = value; }
        }

        public SPAException(SPAError spaerror) : base()
        {
            Error = spaerror;
            Origem = _source;
        }

        public SPAException(Exception ex, string? source = null) : base(ex.Message, ex)
        {
            Error = new SPAError(ex);
            Origem = source!;
        }
    }
}
