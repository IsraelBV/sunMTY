using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RH.Entidades;
using System.Diagnostics;
using System.Web;
using Excel;
using System.Data;
using Common.Utils;
using ClosedXML;
using ClosedXML.Excel;

namespace RH.BLL
{
    public class ImportacionMasivaEmpleados
    {
        public int FormarPlantilla(string fileName, int cliente, int IdSucursal, string nombreCliente)
        {
            using (var wb = new XLWorkbook(fileName))
            {
                var wsMain = wb.Worksheet(1);
                var wsData = wb.Worksheet(2);

                var ps = getPuestos(wsData, cliente);
                insertValidation(ps, 23, wsMain);

                var pp = getPerioPago(wsData, cliente);
                insertValidation(pp, 26, wsMain);

                var mp = getMetodoPago(wsData, cliente);
                insertValidation(mp, 27, wsMain);

                var tj = getTipoJornada(wsData, cliente);
                insertValidation(tj, 42, wsMain);

                var ens = getEntidadServicio(wsData, cliente);
                insertValidation(ens, 44, wsMain);

                var si = getSindicalizado(wsData, cliente);
                insertValidation(si, 45, wsMain);

                var bancos = getBancos(wsData);
                insertValidation(bancos, 41, wsMain);

                var ef = getEmpresas(wsData, IdSucursal, 1, "Fiscales", 4);
                insertValidation(ef, 32, wsMain);

                var ec = getEmpresas(wsData, IdSucursal, 2, "Complemento", 5);
                insertValidation(ec, 33, wsMain);

                var es = getEmpresas(wsData, IdSucursal, 3, "Sindicato", 6);
                insertValidation(es, 34, wsMain);

                var ea = getEmpresas(wsData, IdSucursal, 4, "Asimilado", 7);
                insertValidation(ea, 35, wsMain);

                var pa = GetParentezco(wsData, 13);
                insertValidation(pa,49,wsMain);


                wb.Properties.Keywords = nombreCliente;

                wb.SaveAs(fileName);
            }
            return 1;
        }

        private void insertValidation(IXLRange range, int column, IXLWorksheet ws)
        {
            if (range != null)
            {
                for (int row = 2; row < 102; row++)
                {
                    ws.Cell(row, column).DataValidation.List(range);
                }
            }
        }

        private IXLRange getPuestos(IXLWorksheet ws, int cliente)
        {
            Puestos ctx_puestos = new Puestos();
            var puestos = ctx_puestos.GetPuestosLista(cliente);
            if (puestos.Count > 0)
            {
                ws.Cell(1, 2).Value = "Puestos";
                var rangePuestos = ws.Cell(2, 2).InsertData(puestos);
                return rangePuestos;
            }
            else
                return null;
        }
        private IXLRange getSindicalizado(IXLWorksheet ws, int cliente)
        {
            ws.Cell(1, 12).Value = "Sindicalizado";
            List<string> respuesta = new List<string>();
            respuesta.Add("SI");
            respuesta.Add("NO");
            var rangePuestos = ws.Cell(2, 12).InsertData(respuesta);

            return rangePuestos;


        }
        //periodicidad de pago
        private IXLRange getPerioPago(IXLWorksheet ws, int cliente)
        {
            var lista_PeriodicidadP = Cat_Sat.GetPeriodicidadPagos();


            if (lista_PeriodicidadP.Count > 0)
            {
                var lista_PeriodicidadPagos = lista_PeriodicidadP.Select(x => x.Descripcion).ToList();
                ws.Cell(1, 8).Value = "Periodicidad de pago";
                var rangePuestos = ws.Cell(2, 8).InsertData(lista_PeriodicidadPagos);
                return rangePuestos;
            }
            else
                return null;
        }


        //método de pago 
        private IXLRange getMetodoPago(IXLWorksheet ws, int cliente)
        {
            var lista_MetodoPago = Cat_Sat.GetMetodosPago();

            if (lista_MetodoPago.Count > 0)
            {
                var lista_MetodoPagos = lista_MetodoPago.Select(x => x.Descripcion).ToList();
                ws.Cell(1, 9).Value = "Método Pago";
                var rangePuestos = ws.Cell(2, 9).InsertData(lista_MetodoPagos);
                return rangePuestos;
            }
            else
                return null;
        }

        //tipo de jornada 
        private IXLRange getTipoJornada(IXLWorksheet ws, int cliente)
        {
            var lista_TipoJornada = Cat_Sat.GetTiposJornada();
            if (lista_TipoJornada.Count > 0)
            {
                var lista_TipoJornadas = lista_TipoJornada.Select(x => x.Descripcion).ToList();
                ws.Cell(1, 10).Value = "Tipo de Jornada";
                var rangePuestos = ws.Cell(2, 10).InsertData(lista_TipoJornadas);
                return rangePuestos;
            }
            else
                return null;
        }

        //
        //entidad de servicio 
        private IXLRange getEntidadServicio(IXLWorksheet ws, int cliente)
        {

            var edos = new Estados();
            var listaEntidadServicio = edos.GetEstados();
            if (listaEntidadServicio.Count > 0)
            {
                var lista_EntidadServicios = listaEntidadServicio.Select(x => x.ClaveEstado).ToList();
                ws.Cell(1, 11).Value = "Entidad de Servicio";
                var rangePuestos = ws.Cell(2, 11).InsertData(lista_EntidadServicios);
                return rangePuestos;
            }
            else
                return null;
        }

        //bancos
        private IXLRange getBancos(IXLWorksheet ws)
        {
            Bancos ctx_bancos = new Bancos();
            var bancos = ctx_bancos.ObtenerBancosDescripcion();
            if (bancos.Count > 0)
            {
                ws.Cell(1, 3).Value = "Bancos";
                var range = ws.Cell(2, 3).InsertData(bancos);
                return range;
            }
            else
                return null;
        }

        private IXLRange GetParentezco(IXLWorksheet ws, int columna)
        {
            List<string> listaParentezco = new List<string>();

            listaParentezco.Add("ESPOSO/A");
            listaParentezco.Add("HIJO/A");
            listaParentezco.Add("PADRE/MADRE");
            listaParentezco.Add("HERMANO/A");
            listaParentezco.Add("TÍO/A");
            listaParentezco.Add("PRIMO/A");
            listaParentezco.Add("SOBRINO/A");
            listaParentezco.Add("ABUELO/A");

            ws.Cell(1, columna).Value = "Parentezco";

            var range = ws.Cell(2, columna).InsertData(listaParentezco);

            return range;
        }

        private IXLRange getEmpresas(IXLWorksheet ws, int IdSucursal, int esquema, string titulo, int columna)
        {
            Empresas bll_empresas = new Empresas();
            var empresas = bll_empresas.GetEmpresasBySucursal(IdSucursal, esquema);
            if (empresas.Count > 0)
            {
                ws.Cell(1, columna).Value = titulo;
                var range = ws.Cell(2, columna).InsertData(empresas);
                return range;
            }
            else
            {
                return null;
            }
        }
        public int ValidateExcel(HttpPostedFile fileBase, string nombreCliente)
        {
            var response = 1;
            if (fileBase.FileName.EndsWith(".xls") || fileBase.FileName.EndsWith(".xlsx"))
            {
                //Guardar el archivo en una ubicación temporal
                string filePath = HttpContext.Current.Server.MapPath("~/Files/ImportacionMasiva") + Path.GetFileName(fileBase.FileName);
                fileBase.SaveAs(filePath);

                //Leer las propiedades de la hoja para comprobar que el archivo sea la plantilla de la empresa
                using (XLWorkbook wBook = new XLWorkbook(filePath))
                {
                    var tag = wBook.Properties.Keywords;
                    var keyword = tag.Trim(';');
                    if (!keyword.Contains(nombreCliente)) response = 4;
                }
                File.Delete(filePath);
            }
            else //Archivo no tiene el formato indicado
            {
                response = 3;
            }
            return response;
        }


        public DataTable ExcelToDataTable(HttpPostedFile fileBase)
        {

            Stream stream = fileBase.InputStream;
            IExcelDataReader reader = null;
            if (fileBase.FileName.EndsWith(".xls"))
                reader = ExcelReaderFactory.CreateBinaryReader(stream);
            else
                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            reader.IsFirstRowAsColumnNames = true;
            var result = reader.AsDataSet();
            reader.Close();
            reader.Dispose();
            reader = null;
            return result.Tables[0];

        }

        public string[] requiredData = { "Nombres", "Paterno", "Materno", "Fecha de Nacimiento", "Sexo", "RFC",
                                            "CURP", "Nacionalidad", "Estado de Origen", "Edo Civil", "Fecha Alta", "Fecha Real", "Tipo Contrato",
                                            "Puesto", "Turno", "Descanso", "Periodicidad de Pago", "Método Pago", "SD", "SDI", "SBC", "Salario Real",
                                            "Banco", "Tipo de Jornada", "Tipo Salario",  "Entidad de Servicio", "Sindicalizado"};

        public void UploadRecords(DataTable data, int idSucursal, int idCliente, int idUsuario)
        {
            int columnas = data.Columns.Count;
            Empleados ctx = new Empleados();

            DataView dv = data.DefaultView;
            dv.Sort = "Paterno asc";//desc
            DataTable sortedDT = dv.ToTable();

            foreach (DataRow row in sortedDT.Rows)
            {

                if (validateRow(row))
                {

                    //Datos Personales
                    Empleado empleado = new Empleado();
                    empleado.Nombres = row["Nombres"].ToString();
                    empleado.APaterno = row["Paterno"].ToString();
                    empleado.AMaterno = row["Materno"].ToString();
                    empleado.FechaNacimiento = Convert.ToDateTime(row["Fecha de Nacimiento"].ToString());
                    empleado.Sexo = row["Sexo"].ToString().Equals("Hombre") ? "H" : "M";
                    empleado.RFC = row["RFC"].ToString().Trim();
                    empleado.CURP = row["CURP"].ToString().Trim();
                    empleado.NSS = row["NSS"].ToString();
                    empleado.Nacionalidad = row["Nacionalidad"].ToString();
                    empleado.Estado = row["Estado de Origen"].ToString();
                    empleado.Telefono = row["Teléfono"].ToString();
                    empleado.Celular = row["Celular"].ToString();
                    empleado.Email = row["Email"].ToString();
                    //Se guarda la dirección del empleado
                    empleado.Direccion = row["Dirección"].ToString();
                    //si la direccion es null se coloca en la celda de dirección "No proporcionada"
                    if (row["Dirección"] == DBNull.Value)
                    {
                        empleado.Direccion = "Dirección no proporcionada";
                    }
                    else
                    {
                        empleado.Direccion = row["Dirección"].ToString();
                    }
                    empleado.IdSucursal = idSucursal;
                    empleado.EstadoCivil = row["Edo Civil"].ToString();
                    empleado.Status = true;
                    empleado.RFCValidadoSAT = 2;

                    var idEmpleado = ctx.CrearEmpleado(empleado, idUsuario);

                    if (idEmpleado > 0)
                    {
                        //Datos de Contratación
                        Empleado_Contrato contrato = new Empleado_Contrato();
                        contrato.IdEmpleado = idEmpleado;
                        contrato.FechaAlta = Convert.ToDateTime(row["Fecha Alta"].ToString());
                        contrato.FechaReal = Convert.ToDateTime(row["Fecha Real"].ToString());
                        if (row["Fecha IMSS"] != DBNull.Value)
                            contrato.FechaIMSS = Convert.ToDateTime(row["Fecha IMSS"].ToString());
                        if (row["UMF"] != DBNull.Value)
                            contrato.UMF = row["UMF"].ToString();
                        contrato.TipoContrato = row["Tipo Contrato"].ToString().Equals("Temporal") ? 2 : 1;
                        if (contrato.TipoContrato == 2)
                        {
                            contrato.Vigencia = Convert.ToDateTime(row["Vigencia"].ToString());
                            contrato.DiasContrato = Convert.ToInt32(row["Días Contrato"]);
                        }
                        Puestos ctxPuestos = new Puestos();
                        contrato.IdPuesto = ctxPuestos.ObtenerPuestoPorDescripcion(row["Puesto"].ToString());
                        contrato.Turno = UtilsEmpleados.SeleccionarTurno(row["Turno"].ToString());
                        contrato.DiaDescanso = UtilsEmpleados.selectDay(row["Descanso"].ToString());
                        contrato.IdPeriodicidadPago = UtilsEmpleados.SeleccionarPeriodicidadDePago(row["Periodicidad de pago"].ToString());
                        contrato.FormaPago = UtilsEmpleados.SeleccionarFormaPago(row["Método Pago"].ToString());
                        contrato.PagoElectronico = (contrato.FormaPago == 3 || contrato.FormaPago == 4 || contrato.FormaPago == 5 || contrato.FormaPago == 6 || contrato.FormaPago == 7) ? true : false;
                        contrato.SD = Convert.ToDecimal(row["SD"].ToString());
                        contrato.SDI = Convert.ToDecimal(row["SDI"].ToString());
                        contrato.SBC = Convert.ToDecimal(row["SBC"].ToString());
                        contrato.SalarioReal = Convert.ToDecimal(row["Salario Real"].ToString());
                        contrato.IdTipoJornada = UtilsEmpleados.SeleccionarTipoNomina(row["Tipo de Jornada"].ToString());
                        contrato.TipoSalario = UtilsEmpleados.SeleccionarTipoSalario(row["Tipo Salario"].ToString());
                        contrato.EntidadDeServicio = (row["Entidad de Servicio"].ToString());
                        contrato.Sindicalizado = (row["Sindicalizado"].ToString().ToUpper().Equals("SI")) ? true : false;
                        contrato.Status = true;
                        contrato.IdSucursal = idSucursal;



                        Empresas ctxRP = new Empresas();
                        int idEmpresaFiscal = 0;
                        int idEmpresaAsimilado = 0;

                        if (row["Empresa Fiscal"] != DBNull.Value)
                        {
                            idEmpresaFiscal = ctxRP.GetIdByRazonSocial(row["Empresa Fiscal"].ToString(), idCliente);

                            contrato.IdEmpresaFiscal = idEmpresaFiscal;
                        }

                        if (row["Empresa Complemento"] != DBNull.Value)
                            contrato.IdEmpresaComplemento = ctxRP.GetIdByRazonSocial(row["Empresa Complemento"].ToString(), idCliente);

                        if (row["Empresa Sindicato"] != DBNull.Value)
                            contrato.IdEmpresaSindicato = ctxRP.GetIdByRazonSocial(row["Empresa Sindicato"].ToString(), idCliente);

                        if (row["Empresa Asimilado"] != DBNull.Value)
                        {
                            idEmpresaAsimilado = ctxRP.GetIdByRazonSocial(row["Empresa Asimilado"].ToString(), idCliente);
                            contrato.IdEmpresaAsimilado = idEmpresaAsimilado;
                        }

                        //Tipo Regimen
                        contrato.IdTipoRegimen = idEmpresaAsimilado > 0 ? 8 : 1;//Asimilado Honorarios sino Sueldo

                        //Tipo Jornada
                        contrato.IdTipoJornada = 3;

                        //Crea el contrato en la base de datos
                        //factorfx
                        ctx.CrearContrato(contrato,idUsuario);

                        DatosBancarios bancarios = new DatosBancarios();
                        bancarios.IdEmpleado = idEmpleado;
                        Bancos bllBancos = new Bancos();

                        if (row["Banco"].ToString().Trim() != "")
                        {
                            bancarios.IdBanco = bllBancos.ObtenerIdBancoPorDescripcion(row["Banco"].ToString());
                        }

                        int numDatosBancarios = 0;
                        if (row["No Siga Fiscal"] != DBNull.Value)
                        {
                            if (row["No Siga Fiscal"].ToString().Trim() != "")
                            {
                                bancarios.NoSigaF = Convert.ToInt32(row["No Siga Fiscal"]);
                                numDatosBancarios++;
                            }
                        }
                        if (row["No Siga Complemento"] != DBNull.Value)
                        {
                            if (row["No Siga Complemento"].ToString().Trim() != "")
                            {
                                bancarios.NoSigaC = Convert.ToInt32(row["No Siga Complemento"]);
                                numDatosBancarios++;
                            }
                        }


                        if (row["Cuenta Bancaria"] != DBNull.Value)
                        {
                            bancarios.CuentaBancaria = row["Cuenta Bancaria"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["# Tarjeta"] != DBNull.Value)
                        {
                            bancarios.NumeroTarjeta = row["# Tarjeta"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["Clabe"] != DBNull.Value)
                        {
                            bancarios.Clabe = row["Clabe"].ToString();
                            numDatosBancarios++;
                        }


                        #region BENEFICIARIOS
                        
                        if (row["Nombre Beneficiario"] != DBNull.Value)
                        {
                            bancarios.NombreBeneficiario = row["Nombre Beneficiario"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["RFC Beneficiario"] != DBNull.Value)
                        {
                            bancarios.RFCBeneficiario = row["RFC Beneficiario"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["CURP Beneficiario"] != DBNull.Value)
                        {
                            bancarios.CURPBeneficiario = row["CURP Beneficiario"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["Parentezco Beneficiario"] != DBNull.Value)
                        {
                            bancarios.ParentezcoBeneficiario = row["Parentezco Beneficiario"].ToString();
                            numDatosBancarios++;
                        }

                        if (row["Domicilio Beneficiario"] != DBNull.Value)
                        {
                            bancarios.DomicilioBeneficiario = row["Domicilio Beneficiario"].ToString();
                            numDatosBancarios++;
                        }
                        #endregion



                        bancarios.Status = true;

                        if (numDatosBancarios > 0)
                            ctx.NewDatosBancarios(bancarios, idUsuario,false);

                        var noti = new Notificaciones();
                        noti.Alta(idEmpleado);

                        //Asignar conceptos Default
                        ConceptosNomina.AsignarConceptosDefaultByEmpleado(idSucursal, idEmpleado);
                    }
                }
            }
        }

        public DataTable LimpiarTabla(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                var isValid = validateRow(row);
                if (!isValid)
                    row.Delete();
            }
            table.AcceptChanges();
            return table;
        }

        /// <summary>
        /// valida que una fila recibida desde excel tenga los campos requeridos debidamente llenados
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool validateRow(DataRow row)
        {
            for (int j = 0; j < requiredData.Length; j++)
            {
                if (row[requiredData[j]] == DBNull.Value)
                    return false;
            }
            return true;
        }
    }
}
