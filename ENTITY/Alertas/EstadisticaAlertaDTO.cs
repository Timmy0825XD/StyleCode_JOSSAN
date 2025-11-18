using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY.Alertas
{
    public class EstadisticaAlertaDTO
    {
        public int TotalAlertas { get; set; }
        public int AlertasPendientes { get; set; }
        public int AlertasResueltas { get; set; }
        public int AlertasCriticas { get; set; }
        public decimal DiasPromedioResolucion { get; set; }
        public int AlertasHoy { get; set; }
        public int AlertasSemana { get; set; }
    }
}
