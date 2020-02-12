using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using RH.BLL;
using RH.Entidades;
using Common.Enums;
using FiltrosWeb;
using Common.Helpers;

namespace RH.WEB.Controllers
{
    public class HorasExtrasController : Controller
    {
        // GET: HorasExtras
        public ActionResult Index()
        {
            var sucursal = Session["sucursal"] as SucursalDatos;

            if (sucursal == null)
                return RedirectToAction("Index", "Home");

            Empleados e = new Empleados();
            var model = e.GetEmpleadosBySucursal(sucursal.IdSucursal);

            return View(model);
        }

        [HttpPost]
        public JsonResult RegistrarHorasExtras(string[] fechas, int[] idEmpleados, int horas, bool tipoSimple)
        {
            HorasExtrasClass obj = new HorasExtrasClass();
            var IdUser = SessionHelpers.GetIdUsuario();

            var result = obj.GuardarHorasExtras(idEmpleados, fechas, horas, tipoSimple, IdUser);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detalle(int id)
        {
            HorasExtrasClass obj = new HorasExtrasClass();
            var model = obj.GetHorasExtrasEmpleados(id);

            var e = new Empleados();
            ViewBag.Empleado = e.GetEmpleadoById(id);

            return View(model);
        }
        
        public JsonResult DeleteHe(int id)
        {
            HorasExtrasClass obj = new HorasExtrasClass();
            var result = obj.DeleteHoraExtra(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetHorasEmpleados(int[] idEmpleados)
        {
            HorasExtrasClass obj = new HorasExtrasClass();
            var result = obj.GetHorasEmpleados(idEmpleados);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}