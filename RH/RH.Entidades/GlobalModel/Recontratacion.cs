﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RH.Entidades.GlobalModel
{
   public class Recontratacion
    {
        public int IdContrato { get; set; }
        public int IdEmpleado { get; set; }
        public System.DateTime FechaAlta { get; set; }
        public System.DateTime FechaReal { get; set; }
        public Nullable<System.DateTime> FechaIMSS { get; set; }
        public int IdPuesto { get; set; }
        public decimal SD { get; set; }
        public decimal SDI { get; set; }
        public decimal SalarioReal { get; set; }
        public int TipoContrato { get; set; }
        public int FormaPago { get; set; }
        public int TipoSalario { get; set; }
        public Nullable<int> IdEmpresaFiscal { get; set; }
        public Nullable<int> IdEmpresaComplemento { get; set; }
        public Nullable<int> IdEmpresaAsimilado { get; set; }
        public Nullable<int> IdEmpresaSindicato { get; set; }
        public bool Status { get; set; }
        public int IdTipoJornada { get; set; }
        public int IdTipoRegimen { get; set; }
        public int IdSucursal { get; set; }
        public Nullable<decimal> PensionAlimenticiaPorcentaje { get; set; }
        public Nullable<int> PensionAlimenticiaSueldo { get; set; }
        public string EntidadDeServicio { get; set; }
        public bool Sindicalizado { get; set; }
        public bool PagoElectronico { get; set; }
        public bool IsReingreso { get; set; }
        public string ComentarioBaja { get; set; }
        public System.DateTime FechaReg { get; set; }
        public int IdUsuarioReg { get; set; }
        public Nullable<System.DateTime> FechaMod { get; set; }
        public Nullable<int> IdUsuarioMod { get; set; }
        public Nullable<System.DateTime> FechaRegBaja { get; set; }
        public Nullable<int> IdUsuarioRegBaja { get; set; }
    }
}
