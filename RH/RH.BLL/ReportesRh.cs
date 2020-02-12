using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RH.Entidades;
using ClosedXML.Excel;
using Common.Utils;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using Common.Enums;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading.Tasks;
using iTextSharp.text.pdf.draw;

namespace RH.BLL
{
    public class ReportesRh
    {
        private readonly  static iTextSharp.text.Font Font10 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        readonly static iTextSharp.text.Font Font10B = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        readonly static iTextSharp.text.Font Font14B = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 14, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
        private readonly static iTextSharp.text.Font Font9 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10, iTextSharp.text.Font.NORMAL, BaseColor.GRAY);

        public static string ReporteEmpleados(int idUsuario, string pathFolder, string pathDescarga, int idEmpresa = 0, int idSucursal = 0, int status = 0)
        {
            int[] arrayIdEmpresa;

            List<Empleado> listaEmpleados = new List<Empleado>();
            List<Empleado_Contrato> listaContratos = new List<Empleado_Contrato>();
            List<Empresa> listaEmpresas = new List<Empresa>();
            List<Puesto> listaPuestos = new List<Puesto>();
            List<Empleado_Infonavit> listaEmpleadoInfonavits = new List<Empleado_Infonavit>();
            List<Sucursal> listaSucursales = new List<Sucursal>();
            List<Cliente> listaClientes = new List<Cliente>();
            List<Departamento> listaDepartamentos = new List<Departamento>();
            List<C_TipoContrato_SAT> listaCTipoContratoSats = new List<C_TipoContrato_SAT>();
            List<C_PeriodicidadPago_SAT> listaPeriodicidadPagoSats = new List<C_PeriodicidadPago_SAT>();
            List<C_TipoRegimen_SAT> listaCTipoRegimenSats = new List<C_TipoRegimen_SAT>();
            List<C_TipoJornada_SAT> listaJornadaSats = new List<C_TipoJornada_SAT>();
            List<DatosBancarios> listaDatosBancarios = new List<DatosBancarios>();
            List<C_Banco_SAT> listaBancoSats = new List<C_Banco_SAT>();
            List<C_Estado> listaEstados = new List<C_Estado>();
            List<C_Metodos_Pago> listaMetodosPagos = new List<C_Metodos_Pago>();


            #region GET DATA
            using (var context = new RHEntities())
            {

                listaEmpresas = context.Empresa.ToList();
                listaSucursales = context.Sucursal.ToList();
                listaClientes = context.Cliente.ToList();
                listaEmpleadoInfonavits = context.Empleado_Infonavit.ToList();
                listaPuestos = context.Puesto.ToList();
                listaDepartamentos = context.Departamento.ToList();
                listaBancoSats = context.C_Banco_SAT.ToList();
                listaEstados = context.C_Estado.ToList();
                listaDatosBancarios = context.DatosBancarios.ToList(); //buscar segun los empleados encontrados

                listaJornadaSats = context.C_TipoJornada_SAT.ToList();
                listaCTipoRegimenSats = context.C_TipoRegimen_SAT.ToList();
                listaCTipoContratoSats = context.C_TipoContrato_SAT.ToList();
                listaPeriodicidadPagoSats = context.C_PeriodicidadPago_SAT.ToList();


                listaMetodosPagos = context.C_Metodos_Pago.ToList();

                if (idEmpresa == 0) //todas las empresas
                {
                    arrayIdEmpresa = listaEmpresas.Select(x => x.IdEmpresa).ToArray();
                }
                else //Por empresa
                {
                    arrayIdEmpresa = new int[1];
                    arrayIdEmpresa[0] = idEmpresa;
                }

                //Obtiene los contratos de la empresa seleccionada
                listaContratos = (from c in context.Empleado_Contrato
                                  where arrayIdEmpresa.Contains(c.IdEmpresaFiscal ?? 0) || arrayIdEmpresa.Contains(c.IdEmpresaComplemento ?? 0) || arrayIdEmpresa.Contains(c.IdEmpresaSindicato ?? 0) || arrayIdEmpresa.Contains(c.IdEmpresaAsimilado ?? 0)
                                  select c).ToList();

                //Filtro por status
                switch (status)
                {
                    case 1:
                        listaContratos = listaContratos.Where(x => x.Status == true).ToList();
                        break;
                    case 2:
                        listaContratos = listaContratos.Where(x => x.Status == false).ToList();
                        break;
                }


                //Filtro por Sucursal
                if (idSucursal > 0)
                {
                    listaContratos = listaContratos.Where(x => x.IdSucursal == idSucursal).ToList();
                }


                //Obtiene los empleados de los contratos encontrados
                var arrayIdEmpleados = listaContratos.Select(x => x.IdEmpleado).ToArray();

                bool statusE = status == 1;
                listaEmpleados = (from e in context.Empleado
                                  where arrayIdEmpleados.Contains(e.IdEmpleado)
                                  && e.Status == statusE
                                  select e).ToList();
            }

            #endregion

            var workbook = new XLWorkbook();
            var nombreHoja = status == 1 ? "Empleados Activos" : "Empleados Bajas";
            var worksheet = workbook.Worksheets.Add(nombreHoja);


            #region HEADER
            worksheet.Cell(1, 1).Value = "IdEmpleado";
            worksheet.Cell(1, 2).Value = "Paterno";
            worksheet.Cell(1, 3).Value = "Materno";
            worksheet.Cell(1, 4).Value = "Nombres";
            worksheet.Cell(1, 5).Value = "Fe Nacimiento";
            worksheet.Cell(1, 6).Value = "Sexo";
            worksheet.Cell(1, 7).Value = "RFC";
            worksheet.Cell(1, 8).Value = "CURP";
            worksheet.Cell(1, 9).Value = "NSS";
            worksheet.Cell(1, 10).Value = "UMF";
            worksheet.Cell(1, 11).Value = "NACIONALIDAD";
            worksheet.Cell(1, 12).Value = "Edo Origen";
            worksheet.Cell(1, 13).Value = "Direcccion";
            worksheet.Cell(1, 14).Value = "Telefono";
            worksheet.Cell(1, 15).Value = "Celular";
            worksheet.Cell(1, 16).Value = "Email";
            worksheet.Cell(1, 17).Value = "Edo Civil";
            worksheet.Cell(1, 18).Value = "Fecha Alta";
            worksheet.Cell(1, 19).Value = "Fecha Imss";
            worksheet.Cell(1, 20).Value = "Baja imss";
            worksheet.Cell(1, 21).Value = "Fecha Real";
            worksheet.Cell(1, 22).Value = "Fecha Baja";
            worksheet.Cell(1, 23).Value = "Motivo Baja";

            worksheet.Cell(1, 24).Value = "Reingreso";


            worksheet.Cell(1, 25).Value = "Tipo contrato";
            worksheet.Cell(1,26 ).Value = "Dias Contrato";
            worksheet.Cell(1, 27).Value = "Vigencia";
            worksheet.Cell(1, 28).Value = "Puesto";

            worksheet.Cell(1,29 ).Value = "Departamento";
            worksheet.Cell(1,30 ).Value = "Turno";
            worksheet.Cell(1, 31).Value = "Descanso";
            worksheet.Cell(1, 32).Value = "Periodicidad de Pago";
            worksheet.Cell(1, 33).Value = "Metodo Pago";
            worksheet.Cell(1, 34).Value = "Tipo de Regimen";
            worksheet.Cell(1,35 ).Value = "SD";
            worksheet.Cell(1, 36).Value = "SDI";
            worksheet.Cell(1, 37).Value = "SBC";
            worksheet.Cell(1, 38).Value = "Salario Real";

            worksheet.Cell(1, 39).Value = "Cliente";
            worksheet.Cell(1, 40).Value = "Sucursal";

            worksheet.Cell(1, 41).Value = "Empresa fiscal";
            worksheet.Cell(1, 42).Value = "Empresa complemento";
            worksheet.Cell(1, 43).Value = "Empresa sindicato";
            worksheet.Cell(1, 44).Value = "Asimilado";
            worksheet.Cell(1, 45).Value = "No. SIGA";
            worksheet.Cell(1,46 ).Value = "No. SIGA C";
            worksheet.Cell(1, 47).Value = "Cuenta Bancaria";
            worksheet.Cell(1, 48).Value = "# Tarjeta";
            worksheet.Cell(1, 49).Value = "Clabe";
            worksheet.Cell(1, 50).Value = "Banco";
            worksheet.Cell(1, 51).Value = "Tipo Jornada";
            worksheet.Cell(1, 52).Value = "Tipo Salario";
            worksheet.Cell(1, 53).Value = "Edo de Servicio";
            worksheet.Cell(1, 54).Value = "Sindicalizado";
            worksheet.Cell(1, 55).Value = "Infonavit";//tipo
            worksheet.Cell(1, 56).Value = "No. Credito";
            worksheet.Cell(1, 57).Value = "Factor Descuento";


            #endregion

            #region CONTENIDO

            listaContratos = listaContratos.OrderBy(x => x.IdEmpleado).ToList();

            int row = 2;
            foreach (var itemContrato in listaContratos)
            {
                var itemEmpleado = listaEmpleados.FirstOrDefault(x => x.IdEmpleado == itemContrato.IdEmpleado);

                if (itemEmpleado == null)
                {
                    continue;
                }


                worksheet.Cell(row, 1).Value = itemEmpleado.IdEmpleado;
                worksheet.Cell(row, 2).Value = itemEmpleado.APaterno;
                worksheet.Cell(row, 3).Value = itemEmpleado.AMaterno;
                worksheet.Cell(row, 4).Value = itemEmpleado.Nombres;
                worksheet.Cell(row, 5).Value = itemEmpleado.FechaNacimiento;
                worksheet.Cell(row, 6).Value = itemEmpleado.Sexo;
                worksheet.Cell(row, 7).Value = itemEmpleado.RFC;
                worksheet.Cell(row, 8).Value = itemEmpleado.CURP;
                worksheet.Cell(row, 9).Value = itemEmpleado.NSS;
                worksheet.Cell(row, 10).Value = itemContrato.UMF;
                worksheet.Cell(row, 11).Value = itemEmpleado.Nacionalidad;
                worksheet.Cell(row, 12).Value = itemEmpleado.Estado;
                worksheet.Cell(row, 13).Value = itemEmpleado.Direccion;
                worksheet.Cell(row, 14).Value = itemEmpleado.Telefono;
                worksheet.Cell(row, 15).Value = itemEmpleado.Celular;
                worksheet.Cell(row, 16).Value = itemEmpleado.Email;
                worksheet.Cell(row, 17).Value = itemEmpleado.EstadoCivil;
                worksheet.Cell(row, 18).Value = itemContrato.FechaAlta;
                worksheet.Cell(row, 19).Value = itemContrato.FechaIMSS;
                worksheet.Cell(row, 20).Value = itemContrato.BajaIMSS;
                worksheet.Cell(row, 21).Value = itemContrato.FechaReal;
                worksheet.Cell(row, 22).Value = itemContrato.FechaBaja;
                worksheet.Cell(row, 23).Value = itemContrato.MotivoBaja;
                worksheet.Cell(row, 24).Value = itemContrato.IsReingreso ? "Si" :"No";

                var itemTipoContrato = listaCTipoContratoSats.FirstOrDefault(x => x.IdTipoContrato == itemContrato.TipoContrato);

                worksheet.Cell(row, 25).Value = itemTipoContrato.Descripcion;
                worksheet.Cell(row, 26).Value = itemContrato.DiasContrato;
                worksheet.Cell(row, 27).Value = itemContrato.Vigencia;

                var itemPuesto = listaPuestos.FirstOrDefault(x => x.IdPuesto == itemContrato.IdPuesto);

                var itemDepartamento =
                    listaDepartamentos.FirstOrDefault(x => x.IdDepartamento == itemPuesto.IdDepartamento);

                worksheet.Cell(row, 28).Value = itemPuesto.Descripcion;
                worksheet.Cell(row, 29).Value = itemDepartamento.Descripcion;

                var strTurno = ((Turnos)itemContrato.Turno.Value).ToString();

                worksheet.Cell(row, 30).Value = strTurno;

                var strDescanso = ((DiasSemana)itemContrato.DiaDescanso).ToString();

                worksheet.Cell(row, 31).Value = strDescanso;

                var itemPeriodicidad =
                    listaPeriodicidadPagoSats.FirstOrDefault(
                        x => x.IdPeriodicidadPago == itemContrato.IdPeriodicidadPago);

                worksheet.Cell(row, 32).Value = itemPeriodicidad.Descripcion;

                var itemMetodoPago = listaMetodosPagos.FirstOrDefault(x => x.IdMetodo == itemContrato.FormaPago);

                worksheet.Cell(row, 33).Value = itemMetodoPago.Descripcion;


                var itemRegimen =
                    listaCTipoRegimenSats.FirstOrDefault(x => x.IdTipoRegimen == itemContrato.IdTipoRegimen);
                worksheet.Cell(row, 34).Value = itemRegimen.Descripcion;
                worksheet.Cell(row, 35).Value = itemContrato.SD;
                worksheet.Cell(row, 36).Value = itemContrato.SDI;
                worksheet.Cell(row, 37).Value = itemContrato.SBC;
                worksheet.Cell(row, 38).Value = itemContrato.SalarioReal;

                var itemSucursal = listaSucursales.FirstOrDefault(x => x.IdSucursal == itemContrato.IdSucursal);

                var itemCliente = listaClientes.FirstOrDefault(x => x.IdCliente == itemSucursal.IdCliente);

                worksheet.Cell(row, 39).Value = itemCliente.Nombre;

                worksheet.Cell(row, 40).Value = itemSucursal.Ciudad;

                int intIdEmpresa = 0;

                if (itemContrato.IdEmpresaFiscal != null)
                    intIdEmpresa = itemContrato.IdEmpresaFiscal.Value;

                var itemEmpresaF = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == intIdEmpresa);
                worksheet.Cell(row, 41).Value = itemEmpresaF != null ? itemEmpresaF.RazonSocial : "-";


                intIdEmpresa = 0;
                if (itemContrato.IdEmpresaComplemento != null)
                    intIdEmpresa = itemContrato.IdEmpresaComplemento.Value;

                var itemEmpresaC = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == intIdEmpresa);
                worksheet.Cell(row, 42).Value = itemEmpresaC != null ? itemEmpresaC.RazonSocial : "-";


                intIdEmpresa = 0;
                if (itemContrato.IdEmpresaSindicato != null)
                    intIdEmpresa = itemContrato.IdEmpresaSindicato.Value;

                var itemEmpresaS = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == intIdEmpresa);
                worksheet.Cell(row, 43).Value = itemEmpresaS != null ? itemEmpresaS.RazonSocial : "-";

                intIdEmpresa = 0;
                if (itemContrato.IdEmpresaAsimilado != null)
                    intIdEmpresa = itemContrato.IdEmpresaAsimilado.Value;

                var itemEmpresaA = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == intIdEmpresa);

                worksheet.Cell(row, 44).Value = itemEmpresaA != null ? itemEmpresaA.RazonSocial : "-";

                var itemDatosBancarios = listaDatosBancarios.FirstOrDefault(x => x.IdEmpleado == itemContrato.IdEmpleado);

                worksheet.Cell(row, 45).Value = itemDatosBancarios?.NoSigaF;
                worksheet.Cell(row, 46).Value = itemDatosBancarios?.NoSigaC;
                worksheet.Cell(row, 47).Value = itemDatosBancarios?.CuentaBancaria;
                worksheet.Cell(row, 48).Value = itemDatosBancarios?.NumeroTarjeta;
                worksheet.Cell(row, 49).Value = itemDatosBancarios?.Clabe;

                int intIdBanco = 0;
                if (itemDatosBancarios != null)
                {
                    intIdBanco = itemDatosBancarios.IdBanco;
                }
                var itemBanco = listaBancoSats.FirstOrDefault(x => x.IdBanco == intIdBanco);

                worksheet.Cell(row, 50).Value = itemBanco?.Descripcion;

                var itemJornada = listaJornadaSats.FirstOrDefault(x => x.IdTipoJornada == itemContrato.IdTipoJornada);

                worksheet.Cell(row, 51).Value = itemJornada.Descripcion;

                worksheet.Cell(row, 52).Value = ((TipoSalario)itemContrato.TipoSalario).ToString();



                worksheet.Cell(row, 53).Value = itemContrato.EntidadDeServicio;
                worksheet.Cell(row, 54).Value = itemContrato.Sindicalizado ? "si" : "no";


                var itemInfonavit =
                    listaEmpleadoInfonavits.FirstOrDefault(x => x.IdEmpleadoContrato == itemContrato.IdContrato);


                var tipoCredito = "";
                if (itemInfonavit != null)
                {
                    tipoCredito = ((TipoCreditoInfonavit)itemInfonavit.TipoCredito).ToString();
                }

                worksheet.Cell(row, 55).Value = tipoCredito;
                worksheet.Cell(row, 56).Value = itemInfonavit?.NumCredito;
                worksheet.Cell(row, 57).Value = itemInfonavit?.FactorDescuento;

                row++;
            }

            #endregion
            
            #region DISEÑO
            //Fix
            worksheet.SheetView.Freeze(1, 4);

            //Color
            worksheet.Range("A1:BD1").Style
               .Font.SetFontSize(14)
               .Font.SetBold(true)
               .Font.SetFontColor(XLColor.White)
               .Fill.SetBackgroundColor(XLColor.AirForceBlue);

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
            worksheet.Column(19).AdjustToContents();
            worksheet.Column(20).AdjustToContents();
            worksheet.Column(21).AdjustToContents();
            worksheet.Column(22).AdjustToContents();
            worksheet.Column(23).AdjustToContents();
            worksheet.Column(24).AdjustToContents();
            worksheet.Column(25).AdjustToContents();
            worksheet.Column(26).AdjustToContents();
            worksheet.Column(27).AdjustToContents();
            worksheet.Column(28).AdjustToContents();
            worksheet.Column(29).AdjustToContents();
            worksheet.Column(30).AdjustToContents();
            worksheet.Column(31).AdjustToContents();
            worksheet.Column(32).AdjustToContents();
            worksheet.Column(33).AdjustToContents();
            worksheet.Column(34).AdjustToContents();
            worksheet.Column(35).AdjustToContents();
            worksheet.Column(36).AdjustToContents();
            worksheet.Column(37).AdjustToContents();
            worksheet.Column(38).AdjustToContents();
            worksheet.Column(39).AdjustToContents();
            worksheet.Column(40).AdjustToContents();
            worksheet.Column(41).AdjustToContents();
            worksheet.Column(42).AdjustToContents();
            worksheet.Column(43).AdjustToContents();
            worksheet.Column(44).AdjustToContents();
            worksheet.Column(45).AdjustToContents();
            worksheet.Column(46).AdjustToContents();
            worksheet.Column(47).AdjustToContents();
            worksheet.Column(48).AdjustToContents();
            worksheet.Column(49).AdjustToContents();
            worksheet.Column(50).AdjustToContents();
            worksheet.Column(51).AdjustToContents();
            worksheet.Column(52).AdjustToContents();
            worksheet.Column(53).AdjustToContents();
            worksheet.Column(54).AdjustToContents();
            worksheet.Column(55).AdjustToContents();
            worksheet.Column(56).AdjustToContents();
            worksheet.Column(57).AdjustToContents();

            #endregion

            //Creamos el folder para guardar el archivo
            var pathUsuario = Utils.ValidarFolderUsuario(idUsuario, pathFolder);

            string nombreArchivo = status == 1 ? "Empleados_Activos.xlsx" : "Empleados_Bajas.xlsx";

            var fileName = pathUsuario + nombreArchivo;

            //Guarda el archivo
            workbook.SaveAs(fileName);
            
            return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;

        }


        public static Task<byte[]> GetDatosEmpleadosPdfAsync(int id)
        {
            var r = Task.Factory.StartNew(() => GetDatosEmpleadosPdf(id));

            return r;
        }

        private static byte[] GetDatosEmpleadosPdf(int idEmpleado)
        {

            byte[] byteInfo;
            MemoryStream ms = new MemoryStream();
            Document doc = new Document(PageSize.LETTER, 50, 50, 25, 25);
            PdfWriter writer = PdfWriter.GetInstance(doc, ms);
            writer.CloseStream = false;
            doc.Open();

            try
            {


                Empleado itemEmpleado = new Empleado();
                Empleado_Contrato itemContrato = new Empleado_Contrato();
                Cliente itemCliente = new Cliente();
                Sucursal itemSucursal = new Sucursal();
                List<Empresa> listaEmpresas = new List<Empresa>();
                DatosBancarios itemBancarios = new DatosBancarios();
                Puesto itemPuesto = new Puesto();
                C_Banco_SAT itemBancoSat = new C_Banco_SAT();
                string empFiscal = "";
                string empComplemento = "";
                SYA_Usuarios usuario = new SYA_Usuarios();

                using (var context = new RHEntities())
                {
                    itemEmpleado = context.Empleado.FirstOrDefault(x => x.IdEmpleado == idEmpleado);

                    itemContrato =
                        context.Empleado_Contrato.Where(x => x.IdEmpleado == idEmpleado)
                            .OrderByDescending(x => x.IdContrato)
                            .FirstOrDefault();

                    if (itemContrato == null) return null;

                    itemSucursal = context.Sucursal.FirstOrDefault(x => x.IdSucursal == itemEmpleado.IdSucursal);

                    itemCliente = context.Cliente.FirstOrDefault(x => x.IdCliente == itemSucursal.IdCliente);

                    listaEmpresas = context.Empresa.ToList();

                    itemPuesto = context.Puesto.FirstOrDefault(x => x.IdPuesto == itemContrato.IdPuesto);

                    itemBancarios = context.DatosBancarios.FirstOrDefault(x => x.IdEmpleado == idEmpleado);

                    itemBancoSat = context.C_Banco_SAT.FirstOrDefault(x => x.IdBanco == itemBancarios.IdBanco);

                    usuario = context.SYA_Usuarios.FirstOrDefault(x => x.IdUsuario == itemContrato.IdUsuarioReg);

                }


                if (itemContrato.IdEmpresaFiscal != null)
                {
                    Empresa itemEmpF = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == itemContrato.IdEmpresaFiscal.Value);
                    empFiscal = itemEmpF != null ? itemEmpF.RazonSocial : "na";
                }

                if (itemContrato.IdEmpresaComplemento != null)
                {
                    Empresa itemEmpC = listaEmpresas.FirstOrDefault(x => x.IdEmpresa == itemContrato.IdEmpresaComplemento.Value);
                    empComplemento = itemEmpC != null ? itemEmpC.RazonSocial : "na";
                }
                

                doc.NewPage();


                //// Paragraph to draw line.
                //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                //// call the below to add the line when required.
                //doc.Add(p);

                //doc.Add(Chunk.NEWLINE);

                string strNombre = "";

                strNombre = itemContrato.Status == true ? $" {itemEmpleado.Nombres} {itemEmpleado.APaterno} {itemEmpleado.AMaterno}" : $" {itemEmpleado.Nombres} {itemEmpleado.APaterno} {itemEmpleado.AMaterno} - Baja";

                doc.Add(new Phrase(strNombre, Font14B));
                doc.Add(Chunk.NEWLINE);
                if (usuario != null)
                {
                    string strUsuario =
                        $" Registrado por: {usuario.Nombres} {usuario.ApPaterno} {usuario.ApMaterno}          Fecha Reg: {itemContrato.FechaReg}";
                    doc.Add(new Phrase(strUsuario, Font9));
                }
                //doc.Add(Chunk.NEWLINE);

                // Paragraph to draw line.
                Paragraph p =
                    new Paragraph(
                        new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK,
                            Element.ALIGN_LEFT, 1)));
                // call the below to add the line when required.
                doc.Add(p);

                // LineSeparator line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                //doc.Add(line);


                doc.Add(Chunk.NEWLINE);
                SetDataDoc(ref doc, "IdEmpleado : ", itemEmpleado.IdEmpleado.ToString());
                SetDataDoc(ref doc, "Nombre(s) : ", itemEmpleado.Nombres);
                SetDataDoc(ref doc, "Paterno : ", itemEmpleado.APaterno);
                SetDataDoc(ref doc, "Materno : ", itemEmpleado.AMaterno);
                SetDataDoc(ref doc, "NSS : ", itemEmpleado.NSS);
                SetDataDoc(ref doc, "RFC : ", itemEmpleado.RFC);
                SetDataDoc(ref doc, "CURP : ", itemEmpleado.CURP);

                doc.Add(Chunk.NEWLINE);

                SetDataDoc(ref doc, "Cliente   : ", itemCliente.Nombre);
                SetDataDoc(ref doc, "Sucursal : ", itemSucursal.Ciudad);
                SetDataDoc(ref doc, "Puesto : ", itemPuesto.Descripcion);

                doc.Add(Chunk.NEWLINE);
                SetDataDoc(ref doc, "Empresa Fiscal : ", empFiscal);
                SetDataDoc(ref doc, "Empresa Comp : ", empComplemento);
                SetDataDoc(ref doc, "SD : ", itemContrato.SD.ToString());
                SetDataDoc(ref doc, "SDI : ", itemContrato.SDI.ToString());
                SetDataDoc(ref doc, "SDR : ", itemContrato.SalarioReal.ToString());

                SetDataDoc(ref doc, itemContrato.IsReingreso ? "Fecha Reingreso : " : "Fecha Alta : ",itemContrato.FechaAlta.ToString("dd MMM yyyy"));
                SetDataDoc(ref doc, "Fecha Imss : ",itemContrato.FechaIMSS != null ? itemContrato.FechaIMSS.Value.ToString("dd MMM yyyy") : "--");
                SetDataDoc(ref doc, "Fecha Real : ", itemContrato.FechaReal.ToString("dd MMM yyyy"));
                SetDataDoc(ref doc, "Tipo Contrato : ", itemContrato.IsReingreso ? "Reingreso" : "Nuevo");

                SetDataDoc(ref doc, "Estatus contrato : ", itemContrato.Status ? "Vigente" : "Baja");

                if (itemContrato.Status == false)
                {
                    
                    SetDataDoc(ref doc, "Fecha Baja : ", itemContrato.FechaBaja != null ? itemContrato.FechaBaja.Value.ToString("dd MMM yyyy") : "--");
                    SetDataDoc(ref doc, "Baja Imss : ", itemContrato.BajaIMSS != null ? itemContrato.BajaIMSS.Value.ToString("dd MMM yyyy") : "--");
                    SetDataDoc(ref doc, "Motivo Baja : ", itemContrato.MotivoBaja != null ? itemContrato.MotivoBaja : "--");
                    SetDataDoc(ref doc, "Comentario Baja : ", itemContrato.ComentarioBaja != null ? itemContrato.ComentarioBaja : "--");

                }

                doc.Add(Chunk.NEWLINE);

                SetDataDoc(ref doc, "Banco : ", itemBancoSat.Descripcion);
                SetDataDoc(ref doc, "Siga F : ", itemBancarios.NoSigaF.ToString());
                SetDataDoc(ref doc, "Siga C : ", itemBancarios.NoSigaC.ToString());
                SetDataDoc(ref doc, "Cuenta Bancaria : ", itemBancarios.CuentaBancaria);
                SetDataDoc(ref doc, "Num. de Tarjeta  : ", itemBancarios.NumeroTarjeta);
                SetDataDoc(ref doc, "Clabe Interbancaria : ", itemBancarios.Clabe);
                doc.Add(Chunk.NEWLINE);
                SetDataDoc(ref doc, "Beneficiario : ", itemBancarios.NombreBeneficiario);
                SetDataDoc(ref doc, "RFC Beneficiario   : ", itemBancarios.RFCBeneficiario);
                SetDataDoc(ref doc, "CURP Beneficiario : ", itemBancarios.CURPBeneficiario);
                SetDataDoc(ref doc, "Parentezco : ", itemBancarios.ParentezcoBeneficiario);
                SetDataDoc(ref doc, "Domicilio Beneficiario : ", itemBancarios.DomicilioBeneficiario);

            }
            catch (Exception e)
            {
                doc.Add(new Phrase($"Error ? {e.Message}", Font14B));
            }
            finally
            {
                doc.Close();

               byteInfo = ms.ToArray();
                ms.Write(byteInfo, 0, byteInfo.Length);
                ms.Position = 0;
                ms.Flush();
                ms.Dispose();
            }

            return byteInfo;

        }

        private static void SetDataDoc(ref Document doc,  string titulo, string dato)
        {

            var f1 = new Phrase(titulo, Font10B);
            var f2 = new Phrase(dato, Font10);

            doc.Add(f1);
            doc.Add(f2);

            doc.Add(Chunk.NEWLINE);
        }



    }
}
