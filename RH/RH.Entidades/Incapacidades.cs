//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RH.Entidades
{
    using System;
    using System.Collections.Generic;
    
    public partial class Incapacidades
    {
        public int Id { get; set; }
        public int IdEmpleado { get; set; }
        public string Folio { get; set; }
        public string Tipo { get; set; }
        public string Clase { get; set; }
        public System.DateTime FechaInicio { get; set; }
        public System.DateTime FechaFin { get; set; }
        public int Dias { get; set; }
        public string Observaciones { get; set; }
        public int IdIncapacidadesSat { get; set; }
        public int IdSucursal { get; set; }
        public int IdContrato { get; set; }
        public int IdEmpresaFiscal { get; set; }
        public decimal Sdi { get; set; }
        public System.DateTime FechaReg { get; set; }
        public int IdUsuarioReg { get; set; }
        public Nullable<System.DateTime> FechaMod { get; set; }
        public Nullable<int> IdUsuarioMod { get; set; }
    }
}
