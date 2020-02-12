using RH.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nomina.BLL;
using Common.Helpers;
using RH.BLL;

namespace Nomina.WEB.Controllers
{
    public class LayoutController : Controller
    {
        // GET: Layout
        public ActionResult Index()
        {
            _Layout lay = new _Layout();
            var periodo = Session["periodo"] as NOM_PeriodosPago;
            ViewBag.periodo = periodo;
            var sucursal = Session["sucursal"] as SucursalDatos;
            var empresas = lay.empresas(sucursal.IdSucursal);
                return PartialView(empresas);
         
         
            
        }

        public ActionResult tablaLayout(int idEmpresa)
        {
            _Layout lay = new _Layout();
            var periodo = Session["periodo"] as NOM_PeriodosPago;
            var empresa = lay.empresas(periodo.IdSucursal);
            var datos = lay.listaEmpleados(periodo.IdPeriodoPago,idEmpresa);
            ViewBag.empresa = empresa;
            return PartialView(datos);
        }

        public ActionResult tablaLayoutFiniquito(int idEmpresa)
        {
            _Layout lay = new _Layout();
            var periodo = Session["periodo"] as NOM_PeriodosPago;
            var empresa = lay.empresas(periodo.IdSucursal);
            var datos = lay.listaEmpleadosFiniquito(periodo.IdPeriodoPago, idEmpresa);
            ViewBag.empresa = empresa;
            return PartialView(datos);
        }
        public JsonResult crearLayout(encabezado Encabezado, List<detallado> Detalle,List<emisoras> emisoras)
        {
            ////se obtiene id usuario
            var idU = SessionHelpers.GetIdUsuario();
            //declaramos objeto
            _Layout lay = new _Layout();
            var ruta = Server.MapPath("~//Files/Layout/");
            string[] archivoGen = lay.GenerarLayout(ruta,Encabezado, idU, Detalle, emisoras);
            return Json(archivoGen, JsonRequestBehavior.AllowGet);
        }

        public FileResult descargarLayout(string ruta) 
        {
            string[] arrayRuta = ruta.Split('\\');
            int count = arrayRuta.Length;
            byte[] fyleBytes = System.IO.File.ReadAllBytes(ruta);
                return File(fyleBytes, System.Net.Mime.MediaTypeNames.Application.Octet, arrayRuta[count-1]);
        }
    }
}