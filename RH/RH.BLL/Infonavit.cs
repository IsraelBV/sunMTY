using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RH.Entidades;
using Common.Enums;
using Common.Utils;

namespace RH.BLL
{
    public class Infonavit
    {
      //  RHEntities ctx;

        public Infonavit()
        {
           // ctx = new RHEntities();
        }

        public List<HistorialPagosDetalle> GetHistorialPagos(int IdInfonavit)
        {
            using (var context = new RHEntities())
            {
                return (from hp in context.HistorialPagos
                    join n in context.NOM_Nomina on hp.IdNomina equals n.IdNomina
                    join p in context.NOM_PeriodosPago on n.IdPeriodo equals p.IdPeriodoPago
                    where hp.IdPrestamo == IdInfonavit && hp.TipoPrestamo == (int) TiposPrestamo.INFONAVIT
                    select new HistorialPagosDetalle
                    {
                        IdPago = hp.IdPagos,
                        IdPrestamo = hp.IdPrestamo,
                        IdNomina = hp.IdNomina,
                        CantidadDescuento = hp.CantidadDescuento,
                        FechaRegistro = hp.FechaRegistro,
                        DescripcionPeriodo = p.Descripcion
                    }
                ).ToList();
            }
        }

        public Empleado_Infonavit GetLastInfonavit(int id)
        {
            using (var context = new RHEntities())
            {
                return
                    context.Empleado_Infonavit.Where(x => x.IdEmpleadoContrato == id)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
            }
        }

        public CalculoInfonavit GetInfonavitById(int idInfonavit)
        {
            using (var context = new RHEntities())
            {
                var infonavit = context.Empleado_Infonavit.FirstOrDefault(x => x.Id == idInfonavit);
                return calcularInfonavit(infonavit);
            }
        }

        public CalculoInfonavit calcularInfonavit(Empleado_Infonavit infonavit)
        {
            //Obtener el contrato relacionado al credito
            Empleado_Contrato itemContrato = new Empleado_Contrato();
            using (var context = new RHEntities())
            {
                itemContrato =
                    context.Empleado_Contrato.FirstOrDefault(x => x.IdContrato == infonavit.IdEmpleadoContrato);
            }

            CalculoInfonavit calculo = new CalculoInfonavit();
            calculo.FechaInicio = infonavit.FechaInicio;
            calculo.IdEmpleadoContrato = infonavit.IdEmpleadoContrato;
            calculo.IdInfonavit = infonavit.Id;
            calculo.NumCredito = infonavit.NumCredito;
            calculo.Salario = infonavit.Salario;

            if (itemContrato != null)
            {//si el contrato tiene nuevo sdi, tomará el valor de ese nuevo sdi
                calculo.Salario = itemContrato.SDI;
            }

            calculo.FactorDescuento = infonavit.FactorDescuento;
            calculo.FechaSuspension = infonavit.FechaSuspension;
            calculo.Status = infonavit.Status;
            calculo.TipoCredito = infonavit.TipoCredito;

            if (calculo.FechaInicio < DateTime.Today)
                calculo.FechaAplicada = DateTime.Today;
            else
                calculo.FechaAplicada = calculo.FechaInicio;

            var numBimestre = Utils.GetBimestre(calculo.FechaAplicada.Value.Month);
            calculo.DiasBimestre = Utils.GetDiasDelBimestre(calculo.FechaAplicada.Value.Year, numBimestre);

            decimal salarioCalculo = 0;
            var zonaSalario = GetZonaSalario();

            switch (infonavit.TipoCredito)
            {
                case 1: //Cuota Fijam
                    calculo.DescuentoBimestral = calcularCuotaFija(calculo.FactorDescuento);
                    calculo.DescuentoDiario = calculo.DescuentoBimestral / calculo.DiasBimestre;
                    calculo.TipoCredito = infonavit.TipoCredito;
                    break;
                case 2: // Porcentaje//tomar el sdi del contrato
                    calculo.DescuentoDiario = calcularPorcentaje(calculo.FactorDescuento, calculo.Salario.Value);
                    calculo.DescuentoBimestral = (calculo.DescuentoDiario * calculo.DiasBimestre);// + 15; // cambio 23-01-2021, se quitaron los 15 pesos bimestrales por que al sacar el diario y multiplicarlo por los dias trabajados si el empledo falto ni se cobra correctamente. + 15;
                    calculo.TipoCredito = infonavit.TipoCredito;
                    break;
                case 3: //VSM

                    if (infonavit.UsarUMA)
                    {
                        //salarioCalculo = zonaSalario.UMA;
                        //salarioCalculo = 93.63M;
                        salarioCalculo = zonaSalario.UMI;
                    }
                    else
                    {
                        salarioCalculo = zonaSalario.SMG;
                    }                 

                    //calculo.Salario = sm != calculo.Salario ? sm : calculo.Salario;
                    calculo.Salario = salarioCalculo != calculo.Salario ? salarioCalculo : calculo.Salario;

                    calculo.DescuentoBimestral = calcularVSM(calculo.FactorDescuento, calculo.Salario.Value);
                    calculo.DescuentoDiario = calculo.DescuentoBimestral / calculo.DiasBimestre;
                    calculo.TipoCredito = infonavit.TipoCredito;
                    break;
                case 4: //VSM

                  

                    if (infonavit.UsarUMA)
                    {
                        //salarioCalculo = zonaSalario.UMA;
                        //salarioCalculo = 93.63M;
                        salarioCalculo = zonaSalario.UMI;
                    }
                    else
                    {
                        salarioCalculo = zonaSalario.SMG;
                    }

                    //calculo.Salario = sm != calculo.Salario ? sm : calculo.Salario;
                    calculo.Salario = salarioCalculo != calculo.Salario ? salarioCalculo : calculo.Salario;

                    calculo.DescuentoBimestral = calcularVSM(calculo.FactorDescuento, calculo.Salario.Value);
                    calculo.DescuentoDiario = calculo.DescuentoBimestral / calculo.DiasBimestre;
                    calculo.TipoCredito = infonavit.TipoCredito;
                    break;
                default:
                    break;
            }
            var iniBim = new DateTime(calculo.FechaAplicada.Value.Year, (numBimestre * 2) - 1, 1);
            var finBim = new DateTime(calculo.FechaAplicada.Value.Year, (numBimestre * 2), 1);
            calculo.BimestreAplicado = iniBim.ToString("MMMM") + " - " + finBim.ToString("MMMM");

            calculo.DescuentoBimestral = calculo.DescuentoBimestral; //Math.Round(calculo.DescuentoBimestral, 2);
            calculo.DescuentoDiario = calculo.DescuentoDiario;//Math.Round(calculo.DescuentoDiario, 2);
            return calculo;
        }

        private decimal calcularCuotaFija(decimal factorDescuento)
        {
            var descuentoBimestral = (factorDescuento * 2);// + 15; // cambio 23-01-2021, se quitaron los 15 pesos bimestrales por que al sacar el diario y multiplicarlo por los dias trabajados si el empledo falto ni se cobra correctamente. + 15;
            return descuentoBimestral;
        }

        private decimal calcularPorcentaje(decimal fd, decimal SD)
        {
            var porcentaje = fd / 100;
            var dDiario = SD * porcentaje;
            return dDiario;
        }

        public decimal calcularVSM(decimal fd, decimal SM)
        {
            var dBimestral = ((SM * fd) * 2);// + 15; // cambio 23-01-2021, se quitaron los 15 pesos bimestrales por que al sacar el diario y multiplicarlo por los dias trabajados si el empledo falto ni se cobra correctamente.
            return dBimestral;
        }

        public List<Empleado_Infonavit> GetListaInfonavit(int IdEmpleadoContrato)
        {
            using (var context = new RHEntities())
            {
                return context.Empleado_Infonavit.Where(x => x.IdEmpleadoContrato == IdEmpleadoContrato).ToList();
            }
        }

        public int Create(Empleado_Infonavit model, int idUser)
        {
            if(model.TipoCredito == 4)
            {
                model.UsarUMA = true;
                model.TipoCredito = 4;
            }else
            {
                model.UsarUMA = false;
            }

            model.Status = true;
            model.FechaReg = DateTime.Now;
            model.IdUsuarioReg = idUser;

            using (var context = new RHEntities())
            {
                context.Empleado_Infonavit.Add(model);

                var status = context.SaveChanges();
                if (status > 0)
                {
                    Notificaciones.INFONAVIT(model, "Nuevo Crédito");
                    return model.Id;
                }
                return 0;
            }
        }

        public int Update(Empleado_Infonavit newModel, int idUser)
        {
            using (var context = new RHEntities())
            {
                var title = "Actualización de Crédito";
                if (newModel.FechaSuspension != null)
                {
                    newModel.Status = false;
                    title = "Suspensión de Crédito";
                }

                var old = context.Empleado_Infonavit.FirstOrDefault(x => x.Id == newModel.Id);

                if (old == null) return 0;

                old.NumCredito = newModel.NumCredito;
                old.TipoCredito = newModel.TipoCredito;
                old.FactorDescuento = newModel.FactorDescuento;
                //old.Salario = newModel.Salario;
                old.FechaInicio = newModel.FechaInicio;
                old.FechaSuspension = newModel.FechaSuspension;
                old.UsarUMA = newModel.TipoCredito == 4;//VSM UMA
                old.Status = newModel.Status;
                old.FechaMod = DateTime.Now;
                old.IdUsuarioMod = idUser;
                var status = context.SaveChanges();

                if (status > 0)
                {
                    Notificaciones.INFONAVIT(newModel, title);
                    return newModel.Id;
                }
                return 0;
            }
        }

        public ZonaSalario GetZonaSalario()
        {
            using (var context = new RHEntities())
            {
                return context.ZonaSalario.Where(x => x.Status == true).FirstOrDefault();
            }                
        }

        public decimal GetValorUMA()
        {
            using (var context = new RHEntities())
            {
                //se debe usar el UMI no el UMA
                //return context.ZonaSalario.Where(x => x.Status == true).Select(x => x.UMA).FirstOrDefault();
                return context.ZonaSalario.Where(x => x.Status == true).Select(x => x.UMI).FirstOrDefault();
                //return 93.63M;
            }
        }
    }

    public class CalculoInfonavit
    {
        public int IdInfonavit { get; set; }
        public int IdEmpleadoContrato { get; set; }
        public string NumCredito { get; set; }
        public int TipoCredito { get; set; }
        public decimal FactorDescuento { get; set; }
        public decimal? Salario { get; set; }
        public decimal DescuentoDiario { get; set; }
        public decimal DescuentoBimestral { get; set; }
        public int DiasBimestre { get; set; }
        public string BimestreAplicado { get; set; }
        public DateTime? FechaAplicada { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaSuspension { get; set; }
        public Boolean Status { get; set; }
    }

    public class HistorialPagosDetalle
    {
        public int IdPago { get; set; }
        public int IdPrestamo { get; set; }
        public string DescripcionPeriodo { get; set; }
        public int IdNomina { get; set; }
        public decimal CantidadDescuento { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
