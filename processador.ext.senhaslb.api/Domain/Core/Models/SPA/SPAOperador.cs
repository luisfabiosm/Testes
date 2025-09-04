using Domain.Core.Models.Settings;

namespace Domain.Core.Models.SPA
{
    public sealed record SPAOperador : IDisposable
    {
        private string? _supervisor;
        public string? CodigoOperador { get; internal set; }

        public string CodigoSupervisor
        {
            get { return _supervisor!; }
        }

        public SPAOperador(SPASettings spaConfig)
        {
            this.CodigoOperador = spaConfig.Operador;
            this._supervisor = null;
        }

        public void ConfigSupervisor(string codigo)
        {
            this._supervisor = codigo;
        }

        public void Dispose()
        {
            this.CodigoOperador = null;
            this._supervisor = null;
        }
    }
}
