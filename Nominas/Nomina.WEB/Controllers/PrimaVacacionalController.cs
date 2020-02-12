using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Nomina.BLL;
using RH.Entidades;
using Nomina.WEB.Filtros;
using Common.Helpers;
using System.Threading.Tasks;
using Common.Utils;
using RH.BLL;
using RH.Entidades.GlobalModel;
using FiltrosWeb;
using Newtonsoft.Json;

namespace Nomina.WEB.Controllers
{
    public class PrimaVacacionalController : Controller
    {
        // GET: PrimaVacacional
        public ActionResult Index()
        {

            int idUsuario = SessionHelpers.GetIdUsuario();

            var periodo = Session["periodo"] as NOM_PeriodosPago;
            var sucursal = Session["sucursal"] as SucursalDatos;

            PrimaVacacionalModulo pv = new PrimaVacacionalModulo();
            var listaPrima = pv.GetDatosPrimasByPeriodo(periodo.IdPeriodoPago);

            //get SM
            ProcesoNomina pn = new ProcesoNomina();
            var zonaSalario = pn.GetZonaSalario();
            //get ISN
            var isnValor = pn.GetParametrosConfig("ISN");

            ViewBag.IsnValor = isnValor.ValorDecimal;
            ViewBag.SmValor = zonaSalario.SMG;


            return PartialView(listaPrima);
        }

        [HttpPost]
        public JsonResult CalcularPrima(decimal dias, decimal porcentaje, decimal sd, decimal smg, decimal xisn)
        {
            var r = CalculoPrima(smg,dias,porcentaje,sd,xisn);

            var test = JsonConvert.SerializeObject(r);

            return Json(test, JsonRequestBehavior.AllowGet);
        }


        public Tuple<decimal, decimal, decimal, decimal> CalculoPrima(decimal smg, decimal dias, decimal porcentaje, decimal sd,decimal xisn)
        {
            decimal exento = 0;
            decimal gravado = 0;
            decimal isn = 0;
            //tope
            var topePrima = smg * 15;
            var total = (sd * 0.25m) * dias;

            total = total.RedondeoDecimal();

            exento = total <= topePrima ? total : topePrima;
            gravado = total - exento;

            isn = total * xisn;
            isn = isn.RedondeoDecimal();

            Tuple<decimal, decimal, decimal, decimal> result = new Tuple<decimal, decimal, decimal, decimal>(total,gravado,exento,isn);


            var test = JsonConvert.SerializeObject(result);

            return result;
        }

        [HttpPost]
        public JsonResult GuardarPrimaVacacional(int idE, decimal dias, decimal porcentaje, decimal sd, decimal gravado, decimal exento, decimal total, decimal isn)
        {
            var periodo = Session["periodo"] as NOM_PeriodosPago;
            var idusuario = SessionHelpers.GetIdUsuario();

            if (periodo == null || total <= 0)
            return Json(new { resultado = false });

            var pvm = new PrimaVacacionalModulo();

            var obj = new NOM_Nomina_PrimaVacacional
            {
                Id = 0,
                IdPeriodo = periodo.IdPeriodoPago,
                IdEmpleado = idE,
                DiasPrima = dias,
                Porcentaje = porcentaje,
                SD = sd,
                Gravado = gravado,
                Exento = exento,
                Total = total,
                Isn = isn,
                FechaReg = DateTime.Now,
                IdUsuarioReg = idusuario
            };
            
            var r =pvm.SavePrimaVacacional(obj,periodo.IdPeriodoPago);
            
            return Json(new { resultado = r});

        }


        [HttpPost]
        public JsonResult EliminarPrimaVacacional(int idPrima)
        {
            var periodo = Session["periodo"] as NOM_PeriodosPago;
            var idusuario = SessionHelpers.GetIdUsuario();

            if (periodo == null )
                return Json(new { resultado = false });

            var pvm = new PrimaVacacionalModulo();

            var r = pvm.DeletePrima(idPrima, periodo.IdPeriodoPago);

            return Json(new { resultado = r });
            
        }

    }
}