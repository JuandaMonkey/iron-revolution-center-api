using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iron_revolution_center_api.Models
{
    public class DashboardModel
    {
        public long? Clientes_Registrados { get; set; } // registered clients

        public long? Clientes_Activos { get; set; } // active clients

        public long? Empleados_Registrados { get; set; } // registered employees

        public string? Sucursal_Mas_Frecuntada { get; set; } // most frecuented branch

        //public MembershipsCountModel[]? Preferencias { get; set; } // preferences

        //public MostPopularMembershipsModel[]? Membresias_Populares { get; set; } // most popular
    }

    //public class MembershipsCountModel
    //{
    //    public string? Nombre { get; set; } // name membership

    //    public long? Contro { get; set; } // count
    //}

    //public class MostPopularMembershipsModel
    //{
    //    public string? Nombre { get; set; } // name membership

    //    public long? Contro { get; set; } // count
    //}
}
