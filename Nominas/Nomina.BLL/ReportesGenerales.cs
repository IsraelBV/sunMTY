using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RH.Entidades;
using RH.Entidades.GlobalModel;
using SYA.BLL;
using ClosedXML.Excel;
using Common.Utils;
using Nomina.Procesador.Metodos;
using RH.BLL;
using System.Diagnostics;

namespace Nomina.BLL
{
    public class ReportesGenerales
    {
        public static List<ClienteSucursal> GetSucusalesByUsuario(int idUsuario)
        {
            ControlUsuario sya = new ControlUsuario();
            var sucursales = sya.GetSucusalesByIdUsuario(idUsuario);
            List<Cliente> listaClientes = new List<Cliente>();
            var arrayIdClientes = sucursales.Select(x => x.IdCliente).Distinct().ToArray();
            List<ClienteSucursal> listaFinal = new List<ClienteSucursal>();
            using (var context = new RHEntities())
            {
                listaClientes = (from c in context.Cliente
                                 where arrayIdClientes.Contains(c.IdCliente)
                                 select c).ToList();
            }

            foreach (var itemS in sucursales)
            {
                var itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemS.IdCliente);

                listaFinal.Add(new ClienteSucursal()
                {
                    IdCliente = itemS.IdCliente,
                    IdSucursal = itemS.IdSucursal,
                    Cliente = itemCliente?.Nombre ?? "?",
                    Sucursal = itemS.Ciudad
                });
            }

            listaFinal = listaFinal.OrderBy(x => x.Cliente).ToList();

            return listaFinal;
        }


        public static List<EmpleadosClientes> GetEmpleadosClienteByIdSucursal(int idSucursal)
        {
            //Cliente - Sucursal 
            //IdEmpleado-IdContrato-Nombre-SD-SDI-SReal-Alta-AltaImss-Puesto-Departamento-Reingreso-NSS-RFC-ValidadoSAT-Status

            List<Empleado> listaEmpleados = new List<Empleado>();
            List<Empleado_Contrato> listaContratos = new List<Empleado_Contrato>();
            List<Puesto> listaPuestos = new List<Puesto>();
            List<Departamento> listaDepartamentos = new List<Departamento>();
            List<EmpleadosClientes> listaEmpleadosClientes = new List<EmpleadosClientes>();

            using (var context = new RHEntities())
            {
                listaEmpleados = (from e in context.Empleado
                                  where e.IdSucursal == idSucursal
                                  select e).ToList();

                var arrayIdE = listaEmpleados.Select(x => x.IdEmpleado).ToArray();

                listaContratos = (from c in context.Empleado_Contrato
                                  where arrayIdE.Contains(c.IdEmpleado)
                                  select c).ToList();

                listaPuestos = context.Puesto.ToList();
                listaDepartamentos = context.Departamento.ToList();
            }



            foreach (var itemEmpleado in listaEmpleados)
            {
                //obtener el contrato    
                var itemContrato = listaContratos.OrderByDescending(x => x.IdEmpleado == itemEmpleado.IdEmpleado).FirstOrDefault();

                if (itemContrato == null) continue;

                //puesto
                var itemPuesto = listaPuestos.FirstOrDefault(x => x.IdPuesto == itemContrato.IdPuesto);
                //departamento
                Departamento itemDepa = new Departamento();

                if (itemPuesto != null)
                {
                    itemDepa = listaDepartamentos.FirstOrDefault(x => x.IdDepartamento == itemPuesto.IdDepartamento);
                }

                EmpleadosClientes item = new EmpleadosClientes()
                {
                    IdEmpleado = itemEmpleado.IdEmpleado,
                    IdContrato = itemContrato.IdContrato,
                    EsReingreso = itemContrato.IsReingreso,
                    Nombre = $"{itemEmpleado.Nombres} {itemEmpleado.APaterno} {itemEmpleado.AMaterno}",
                    Sd = itemContrato.SD,
                    Sdi = itemContrato.SDI,
                    SReal = itemContrato.SalarioReal,
                    FechaAlta = itemContrato.FechaAlta.ToString("d"),
                    AltaImss = itemContrato.FechaIMSS?.ToString("d") ?? "-",
                    Puesto = itemPuesto?.Descripcion ?? "-",
                    Departamento = itemDepa?.Descripcion ?? "-",
                    Nss = itemEmpleado.NSS,
                    Rfc = itemEmpleado.RFC,
                    ValidSat = itemEmpleado.RFCValidadoSAT,
                };

                listaEmpleadosClientes.Add(item);
            }



            return listaEmpleadosClientes;
        }

        //public static List<calculoanual> Getcalculonual(int ideje, int idempresa)
        //{

        //    List<calculoanual> lista = new List<calculoanual>();




        //    using (var context = new RHEntities())
        //    {
        //        try
        //        {
        //            var datos = (from e in context.NOM_CalculoAnual
        //                         join em in context.Empleado on e.idEmpleado equals em.IdEmpleado
        //                         join p in context.NOM_PeriodosPago on e.idPeriodo equals p.IdPeriodoPago
        //                         join s in context.Sucursal on em.IdSucursal equals s.IdSucursal
        //                         join c in context.Cliente on s.IdCliente equals c.IdCliente
        //                         where e.idEmpresa == idempresa && p.IdEjercicio == ideje
        //                         select new { e.idEmpleado, nombre = (em.APaterno + " " + em.AMaterno + " " + em.Nombres), c.Nombre, e.baseGravable, e.Exento, e.subCausado, e.isrAntes, e.isrPagado, e.isrRetener, e.saldoFavor}
        //                         ).OrderBy(x=> x.Nombre).ThenBy(x=> x.nombre).ToList();

        //            foreach (var d in datos)
        //            {
        //                calculoanual item = new calculoanual()
        //                {
        //                    idEmpleado = d.idEmpleado,
        //                    nombre = d.nombre,
        //                    cliente = d.Nombre,
        //                    baseGravable = d.baseGravable,
        //                    Exento = d.Exento,
        //                    subCausado = d.subCausado,
        //                    isrAntes = d.isrAntes,
        //                    isrPagado = d.isrPagado,
        //                    isrRetener = d.isrRetener,
        //                    saldoFavor = d.saldoFavor                            
        //                };
        //                lista.Add(item);
        //            }
        //        }
        //        catch (Exception e) { string test=e.Message; }

        //    }         



        //    return lista;
        //}

        public static byte[] GetFileEmpleadosBySucursal(int id)
        {
            var lista = GetEmpleadosClienteByIdSucursal(id);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            #region DOCUMENTO EXCEL - NOMINA ********************************************************
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Empleados Sucursal");

            #region HEADER
            worksheet.Cell(1, 1).Value = "IdE";
            worksheet.Cell(1, 2).Value = "IdC";
            worksheet.Cell(1, 3).Value = "Nombre";
            worksheet.Cell(1, 4).Value = "Sd";
            worksheet.Cell(1, 5).Value = "Sdi";
            worksheet.Cell(1, 6).Value = "SReal";
            worksheet.Cell(1, 7).Value = "FechaAlta";
            worksheet.Cell(1, 8).Value = "AltaImss";
            worksheet.Cell(1, 9).Value = "Puesto";
            worksheet.Cell(1, 10).Value = "Departamento";
            worksheet.Cell(1, 11).Value = "Nss";
            worksheet.Cell(1, 12).Value = "Rfc";
            worksheet.Cell(1, 13).Value = "Validacion Sat";
            worksheet.Cell(1, 14).Value = "EsReingreso";
            #endregion

            #region CONTENIDO
            int row = 2;

            foreach (var item in lista)
            {
                worksheet.Cell(row, 1).Value = item.IdEmpleado;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 2).Value = item.IdContrato;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).Value = item.Nombre;
                worksheet.Cell(row, 4).Value = item.Sd;
                worksheet.Cell(row, 5).Value = item.Sdi;
                worksheet.Cell(row, 6).Value = item.SReal;
                worksheet.Cell(row, 7).Value = item.FechaAlta;
                worksheet.Cell(row, 8).Value = item.AltaImss;
                worksheet.Cell(row, 9).Value = item.Puesto;
                worksheet.Cell(row, 10).Value = item.Departamento;
                worksheet.Cell(row, 11).Value = item.Nss;
                worksheet.Cell(row, 12).Value = item.Rfc;
                var strValido = "valido";
                if (item.ValidSat == 1)
                {
                    strValido = "valido";
                }
                else if (item.ValidSat == 2)
                {
                    strValido = "pendiente";
                }
                else if (item.ValidSat == 0)
                {
                    strValido = "incorrecto";
                }

                worksheet.Cell(row, 13).Value = strValido;
                worksheet.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 14).Value = item.EsReingreso ? "Si" : "";
                worksheet.Cell(row, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row++;

            }
            #endregion


            #region DISEÑO
            //Fix
            worksheet.SheetView.Freeze(1, 3);

            //Color
            worksheet.Range("A1:N1").Style
               .Font.SetFontSize(14)
               .Font.SetBold(true)
               .Font.SetFontColor(XLColor.White)
               .Fill.SetBackgroundColor(XLColor.Maroon);

            //Ajuste de columnas
            worksheet.Column(1).AdjustToContents();
            worksheet.Column(2).AdjustToContents();
            worksheet.Column(3).AdjustToContents();
            worksheet.Column(4).AdjustToContents();
            worksheet.Column(5).AdjustToContents();
            worksheet.Column(6).AdjustToContents();
            worksheet.Column(7).AdjustToContents();
            worksheet.Column(8).AdjustToContents();
            worksheet.Column(9).AdjustToContents();
            worksheet.Column(10).AdjustToContents();
            worksheet.Column(11).AdjustToContents();
            worksheet.Column(12).AdjustToContents();
            worksheet.Column(13).AdjustToContents();
            worksheet.Column(14).AdjustToContents();
            worksheet.Column(15).AdjustToContents();
            worksheet.Column(16).AdjustToContents();
            worksheet.Column(17).AdjustToContents();
            worksheet.Column(18).AdjustToContents();


            #endregion


            #endregion

            workbook.SaveAs(ms, false);
            return ms.ToArray();
        }

        //public static byte[] GetFileAjusteAnual(int ideje, int idempresa)
        //{
        //    var lista = Getcalculonual(ideje, idempresa);
        //    System.IO.MemoryStream ms = new System.IO.MemoryStream();

        //    #region DOCUMENTO EXCEL - NOMINA ********************************************************
        //    var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Empleados Sucursal");

        //    #region HEADER
        //    worksheet.Cell(1, 1).Value = "IdEmpleado";
        //    worksheet.Cell(1, 2).Value = "Nombre";
        //    worksheet.Cell(1, 3).Value = "Cliente";
        //    worksheet.Cell(1, 4).Value = "Base Gravable";
        //    worksheet.Cell(1, 5).Value = "Exento";
        //    worksheet.Cell(1, 6).Value = "Subsidio Causado Anual";
        //    worksheet.Cell(1, 7).Value = "ISR Causado Anual";
        //    worksheet.Cell(1, 8).Value = "ISR Pagado Anual";
        //    worksheet.Cell(1, 9).Value = "ISR a Retener";
        //    worksheet.Cell(1, 10).Value = "Saldo Favor";

        //    #endregion

        //    #region CONTENIDO
        //    int row = 2;

        //    foreach (var item in lista)
        //    {
        //        worksheet.Cell(row, 1).Value = item.idEmpleado;
        //        worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        worksheet.Cell(row, 2).Value = item.nombre;
        //        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        worksheet.Cell(row, 3).Value = item.cliente;
        //        worksheet.Cell(row, 4).Value = item.baseGravable;
        //        worksheet.Cell(row, 5).Value = item.Exento;
        //        worksheet.Cell(row, 6).Value = item.subCausado;
        //        worksheet.Cell(row, 7).Value = item.isrAntes;
        //        worksheet.Cell(row, 8).Value = item.isrPagado;
        //        worksheet.Cell(row, 9).Value = item.isrRetener;
        //        worksheet.Cell(row, 10).Value = item.saldoFavor;

        //        row++;

        //    }
        //    #endregion


        //    #region DISEÑO
        //    //Fix
        //    worksheet.SheetView.Freeze(1, 3);

        //    //Color
        //    worksheet.Range("A1:K1").Style
        //       .Font.SetFontSize(14)
        //       .Font.SetBold(true)
        //       .Font.SetFontColor(XLColor.White)
        //       .Fill.SetBackgroundColor(XLColor.Maroon);

        //    //Ajuste de columnas
        //    worksheet.Column(1).AdjustToContents();
        //    worksheet.Column(2).AdjustToContents();
        //    worksheet.Column(3).AdjustToContents();
        //    worksheet.Column(4).AdjustToContents();
        //    worksheet.Column(5).AdjustToContents();
        //    worksheet.Column(6).AdjustToContents();
        //    worksheet.Column(7).AdjustToContents();
        //    worksheet.Column(8).AdjustToContents();
        //    worksheet.Column(9).AdjustToContents();
        //    worksheet.Column(10).AdjustToContents();
        //    worksheet.Column(11).AdjustToContents();
        //    #endregion


        //    #endregion

        //    workbook.SaveAs(ms, false);
        //    return ms.ToArray();
        //}

        //public string GenerarArchivoAcumuladoComplete_NUEVO2018(int idUsuario, string pathFolder, string pathDescarga, int idEjercicio, DateTime? dateInicial, DateTime? dateFinal, int idEmpresa = 0, int idCliente = 0, bool incluirNoAutorizados = false, bool calculoanual = false)
        //{
        //    //Validaciones
        //    if (pathFolder.Trim() == "" || pathDescarga.Trim() == "" || dateInicial == null || dateFinal == null || idEjercicio == null)
        //        return null;


        //    #region Variables - List<>

        //    List<C_NOM_Conceptos> listaConceptos;
        //    List<Empleado> listaEmpleados;
        //    List<NOM_Nomina> listaNominas;
        //    List<NOM_Nomina_Detalle> listaDetalles;
        //    List<NOM_Cuotas_IMSS> listaCuotas;
        //    List<NOM_Cuotas_IMSS> listaCuotasFiniquito;
        //    List<Empresa> listaEmpresasF;
        //    List<NOM_PeriodosPago> listaPeriodos;
        //    List<NOM_Finiquito> listaFiniquitos;
        //    List<NOM_Finiquito_Detalle> listaFiniquitosDetalles;
        //    List<C_NOM_Conceptos> listaConceptosFiniquito;
        //    List<Empleado> listaEmpleadosFiniquitos;
        //    List<Cliente> listaClientes;
        //    List<NOM_Finiquito_Descuento_Adicional> listaDescuentoComisionFiniquito;
        //    List<Empleado_Contrato> listaContratos;
        //    NOM_Ejercicio_Fiscal ejercicioFiscal;

        //    List<Permisos> listaPermisos;
        //    List<Inasistencias> listafaltas;
        //    List<Incapacidades> listaIncapacidadeses;

        //    List<NOM_CFDI_Timbrado> registrosQueSonNomina;
        //    List<NOM_CFDI_Timbrado> registrosQueSonFiniquito;
        //    #endregion

        //    //se deberá agregar al metodo la opcion de incluir cancelados
        //    var listaTimbrados = QueryData.BuscarTimbrados(dateInicial, dateFinal, 0, idEjercicio, idEmpresa, 0, false);

        //    var arrayListPeriodos = listaTimbrados.Select(x => x.IdPeriodo).Distinct().ToList();
        //    var arrayListNominas = listaTimbrados.Select(x => x.IdNomina).Distinct().ToList();
        //    var arrayListFiniquitos = listaTimbrados.Select(x => x.IdFiniquito).Distinct().ToList();
        //    var arrayListEmpleados = listaTimbrados.Select(x => x.IdEmpleado).Distinct().ToList();


        //    using (var context = new RHEntities())
        //    {
        //        #region GET DATA ACUMULADO

        //        ejercicioFiscal = context.NOM_Ejercicio_Fiscal.FirstOrDefault(x => x.IdEjercicio == idEjercicio);

        //        if (incluirNoAutorizados)
        //        {
        //            //buscamos los periodos creados
        //            var listaNoTimbrados = (from p in context.NOM_PeriodosPago
        //                                    where DbFunctions.TruncateTime(p.Fecha_Inicio) >= DbFunctions.TruncateTime(dateInicial)
        //                                          && DbFunctions.TruncateTime(p.Fecha_Inicio) <= DbFunctions.TruncateTime(dateFinal)
        //                                          && !arrayListPeriodos.Contains(p.IdPeriodoPago)
        //                                    select p.IdPeriodoPago).ToList();




        //            if (listaNoTimbrados.Any())
        //            {
        //                //aseguramos que sean periodos con nominas generadas
        //                var nominasNoTim = (from n in context.NOM_Nomina
        //                                    where listaNoTimbrados.Contains(n.IdPeriodo)
        //                                    && (n.IdEmpresaFiscal == idEmpresa || n.IdEmpresaAsimilado == idEmpresa)
        //                                    select n).ToList();

        //                var listNomNo = nominasNoTim.Select(x => x.IdNomina).ToList();
        //                var listaEmplNn = nominasNoTim.Select(x => x.IdEmpleado).ToList();
        //                var periodosPorNomina = nominasNoTim.Select(x => x.IdPeriodo).ToList();

        //                //buscamos los finiquitos
        //                var finiquitosNoTim = (from f in context.NOM_Finiquito
        //                                       where listaNoTimbrados.Contains(f.IdPeriodo)
        //                                       && (f.IdEmpresaFiscal == idEmpresa || f.IdEmpresaAsimilado == idEmpresa)
        //                                       select f).ToList();

        //                var listFinNo = finiquitosNoTim.Select(x => x.IdFiniquito).ToList();
        //                var listaEmplNnF = finiquitosNoTim.Select(x => x.IdEmpleado).ToList();
        //                var periodosPorFiniquito = finiquitosNoTim.Select(x => x.IdPeriodo).ToList();


        //                arrayListPeriodos.AddRange(periodosPorNomina);
        //                arrayListPeriodos.AddRange(periodosPorFiniquito);

        //                arrayListNominas.AddRange(listNomNo);
        //                arrayListFiniquitos.AddRange(listFinNo);

        //                arrayListEmpleados.AddRange(listaEmplNn);
        //                arrayListEmpleados.AddRange(listaEmplNnF);
        //            }
        //        }

        //        if (!arrayListPeriodos.Any())
        //            return null;

        //        var arrayPeriodos = arrayListPeriodos.Distinct().ToArray();
        //        var arrayNominas = arrayListNominas.Distinct().ToArray();
        //        var arrayFiniquitos = arrayListFiniquitos.Distinct().ToArray();
        //        var arrayEmpleados = arrayListEmpleados.Distinct().ToArray();



        //        //aqui se podrian incluir los no autorizados
        //        listaPeriodos = (from p in context.NOM_PeriodosPago
        //                         where arrayPeriodos.Contains(p.IdPeriodoPago)
        //                         select p).ToList();


        //        // var arrayPeriodosAutorizados = listaPeriodos.Select(x => x.IdPeriodoPago).ToArray();

        //        //GET - lista de nominas que estan en el array de periodos
        //        listaNominas = (from n in context.NOM_Nomina
        //                        where arrayNominas.Contains(n.IdNomina)
        //                        select n).ToList();

        //        //GET - lista de finiquitos que estan en el array de periodos
        //        listaFiniquitos = (from f in context.NOM_Finiquito
        //                           where arrayFiniquitos.Contains(f.IdFiniquito)
        //                           select f).ToList();

        //        ////1er Filtro- por Empresa
        //        //if (idEmpresa > 0)
        //        //{
        //        //    #region NOMINAS FILTRO

        //        //    //Buscamos las nominas que se generaron con la empresa en Fiscal
        //        //    var arrayPeriodosFiscales =
        //        //        listaPeriodos.Where(x => x.IdTipoNomina <= 15).Select(x => x.IdPeriodoPago).ToArray();

        //        //    //Consultamos la nosminas Fiscales
        //        //    var listaF = (from n in listaNominas
        //        //                  where arrayPeriodosFiscales.Contains(n.IdPeriodo)
        //        //                        && n.IdEmpresaFiscal == idEmpresa
        //        //                  select n).ToList();

        //        //    //Buscamos las nominas que se generaron con la empresa en Asimilado
        //        //    var arrayPeriodosAsimilados =
        //        //        listaPeriodos.Where(x => x.IdTipoNomina == 16).Select(x => x.IdPeriodoPago).ToArray();

        //        //    //Consultamos la nosminas Asimilados
        //        //    var listaA = (from n in listaNominas
        //        //                  where arrayPeriodosAsimilados.Contains(n.IdPeriodo)
        //        //                        && n.IdEmpresaAsimilado == idEmpresa
        //        //                  select n).ToList();

        //        //    //Unimos las dos listas
        //        //    listaNominas = listaF;
        //        //    listaNominas.AddRange(listaA);

        //        //    #endregion

        //        //    #region FINIQUITOS FILTRO
        //        //    //Busca los finiquitos que se generaron con la empresa fiscal
        //        //    listaFiniquitos = listaFiniquitos.Where(x => x.IdEmpresaFiscal == idEmpresa).ToList();

        //        //    #endregion


        //        //}

        //        //2do Filtro
        //        if (idCliente > 0)
        //        {
        //            //De la lista anterior filtramos las nominas dond el cliente sea idCliente
        //            listaNominas = listaNominas.Where(x => x.IdCliente == idCliente).ToList();

        //            //Filtramos los finiquitos por IdCliente
        //            listaFiniquitos = listaFiniquitos.Where(x => x.IdCliente == idCliente).ToList();
        //        }


        //        if (listaNominas.Count <= 0) return null;

        //        //Generamos los array de id's para
        //        //Buscar sus detalles de nomina
        //        //Datos del empleado
        //        //Detalles del finiquito
        //        //var arrayIdNominas = listaNominas.Select(x => x.IdNomina).ToArray();
        //        //var arrayIdEmpleados = listaNominas.Select(x => x.IdEmpleado).ToArray();
        //        //var arrayIdFiniquitos = listaFiniquitos.Select(x => x.IdFiniquito).ToArray();

        //        //GET Detalle de finiquitos
        //        listaFiniquitosDetalles = (from fd in context.NOM_Finiquito_Detalle
        //                                   where arrayFiniquitos.Contains(fd.IdFiniquito)
        //                                   select fd).ToList();

        //        //GET PERMISOS E INCAPACIDADES
        //        //var fecha2mesesMenos = dateInicial.Value.AddMonths(-2);


        //        var primerPeriodo = listaPeriodos.OrderByDescending(x => x.Fecha_Inicio).LastOrDefault();
        //        var ultimoPeriodo = listaPeriodos.OrderByDescending(x => x.Fecha_Inicio).FirstOrDefault();

        //        var fechaInicioIncidencias = primerPeriodo.Fecha_Inicio;
        //        var fechaFinInicidencias = ultimoPeriodo.Fecha_Fin;


        //        //abc
        //        //listaPermisos = (from p in context.Permisos
        //        //                 where arrayEmpleados.Contains(p.IdEmpleado)
        //        //                 && DbFunctions.TruncateTime(p.FechaInicio) >= DbFunctions.TruncateTime(fechaInicioIncidencias) && DbFunctions.TruncateTime(p.FechaInicio) <= DbFunctions.TruncateTime(fechaFinInicidencias)
        //        //                 //  && p.FechaInicio >= fecha2mesesMenos && p.FechaInicio <= dateFinal.Value  DbFunctions.TruncateTime(
        //        //                 select p).ToList();

        //        //abc
        //        //listaIncapacidadeses = (from i in context.Incapacidades
        //        //                        where arrayEmpleados.Contains(i.IdEmpleado)
        //        //                          && DbFunctions.TruncateTime(i.FechaInicio) >= DbFunctions.TruncateTime(fechaInicioIncidencias) && DbFunctions.TruncateTime(i.FechaInicio) <= DbFunctions.TruncateTime(fechaFinInicidencias)
        //        //                        //   && i.FechaInicio >= fecha2mesesMenos && i.FechaInicio <= dateFinal.Value
        //        //                        select i).ToList();

        //        //abc
        //        //listafaltas = (from i in context.Inasistencias
        //        //               where arrayEmpleados.Contains(i.IdEmpleado)
        //        //                 && DbFunctions.TruncateTime(i.Fecha) >= DbFunctions.TruncateTime(fechaInicioIncidencias) && DbFunctions.TruncateTime(i.Fecha) <= DbFunctions.TruncateTime(fechaFinInicidencias)
        //        //               //&& i.Fecha >= fecha2mesesMenos && i.Fecha <= dateFinal.Value
        //        //               select i).ToList();



        //        //GET - Lista de detalles de nominas del arrray de nominas
        //        listaDetalles = (from nd in context.NOM_Nomina_Detalle
        //                         where arrayNominas.Contains(nd.IdNomina)
        //                         select nd).ToList();

        //        //GET - lista de cuotas imss del array de nominas
        //        listaCuotas = (from cuotas in context.NOM_Cuotas_IMSS
        //                       where arrayNominas.Contains(cuotas.IdNomina)
        //                       select cuotas).ToList();


        //        //GET - lista de cuotas imss de los finiquitos
        //        //listaCuotasFiniquito = (from cuotas in context.NOM_Cuotas_IMSS
        //        //                        where arrayIdFiniquitos.Contains(cuotas.IdFiniquito)
        //        //                        select cuotas).ToList();

        //        listaDescuentoComisionFiniquito = (from dc in context.NOM_Finiquito_Descuento_Adicional
        //                                           where arrayPeriodos.Contains(dc.IdPeriodo)
        //                                           select dc).ToList();

        //        //GET -  Datos del empleado
        //        listaEmpleados = (from emp in context.Empleado
        //                          where arrayEmpleados.Contains(emp.IdEmpleado)
        //                          orderby emp.APaterno
        //                          select emp).ToList();

        //        //Agrupa por IdConcepto
        //        var listaGroupConceptos = listaDetalles.GroupBy(x => x.IdConcepto).ToList();

        //        //Genera Array de Id de los conceptos
        //        var arrayIdConceptos = listaGroupConceptos.Select(x => x.Key).ToArray();

        //        //GET - Lista de Conceptos de las Nominas
        //        listaConceptos = (from conce in context.C_NOM_Conceptos
        //                          where arrayIdConceptos.Contains(conce.IdConcepto)
        //                          select conce).ToList();

        //        var listaGroupConceptosF = listaFiniquitosDetalles.GroupBy(x => x.IdConcepto).ToList();

        //        arrayIdConceptos = listaGroupConceptosF.Select(x => x.Key).ToArray();

        //        //Lista de Conceptos de los finiquitos
        //        listaConceptosFiniquito = (from conce in context.C_NOM_Conceptos
        //                                   where arrayIdConceptos.Contains(conce.IdConcepto)
        //                                   select conce).ToList();

        //        //  arrayIdEmpleados = listaFiniquitos.Select(x => x.IdEmpleado).ToArray();

        //        listaEmpleadosFiniquitos = (from emp in context.Empleado
        //                                    where arrayEmpleados.Contains(emp.IdEmpleado)
        //                                    orderby emp.APaterno
        //                                    select emp).ToList();

        //        //GET - Lista empresas fiscales
        //        listaEmpresasF = (from em in context.Empresa
        //                          select em).ToList();

        //        listaClientes = (from clie in context.Cliente
        //                         select clie).ToList();



        //        //Buscamos los contratos que se usaron en las nominas del ejercicio fiscal
        //        var contratosNominas = listaNominas.Select(x => x.IdContrato).ToList();
        //        var contratosFiniquitos = listaFiniquitos.Select(x => x.IdFiniquito).ToList();

        //        contratosNominas.AddRange(contratosFiniquitos);

        //        var arrayIdContratos = contratosNominas.ToArray();

        //        listaContratos = (from con in context.Empleado_Contrato
        //                          where arrayIdContratos.Contains(con.IdContrato)
        //                          select con).ToList();


        //        //-------------------------------------intentar  recuperar uids de NOM_CFDI_Timbrado
        //        var listaUidsNomina = from s in listaNominas
        //                              select s.IdNomina;

        //        var listaUidsFiniquito = from s in listaFiniquitos
        //                                 select s.IdFiniquito;

        //        registrosQueSonNomina = (from s in context.NOM_CFDI_Timbrado
        //                                 where listaUidsNomina.Contains(s.IdNomina) && s.Cancelado == false && s.IdFiniquito == 0 && s.ErrorTimbrado == false && String.IsNullOrEmpty(s.FolioFiscalUUID) == false
        //                                 select s).ToList();

        //        registrosQueSonFiniquito = (from s in context.NOM_CFDI_Timbrado
        //                                    where listaUidsFiniquito.Contains(s.IdFiniquito) && s.Cancelado == false && s.IdNomina == 0 && s.ErrorTimbrado == false && String.IsNullOrEmpty(s.FolioFiscalUUID) == false
        //                                    select s).ToList();

        //        #endregion
        //    }


        //    #region UNIR LISTAS NOMINAS Y FINIQUITOS
        //    //Unir la lista de conceptos de nomina con la lista de conceptos del finiquito
        //    listaConceptos.AddRange(listaConceptosFiniquito);
        //    listaConceptos = listaConceptos.Distinct().ToList();

        //    //Unir la lista de empleados de la nomina con la lista de empleado del finiquito
        //    listaEmpleados.AddRange(listaEmpleadosFiniquitos);
        //    listaEmpleados = listaEmpleados.Distinct().ToList();
        //    #endregion


        //    //Validamos las listas
        //    if (listaConceptos.Count <= 0) return null;


        //    List<EmpleadoIncidencias> listaIncidencias = new List<EmpleadoIncidencias>();
        //    _Incidencias inc = new _Incidencias();
        //    foreach (var itemP in listaPeriodos)
        //    {

        //        var arrayEmp = listaTimbrados.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).Select(x => x.IdEmpleado).Distinct().ToArray();

        //        var listaInc = inc.GetIncidenciasByPeriodo2(itemP, arrayEmp);

        //        listaIncidencias.AddRange(listaInc);
        //    }



        //    Empresa itemEmpresa = null;

        //    //Guarda el archivo en la memoria
        //    //System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //    //Crea el libro y la hoja para el Layout
        //    var workbook = new XLWorkbook();

        //    #region DOCUMENTO EXCEL - NOMINA ********************************************************

        //    var worksheet = workbook.Worksheets.Add("Acumulado");
        //    worksheet.SheetView.Freeze(2, 4);



        //    #region HEADERS

        //    #region HEADER EMPLEADOS

        //    worksheet.Cell(2, 1).Value = "Id";
        //    worksheet.Cell(2, 2).Value = "Clave";
        //    worksheet.Cell(2, 3).Value = "Paterno";
        //    worksheet.Cell(2, 4).Value = "Materno";
        //    worksheet.Cell(2, 5).Value = "Nombre";
        //    worksheet.Cell(2, 6).Value = "RFC";
        //    worksheet.Cell(2, 7).Value = "UUID";   // se agrego esta columna  
        //    worksheet.Cell(2, 8).Value = "CURP"; //originalmente este era columna 7

        //    #endregion

        //    #region HEADER NOMINA
        //    worksheet.Cell(2, 9).Value = "SD";
        //    worksheet.Cell(2, 10).Value = "SDI";
        //    worksheet.Cell(2, 11).Value = "Cliente";
        //    worksheet.Cell(2, 12).Value = "Empresa";
        //    worksheet.Cell(2, 13).Value = "Periodo";
        //    worksheet.Cell(2, 14).Value = "Fecha timbrado";  //se agrego esta columna
        //    worksheet.Cell(2, 15).Value = "Bimestre";
        //    worksheet.Cell(2, 16).Value = "Dias Trabajados";
        //    worksheet.Cell(2, 17).Value = "Total Nomina";
        //    worksheet.Cell(2, 18).Value = "Total Finiquito";
        //    worksheet.Cell(2, 19).Value = "Total Liq";
        //    worksheet.Cell(2, 20).Value = "Total Neto";
        //    worksheet.Cell(2, 21).Value = "Descuento Adicional Fin";
        //    worksheet.Cell(2, 22).Value = "Comisiones Fin";
        //    worksheet.Cell(2, 23).Value = "Permisos";
        //    worksheet.Cell(2, 24).Value = "Incapacidades";
        //    worksheet.Cell(2, 25).Value = "Faltas";
        //    #endregion

        //    #region HEADER CONCEPTOS

        //    int columnaH = 26; //inicia en el 18    //---aqui decia 24
        //    //Por cada concepto busca su detalle-- GUARDAR EL TOTAL DE GRAVADO Y EXENTO

        //    var listaConceptosPercecpciones = listaConceptos.Where(x => x.TipoConcepto == 1).ToList();
        //    var listaConceptosDeducciones = listaConceptos.Where(x => x.TipoConcepto == 2).ToList();

        //    int rangeInicial = columnaH;
        //    #region PERCEPCIONES
        //    foreach (var itemConcepto in listaConceptosPercecpciones)
        //    {
        //        int colInicial = columnaH;
        //        int colFinal = 0;
        //        worksheet.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;
        //        //  43 - ISR
        //        //  144 - Subsidio
        //        //  42 - SS
        //        if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
        //        {
        //            worksheet.Cell(2, columnaH).Value = " ";
        //            colFinal = columnaH;
        //            columnaH++;
        //        }
        //        else
        //        {
        //            worksheet.Cell(2, columnaH).Value = "Total ";
        //            columnaH++;
        //            worksheet.Cell(2, columnaH).Value = "Gravado";
        //            columnaH++;
        //            worksheet.Cell(2, columnaH).Value = "Exento";
        //            colFinal = columnaH;
        //            columnaH++;
        //        }

        //        //MERGE CELL
        //        worksheet.Range(1, colInicial, 1, colFinal).Row(1).Merge().Style
        //            .Font.SetBold(true)
        //            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    }

        //    #endregion

        //    //TOTAL DE PERCEPCIONES
        //    worksheet.Cell(2, columnaH).Value = "Total Percepciones";
        //    columnaH++;

        //    //Establece un estilo al header
        //    worksheet.Range(1, rangeInicial, 2, columnaH - 1).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.BlueGray);


        //    rangeInicial = columnaH;
        //    #region DEDUCCIONES
        //    foreach (var itemConcepto in listaConceptosDeducciones)
        //    {
        //        int colInicial = columnaH;
        //        int colFinal = 0;
        //        worksheet.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;
        //        //  43 - ISR
        //        //  144 - Subsidio
        //        //  42 - SS
        //        if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
        //        {
        //            worksheet.Cell(2, columnaH).Value = " ";
        //            colFinal = columnaH;
        //            columnaH++;
        //        }
        //        else
        //        {
        //            worksheet.Cell(2, columnaH).Value = "Total ";
        //            columnaH++;
        //            worksheet.Cell(2, columnaH).Value = "Gravado";
        //            columnaH++;
        //            worksheet.Cell(2, columnaH).Value = "Exento";
        //            colFinal = columnaH;
        //            columnaH++;
        //        }

        //        //MERGE CELL
        //        worksheet.Range(1, colInicial, 1, colFinal).Row(1).Merge().Style
        //            .Font.SetBold(true)
        //            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    }


        //    worksheet.Cell(1, columnaH).Value = "Subsidio Causado";
        //    columnaH++;
        //    worksheet.Cell(1, columnaH).Value = "Subsidio Entregado";
        //    columnaH++;
        //    worksheet.Cell(1, columnaH).Value = "Isr antes Subsidio";
        //    columnaH++;


        //    //TOTAL DE PERCEPCIONES
        //    worksheet.Cell(1, columnaH).Value = "Total Deducciones";
        //    columnaH++;
        //    worksheet.Cell(1, columnaH).Value = "Total ISN";
        //    columnaH++;
        //    #endregion

        //    //Establece un estilo al header
        //    worksheet.Range(1, rangeInicial, 2, columnaH - 1).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.Amber);


        //    #endregion

        //    #region HEADER IMSS

        //    int mergeI = columnaH;
        //    int colorI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Cuotas IMSS";
        //    worksheet.Cell(2, columnaH).Value = "Total Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Total Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Cuota Fija";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Excedente";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Prestaciones";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Pensionados";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Invalidez y Vida";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Guarderia";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Seguro Retiro";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Cesantía y Vejez";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Infonavit";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheet.Cell(1, columnaH).Value = "Riesgo Trabajo";
        //    worksheet.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Obrero";
        //    worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    //Establece un estilo al header
        //    worksheet.Range(1, colorI, 2, columnaH).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.BlueGreen);
        //    columnaH++;

        //    #endregion

        //    #region ANEXO




        //    worksheet.Cell(2, columnaH).Value = "Complemento";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Ejercicio Completo";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Base Gravable Anual";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Limite Inferior";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Excedente";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Tasa";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Impuesto Marginal";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;
        //    worksheet.Cell(2, columnaH).Value = "Cuota Fija";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "ISR Retener Anual";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);
        //    columnaH++;

        //    worksheet.Cell(2, columnaH).Value = "Saldo Favor Anual";
        //    worksheet.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);

        //    #endregion

        //    worksheet.Cell(2, columnaH).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    //MERGE CELL
        //    //  worksheet.Range(1, columnaH, 1, columnaH + 1).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


        //    #endregion

        //    //Variables
        //    int fila = 3;
        //    int filaInicial = 0;
        //    int filaFinal = 0;
        //    bool rowColor = false;
        //    int idEmp = 0;
        //    int arraySize = 7 + 22 + 1;

        //    var periodosIdx = listaPeriodos.Count * 3;
        //    arraySize += periodosIdx;

        //    double[] totales = new double[arraySize];
        //    #region CONTENIDO

        //    //Por Empleado
        //    //Buscamos sus nominas y sus detalles de cada periodo
        //    foreach (var itemEmpleado in listaEmpleados)
        //    {
        //        //   var columna = 0;
        //        //Cuantas nominas tiene el empleado
        //        var nominasDelEmpleado = listaNominas.Where(x => x.IdEmpleado == itemEmpleado.IdEmpleado).ToList();
        //        bool ejercicioCompleto = false;

        //        if (idEmp != itemEmpleado.IdEmpleado)
        //        {
        //            idEmp = itemEmpleado.IdEmpleado;
        //            rowColor = !rowColor;
        //            rowColor = true;
        //            filaInicial = fila;
        //        }

        //        //Por cada Nomina
        //        #region CONTENIDO NOMINA
        //        foreach (var itemNomina in nominasDelEmpleado)
        //        {
        //            ejercicioCompleto = ContenidoAcumulado(ref worksheet, itemNomina, null, itemEmpleado, listaPeriodos, listaEmpresasF, listaClientes, listaCuotas, listaDetalles, null, listaConceptosPercecpciones, listaConceptosDeducciones, incluirNoAutorizados, false, fila, null, listaContratos, ejercicioFiscal, listaIncidencias, registrosQueSonNomina, registrosQueSonFiniquito);

        //            //Sumar los totales de Gravados Exentos - ISN

        //            fila++;
        //            //rowColor = !rowColor;
        //        }//Fin del For de la nominas del empleado
        //        #endregion


        //        #region CONTENIDO FINIQUITO

        //        //Obtener los finiquitos del empleado  
        //        var finiquitosEmpleado = listaFiniquitos.Where(x => x.IdEmpleado == itemEmpleado.IdEmpleado).ToList();

        //        foreach (var itemFiniquito in finiquitosEmpleado)
        //        {
        //            ejercicioCompleto = ContenidoAcumulado(ref worksheet, null, itemFiniquito, itemEmpleado, listaPeriodos, listaEmpresasF, listaClientes, listaCuotas, null, listaFiniquitosDetalles, listaConceptosPercecpciones, listaConceptosDeducciones, incluirNoAutorizados, true, fila, listaDescuentoComisionFiniquito, listaContratos, ejercicioFiscal, listaIncidencias, registrosQueSonNomina, registrosQueSonFiniquito);

        //            //set color al row de finiquito
        //            worksheet.Range(fila, 1, fila, columnaH).Style.Fill.SetBackgroundColor(XLColor.Lavender);
        //            fila++;
        //        }


        //        #endregion


        //        if (rowColor)
        //        {
        //            rowColor = false;
        //            worksheet.Range(fila, 1, fila, columnaH).Style.Border.TopBorder = XLBorderStyleValues.Thin;
        //            worksheet.Range(fila, 1, fila, columnaH).Style.Border.TopBorderColor = XLColor.SlateGray;
        //            //worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Lavender;
        //            //worksheet.Range(fila, 1, fila, columna).Style.Fill.SetBackgroundColor(XLColor.Lavender);

        //            //Agregamos la sumatoria
        //            if (nominasDelEmpleado.Count > 0)
        //            {
        //                SumarColumnas(ref worksheet, filaInicial, fila - 1, columnaH - 8, ejercicioCompleto, listaPeriodos[0].IdEjercicio, nominasDelEmpleado[0].IdEmpleado, nominasDelEmpleado[nominasDelEmpleado.Count - 1].SD, nominasDelEmpleado[nominasDelEmpleado.Count - 1].SDI, nominasDelEmpleado[0].IdEmpresaFiscal, calculoanual);
        //            }
        //            fila++;
        //            fila++;
        //        }

        //    }//Fin For por empleado

        //    #endregion

        //    #region  COLORES DISEÑO DE CELDAS

        //    //Establece un estilo al header
        //    worksheet.Range("G2:Y2").Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Font.SetFontColor(XLColor.White)
        //        .Fill.SetBackgroundColor(XLColor.Raspberry);

        //    //Establece un estilo al header
        //    worksheet.Range("A2:F2").Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Font.SetFontColor(XLColor.White)
        //        .Fill.SetBackgroundColor(XLColor.DarkBlue);


        //    #endregion

        //    #region AJUSTE DE LAS CELDAS AL CONTENIDO

        //    //Ajustar el header al contenido
        //    worksheet.Column(1).AdjustToContents();
        //    worksheet.Column(2).AdjustToContents();
        //    worksheet.Column(3).AdjustToContents();
        //    worksheet.Column(4).AdjustToContents();
        //    worksheet.Column(5).AdjustToContents();
        //    worksheet.Column(6).AdjustToContents();
        //    worksheet.Column(7).AdjustToContents();
        //    worksheet.Column(8).AdjustToContents();
        //    worksheet.Column(9).AdjustToContents();
        //    worksheet.Column(10).AdjustToContents();
        //    worksheet.Column(11).AdjustToContents();
        //    worksheet.Column(12).AdjustToContents();
        //    worksheet.Column(13).AdjustToContents();
        //    worksheet.Column(14).AdjustToContents();
        //    worksheet.Column(15).AdjustToContents();
        //    worksheet.Column(16).AdjustToContents();
        //    worksheet.Column(17).AdjustToContents();
        //    worksheet.Column(18).AdjustToContents();
        //    worksheet.Column(19).AdjustToContents();
        //    worksheet.Column(20).AdjustToContents();
        //    worksheet.Column(21).AdjustToContents();

        //    #endregion

        //    #endregion

        //    #region DOCUMENTO EXCEL - FINIQUITO *****************************************************
        //    /*
        //    var worksheetF = workbook.Worksheets.Add("Finiquitos");
        //    worksheetF.SheetView.Freeze(2, 4);

        //    #region HEADERS

        //    #region EMPLEADO 

        //    worksheetF.Cell(2, 1).Value = "IdFiniquito";
        //    worksheetF.Cell(2, 2).Value = "Clave";
        //    worksheetF.Cell(2, 3).Value = "Paterno";
        //    worksheetF.Cell(2, 4).Value = "Materno";
        //    worksheetF.Cell(2, 5).Value = "Nombre";
        //    worksheetF.Cell(2, 6).Value = "RFC";
        //    worksheetF.Cell(2, 7).Value = "CURP";

        //    #endregion

        //    #region DATOS

        //    worksheetF.Cell(2, 8).Value = "SD";
        //    worksheetF.Cell(2, 9).Value = "SDI";

        //    worksheetF.Cell(2, 10).Value = "Cliente";
        //    worksheetF.Cell(2, 11).Value = "Periodo";
        //    worksheetF.Cell(2, 12).Value = "Bimestre";
        //    worksheetF.Cell(2, 13).Value = "Total Finiquito";
        //    worksheetF.Cell(2, 14).Value = "Total Liq";
        //    worksheetF.Cell(2, 15).Value = "Total Neto";

        //    //Establece un estilo al header
        //    worksheetF.Range("A2:O2").Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Font.SetFontColor(XLColor.White)
        //        .Fill.SetBackgroundColor(XLColor.DarkBlue); //AirForceBlue

        //    #endregion

        //    #region CONCEPTOS

        //    columnaH = 16; //inicia en el 18
        //                   //Por cada concepto busca su detalle-- GUARDAR EL TOTAL DE GRAVADO Y EXENTO

        //    rangeInicial = columnaH;
        //    foreach (var itemConcepto in listaConceptosFiniquito)
        //    {
        //        int colInicial = columnaH;
        //        int colFinal = 0;
        //        worksheetF.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;

        //        //  43 - ISR
        //        //  144 - Subsidio
        //        //  42 - SS
        //        if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
        //        {
        //            worksheetF.Cell(2, columnaH).Value = " ";
        //            colFinal = columnaH;
        //            columnaH++;
        //        }
        //        else
        //        {
        //            // worksheetF.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;
        //            worksheetF.Cell(2, columnaH).Value = "Total ";
        //            columnaH++;
        //            worksheetF.Cell(2, columnaH).Value = "Gravado";
        //            columnaH++;
        //            worksheetF.Cell(2, columnaH).Value = "Exento";
        //            colFinal = columnaH;
        //            columnaH++;
        //            //worksheetF.Cell(2, columnaH).Value = "ISN";
        //            //colFinal = columnaH;
        //            //columnaH++;
        //        }


        //        //MERGE CELL
        //        worksheetF.Range(1, colInicial, 1, colFinal).Row(1).Merge().Style
        //            .Font.SetBold(true)
        //            .Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    }

        //    //Establece un estilo al header
        //    worksheetF.Range(1, rangeInicial, 2, columnaH - 1).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.BlueGray);

        //    #endregion

        //    #region HEADER DESCUENTOS Y COMISIONES
        //    //TOTAL ISN
        //    rangeInicial = columnaH;
        //    worksheetF.Cell(2, columnaH).Value = "TOTAL ISN";
        //    columnaH++;
        //    //DESCUENTOS
        //    worksheetF.Cell(2, columnaH).Value = "DESCUENTO ADICIONAL";
        //    columnaH++;
        //    //COMISIONES
        //    worksheetF.Cell(2, columnaH).Value = "COMISIONES";
        //    columnaH++;


        //    //Establece un estilo al header
        //    worksheetF.Range(1, rangeInicial, 2, columnaH - 1).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.Amber);
        //    #endregion

        //    #region HEADER IMSS

        //    mergeI = columnaH;
        //    colorI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Cuotas IMSS";
        //    worksheetF.Cell(2, columnaH).Value = "Total Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Total Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Cuota Fija";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Excedente";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Prestaciones";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Pensionados";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Invalidez y Vida";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Guarderia";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Seguro Retiro";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Cesantía y Vejez";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Infonavit";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //    columnaH++;

        //    mergeI = columnaH;
        //    worksheetF.Cell(1, columnaH).Value = "Riesgo Trabajo";
        //    worksheetF.Cell(2, columnaH).Value = "Patron";
        //    columnaH++;
        //    worksheetF.Cell(2, columnaH).Value = "Obrero";
        //    worksheetF.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


        //    //Establece un estilo al header
        //    worksheetF.Range(1, colorI, 2, columnaH).Style
        //        .Font.SetFontSize(13)
        //        .Font.SetBold(true)
        //        .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //        .Font.SetFontColor(XLColor.Black)
        //        .Fill.SetBackgroundColor(XLColor.BlueGreen);
        //    columnaH++;


        //    //MERGE CELL
        //    //  worksheet.Range(1, columnaH, 1, columnaH + 1).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        //    worksheetF.Cell(2, columnaH).Value = "Complemento";
        //    worksheetF.Cell(2, columnaH).Style
        //         .Font.SetFontSize(13)
        //         .Font.SetBold(true)
        //         .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
        //         .Font.SetFontColor(XLColor.Black)
        //         .Fill.SetBackgroundColor(XLColor.DarkSalmon);

        //    #endregion

        //    #endregion

        //    #region CONTENIDO

        //    #region CONCEPTOS
        //    fila = 3;
        //    foreach (var itemFiniquito in listaFiniquitos)
        //    {
        //        //GET dato empleado
        //        var itemEmpleado = listaEmpleadosFiniquitos.FirstOrDefault(x => x.IdEmpleado == itemFiniquito.IdEmpleado);

        //        if (itemEmpleado == null) continue;

        //        worksheetF.Cell(fila, 1).Value = itemFiniquito.IdFiniquito;
        //        worksheetF.Cell(fila, 2).Value = itemEmpleado.IdEmpleado;
        //        worksheetF.Cell(fila, 3).Value = itemEmpleado.APaterno;
        //        worksheetF.Cell(fila, 4).Value = itemEmpleado.AMaterno;
        //        worksheetF.Cell(fila, 5).Value = itemEmpleado.Nombres;
        //        worksheetF.Cell(fila, 6).Value = itemEmpleado.RFC;
        //        worksheetF.Cell(fila, 7).Value = itemEmpleado.CURP;

        //        worksheetF.Cell(fila, 8).Value = itemFiniquito.SD;
        //        worksheetF.Cell(fila, 9).Value = itemFiniquito.SDI;

        //        //Empresa - Periodo
        //        var itemPeriodo = listaPeriodos.FirstOrDefault(x => x.IdPeriodoPago == itemFiniquito.IdPeriodo);
        //        itemEmpresa = listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemFiniquito.IdEmpresaFiscal) ??
        //                         listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemFiniquito.IdEmpresaAsimilado);

        //        var itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemFiniquito.IdCliente);

        //        // worksheetF.Cell(fila, 11).Value = itemEmpresa != null ? itemEmpresa.RazonSocial : "-";
        //        worksheetF.Cell(fila, 10).Value = itemCliente != null ? itemCliente.Nombre : "-";
        //        worksheetF.Cell(fila, 11).Value = itemPeriodo?.Fecha_Fin.ToShortDateString() ?? "null";
        //        worksheetF.Cell(fila, 12).Value = itemPeriodo.Bimestre;
        //        worksheetF.Cell(fila, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        //        worksheetF.Cell(fila, 13).Value = itemFiniquito.Total_Finiquito;
        //        worksheetF.Cell(fila, 14).Value = itemFiniquito.Total_Liquidacion;
        //        worksheetF.Cell(fila, 15).Value = itemFiniquito.TOTAL_total;

        //        int columna = 16; //
        //        decimal totalIsn = 0;
        //        foreach (var itemConcepto in listaConceptosFiniquito)
        //        {
        //            decimal total = 0;
        //            decimal gravado = 0;
        //            decimal exento = 0;
        //            decimal isn = 0;

        //            var itemDetalleF =
        //                listaFiniquitosDetalles.FirstOrDefault(
        //                    x => x.IdFiniquito == itemFiniquito.IdFiniquito && x.IdConcepto == itemConcepto.IdConcepto);

        //            if (itemDetalleF != null)
        //            {
        //                total = itemDetalleF.Total;
        //                gravado = itemDetalleF.GravadoISR;
        //                exento = itemDetalleF.ExentoISR;
        //                isn = itemDetalleF.ImpuestoSobreNomina;
        //                totalIsn += isn;
        //            }


        //            if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
        //            {
        //                worksheetF.Cell(fila, columna).Value = total;
        //                columna++;
        //            }
        //            else
        //            {
        //                worksheetF.Cell(fila, columna).Value = total;
        //                columna++;
        //                worksheetF.Cell(fila, columna).Value = gravado;
        //                columna++;
        //                worksheetF.Cell(fila, columna).Value = exento;
        //                columna++;
        //                //worksheetF.Cell(fila, columna).Value = isn;
        //                //columna++;
        //            }
        //        }



        //        #region  DESCUENTOS Y COMISIONES

        //        var descuentosAdicionales = listaDescuentoComisionFiniquito.Where(x => x.IdPeriodo == itemFiniquito.IdPeriodo && x.IdConcepto == 2)
        //               .Select(x => x.TotalDescuento)
        //               .Sum();

        //        var comisiones = listaDescuentoComisionFiniquito.Where(x => x.IdPeriodo == itemFiniquito.IdPeriodo && x.IdConcepto == 1)
        //                .Select(x => x.TotalDescuento)
        //                .Sum();
        //        //TOTAL ISN
        //        worksheetF.Cell(fila, columna).Value = totalIsn;
        //        columna++;
        //        //DESCUENTOS
        //        worksheetF.Cell(fila, columna).Value = descuentosAdicionales;
        //        columna++;
        //        //COMISIONES
        //        worksheetF.Cell(fila, columna).Value = comisiones;
        //        columna++;
        //        #endregion


        //        #region CUOTAS IMSS

        //        var itemCuotaImss = listaCuotasFiniquito.FirstOrDefault(x => x.IdFiniquito == itemFiniquito.IdFiniquito);
        //        decimal totalPatron = 0;
        //        decimal totalObrero = 0;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.TotalPatron ?? 0;
        //        columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.TotalObrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Cuota_Fija_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Excedente_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Excedente_Obrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.PrestacionesDinero_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.PrestacionesDinero_Obrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Pensionados_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Pensionados_Obrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.InvalidezVida_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.InvalidezVida_Obrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.GuarderiasPrestaciones_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.SeguroRetiro_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.CesantiaVejez_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.CesantiaVejez_Obrero ?? 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.Infonavit_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = 0; columna++;

        //        worksheetF.Cell(fila, columna).Value = itemCuotaImss?.RiesgoTrabajo_Patron ?? 0; columna++;
        //        worksheetF.Cell(fila, columna).Value = 0; columna++;
        //        #endregion

        //        worksheetF.Cell(fila, columna).Value = itemFiniquito.TotalComplemento;

        //        // if (rowColor)
        //        //    {
        //        //        // worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Lavender;
        //        //        worksheetF.Range(fila, 1, fila, columna).Style.Fill.SetBackgroundColor(XLColor.Lavender);
        //        //    }

        //        //rowColor = !rowColor;
        //        fila++;
        //    }

        //    #endregion

        //    #endregion

        //    #region AJUSTE DE CELDAS

        //    worksheetF.Column(1).AdjustToContents(1);
        //    worksheetF.Column(2).AdjustToContents(2);
        //    worksheetF.Column(3).AdjustToContents(3);
        //    worksheetF.Column(4).AdjustToContents(4);
        //    worksheetF.Column(5).AdjustToContents(5);
        //    worksheetF.Column(6).AdjustToContents(6);
        //    worksheetF.Column(7).AdjustToContents(7);
        //    worksheetF.Column(8).AdjustToContents(8);
        //    worksheetF.Column(9).AdjustToContents(9);
        //    worksheetF.Column(10).AdjustToContents(10);
        //    worksheetF.Column(11).AdjustToContents(11);
        //    worksheetF.Column(12).AdjustToContents(12);
        //    worksheetF.Column(13).AdjustToContents(13);
        //    worksheetF.Column(14).AdjustToContents(14);
        //    worksheetF.Column(15).AdjustToContents(15);
        //    worksheetF.Column(16).AdjustToContents(16);
        //    worksheetF.Column(17).AdjustToContents(17);
        //    worksheetF.Column(18).AdjustToContents(18);
        //    worksheetF.Column(19).AdjustToContents(19);
        //    worksheetF.Column(20).AdjustToContents(20);
        //    worksheetF.Column(21).AdjustToContents(21);


        //    #endregion
        //    */
        //    //fin hoja de finiquito
        //    #endregion

        //    // AddColsUuidFechaCertificacion( worksheet, ctx.NOM_CFDI_Timbrado.ToList<NOM_CFDI_Timbrado>() );    //mandar a agregar columna  UUID y fecha de certificacion


        //    #region CREA EL ARCHIVO Y RETORNA EL PATH RELATIVO PARA LA DESCARGA



        //    //Creamos el folder para guardar el archivo
        //    var pathUsuario = Utils.ValidarFolderUsuario(idUsuario, pathFolder);

        //    string nombreArchivo = "Acumulado_.xlsx";

        //    if (itemEmpresa != null)
        //    {
        //        nombreArchivo = "ACUM_" + itemEmpresa.RazonSocial + "_.xlsx";
        //    }

        //    var fileName = pathUsuario + nombreArchivo;
        //    //Guarda el archivo
        //    workbook.SaveAs(fileName);
        //    //  return fileName;
        //    return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;

        //    //Guarda el documento en la memori
        //    //workbook.SaveAs(ms, false);
        //    //var archivoByte =  ms.ToArray();
        //    //return archivoByte;
        //    #endregion
        //}

        #region ACUMULADO

        public string GenerarArchivoAcumuladoComplete_NUEVO2018(int idUsuario, string pathFolder, string pathDescarga, int idEjercicio, DateTime? dateInicial, DateTime? dateFinal, int idEmpresa = 0, int idCliente = 0, bool incluirNoAutorizados = false, bool calculoanual = false)
        {
            //Validaciones
            if (pathFolder.Trim() == "" || pathDescarga.Trim() == "" || dateInicial == null || dateFinal == null || idEjercicio == null)
                return null;


            #region Variables - List<>

            List<C_NOM_Conceptos> listaConceptos;
            List<Empleado> listaEmpleados;
            List<NOM_Nomina> listaNominas;
            List<NOM_Nomina_Detalle> listaDetalles;
            List<NOM_Cuotas_IMSS> listaCuotas;
            List<NOM_Cuotas_IMSS> listaCuotasFiniquito;
            List<Empresa> listaEmpresasF;
            List<NOM_PeriodosPago> listaPeriodos;
            List<NOM_Finiquito> listaFiniquitos;
            List<NOM_Finiquito_Detalle> listaFiniquitosDetalles;
            List<C_NOM_Conceptos> listaConceptosFiniquito;
            List<Empleado> listaEmpleadosFiniquitos;
            List<Cliente> listaClientes;
            List<NOM_Finiquito_Descuento_Adicional> listaDescuentoComisionFiniquito;
            List<Empleado_Contrato> listaContratos;
            NOM_Ejercicio_Fiscal ejercicioFiscal;

            List<Permisos> listaPermisos;
            List<Inasistencias> listafaltas;
            List<Incapacidades> listaIncapacidadeses;

            List<NOM_CFDI_Timbrado> registrosQueSonNomina;
            List<NOM_CFDI_Timbrado> registrosQueSonFiniquito;
            #endregion

            var contextt = new RHEntities();

            //#######################################################################################################
            ////Obtiene todos los periodos en el rango de fecha 
            //var ListPeriodos = (from p in contextt.NOM_PeriodosPago
            //                    where p.Fecha_Fin >= DbFunctions.TruncateTime(dateInicial) && p.Fecha_Fin <= DbFunctions.TruncateTime(dateFinal)
            //                      && p.Autorizado == true
            //                      && (p.IdTipoNomina <= 16) //Nominas - Asimilados - y finiquitos
            //                      && p.SoloComplemento == false
            //                    select p).ToList();

            ////se guardan los ids de los periodos en la fecha
            ////var arrayListPeriodos = ListPeriodos.Select(x => x.IdPeriodoPago).ToList();

            ////se dividen los ids para buscarlos en la tabla de nomina y finiquitos
            //var listidNominas = ListPeriodos.Where(x => x.IdTipoNomina != 11).Select(x => x.IdPeriodoPago).ToList();
            //var listidFiniquitos = ListPeriodos.Where(x => x.IdTipoNomina == 11).Select(x => x.IdPeriodoPago).ToList();
            //#######################################################################################################

            var ListPeriodos = (from p in contextt.NOM_PeriodosPago
                                where p.Fecha_Fin >= DbFunctions.TruncateTime(dateInicial) && p.Fecha_Fin <= DbFunctions.TruncateTime(dateFinal)
                                      && p.Autorizado == true
                                      && (p.IdTipoNomina <= 16 && p.IdTipoNomina != 11)
                                      && p.SoloComplemento == false
                                select p).ToList();

            var listPeriodosFiniquito = (from p in contextt.NOM_PeriodosPago
                                         join t in contextt.NOM_CFDI_Timbrado on p.IdPeriodoPago equals t.IdPeriodo
                                         where t.FechaCertificacion >= DbFunctions.TruncateTime(dateInicial) && t.FechaCertificacion <= DbFunctions.TruncateTime(dateFinal)
                                         && p.Autorizado == true
                                         && p.IdTipoNomina == 11
                                         && p.SoloComplemento == false
                                         select p).ToList();

            var listidNominas = ListPeriodos.Select(x => x.IdPeriodoPago).ToList();
            var listidFiniquitos = listPeriodosFiniquito.Select(x => x.IdPeriodoPago).ToList();



            // sirve para filtrar las nominas en lista var nominas que se estan pagando por asimilados ya que existe tambien una empresa fiscal
            var arrayIdperiodoAsimilados = ListPeriodos.Where(w => w.IdTipoNomina == 16).Select(x => x.IdPeriodoPago).ToArray();

            //----------------------------------------------------------------------------------------------------------------

            var listaNomina = (from p in contextt.NOM_Nomina
                               join t in contextt.NOM_CFDI_Timbrado on p.IdNomina equals t.IdNomina
                               where listidNominas.Contains(p.IdPeriodo)
                               && (p.IdEmpresaFiscal == idEmpresa || (p.IdEmpresaAsimilado == idEmpresa && arrayIdperiodoAsimilados.Contains(p.IdPeriodo)))
                               && (t.Cancelado == false && t.FechaCancelacion == null)
                               select p
                              ).ToList();

            var listaFiniquito = (from p in contextt.NOM_Finiquito
                                  join t in contextt.NOM_CFDI_Timbrado on p.IdFiniquito equals t.IdFiniquito
                                  where listidFiniquitos.Contains(p.IdPeriodo)
                                  && (t.Cancelado == false && t.FechaCancelacion == null)
                                  && p.IdEmpresaFiscal == idEmpresa
                                  select p
                                 ).ToList();

            var arrayListPeriodos = listaNomina.Select(x => x.IdPeriodo).Distinct().ToList();
            arrayListPeriodos.AddRange(listaFiniquito.Select(x => x.IdPeriodo).Distinct().ToList());

            //---------------------------------------------------------------------------------------------------------------

            var arrayListNominas = listaNomina.Select(x => x.IdNomina).ToList();
            var arrayListFiniquitos = listaFiniquito.Select(x => x.IdFiniquito).ToList();

            var arrayListEmpleados = listaNomina.Select(x => x.IdEmpleado).ToList();
            arrayListEmpleados.AddRange(listaFiniquito.Select(x => x.IdEmpleado).ToList());


            using (var context = new RHEntities())
            {
                #region GET DATA ACUMULADO

                ejercicioFiscal = context.NOM_Ejercicio_Fiscal.FirstOrDefault(x => x.IdEjercicio == idEjercicio);

                if (incluirNoAutorizados)
                {
                    //buscamos los periodos creados
                    var listaNoTimbrados = (from p in context.NOM_PeriodosPago
                                            where DbFunctions.TruncateTime(p.Fecha_Inicio) >= DbFunctions.TruncateTime(dateInicial)
                                                  && DbFunctions.TruncateTime(p.Fecha_Inicio) <= DbFunctions.TruncateTime(dateFinal)
                                                  && !arrayListPeriodos.Contains(p.IdPeriodoPago)
                                            select p.IdPeriodoPago).ToList();

                    if (listaNoTimbrados.Any())
                    {
                        //aseguramos que sean periodos con nominas generadas
                        var nominasNoTim = (from n in context.NOM_Nomina
                                            where listaNoTimbrados.Contains(n.IdPeriodo)
                                            && (n.IdEmpresaFiscal == idEmpresa || n.IdEmpresaAsimilado == idEmpresa)
                                            select n).ToList();

                        var listNomNo = nominasNoTim.Select(x => x.IdNomina).ToList();
                        var listaEmplNn = nominasNoTim.Select(x => x.IdEmpleado).ToList();
                        var periodosPorNomina = nominasNoTim.Select(x => x.IdPeriodo).ToList();

                        //buscamos los finiquitos
                        var finiquitosNoTim = (from f in context.NOM_Finiquito
                                               where listaNoTimbrados.Contains(f.IdPeriodo)
                                               && (f.IdEmpresaFiscal == idEmpresa || f.IdEmpresaAsimilado == idEmpresa)
                                               select f).ToList();

                        var listFinNo = finiquitosNoTim.Select(x => x.IdFiniquito).ToList();
                        var listaEmplNnF = finiquitosNoTim.Select(x => x.IdEmpleado).ToList();
                        var periodosPorFiniquito = finiquitosNoTim.Select(x => x.IdPeriodo).ToList();


                        arrayListPeriodos.AddRange(periodosPorNomina);
                        arrayListPeriodos.AddRange(periodosPorFiniquito);

                        arrayListNominas.AddRange(listNomNo);
                        arrayListFiniquitos.AddRange(listFinNo);

                        arrayListEmpleados.AddRange(listaEmplNn);
                        arrayListEmpleados.AddRange(listaEmplNnF);
                    }
                }

                if (!arrayListPeriodos.Any())
                    return null;

                var arrayPeriodos = arrayListPeriodos.Distinct().ToArray();
                var arrayNominas = arrayListNominas.Distinct().ToArray();
                var arrayFiniquitos = arrayListFiniquitos.Distinct().ToArray();
                var arrayEmpleados = arrayListEmpleados.Distinct().ToArray();



                //aqui se podrian incluir los no autorizados
                listaPeriodos = (from p in context.NOM_PeriodosPago
                                 where arrayPeriodos.Contains(p.IdPeriodoPago)
                                 select p).ToList();


                // var arrayPeriodosAutorizados = listaPeriodos.Select(x => x.IdPeriodoPago).ToArray();

                //GET - lista de nominas que estan en el array de periodos
                listaNominas = (from n in context.NOM_Nomina
                                where arrayNominas.Contains(n.IdNomina)
                                select n).ToList();

                //GET - lista de finiquitos que estan en el array de periodos
                listaFiniquitos = (from f in context.NOM_Finiquito
                                   where arrayFiniquitos.Contains(f.IdFiniquito)
                                   select f).ToList();

                if (idCliente > 0)
                {
                    //De la lista anterior filtramos las nominas dond el cliente sea idCliente
                    listaNominas = listaNominas.Where(x => x.IdCliente == idCliente).ToList();

                    //Filtramos los finiquitos por IdCliente
                    listaFiniquitos = listaFiniquitos.Where(x => x.IdCliente == idCliente).ToList();
                }


                if (listaNominas.Count <= 0) return null;

                //GET Detalle de finiquitos
                listaFiniquitosDetalles = (from fd in context.NOM_Finiquito_Detalle
                                           where arrayFiniquitos.Contains(fd.IdFiniquito)
                                           select fd).ToList();

                //GET PERMISOS E INCAPACIDADES
                //var fecha2mesesMenos = dateInicial.Value.AddMonths(-2);


                var primerPeriodo = listaPeriodos.OrderByDescending(x => x.Fecha_Inicio).LastOrDefault();
                var ultimoPeriodo = listaPeriodos.OrderByDescending(x => x.Fecha_Inicio).FirstOrDefault();

                var fechaInicioIncidencias = primerPeriodo.Fecha_Inicio;
                var fechaFinInicidencias = ultimoPeriodo.Fecha_Fin;


                //GET - Lista de detalles de nominas del arrray de nominas
                listaDetalles = (from nd in context.NOM_Nomina_Detalle
                                 where arrayNominas.Contains(nd.IdNomina)
                                 select nd).ToList();

                //GET - lista de cuotas imss del array de nominas
                listaCuotas = (from cuotas in context.NOM_Cuotas_IMSS
                               where arrayNominas.Contains(cuotas.IdNomina)
                               select cuotas).ToList();


                //GET - lista de cuotas imss de los finiquitos
                //listaCuotasFiniquito = (from cuotas in context.NOM_Cuotas_IMSS
                //                        where arrayIdFiniquitos.Contains(cuotas.IdFiniquito)
                //                        select cuotas).ToList();

                listaDescuentoComisionFiniquito = (from dc in context.NOM_Finiquito_Descuento_Adicional
                                                   where arrayPeriodos.Contains(dc.IdPeriodo)
                                                   select dc).ToList();

                //GET -  Datos del empleado
                listaEmpleados = (from emp in context.Empleado
                                  where arrayEmpleados.Contains(emp.IdEmpleado)
                                  orderby emp.APaterno
                                  select emp).ToList();

                //Agrupa por IdConcepto
                var listaGroupConceptos = listaDetalles.GroupBy(x => x.IdConcepto).ToList();

                //Genera Array de Id de los conceptos
                var arrayIdConceptos = listaGroupConceptos.Select(x => x.Key).ToArray();

                //GET - Lista de Conceptos de las Nominas
                listaConceptos = (from conce in context.C_NOM_Conceptos
                                  where arrayIdConceptos.Contains(conce.IdConcepto)
                                  select conce).ToList();

                var listaGroupConceptosF = listaFiniquitosDetalles.GroupBy(x => x.IdConcepto).ToList();

                arrayIdConceptos = listaGroupConceptosF.Select(x => x.Key).ToArray();

                //Lista de Conceptos de los finiquitos
                listaConceptosFiniquito = (from conce in context.C_NOM_Conceptos
                                           where arrayIdConceptos.Contains(conce.IdConcepto)
                                           select conce).ToList();

                //  arrayIdEmpleados = listaFiniquitos.Select(x => x.IdEmpleado).ToArray();

                listaEmpleadosFiniquitos = (from emp in context.Empleado
                                            where arrayEmpleados.Contains(emp.IdEmpleado)
                                            orderby emp.APaterno
                                            select emp).ToList();

                //GET - Lista empresas fiscales
                listaEmpresasF = (from em in context.Empresa
                                  select em).ToList();

                listaClientes = (from clie in context.Cliente
                                 select clie).ToList();



                //Buscamos los contratos que se usaron en las nominas del ejercicio fiscal
                var contratosNominas = listaNominas.Select(x => x.IdContrato).ToList();
                var contratosFiniquitos = listaFiniquitos.Select(x => x.IdFiniquito).ToList();

                contratosNominas.AddRange(contratosFiniquitos);

                var arrayIdContratos = contratosNominas.ToArray();

                listaContratos = (from con in context.Empleado_Contrato
                                  where arrayIdContratos.Contains(con.IdContrato)
                                  select con).ToList();


                //-------------------------------------intentar  recuperar uids de NOM_CFDI_Timbrado
                var listaUidsNomina = from s in listaNominas
                                      select s.IdNomina;

                var listaUidsFiniquito = from s in listaFiniquitos
                                         select s.IdFiniquito;

                registrosQueSonNomina = (from s in context.NOM_CFDI_Timbrado
                                         where listaUidsNomina.Contains(s.IdNomina) && s.Cancelado == false && s.IdFiniquito == 0 && s.ErrorTimbrado == false && String.IsNullOrEmpty(s.FolioFiscalUUID) == false
                                         select s).ToList();

                registrosQueSonFiniquito = (from s in context.NOM_CFDI_Timbrado
                                            where listaUidsFiniquito.Contains(s.IdFiniquito) && s.Cancelado == false && s.IdNomina == 0 && s.ErrorTimbrado == false && String.IsNullOrEmpty(s.FolioFiscalUUID) == false
                                            select s).ToList();

                #endregion
            }


            #region UNIR LISTAS NOMINAS Y FINIQUITOS
            //Unir la lista de conceptos de nomina con la lista de conceptos del finiquito
            listaConceptos.AddRange(listaConceptosFiniquito);
            listaConceptos = listaConceptos.Distinct().ToList();

            //Unir la lista de empleados de la nomina con la lista de empleado del finiquito
            listaEmpleados.AddRange(listaEmpleadosFiniquitos);
            listaEmpleados = listaEmpleados.Distinct().ToList();
            #endregion


            //Validamos las listas
            if (listaConceptos.Count <= 0) return null;


            List<EmpleadoIncidencias> listaIncidencias = new List<EmpleadoIncidencias>();
            _Incidencias inc = new _Incidencias();
            foreach (var itemP in listaPeriodos)
            {

                //var arrayEmp = listaTimbrados.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).Select(x => x.IdEmpleado).Distinct().ToArray();
                var listEmp = listaNomina.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).Select(x => x.IdEmpleado).Distinct().ToList();
                listEmp.AddRange(listaFiniquito.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).Select(x => x.IdEmpleado).Distinct().ToList());

                var arrayEmp = listEmp.ToArray();

                var listaInc = inc.GetIncidenciasByPeriodo2(itemP, arrayEmp);

                listaIncidencias.AddRange(listaInc);
            }



            Empresa itemEmpresa = contextt.Empresa.Where(x => x.IdEmpresa == idEmpresa).Select(x => x).FirstOrDefault();

            //Guarda el archivo en la memoria
            //System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //Crea el libro y la hoja para el Layout
            var workbook = new XLWorkbook();

            #region DOCUMENTO EXCEL - NOMINA ********************************************************

            var worksheet = workbook.Worksheets.Add("Acumulado");
            worksheet.SheetView.Freeze(2, 4);



            #region HEADERS

            #region HEADER EMPLEADOS

            worksheet.Cell(2, 1).Value = "Id";
            worksheet.Cell(2, 2).Value = "Clave";
            worksheet.Cell(2, 3).Value = "Paterno";
            worksheet.Cell(2, 4).Value = "Materno";
            worksheet.Cell(2, 5).Value = "Nombre";
            worksheet.Cell(2, 6).Value = "RFC";
            worksheet.Cell(2, 7).Value = "UUID";   // se agrego esta columna  
            worksheet.Cell(2, 8).Value = "CURP"; //originalmente este era columna 7

            #endregion

            #region HEADER NOMINA
            worksheet.Cell(2, 9).Value = "SD";
            worksheet.Cell(2, 10).Value = "SDI";
            worksheet.Cell(2, 11).Value = "Cliente";
            worksheet.Cell(2, 12).Value = "Empresa";
            worksheet.Cell(2, 13).Value = "Periodo";
            worksheet.Cell(2, 14).Value = "Fecha timbrado";  //se agrego esta columna
            worksheet.Cell(2, 15).Value = "Bimestre";
            worksheet.Cell(2, 16).Value = "Dias Trabajados";
            worksheet.Cell(2, 17).Value = "Total Nomina";
            worksheet.Cell(2, 18).Value = "Total Finiquito";
            worksheet.Cell(2, 19).Value = "Total Liq";
            worksheet.Cell(2, 20).Value = "Total Neto";
            worksheet.Cell(2, 21).Value = "Descuento Adicional Fin";
            worksheet.Cell(2, 22).Value = "Comisiones Fin";
            worksheet.Cell(2, 23).Value = "Permisos";
            worksheet.Cell(2, 24).Value = "Incapacidades";
            worksheet.Cell(2, 25).Value = "Faltas";
            #endregion

            #region HEADER CONCEPTOS

            int columnaH = 26; //inicia en el 18    //---aqui decia 24
            //Por cada concepto busca su detalle-- GUARDAR EL TOTAL DE GRAVADO Y EXENTO

            var listaConceptosPercecpciones = listaConceptos.Where(x => x.TipoConcepto == 1).ToList();
            var listaConceptosDeducciones = listaConceptos.Where(x => x.TipoConcepto == 2).ToList();

            int rangeInicial = columnaH;
            #region PERCEPCIONES
            foreach (var itemConcepto in listaConceptosPercecpciones)
            {
                int colInicial = columnaH;
                int colFinal = 0;
                worksheet.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;
                //  43 - ISR
                //  144 - Subsidio
                //  42 - SS
                if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
                {
                    worksheet.Cell(2, columnaH).Value = " ";
                    colFinal = columnaH;
                    columnaH++;
                }
                else
                {
                    worksheet.Cell(2, columnaH).Value = "Total ";
                    columnaH++;
                    worksheet.Cell(2, columnaH).Value = "Gravado";
                    columnaH++;
                    worksheet.Cell(2, columnaH).Value = "Exento";
                    colFinal = columnaH;
                    columnaH++;
                }

                //MERGE CELL
                worksheet.Range(1, colInicial, 1, colFinal).Row(1).Merge().Style
                    .Font.SetBold(true)
                    .Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            #endregion

            //TOTAL DE PERCEPCIONES
            worksheet.Cell(2, columnaH).Value = "Total Percepciones";
            columnaH++;

            //Establece un estilo al header
            worksheet.Range(1, rangeInicial, 2, columnaH - 1).Style
                .Font.SetFontSize(13)
                .Font.SetBold(true)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Font.SetFontColor(XLColor.Black)
                .Fill.SetBackgroundColor(XLColor.BlueGray);


            rangeInicial = columnaH;
            #region DEDUCCIONES
            foreach (var itemConcepto in listaConceptosDeducciones)
            {
                int colInicial = columnaH;
                int colFinal = 0;
                worksheet.Cell(1, columnaH).Value = itemConcepto.DescripcionCorta;
                //  43 - ISR
                //  144 - Subsidio
                //  42 - SS
                if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
                {
                    worksheet.Cell(2, columnaH).Value = " ";
                    colFinal = columnaH;
                    columnaH++;
                }
                else
                {
                    worksheet.Cell(2, columnaH).Value = "Total ";
                    columnaH++;
                    worksheet.Cell(2, columnaH).Value = "Gravado";
                    columnaH++;
                    worksheet.Cell(2, columnaH).Value = "Exento";
                    colFinal = columnaH;
                    columnaH++;
                }

                //MERGE CELL
                worksheet.Range(1, colInicial, 1, colFinal).Row(1).Merge().Style
                    .Font.SetBold(true)
                    .Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }


            worksheet.Cell(1, columnaH).Value = "Subsidio Causado";
            columnaH++;
            worksheet.Cell(1, columnaH).Value = "Subsidio Entregado";
            columnaH++;
            worksheet.Cell(1, columnaH).Value = "Isr antes Subsidio";
            columnaH++;


            //TOTAL DE PERCEPCIONES
            worksheet.Cell(1, columnaH).Value = "Total Deducciones";
            columnaH++;
            worksheet.Cell(1, columnaH).Value = "Total ISN";
            columnaH++;
            #endregion

            //Establece un estilo al header
            worksheet.Range(1, rangeInicial, 2, columnaH - 1).Style
                .Font.SetFontSize(13)
                .Font.SetBold(true)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Font.SetFontColor(XLColor.Black)
                .Fill.SetBackgroundColor(XLColor.Amber);


            #endregion

            #region HEADER IMSS

            int mergeI = columnaH;
            int colorI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Cuotas IMSS";
            worksheet.Cell(2, columnaH).Value = "Total Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Total Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Cuota Fija";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Excedente";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Prestaciones";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Pensionados";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Invalidez y Vida";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Guarderia";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Seguro Retiro";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Cesantía y Vejez";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Infonavit";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            columnaH++;

            mergeI = columnaH;
            worksheet.Cell(1, columnaH).Value = "Riesgo Trabajo";
            worksheet.Cell(2, columnaH).Value = "Patron";
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Obrero";
            worksheet.Range(1, mergeI, 1, columnaH).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //Establece un estilo al header
            worksheet.Range(1, colorI, 2, columnaH).Style
                .Font.SetFontSize(13)
                .Font.SetBold(true)
                .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Font.SetFontColor(XLColor.Black)
                .Fill.SetBackgroundColor(XLColor.BlueGreen);
            columnaH++;

            #endregion

            #region ANEXO




            //worksheet.Cell(2, columnaH).Value = "Complemento";
            //worksheet.Cell(2, columnaH).Style
            //     .Font.SetFontSize(13)
            //     .Font.SetBold(true)
            //     .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
            //     .Font.SetFontColor(XLColor.Black)
            //     .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            //columnaH++;

            worksheet.Cell(2, columnaH).Value = "Ejercicio Completo";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "Base Gravable Anual";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "Limite Inferior";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "Excedente";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "Tasa";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Impuesto Marginal";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;
            worksheet.Cell(2, columnaH).Value = "Cuota Fija";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "ISR Retener Anual";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);
            columnaH++;

            worksheet.Cell(2, columnaH).Value = "Saldo Favor Anual";
            worksheet.Cell(2, columnaH).Style
                 .Font.SetFontSize(13)
                 .Font.SetBold(true)
                 .Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                 .Font.SetFontColor(XLColor.Black)
                 .Fill.SetBackgroundColor(XLColor.DarkSalmon);

            #endregion

            worksheet.Cell(2, columnaH).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //MERGE CELL
            //  worksheet.Range(1, columnaH, 1, columnaH + 1).Row(1).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


            #endregion

            //Variables
            int fila = 3;
            int filaInicial = 0;
            int filaFinal = 0;
            bool rowColor = false;
            int idEmp = 0;
            int arraySize = 7 + 22 + 1;

            var periodosIdx = listaPeriodos.Count * 3;
            arraySize += periodosIdx;

            double[] totales = new double[arraySize];
            #region CONTENIDO

            //Por Empleado
            //Buscamos sus nominas y sus detalles de cada periodo
            foreach (var itemEmpleado in listaEmpleados)
            {
                //   var columna = 0;
                //Cuantas nominas tiene el empleado
                var nominasDelEmpleado = listaNominas.Where(x => x.IdEmpleado == itemEmpleado.IdEmpleado).ToList();
                bool ejercicioCompleto = false;

                if (idEmp != itemEmpleado.IdEmpleado)
                {
                    idEmp = itemEmpleado.IdEmpleado;
                    rowColor = !rowColor;
                    rowColor = true;
                    filaInicial = fila;
                }

                //Por cada Nomina
                #region CONTENIDO NOMINA
                foreach (var itemNomina in nominasDelEmpleado)
                {
                    ejercicioCompleto = ContenidoAcumulado(ref worksheet, itemNomina, null, itemEmpleado, listaPeriodos, listaEmpresasF, listaClientes, listaCuotas, listaDetalles, null, listaConceptosPercecpciones, listaConceptosDeducciones, incluirNoAutorizados, false, fila, null, listaContratos, ejercicioFiscal, listaIncidencias, registrosQueSonNomina, registrosQueSonFiniquito);

                    //Sumar los totales de Gravados Exentos - ISN

                    fila++;
                    //rowColor = !rowColor;
                }//Fin del For de la nominas del empleado
                #endregion


                #region CONTENIDO FINIQUITO

                //Obtener los finiquitos del empleado  
                var finiquitosEmpleado = listaFiniquitos.Where(x => x.IdEmpleado == itemEmpleado.IdEmpleado).ToList();

                foreach (var itemFiniquito in finiquitosEmpleado)
                {
                    ejercicioCompleto = ContenidoAcumulado(ref worksheet, null, itemFiniquito, itemEmpleado, listaPeriodos, listaEmpresasF, listaClientes, listaCuotas, null, listaFiniquitosDetalles, listaConceptosPercecpciones, listaConceptosDeducciones, incluirNoAutorizados, true, fila, listaDescuentoComisionFiniquito, listaContratos, ejercicioFiscal, listaIncidencias, registrosQueSonNomina, registrosQueSonFiniquito);

                    //set color al row de finiquito
                    worksheet.Range(fila, 1, fila, columnaH).Style.Fill.SetBackgroundColor(XLColor.Lavender);
                    fila++;
                }


                #endregion


                if (rowColor)
                {
                    rowColor = false;
                    worksheet.Range(fila, 1, fila, columnaH).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(fila, 1, fila, columnaH).Style.Border.TopBorderColor = XLColor.SlateGray;
                    //worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Lavender;
                    //worksheet.Range(fila, 1, fila, columna).Style.Fill.SetBackgroundColor(XLColor.Lavender);

                    //Agregamos la sumatoria
                    if (nominasDelEmpleado.Count > 0)
                    {
                        SumarColumnas(ref worksheet, filaInicial, fila - 1, columnaH - 8, ejercicioCompleto, listaPeriodos[0].IdEjercicio, nominasDelEmpleado[0].IdEmpleado, nominasDelEmpleado[nominasDelEmpleado.Count - 1].SD, nominasDelEmpleado[nominasDelEmpleado.Count - 1].SDI, nominasDelEmpleado[0].IdEmpresaFiscal, calculoanual);
                    }
                    fila++;
                    fila++;
                }

            }//Fin For por empleado

            #endregion

            #region  COLORES DISEÑO DE CELDAS

            //Establece un estilo al header
            worksheet.Range("G2:Y2").Style
                .Font.SetFontSize(13)
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.Raspberry);

            //Establece un estilo al header
            worksheet.Range("A2:F2").Style
                .Font.SetFontSize(13)
                .Font.SetBold(true)
                .Font.SetFontColor(XLColor.White)
                .Fill.SetBackgroundColor(XLColor.DarkBlue);


            #endregion

            #region AJUSTE DE LAS CELDAS AL CONTENIDO

            //Ajustar el header al contenido
            //worksheet.Column(1).AdjustToContents();          

            worksheet.Columns(1, 21).AdjustToContents();

            #endregion

            #endregion

            #region CREA EL ARCHIVO Y RETORNA EL PATH RELATIVO PARA LA DESCARGA



            //Creamos el folder para guardar el archivo
            var pathUsuario = Utils.ValidarFolderUsuario(idUsuario, pathFolder);

            string nombreArchivo = "Acumulado_.xlsx";

            if (itemEmpresa != null)
            {
                nombreArchivo = "ACUM_" + itemEmpresa.RazonSocial + "_.xlsx";
            }

            var fileName = pathUsuario + nombreArchivo;
            //Guarda el archivo
            workbook.SaveAs(fileName);
            //  return fileName;
            return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;
            #endregion
        }


        private bool ContenidoAcumulado(ref IXLWorksheet worksheet, NOM_Nomina itemNomina, NOM_Finiquito itemFiniquito, Empleado itemEmpleado,
            List<NOM_PeriodosPago> listaPeriodos,
            List<Empresa> listaEmpresasF,
            List<Cliente> listaClientes,
            List<NOM_Cuotas_IMSS> listaCuotas,
            List<NOM_Nomina_Detalle> listaDetalles,
            List<NOM_Finiquito_Detalle> listaDetallesF,
            List<C_NOM_Conceptos> listaConceptosPercecpciones,
            List<C_NOM_Conceptos> listaConceptosDeducciones,
            bool incluirNoAutorizados,
            bool isFiniquito,
            int fila,
            List<NOM_Finiquito_Descuento_Adicional> listaDescuentoComisionFiniquito,
            List<Empleado_Contrato> listaContratos,
            NOM_Ejercicio_Fiscal itemEjercicioFiscal,
            List<EmpleadoIncidencias> listaIncidencias,
            // List<Permisos> listaPermisos,
            // List<Incapacidades> listaIncapacidades,
            //List<Inasistencias> listafaltas,
            List<NOM_CFDI_Timbrado> registrosQueSonNomina,
            List<NOM_CFDI_Timbrado> registrosQueSonFiniquito
            )
        {
            #region DATOS DEL EMPLEADO
            worksheet.Cell(fila, 1).Value = isFiniquito ? itemFiniquito.IdFiniquito : itemNomina.IdNomina;
            worksheet.Cell(fila, 2).Value = itemEmpleado.IdEmpleado;
            worksheet.Cell(fila, 3).Value = itemEmpleado.APaterno;
            worksheet.Cell(fila, 4).Value = itemEmpleado.AMaterno;
            worksheet.Cell(fila, 5).Value = itemEmpleado.Nombres;
            worksheet.Cell(fila, 6).Value = itemEmpleado.RFC;


            NOM_CFDI_Timbrado auxTimbrado = isFiniquito ? registrosQueSonFiniquito.SingleOrDefault(s => s.IdFiniquito == itemFiniquito.IdFiniquito) : registrosQueSonNomina.SingleOrDefault(s => s.IdNomina == itemNomina.IdNomina);
            worksheet.Cell(fila, 7).Value = auxTimbrado != null ? auxTimbrado.FolioFiscalUUID : "";  //asignando UUID 
            worksheet.Cell(fila, 8).Value = itemEmpleado.CURP;

            #endregion

            #region DATOS NOMINA

            worksheet.Cell(fila, 9).Value = isFiniquito ? itemFiniquito.SD : itemNomina.SD;
            worksheet.Cell(fila, 10).Value = isFiniquito ? itemFiniquito.SDI : itemNomina.SDI;
            //worksheet.Cell(fila, 10).Value = itemNomina.SBC;

            //Empresa - Periodo - Cliente
            NOM_PeriodosPago itemPeriodo = null;

            if (isFiniquito)
            {
                itemPeriodo = listaPeriodos.FirstOrDefault(x => x.IdPeriodoPago == itemFiniquito.IdPeriodo);
            }
            else
            {
                itemPeriodo = listaPeriodos.FirstOrDefault(x => x.IdPeriodoPago == itemNomina.IdPeriodo);
            }


            Empresa itemEmpresa = null;

            if (itemPeriodo.IdTipoNomina <= 15) //normal
            {
                if (isFiniquito)
                {
                    itemEmpresa = listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemFiniquito.IdEmpresaFiscal);
                }
                else
                {
                    itemEmpresa = listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemNomina.IdEmpresaFiscal);
                }
            }
            else if (itemPeriodo.IdTipoNomina == 16)//Asimilado
            {
                if (isFiniquito)
                {
                    itemEmpresa = listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemFiniquito.IdEmpresaAsimilado);
                }
                else
                {
                    itemEmpresa = listaEmpresasF.FirstOrDefault(x => x.IdEmpresa == itemNomina.IdEmpresaAsimilado);
                }

            }


            Cliente itemCliente = null;

            if (isFiniquito)
            {
                itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemFiniquito.IdCliente);
            }
            else
            {
                itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemNomina.IdCliente);
            }



            // worksheet.Cell(fila, 11).Value = itemEmpresa != null ? itemEmpresa.RazonSocial : "-";
            worksheet.Cell(fila, 11).Value = itemCliente != null ? itemCliente.Nombre : "-";
            worksheet.Cell(fila, 12).Value = itemEmpresa != null ? $"{itemEmpresa.IdEmpresa} {itemEmpresa.RazonSocial}" : "-";
            worksheet.Cell(fila, 13).Value = itemPeriodo?.Fecha_Fin.ToShortDateString() ?? "null";
            worksheet.Cell(fila, 14).Value = auxTimbrado != null ? auxTimbrado.FechaCertificacion.Value.Date.ToString() : ""; //asignando  la fecha de certificacion (nombre en bd)

            worksheet.Cell(fila, 15).Value = itemPeriodo?.Bimestre ?? 0;

            if (incluirNoAutorizados)
            {
                if (itemPeriodo.Autorizado == false)
                {
                    worksheet.Cell(fila, 15).Style.Font.SetFontSize(13).Font.SetBold(true)//.Font.SetFontColor(XLColor.White)
                        .Fill.SetBackgroundColor(XLColor.Gray);
                }
            }

            worksheet.Cell(fila, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(fila, 16).Value = isFiniquito ? 0 : itemNomina.Dias_Laborados;

            //Dias Permisos - Dias Incapacidad

            #region PERMISOS - INCAPACIDADES
            int totalPermisos = 0;
            int totalIncapacidades = 0;
            int totalfaltas = 0;

            if (!isFiniquito)
            {
                var listaIncicendiasByEmpleado =
                    listaIncidencias.FirstOrDefault(x => x.idPeriodo == itemNomina.IdPeriodo && x.IdEmpleado == itemEmpleado.IdEmpleado);

                if (listaIncicendiasByEmpleado != null)
                {
                    //Faltas
                    var faltas =
                        listaIncicendiasByEmpleado.Incidencias.Where(
                                x =>
                                    x.TipoIncidencia.Trim() == "F" || x.TipoIncidencia.Trim() == "FI" ||
                                    x.TipoIncidencia.Trim() == "FJ" || x.TipoIncidencia.Trim() == "FA")
                            .Select(i => i.TipoIncidencia)
                            .ToList()
                            .Count;
                    //Permisos sin goce
                    var permisoSinG =
                        listaIncicendiasByEmpleado.Incidencias.Where(x => x.TipoIncidencia.Trim() == "PS")
                            .Select(i => i.TipoIncidencia)
                            .ToList()
                            .Count;

                    //Incapacidad


                    var incapacidadE =
                        listaIncicendiasByEmpleado.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IE")
                            .Select(i => i.TipoIncidencia)
                            .ToList()
                            .Count;

                    var incapacidadM =
                        listaIncicendiasByEmpleado.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IM")
                            .Select(i => i.TipoIncidencia)
                            .ToList()
                            .Count;

                    var incapacidadR =
                        listaIncicendiasByEmpleado.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IR")
                            .Select(i => i.TipoIncidencia)
                            .ToList()
                            .Count;

                    var incapacidadTotalDias = incapacidadE + incapacidadM + incapacidadR;


                    totalfaltas = faltas;
                    totalPermisos = permisoSinG;
                    totalIncapacidades = incapacidadTotalDias;
                }

            }

            #endregion

            worksheet.Cell(fila, 17).Value = isFiniquito ? 0 : itemNomina.TotalNomina;
            worksheet.Cell(fila, 18).Value = isFiniquito ? itemFiniquito.Total_Finiquito : 0;
            worksheet.Cell(fila, 19).Value = isFiniquito ? itemFiniquito.Total_Liquidacion : 0;
            worksheet.Cell(fila, 20).Value = isFiniquito ? itemFiniquito.TOTAL_total : itemNomina.TotalNomina;

            #region  DESCUENTOS Y COMISIONES

            decimal descuentosAdicionales = 0;
            decimal comisiones = 0;

            if (isFiniquito)
            {
                descuentosAdicionales =
                    listaDescuentoComisionFiniquito.Where(
                            x => x.IdPeriodo == itemFiniquito.IdPeriodo && x.IdConcepto == 2)
                        .Select(x => x.TotalDescuento)
                        .Sum();


                comisiones =
                    listaDescuentoComisionFiniquito.Where(
                            x => x.IdPeriodo == itemFiniquito.IdPeriodo && x.IdConcepto == 1)
                        .Select(x => x.TotalDescuento)
                        .Sum();
            }

            #endregion

            worksheet.Cell(fila, 21).Value = isFiniquito ? descuentosAdicionales : 0;
            worksheet.Cell(fila, 22).Value = isFiniquito ? comisiones : 0;

            worksheet.Cell(fila, 23).Value = totalPermisos;
            worksheet.Cell(fila, 24).Value = totalIncapacidades;
            worksheet.Cell(fila, 25).Value = totalfaltas;


            #region CONCEPTOS

            int columna = 26;
            decimal totalIsn = 0;
            #region PERCEPCIONES
            //Por cada concepto busca su detalle-- GUARDAR EL TOTAL DE GRAVADO Y EXENTO
            foreach (var itemConcepto in listaConceptosPercecpciones)
            {
                decimal total = 0;
                decimal gravado = 0;
                decimal exento = 0;
                //Busca en los detalles de la nomina por concepto y por id de nomina
                if (isFiniquito)
                {
                    //var itemDetalleF = listaDetallesF.FirstOrDefault(x => x.IdFiniquito == itemFiniquito.IdFiniquito && x.IdConcepto == itemConcepto.IdConcepto);
                    var itemDetalleF = listaDetallesF.Where(x => x.IdFiniquito == itemFiniquito.IdFiniquito && x.IdConcepto == itemConcepto.IdConcepto).ToList();

                    if (itemDetalleF != null)
                    {
                        if (itemDetalleF.Any())
                        {
                            total = itemDetalleF.Sum(x => x.Total);
                            gravado = itemDetalleF.Sum(x => x.GravadoISR);
                            exento = itemDetalleF.Sum(x => x.ExentoISR);
                            totalIsn += itemDetalleF.Sum(x => x.ImpuestoSobreNomina);
                        }
                    }
                }
                else
                {
                    //var itemDetalle = listaDetalles.FirstOrDefault(x => x.IdNomina == itemNomina.IdNomina && x.IdConcepto == itemConcepto.IdConcepto);
                    var itemDetalle = listaDetalles.Where(x => x.IdNomina == itemNomina.IdNomina && x.IdConcepto == itemConcepto.IdConcepto).ToList();

                    if (itemDetalle != null)
                    {
                        if (itemDetalle.Any())
                        {
                            total = itemDetalle.Sum(x => x.Total);
                            gravado = itemDetalle.Sum(x => x.GravadoISR);
                            exento = itemDetalle.Sum(x => x.ExentoISR);
                        }
                    }
                }

                //  43 - ISR
                //  144 - Subsidio
                //  42 - SS
                if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
                {
                    worksheet.Cell(fila, columna).Value = total;
                    columna++;
                }
                else
                {
                    worksheet.Cell(fila, columna).Value = total;
                    columna++;
                    worksheet.Cell(fila, columna).Value = gravado;
                    columna++;
                    worksheet.Cell(fila, columna).Value = exento;
                    columna++;
                    //worksheet.Cell(fila, columna).Value = isn;
                    //columna++;
                }
            }


            #endregion

            worksheet.Cell(fila, columna).Value = isFiniquito ? itemFiniquito.TotalPercepciones : itemNomina.TotalPercepciones;
            columna++;

            #region DEDUCCIONES
            //Por cada concepto busca su detalle-- GUARDAR EL TOTAL DE GRAVADO Y EXENTO
            foreach (var itemConcepto in listaConceptosDeducciones)
            {
                decimal total = 0;
                decimal gravado = 0;
                decimal exento = 0;

                if (isFiniquito)
                {
                    //var itemDetalleF = listaDetallesF.FirstOrDefault(x => x.IdFiniquito == itemFiniquito.IdFiniquito && x.IdConcepto == itemConcepto.IdConcepto);
                    var itemDetalleF = listaDetallesF.Where(x => x.IdFiniquito == itemFiniquito.IdFiniquito && x.IdConcepto == itemConcepto.IdConcepto).ToList();

                    if (itemDetalleF != null)
                    {
                        if (itemDetalleF.Any())
                        {
                            total = itemDetalleF.Sum(x => x.Total);
                            gravado = itemDetalleF.Sum(x => x.GravadoISR);
                            exento = itemDetalleF.Sum(x => x.ExentoISR);
                        }
                    }
                }
                else
                {
                    //Busca en los detalles de la nomina por concepto y por id de nomina
                    //var itemDetalle = listaDetalles.FirstOrDefault(x => x.IdNomina == itemNomina.IdNomina && x.IdConcepto == itemConcepto.IdConcepto);
                    var itemDetalle = listaDetalles.Where(x => x.IdNomina == itemNomina.IdNomina && x.IdConcepto == itemConcepto.IdConcepto).ToList();

                    if (itemDetalle != null)
                    {
                        if (itemDetalle.Any())
                        {
                            total = itemDetalle.Sum(x => x.Total);
                            gravado = itemDetalle.Sum(x => x.GravadoISR);
                            exento = itemDetalle.Sum(x => x.ExentoISR);
                        }
                    }
                }

                //  43 - ISR
                //  144 - Subsidio
                //  42 - SS
                if (itemConcepto.IdConcepto == 43 || itemConcepto.IdConcepto == 144 || itemConcepto.IdConcepto == 42)
                {
                    worksheet.Cell(fila, columna).Value = total;
                    columna++;
                }
                else
                {
                    worksheet.Cell(fila, columna).Value = total;
                    columna++;
                    worksheet.Cell(fila, columna).Value = gravado;
                    columna++;
                    worksheet.Cell(fila, columna).Value = exento;
                    columna++;
                    //worksheet.Cell(fila, columna).Value = isn;
                    //columna++;
                }
            }
            #endregion


            #endregion

            worksheet.Cell(fila, columna).Value = isFiniquito ? itemFiniquito.SubsidioCausado : itemNomina.SubsidioCausado;
            columna++;
            worksheet.Cell(fila, columna).Value = isFiniquito ? itemFiniquito.SubsidioEntregado : itemNomina.SubsidioEntregado;
            columna++;
            worksheet.Cell(fila, columna).Value = isFiniquito ? 0 : itemNomina.ISRantesSubsidio;
            columna++;
            worksheet.Cell(fila, columna).Value = isFiniquito ? itemFiniquito.TotalDeducciones : itemNomina.TotalDeducciones;
            columna++;
            worksheet.Cell(fila, columna).Value = isFiniquito ? totalIsn : itemNomina.TotalImpuestoSobreNomina;
            columna++;
            #endregion

            #region CUOTAS IMSS

            NOM_Cuotas_IMSS itemCuotaImss = null;
            if (isFiniquito)
            {
                itemCuotaImss = listaCuotas.FirstOrDefault(x => x.IdNomina == itemFiniquito.IdFiniquito);
            }
            else
            {
                itemCuotaImss = listaCuotas.FirstOrDefault(x => x.IdNomina == itemNomina.IdNomina);
            }

            decimal totalPatron = 0;
            decimal totalObrero = 0;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.TotalPatron ?? 0;
            columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.TotalObrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Cuota_Fija_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Excedente_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Excedente_Obrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.PrestacionesDinero_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.PrestacionesDinero_Obrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Pensionados_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Pensionados_Obrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.InvalidezVida_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.InvalidezVida_Obrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.GuarderiasPrestaciones_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.SeguroRetiro_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.CesantiaVejez_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = itemCuotaImss?.CesantiaVejez_Obrero ?? 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.Infonavit_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = 0; columna++;

            worksheet.Cell(fila, columna).Value = itemCuotaImss?.RiesgoTrabajo_Patron ?? 0; columna++;
            worksheet.Cell(fila, columna).Value = 0; columna++;
            #endregion

            //worksheet.Cell(fila, columna).Value = isFiniquito ? itemFiniquito.TotalComplemento : itemNomina.TotalComplemento;
            //worksheet.Cell(fila, columna).Style.NumberFormat.Format = "$ ###,##0.00";

            //Buscamos su ultimo contrato en el ejercicio
            bool añoCompleto = false;
            var itemContrato =
                listaContratos.OrderByDescending(x => x.FechaAlta)
                    .FirstOrDefault(x => x.IdEmpleado == itemEmpleado.IdEmpleado);

            if (itemContrato != null)
            {
                bool alta = false;
                bool baja = false;
                var fechaAlta = itemContrato.FechaIMSS;
                var fechaBaja = itemContrato.FechaBaja;
                var fechaEjercicio = itemEjercicioFiscal.Anio;

                int añoFiscal = Int32.Parse(fechaEjercicio);

                DateTime inicioEjercicio = new DateTime(añoFiscal, 01, 01);
                DateTime finEjercicio = new DateTime(añoFiscal, 12, 15);


                if (fechaAlta <= inicioEjercicio)
                {
                    alta = true;
                }


                if (fechaBaja == null)
                {
                    baja = true;
                }
                else
                {
                    if (fechaBaja >= finEjercicio)
                    {
                        baja = true;
                    }
                }


                if (alta && baja)
                {
                    añoCompleto = true;
                }

            }

            // worksheet.Cell(fila, columna + 1).Value = añoCompleto ? "x" : "";

            worksheet.Cell(fila, columna + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            return añoCompleto;
        }


        private void SumarColumnas(ref IXLWorksheet worksheet, int filaInicial, int filaFinal, int columna, bool ejercicioCompleto, int idejercicio, int idempleado, decimal sd, decimal sdi, int? idempresa, bool calculoanual)
        {
            int filaDelTotal = filaFinal + 1;

            string[] letra = new[]
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
                "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM",
                "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD",
                "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU",
                "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL",
                "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ"
            };

            for (int c = 2; c < columna - 1; c++)
            {

                if (c < 15)
                {
                    if (c == 12 || c == 13 || c == 7 || c == 14) continue;

                    var contenido = worksheet.Cell(filaDelTotal - 1, c).Value;
                    worksheet.Cell(filaDelTotal, c).Value = contenido;
                }
                else
                {
                    string formula = $"=SUM({letra[c]}{filaInicial}:{letra[c]}{filaFinal})";
                    worksheet.Cell(filaDelTotal, c + 1).FormulaA1 = formula;
                }


                worksheet.Range(filaDelTotal, 1, filaDelTotal, columna).Style.Fill.SetBackgroundColor(XLColor.FromArgb(235, 241, 222));
                worksheet.Range(filaDelTotal, 1, filaDelTotal, columna).Style.Font.SetBold(true);
                worksheet.Range(filaDelTotal, columna, filaDelTotal, columna + 8).Style.Fill.SetBackgroundColor(XLColor.LightBlue);
                worksheet.Range(filaDelTotal, columna, filaDelTotal, columna + 8).Style.Font.SetBold(true);

                //worksheet.Cell("B9").SetFormulaA1("=SUM(B3:B8)");
            }

            if (ejercicioCompleto)
            {
                worksheet.Cell(filaDelTotal, columna).Value = "x";
                worksheet.Cell(filaDelTotal, columna).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                if (calculoanual)
                {
                    //var calculoAnual = MNominas.GetCalculoAnualconPrevision(idempleado, idejercicio, sd, sdi, idempresa, 0);
                    //worksheet.Cell(filaDelTotal, columna + 1).Value = calculoAnual.BaseGravable;
                    //worksheet.Cell(filaDelTotal, columna + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 2).Value = calculoAnual.LimiteInferior;
                    //worksheet.Cell(filaDelTotal, columna + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 3).Value = calculoAnual.Base;
                    //worksheet.Cell(filaDelTotal, columna + 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 4).Value = calculoAnual.Tasa;
                    //worksheet.Cell(filaDelTotal, columna + 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 5).Value = calculoAnual.marginal;// Se usa esta propiedad para tomar el impuesto marginal
                    //worksheet.Cell(filaDelTotal, columna + 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 6).Value = calculoAnual.Cuotafija;// Se usa esta propiedad para tomar la cuota fija
                    //worksheet.Cell(filaDelTotal, columna + 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 7).Value = calculoAnual.ResultadoIsrOSubsidio > 0 ? calculoAnual.ResultadoIsrOSubsidio : 0;
                    //worksheet.Cell(filaDelTotal, columna + 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    //worksheet.Cell(filaDelTotal, columna + 8).Value = calculoAnual.ResultadoIsrOSubsidio < 0 ? calculoAnual.ResultadoIsrOSubsidio : 0;
                    //worksheet.Cell(filaDelTotal, columna + 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

            }

        }

        #endregion



        public static string GenerarReporteNominas(DateTime? fechaInicial, DateTime? fechaFinal, int idEjercicio, int idEmpresa, int sucursal, int idUsuario, string pathFolder, string pathDescarga, int idEmpleado = 0)
        {

            // obtener datos del empleado
            // obtener contrato
            _Incidencias inc = new _Incidencias();
            List<ReporteLucero> listaReporte = new List<ReporteLucero>();
            using (var context = new RHEntities())
            {
                // obtener periodos en el rango de fecha
                var ListPeriodos = (from p in context.NOM_PeriodosPago
                                    where p.Fecha_Fin >= DbFunctions.TruncateTime(fechaInicial) && p.Fecha_Fin <= DbFunctions.TruncateTime(fechaFinal)
                                          && (p.IdTipoNomina <= 16 && p.IdTipoNomina != 11)
                                          && p.SoloComplemento == false
                                    select p).ToList();


                var listaIdPeriodos = ListPeriodos.Select(x => x.IdPeriodoPago).ToList();


                //obtener la nominas del periodo
                // agregar el filtrado por empleado
                var listaNominas = (from n in context.NOM_Nomina
                                    where listaIdPeriodos.Contains(n.IdPeriodo)
                                    && n.IdEmpresaFiscal == idEmpresa
                                    // && n.IdSucursal == sucursal
                                    select new { n.IdNomina, n.IdCliente, n.IdSucursal, n.IdEjercicio, n.IdPeriodo, n.IdEmpleado, n.IdContrato, n.SD, n.SDI, n.SDReal, n.IdEmpresaFiscal, n.IdEmpresaComplemento }).ToList();

                if (sucursal > 0)
                    listaNominas = listaNominas.Where(x => x.IdSucursal == sucursal).ToList();

                //Reducimos el numero de periodos segun las nominas
                listaIdPeriodos = listaNominas.Select(x => x.IdPeriodo).Distinct().ToList();


                var listaIncidencias = (from i in context.NOM_Nomina_Incidencias
                                        where listaIdPeriodos.Contains(i.IdPeriodo)
                                        select i).ToList();


                var arrIdNominas = listaNominas.Select(x => x.IdNomina).Distinct().ToArray();

                int[] arrIdConceptos = { 16, 43, 144, 51 }; // 

                var listaDetalleNomina = (from d in context.NOM_Nomina_Detalle
                                          where arrIdNominas.Contains(d.IdNomina)
                                          && arrIdConceptos.Contains(d.IdConcepto)
                                          select d).ToList();

                var listaIdEmpleados = listaNominas.Select(x => x.IdEmpleado).Distinct().ToList();
                var arrIdContratos = listaNominas.Select(x => x.IdContrato).Distinct().ToList();

                // lista de empleados
                var listaEmpleados = (from e in context.Empleado
                                      where listaIdEmpleados.Contains(e.IdEmpleado)
                                      select new { e.IdEmpleado, e.Nombres, e.APaterno, e.AMaterno, e.RFC, e.CURP, e.NSS, }).ToList();


                var listaContratos = (from c in context.Empleado_Contrato
                                      where arrIdContratos.Contains(c.IdContrato)
                                      select new { c.IdContrato, c.IdEmpleado, c.FechaAlta, c.FechaBaja, c.FechaReal, c.FechaIMSS, c.Status, c.IsReingreso, c.IdPuesto }).ToList();

                var listaInfonavit = (from i in context.Empleado_Infonavit
                                      where arrIdContratos.Contains(i.IdEmpleadoContrato)
                                      select i).ToList();


                // clientes
                var listaClientes = (from c in context.Cliente
                                     select new { c.IdCliente, c.Nombre }).ToList();

                // sucursales
                var listaSucursales = (from s in context.Sucursal
                                       select new { s.IdSucursal, s.IdCliente, s.Ciudad }).ToList();

                // empresas
                var listaEmpresas = (from e in context.Empresa
                                     select new { e.IdEmpresa, e.RazonSocial }).ToList();

                // puestos
                var listaPuestos = (from p in context.Puesto
                                    select new { p.IdPuesto, p.Descripcion }).ToList();

                // Departamento
                var listaDepartamento = (from d in context.Departamento
                                         select new { d.IdDepartamento, d.Descripcion }).ToList();

                //List<EmpleadoIncidencias> listaIncidencias = new List<EmpleadoIncidencias>();
                //foreach (var itemP in ListPeriodos)
                //{
                //    var arrayEmp = listaNominas.Select(x => x.IdEmpleado).ToArray();

                //    var listaInc = inc.GetIncidenciasByPeriodo2(itemP, arrayEmp);
                //    listaIncidencias.AddRange(listaInc);
                //}

                // Inicio proceso          
                foreach (var itemN in listaNominas)
                {
                    ReporteLucero itemReporte = new ReporteLucero();

                    string empresa = "";
                    var itemE = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == itemN.IdEmpresaFiscal);
                    if (itemE == null)
                    {//si no tiene empresa fiscal entonces tomamos la de complemento
                        itemE = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == itemN.IdEmpresaComplemento);
                    }

                    var itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemN.IdCliente);
                    var itemSucursal = listaSucursales.FirstOrDefault(x => x.IdSucursal == itemN.IdSucursal);
                    var itemPeriodo = ListPeriodos.FirstOrDefault(x => x.IdPeriodoPago == itemN.IdPeriodo);
                    var itemEmpleado = listaEmpleados.FirstOrDefault(x => x.IdEmpleado == itemN.IdEmpleado);
                    var itemContrato = listaContratos.FirstOrDefault(x => x.IdContrato == itemN.IdContrato);
                    var itemPuesto = listaPuestos.FirstOrDefault(x => x.IdPuesto == itemContrato.IdPuesto);
                    var itemDep = listaDepartamento.FirstOrDefault(x => x.IdDepartamento == itemCliente.IdCliente);


                    itemReporte.Empresa = itemE.RazonSocial == null ? "" : itemE.RazonSocial;
                    itemReporte.Cliente = $"{itemCliente.Nombre} | {itemSucursal.Ciudad}";
                    itemReporte.IdPeriodo = itemPeriodo.IdPeriodoPago;
                    itemReporte.PeriodoDes = itemPeriodo.Descripcion;
                    itemReporte.IdNomina = itemN.IdNomina;
                    itemReporte.Nss = itemEmpleado.NSS;
                    itemReporte.NombreEmpleado = $"{itemEmpleado.APaterno} {itemEmpleado.AMaterno} {itemEmpleado.Nombres}";
                    itemReporte.Rfc = itemEmpleado.RFC;
                    itemReporte.Departamento = itemDep != null ? itemDep.Descripcion : "-";
                    itemReporte.Puesto = itemPuesto != null ? itemPuesto.Descripcion : "-";
                    itemReporte.FechaImss = itemContrato.FechaIMSS.Value.ToString("dd/MM/yyy");
                    itemReporte.FechaReal = itemContrato.FechaReal.ToString("dd/MM/yyyy");
                    itemReporte.SD = itemN.SD;
                    itemReporte.SDI = itemN.SDI;
                    itemReporte.SDReal = itemN.SDReal;


                    //var inicidenciaTuple = Incidencias(listaIncidencias, itemN.IdEmpleado, itemN.IdPeriodo);
                    var itemIncidencia = listaIncidencias.Where(x => x.IdPeriodo == itemN.IdPeriodo && x.IdEmpleado == itemN.IdEmpleado).FirstOrDefault();

                    if (itemIncidencia != null)
                    {
                        itemReporte.FaltasInjustificadas = itemIncidencia.FI ?? 0;
                        itemReporte.FaltasJustificadas = itemIncidencia.FJ ?? 0;
                        itemReporte.Ausentismo = itemIncidencia.Au ?? 0;
                        itemReporte.IncapacidadE = itemIncidencia.IE ?? 0;
                        itemReporte.FechaIncapacidadE = itemIncidencia.IE_Fecha ?? "";
                        itemReporte.IncapacidadM = itemIncidencia.IM ?? 0;
                        itemReporte.FechaIncapacidadM = itemIncidencia.IM_Fecha ?? "";
                        itemReporte.IncapacidadR = itemIncidencia.IR ?? 0;
                        itemReporte.FechaIncapacidadR = itemIncidencia.IR_Fecha ?? "";
                        itemReporte.Vacaciones = itemIncidencia.Vacaciones ?? 0;
                    }

                    itemReporte.PrimaVacacional = GetTotalConcepto(listaDetalleNomina, 16, itemN.IdNomina);

                    var itemDInfonavit = listaDetalleNomina.FirstOrDefault(x => x.IdConcepto == 51 && x.IdNomina == itemN.IdNomina);

                    itemReporte.TipoCredInfonavit = "";
                    itemReporte.NoCredidoInfonavit = "";
                    itemReporte.FechaInicioInfonavit = "";

                    if (itemDInfonavit != null)
                    {
                        var tpInfonavit = GetDatoInfonavit(listaInfonavit, itemN.IdContrato, itemDInfonavit.IdPrestamo);
                        if (tpInfonavit != null)
                        {
                            itemReporte.TipoCredInfonavit = tpInfonavit.Item1;
                            itemReporte.NoCredidoInfonavit = tpInfonavit.Item2;
                            itemReporte.FechaInicioInfonavit = tpInfonavit.Item3;
                        }
                    }

                    itemReporte.Isr = GetTotalConcepto(listaDetalleNomina, 43, itemN.IdNomina);
                    itemReporte.Subsidio = GetTotalConcepto(listaDetalleNomina, 144, itemN.IdNomina);

                    listaReporte.Add(itemReporte);
                }//<- for

            }//<- using

            // Creamos el excel
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Incidencias");

            worksheet.Cell(1, 4).Value = "Reporte de Incidencias";
            worksheet.Cell(1, 4).Style.Font.SetFontSize(14).Font.SetBold(true);



            // HEADER
            worksheet.Cell(3, 1).Value = "Empresa";
            worksheet.Cell(3, 2).Value = "Cliente";
            worksheet.Cell(3, 3).Value = "Id | Periodo";
            worksheet.Cell(3, 4).Value = "Nombre Empleado";
            worksheet.Cell(3, 5).Value = "Nss";

            worksheet.Cell(3, 6).Value = "Rfc";
            worksheet.Cell(3, 7).Value = "Departamento";
            worksheet.Cell(3, 8).Value = "Puesto";
            worksheet.Cell(3, 9).Value = "Fecha Imss";
            worksheet.Cell(3, 10).Value = "Fecha Real";
            worksheet.Cell(3, 11).Value = "SD";
            worksheet.Cell(3, 12).Value = "SDI";
            worksheet.Cell(3, 13).Value = "SD Real";
            worksheet.Cell(3, 14).Value = "Faltas Injustificadas";
            worksheet.Cell(3, 15).Value = "Faltas Justificadas";
            worksheet.Cell(3, 16).Value = "Ausentismo";
            worksheet.Cell(3, 17).Value = "Vacaciones";
            worksheet.Cell(3, 18).Value = "Prima Vacacional";
            worksheet.Cell(3, 19).Value = "Incapacidades";
            worksheet.Cell(3, 20).Value = "IE";
            worksheet.Cell(3, 21).Value = "Fecha IE";
            worksheet.Cell(3, 22).Value = "IM";
            worksheet.Cell(3, 23).Value = "Fecha IM";
            worksheet.Cell(3, 24).Value = "IR";
            worksheet.Cell(3, 25).Value = "Fecha IR";
            worksheet.Cell(3, 26).Value = "Folio Inc";
            worksheet.Cell(3, 27).Value = "Tipo Infonavit";
            worksheet.Cell(3, 28).Value = "No Credido info";
            worksheet.Cell(3, 29).Value = "Fecha Inicio info";
            worksheet.Cell(3, 30).Value = "Isr";
            worksheet.Cell(3, 31).Value = "Subsidio";

            //CONTENIDO
            int row = 4;
            listaReporte = listaReporte.OrderBy(x => x.NombreEmpleado).ToList();

            foreach (var item in listaReporte)
            {
                worksheet.Cell(row, 1).Value = item.Empresa;
                worksheet.Cell(row, 2).Value = item.Cliente;
                worksheet.Cell(row, 3).Value = $"{item.IdPeriodo} | {item.PeriodoDes}";
                worksheet.Cell(row, 4).Value = item.NombreEmpleado;
                worksheet.Cell(row, 5).Value = item.Nss;

                worksheet.Cell(row, 6).Value = item.Rfc;
                worksheet.Cell(row, 7).Value = item.Departamento;
                worksheet.Cell(row, 8).Value = item.Puesto;
                worksheet.Cell(row, 9).Value = item.FechaImss;
                worksheet.Cell(row, 10).Value = item.FechaReal;
                worksheet.Cell(row, 11).Value = item.SD;
                worksheet.Cell(row, 12).Value = item.SDI;
                worksheet.Cell(row, 13).Value = item.SDReal;
                worksheet.Cell(row, 14).Value = item.FaltasInjustificadas > 0 ? item.FaltasInjustificadas.ToString() : "";
                worksheet.Cell(row, 15).Value = item.FaltasJustificadas > 0 ? item.FaltasJustificadas.ToString() : "";
                worksheet.Cell(row, 16).Value = item.Ausentismo > 0 ? item.Ausentismo.ToString() : "";
                worksheet.Cell(row, 17).Value = item.Vacaciones > 0 ? item.Vacaciones.ToString() : "";
                worksheet.Cell(row, 18).Value = item.PrimaVacacional > 0 ? item.PrimaVacacional.ToString() : "";

                var formula = $"=SUM(T{row} + V{row} + X{row})";
                worksheet.Cell(row, 19).FormulaA1 = formula;
                worksheet.Cell(row, 20).Value = item.IncapacidadE > 0 ? item.IncapacidadE.ToString() : "";
                worksheet.Cell(row, 21).Value = item.FechaIncapacidadE;
                worksheet.Cell(row, 22).Value = item.IncapacidadM > 0 ? item.IncapacidadM.ToString() : "";
                worksheet.Cell(row, 23).Value = item.FechaIncapacidadM;
                worksheet.Cell(row, 24).Value = item.IncapacidadR > 0 ? item.IncapacidadR.ToString() : "";
                worksheet.Cell(row, 25).Value = item.FechaIncapacidadR;
                worksheet.Cell(row, 26).Value = "Folio Inc";//item.FolioIncapacidad;
                worksheet.Cell(row, 27).Value = item.TipoCredInfonavit;
                worksheet.Cell(row, 28).Value = item.NoCredidoInfonavit;
                worksheet.Cell(row, 29).Value = item.FechaInicioInfonavit;
                worksheet.Cell(row, 30).Value = item.Isr;
                worksheet.Cell(row, 31).Value = item.Subsidio;
                row++;
            }



            #region DISEÑO
            //Fix
            worksheet.SheetView.Freeze(3, 1);

            //Color
            worksheet.Range("A3:AE3").Style
               .Font.SetFontSize(10)
               .Font.SetBold(true)
               .Font.SetFontColor(XLColor.Black)
               .Fill.SetBackgroundColor(XLColor.LightGray);


            worksheet.Range($"A4:AE{row}").Style.Font.SetFontSize(8);

            //Ajuste de columnas
            // worksheet.Column(1).AdjustToContents();
            worksheet.Columns(2, 32).AdjustToContents();

            //worksheet.RangeUsed().SetAutoFilter();
            //worksheet.Range("A3:AD3").SetAutoFilter();
            #endregion


            //Creamos el folder para guardar el archivo
            var pathUsuario = Utils.ValidarFolderUsuario(idUsuario, pathFolder);

            string nombreArchivo = "ReporteNominas_Incidencias.xlsx";

            var fileName = pathUsuario + nombreArchivo;
            //Guarda el archivo

            workbook.SaveAs(fileName, false);

            return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;
        }

        private static Tuple<Tuple<int, int, int>, Tuple<int, string, int, string, int, string>, int> Incidencias(List<EmpleadoIncidencias> listaIncidencias, int idEmpleado, int idPeriodo)
        {
            // itemReporte.FaltasInjustificadas =
            //itemReporte.FaltasJustificadas =
            //itemReporte.Ausentismo =

            //itemReporte.Incapacidad =
            //itemReporte.TipoIncapacidad =
            //itemReporte.FechaIncapacidad =
            //itemReporte.DiasIncapacidad =
            //itemReporte.Vacaciones =

            var diasPagoItem = listaIncidencias.Where(x => x.IdEmpleado == idEmpleado && x.idPeriodo == idPeriodo).FirstOrDefault();

            if (diasPagoItem == null) return null;

            //Faltas
            var faltasIn =
                diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "F" || x.TipoIncidencia.Trim() == "FI").Select(i => i.TipoIncidencia).ToList().Count;



            var faltasJus =
               diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "FJ").Select(i => i.TipoIncidencia).ToList().Count;


            var ausentismos =
               diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "FA").Select(i => i.TipoIncidencia).ToList().Count;

            var tpFaltas = Tuple.Create(faltasIn, faltasJus, ausentismos);


            //Incapacidades

            //IE
            var incapacidadE = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IE").Select(i => i.TipoIncidencia).ToList().Count;
            var fechasE = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IE").Select(i => i.Fecha).ToList();
            var fechaEstring = GetFechasString(fechasE);

            //IM
            var incapacidadM = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IM").Select(i => i.TipoIncidencia).ToList().Count;
            var fechasM = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IM").Select(i => i.Fecha).ToList();
            var fechaMstring = GetFechasString(fechasM);


            // IR
            var incapacidadR = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IR").Select(i => i.TipoIncidencia).ToList().Count;
            var fechasR = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "IR").Select(i => i.Fecha).ToList();
            var fechaRstring = GetFechasString(fechasR);


            var incapacidadTotalDias = incapacidadE + incapacidadM + incapacidadR;

            var tpIncapacidad = Tuple.Create(incapacidadE, fechaEstring, incapacidadM, fechaMstring, incapacidadR, fechaRstring);


            //Vacaciones
            var vacacionesDelPeriodo = diasPagoItem.Incidencias.Where(x => x.TipoIncidencia.Trim() == "V")
                 .Select(i => i.TipoIncidencia)
                 .ToList()
                 .Count;


            //agregar los datos a una tupla
            var resultado = Tuple.Create(tpFaltas, tpIncapacidad, vacacionesDelPeriodo);

            return resultado;

        }

        private static string GetFechasString(List<DateTime> listaFecha)
        {
            if (!listaFecha.Any())
            {
                return "";
            }


            if (listaFecha.Count == 1)
            {
                return listaFecha[0].ToString("dd/MM/yyyy");
            }


            return $"{listaFecha[0].ToString("dd/MM/yyyy")} - {listaFecha[listaFecha.Count - 1].ToString("dd/MM/yyyy")}";
        }

        private static decimal GetTotalConcepto(List<NOM_Nomina_Detalle> listaDetalles, int idConcepto, int idNomina)
        {
            if (!listaDetalles.Any())
                return 0;

            var r = listaDetalles.Where(x => x.IdConcepto == idConcepto && x.IdNomina == idNomina).FirstOrDefault();

            if (r == null)
                return 0;

            return r.Total;


        }

        private static Tuple<string, string, string> GetDatoInfonavit(List<Empleado_Infonavit> listaInf, int idcontrato, int id)
        {
            var r = listaInf.Where(x => x.Id == id && x.IdEmpleadoContrato == idcontrato).FirstOrDefault();

            if (r == null) return null;

            var tipo = "";

            switch (r.TipoCredito)
            {
                case 1:
                    tipo = "C.F.";
                    break;
                case 2:
                    tipo = "Porcentaje%";
                    break;
                case 3:
                    tipo = "V.S.M.";
                    break;
                case 4:
                    tipo = "V.S.M./UMA";
                    break;
            }

            var tpInfo = Tuple.Create(tipo, r.NumCredito, r.FechaInicio.ToString("dd/MM/yyyy"));

            return tpInfo;

        }

        public static Task GuardaIncidenciasEnTabla()
        {

            Task tt = Task.Factory.StartNew(() =>
            {
                _Incidencias inc = new _Incidencias();
                using (var context = new RHEntities())
                {
                    int pagina = 0;
                    int cantidaPorPagina = 50;

                    //obtener el ultimo Id Periodo registrado
                    var ultiperiodo = context.NOM_Nomina_Incidencias.OrderByDescending(x => x.IdPeriodo).FirstOrDefault();

                    int idUltimoPeriodo = ultiperiodo == null ? 0 : ultiperiodo.IdPeriodo;

                    if (idUltimoPeriodo > 0)
                    {
                        idUltimoPeriodo = idUltimoPeriodo - 3;
                        // borramos los datos de los ultimos 3 periodos 
                        string sqlQuery1 = $"delete [NOM_Nomina_Incidencias] where IdPeriodo > {idUltimoPeriodo}";
                        context.Database.ExecuteSqlCommand(sqlQuery1);
                    }

                    //consulatmos el id                
                    var numPeriodo = context.NOM_PeriodosPago.Where(x => x.IdPeriodoPago > idUltimoPeriodo).Select(x => x.IdPeriodoPago).Count();

                    var cont = 1;

                    if (numPeriodo > 99)
                    {
                        cont = numPeriodo / 50;
                    }             
                                


                    for (int i = 0; i <= cont; i++)
                    {
                        #region Procesado

                        #region Lista de periodos 100 y sus nominas
                        var listaPeriodos = context.NOM_PeriodosPago.Where(x => x.IdPeriodoPago > idUltimoPeriodo).OrderBy(x => x.IdPeriodoPago).Skip(cantidaPorPagina * pagina).Take(cantidaPorPagina).ToList();

                        pagina++;

                        var arrIdPeriodos = listaPeriodos.Select(x => x.IdPeriodoPago).Distinct().ToArray();

                        var listaNominas = (from n in context.NOM_Nomina
                                            where arrIdPeriodos.Contains(n.IdPeriodo)
                                            select new { n.IdNomina, n.IdPeriodo, n.IdEmpleado, n.IdContrato }).ToList();

                        #endregion


                        #region Cargar las incidencias de los 100 periodos

                        List<EmpleadoIncidencias> listaIncidencias = new List<EmpleadoIncidencias>();

                        foreach (var itemP in listaPeriodos)
                        {
                            var arrayEmp = listaNominas.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).Select(x => x.IdEmpleado).ToArray();

                            if (arrayEmp.Length <= 0) continue;

                            var listaInc = inc.GetIncidenciasByPeriodo2(itemP, arrayEmp);

                            if (!listaInc.Any()) continue;

                            listaIncidencias.AddRange(listaInc);

                        }

                        #endregion


                        if (!listaIncidencias.Any()) continue;

                        //Genera los item para guardarlos en bd
                        foreach (var itemP in listaPeriodos)
                        {

                            #region GUARDA LOS DATOS POR PERIODO Y NOMINA
                            var arrayNom = listaNominas.Where(x => x.IdPeriodo == itemP.IdPeriodoPago).ToList();

                            List<NOM_Nomina_Incidencias> listaNominasIncidencias = new List<NOM_Nomina_Incidencias>();

                            foreach (var itemn in arrayNom)
                            {
                                // obtenemos los datos de incidencia
                                var tp = Incidencias(listaIncidencias, itemn.IdEmpleado, itemn.IdPeriodo);

                                if (tp == null) continue;

                                if (tp.Item1.Item1 == 0 && tp.Item1.Item2 == 0 && tp.Item1.Item3 == 0 && tp.Item2.Item1 == 0 && tp.Item2.Item3 == 0 && tp.Item2.Item5 == 0 && tp.Item3 == 0)
                                    continue;

                                NOM_Nomina_Incidencias item = new NOM_Nomina_Incidencias();
                                item.IdPeriodo = itemn.IdPeriodo;
                                item.IdEmpleado = itemn.IdEmpleado;

                                if (tp.Item1.Item1 > 0)
                                    item.FI = tp.Item1.Item1;
                                item.FI_Fecha = null;

                                if (tp.Item1.Item2 > 0)
                                    item.FJ = tp.Item1.Item2;
                                item.FJ_Fecha = null;

                                if (tp.Item1.Item3 > 0)
                                    item.Au = tp.Item1.Item3;
                                item.Au_Fecha = null;

                                if (tp.Item2.Item1 > 0)
                                {
                                    item.IE = tp.Item2.Item1;
                                    item.IE_Fecha = tp.Item2.Item2;
                                }

                                if (tp.Item2.Item3 > 0)
                                {
                                    item.IM = tp.Item2.Item3;
                                    item.IM_Fecha = tp.Item2.Item4;
                                }

                                if (tp.Item2.Item5 > 0)
                                {
                                    item.IR = tp.Item2.Item5;
                                    item.IR_Fecha = tp.Item2.Item6;
                                }

                                if (tp.Item3 > 0)
                                {
                                    item.Vacaciones = tp.Item3;
                                    item.Vacaciones_Fecha = "FV";
                                }
                                listaNominasIncidencias.Add(item);
                            }

                            context.NOM_Nomina_Incidencias.AddRange(listaNominasIncidencias);
                            context.SaveChanges();

                            System.Threading.Thread.Sleep(1000);
                            #endregion


                        }






                        #endregion
                    }
                }

            });

            return tt;
        }

        public class EmpleadosClientes
        {
            public int IdEmpleado { get; set; }
            public int IdContrato { get; set; }
            public bool EsReingreso { get; set; }
            public string Nombre { get; set; }
            public decimal Sd { get; set; }
            public decimal Sdi { get; set; }
            public decimal SReal { get; set; }

            public string FechaAlta { get; set; }
            public string AltaImss { get; set; }
            public string Puesto { get; set; }
            public string Departamento { get; set; }
            public string Nss { get; set; }
            public string Rfc { get; set; }
            public int ValidSat { get; set; }
        }

        //public class calculoanual:NOM_CalculoAnual
        //{
        //    public string nombre { get; set; }
        //    public string cliente { get; set; }
        //}

        private class ReporteLucero
        {
            public ReporteLucero()
            {
                FaltasInjustificadas = 0;
                FaltasJustificadas = 0;
                Ausentismo = 0;
                Vacaciones = 0;
                IncapacidadE = 0;
                IncapacidadM = 0;
                IncapacidadR = 0;
                PrimaVacacional = 0;
            }

            public string Empresa { get; set; }
            public string Cliente { get; set; }
            public int IdPeriodo { get; set; }
            public string PeriodoDes { get; set; }
            public int IdNomina { get; set; }

            public string Nss { get; set; }
            public string NombreEmpleado { get; set; }
            public string Rfc { get; set; }
            public string Departamento { get; set; }
            public string Puesto { get; set; }
            public string FechaImss { get; set; }
            public string FechaReal { get; set; }
            public decimal SD { get; set; }
            public decimal SDI { get; set; }
            public decimal SDReal { get; set; }
            public int FaltasInjustificadas { get; set; }
            public int FaltasJustificadas { get; set; }
            public int Ausentismo { get; set; }
            public int Vacaciones { get; set; }
            public decimal PrimaVacacional { get; set; }
            public int IncapacidadE { get; set; }
            public string FechaIncapacidadE { get; set; }

            public int IncapacidadM { get; set; }
            public string FechaIncapacidadM { get; set; }

            public int IncapacidadR { get; set; }
            public string FechaIncapacidadR { get; set; }

            public string TipoCredInfonavit { get; set; }
            public string NoCredidoInfonavit { get; set; }
            public string FechaInicioInfonavit { get; set; }
            public decimal Isr { get; set; }
            public decimal Subsidio { get; set; }


        }
    }
}
