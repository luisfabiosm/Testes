using Domain.Core.Models.Settings;

namespace Domain.Core.Models.SPA
{
    public record SPADependencia
    {
        public int Agencia { get; set; }
        public int Posto { get; set; }
        public SPADependencia() { }

        public SPADependencia(SPASettings spaConfig)
        {
            this.Agencia = spaConfig.Dependencia!.Agencia;
            this.Posto = spaConfig.Dependencia.Posto;
        }
    }
}
