using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Nomina.WEB.App_Start
{
    public class BundleConfig
    {


        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;


            #region Layout
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/animate.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/nomina.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/jquery-ui").Include(
                "~/Content/jquery-ui.min.css"));

            bundles.Add(new StyleBundle("~/Content/boostrap").Include(
                "~/Content/bootstrap.min.css",
                "~/Content/bootstrap-notify.css"));

            bundles.Add(new StyleBundle("~/Content/datatable").Include(
                  //"~/Content/TableSelect.css",
              "~/Content/CSSLocal/jquery.dataTables.min.css",
              "~/Content/CSSLocal/select.dataTables.min.css",
              "~/Content/CSSLocal/buttons.dataTables.min.css",
              "~/Content/CSSLocal/fixedColumns.dataTables.min.css",
              "~/Content/CSSLocal/jquery-ui.css",
              "~/Content/CSSLocal/dataTables.jqueryui.min.css",
              "~/Content/CSSLocal/responsive.dataTables.min.css"

             ));

            bundles.Add(new StyleBundle("~/Content/Layout").Include(
              "~/Content/layout.css"
               ));

            bundles.Add(new StyleBundle("~/Content/Sidebar").Include(
                "~/Content/sidebar.css",
                      "~/Content/sidebar2.css"
               ));



            //JS
            bundles.Add(new ScriptBundle("~/bundles/Layout").Include(
                "~/Scripts/layout.js"
                ));


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                 "~/Scripts/jquery-3.1.0.min.js",
                  "~/Scripts/jquery.unobtrusive-ajax.min.js"
                ));


            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                  "~/Scripts/jquery-ui.min.js"));


            bundles.Add(new ScriptBundle("~/bundles/boostrap").Include(
                 "~/Scripts/bootstrap.min.js"
                ));


            bundles.Add(new ScriptBundle("~/bundles/boostrap-js").Include(
                  "~/Scripts/bootstrap-notify.min.js",
                  "~/Scripts/bootstrap-waitingfor.min.js",
                  "~/Scripts/bootstrap-confirmation.min.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/datatable").Include(
                 "~/Scripts/TableSelect.js",
                "~/Scripts/JSLocal/jquery.dataTables.min.js",
                  "~/Scripts/JSLocal/dataTables.select.min.js",
                  "~/Scripts/JSLocal/dataTables.fixedColumns.min.js",
                  "~/Scripts/JSLocal/dataTables.buttons.min.js",
                   "~/Scripts/JSLocal/dataTables.colReorder.min.js",
                  "~/Scripts/JSLocal/dataTables.responsive.min.js"
                  ));

            bundles.Add(new ScriptBundle("~/bundles/jsLocal").Include(
                  "~/Scripts/JSLocal/jszip.min.js",
                  "~/Scripts/Utils.js",
                  "~/Scripts/periodospago.js",
                  "~/Scripts/catalogo.js",
                  "~/Scripts/asignarConcepto.js",
                  "~/Scripts/ProcesoNomina.js",
                  "~/Scripts/cfdi.js",
                  "~/Scripts/complemento.js"
                  ));

            // @*< script src = "https://use.fontawesome.com/150451a2ee.js" ></ script > *@
            #endregion


            //Periodo
            bundles.Add(new ScriptBundle("~/bundles/getperiodoPagos").Include(
                "~/Scripts/periodos/GetPeriodosPagos.js"));

            bundles.Add(new ScriptBundle("~/bundles/nuevoPeriodo").Include(
                "~/Scripts/periodos/NuevoPeriodo.js"));
            
            //Asignacion de conceptos



            //Procesado



            //Autorizado

            //cfdi

            //finiquitos

            //Catalogos




            //Examples JS
            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //        "~/Scripts/bootstrap.js",
            //        "~/Scripts/respond.js"));

            //Examples CSS
            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));
        }
    }
}