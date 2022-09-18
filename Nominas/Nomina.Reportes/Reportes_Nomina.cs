using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML;
using System.Xml;
using RH.Entidades;
using System.IO;
using Nomina.Reportes.Datos;
using Common.Utils;
using SpreadsheetLight;
using ClosedXML.Excel;
using MoreLinq;
using RH.BLL;
using System.Data;
using System.Web;
using Excel;

namespace Nomina.Reportes
{
    public class Reportes_Nomina
    {
        // RHEntities ctx = null;

        public Reportes_Nomina()
        {
            // ctx = new RHEntities();
        }
        public string ExportarExcelReporteNomina(int idusuario, string ruta, NOM_PeriodosPago periodoPago)
        {
            List<DatosReporteNominas> datos;

            List<NOM_Nomina_Detalle> listaDetalles;
            List<NOM_Nomina> listaNominas;
            List<NOM_Cuotas_IMSS> listaCuotasImsss;

            using (var context = new RHEntities())
            {
                datos = (from emp in context.Empleado
                         join empPer in context.NOM_Empleado_PeriodoPago
                         on emp.IdEmpleado equals empPer.IdEmpleado
                         //join nom in ctx.NOM_Nomina
                         //on emp_per.IdPeriodoPago equals nom.IdPeriodo
                         where empPer.IdPeriodoPago == periodoPago.IdPeriodoPago
                         select new DatosReporteNominas
                         {
                             IdEmpleado = emp.IdEmpleado,
                             //IdNomina = nom.IdNomina,
                             Nombres = emp.Nombres,
                             Paterno = emp.APaterno,
                             Materno = emp.AMaterno
                         }).ToList();


                listaNominas = context.NOM_Nomina.Where(x => x.IdPeriodo == periodoPago.IdPeriodoPago).ToList();

                var arrayIdNominas = listaNominas.Select(x => x.IdNomina).ToArray();

                listaDetalles = (from d in context.NOM_Nomina_Detalle
                                 where arrayIdNominas.Contains(d.IdNomina)
                                 select d).ToList();


                listaCuotasImsss = (from c in context.NOM_Cuotas_IMSS
                                    where arrayIdNominas.Contains(c.IdNomina)
                                    select c).ToList();

            }


            var newruta = ValidarFolderUsuario(idusuario, ruta);
            newruta = newruta + "ReporteNomina_" + periodoPago.Descripcion + "_.xlsx";

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Reporte Nominas ");
            ws.Cell("A1").Value = "ID EMPLEADO";
            ws.Cell("B1").Value = "EMPLEADO";
            ws.Cell("C1").Value = "PERCEPCIONES";
            ws.Cell("D1").Value = "DEDUCCIONES";
            ws.Cell("E1").Value = "COMPLEMENTO";
            ws.Cell("F1").Value = "SUELDOS";
            ws.Cell("G1").Value = "SALARIO DIARIO";
            ws.Cell("H1").Value = "SALARIO DIARIO I";
            ws.Cell("I1").Value = "SALARIO BASE C";
            ws.Cell("J1").Value = "SALARIO REAL";
            ws.Cell("K1").Value = "PENSION ALIMENTICIA";
            ws.Cell("L1").Value = "SUBSIDIO AL EMPLEO";
            ws.Cell("M1").Value = "IMPUESTO NOMINAS";
            ws.Cell("N1").Value = "CUOTA PATRONAL";
            ws.Cell("O1").Value = "CUOTA OBRERA";
            ws.Cell("P1").Value = "ISR";
            ws.Cell("Q1").Value = "INFONAVIT";
            ws.Cell("R1").Value = "FONACOT";
            ws.Cell("S1").Value = "TOTAL";


            int i = 2;
            foreach (var emp in datos)
            {
                if (emp == null) continue;

                // var nomina = ctx.NOM_Nomina.Where(x => x.IdPeriodo == idPeriodo && x.IdEmpleado == emp.IdEmpleado).FirstOrDefault();
                var nomina = listaNominas.FirstOrDefault(x => x.IdEmpleado == emp.IdEmpleado);

                if (nomina == null) continue;

                decimal totalPatron = 0;
                decimal totalObrero = 0;

                //var pension = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 48).FirstOrDefault();
                var pension = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 48);

                //var imss = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 42).FirstOrDefault();
                var imss = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 42);

                //var isr = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 43).FirstOrDefault();
                var isr = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 43);

                //var infonavit = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 51).FirstOrDefault();
                var infonavit = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 51);

                //var fonacot = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 52).FirstOrDefault();
                var fonacot = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 52);

                //var sueldo = ctx.NOM_Nomina_Detalle.Where(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 1).FirstOrDefault();
                var sueldo = listaDetalles.FirstOrDefault(x => x.IdNomina == nomina.IdNomina && x.IdConcepto == 1);

                var itemCuota = listaCuotasImsss.FirstOrDefault(x => x.IdNomina == nomina.IdNomina);

                if (itemCuota != null)
                {
                    totalPatron = itemCuota.TotalPatron;
                    totalObrero = itemCuota.TotalObrero;
                }


                ws.Cell($"A{i}").Value = emp.IdEmpleado;
                ws.Cell($"B{i}").Value = emp.Paterno + emp.Materno + emp.Nombres;

                if (nomina != null)
                {
                    ws.Cell($"C{i}").Value = nomina.TotalPercepciones;
                    ws.Cell($"D{i}").Value = nomina.TotalDeducciones;
                    ws.Cell($"E{i}").Value = nomina.TotalComplemento;
                    ws.Cell($"F{i}").Value = sueldo == null ? 0 : sueldo.Total;
                    ws.Cell($"G{i}").Value = nomina.SD;
                    ws.Cell($"H{i}").Value = nomina.SDI;
                    ws.Cell($"I{i}").Value = nomina.SBC;
                    ws.Cell($"J{i}").Value = nomina.SDReal;
                    ws.Cell($"K{i}").Value = pension == null ? 0 : pension.Total;
                    ws.Cell($"L{i}").Value = nomina.SubsidioEntregado;
                    ws.Cell($"M{i}").Value = nomina.TotalImpuestoSobreNomina;
                    ws.Cell($"N{i}").Value = totalPatron;
                    ws.Cell($"O{i}").Value = totalObrero;
                    ws.Cell($"P{i}").Value = isr == null ? 0 : isr.Total;
                    ws.Cell($"Q{i}").Value = infonavit == null ? 0 : infonavit.Total;
                    ws.Cell($"R{i}").Value = fonacot == null ? 0 : fonacot.Total;
                    ws.Cell($"S{i}").Value = nomina.TotalNomina;

                    i++;
                }
                else
                {
                    ws.Range($"C{i}:R{i}").Value = 0;

                    i++;
                }

            }

            ws.Cell($"C{i}").FormulaA1 = $"=SUM(C2:C{i - 1})";
            ws.Cell($"D{i}").FormulaA1 = $"=SUM(D2:D{i - 1})";
            ws.Cell($"E{i}").FormulaA1 = $"=SUM(E2:E{i - 1})";
            ws.Cell($"F{i}").FormulaA1 = $"=SUM(F2:F{i - 1})";
            ws.Cell($"K{i}").FormulaA1 = $"=SUM(K2:K{i - 1})";
            ws.Cell($"L{i}").FormulaA1 = $"=SUM(L2:L{i - 1})";
            ws.Cell($"M{i}").FormulaA1 = $"=SUM(M2:M{i - 1})";
            ws.Cell($"N{i}").FormulaA1 = $"=SUM(N2:N{i - 1})";
            ws.Cell($"O{i}").FormulaA1 = $"=SUM(O2:O{i - 1})";
            ws.Cell($"P{i}").FormulaA1 = $"=SUM(P2:P{i - 1})";
            ws.Cell($"Q{i}").FormulaA1 = $"=SUM(Q2:Q{i - 1})";
            ws.Cell($"R{i}").FormulaA1 = $"=SUM(R2:R{i - 1})";
            ws.Cell($"S{i}").FormulaA1 = $"=SUM(S2:S{i - 1})";

            ws.Range($"C2:S{i}").Style.NumberFormat.Format = "$ #,##0.00";
            ws.Range("A1:S1").Style.Font.SetBold();
            ws.Range($"A{i}:S{i}").Style.Font.SetBold();

            ws.Columns("1:19").AdjustToContents();

            wb.SaveAs(newruta);


            return newruta;
        }

        public string CrearReporteAguinaldo(int idUsuario, string pathFolder, string pathDescarga, int idPeriodo)
        {
            NOM_PeriodosPago periodo;
            List<NOM_Aguinaldo> listaAguinaldos;
            List<Empleado> listaEmpleados;
            List<NOM_Nomina> listaNominas;
            Cliente itemCliente;
            Sucursal itemSucursal;
            using (var context = new RHEntities())
            {

                periodo = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                listaAguinaldos = context.NOM_Aguinaldo.Where(x => x.IdPeriodo == idPeriodo).ToList();

                var arrayIdEmpleados = listaAguinaldos.Select(x => x.IdEmpleado).ToArray();

                listaEmpleados = (from e in context.Empleado
                                  where arrayIdEmpleados.Contains(e.IdEmpleado)
                                  select e).ToList();

                var arrayIdNominas = listaAguinaldos.Select(x => x.IdNomina).ToArray();

                listaNominas = (from n in context.NOM_Nomina
                                where arrayIdNominas.Contains(n.IdNomina)
                                select n).ToList();

                itemCliente = context.Cliente.FirstOrDefault(x => x.IdCliente == periodo.IdCliente);
                itemSucursal = context.Sucursal.FirstOrDefault(x => x.IdSucursal == periodo.IdSucursal);
            }


            var nombreHoja = "Aguinaldos";
            var nombreHojaExcel = "Aguinaldos";

            if (itemCliente != null && itemSucursal != null)
            {
                nombreHoja = $"{itemCliente.Nombre} - {itemSucursal.Ciudad}";
            }

            nombreHojaExcel = nombreHoja;
            //menos de 31 caracteres
            if (nombreHoja.Length > 30)
            {
                nombreHojaExcel = nombreHoja.Substring(0, 30);
            }

            
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(nombreHojaExcel);



            #region HEADER

            worksheet.Cell(1, 1).Value = nombreHoja; //$"AGUINALDOS {periodo.Fecha_Fin.Year}";
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 18;
            worksheet.Range("A1:G1").Merge();

            worksheet.Cell(1, 8).Value = periodo.Descripcion;
            worksheet.Cell(1, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 8).Style.Font.Bold = true;
            worksheet.Cell(1, 8).Style.Font.FontSize = 16;
            worksheet.Cell(1, 8).Style.Font.FontColor = XLColor.Olive;
            worksheet.Range("H1:N1").Merge();

            worksheet.Cell(3, 1).Value = "IDA";
            worksheet.Cell(3, 2).Value = "IDE";
            worksheet.Cell(3, 3).Value = "Nombre Empleado";
            //worksheet.Cell(3, 4).Value = "Materno";
            //worksheet.Cell(3, 5).Value = "Nombres";
            worksheet.Cell(3, 4).Value = "SD";
            worksheet.Cell(3, 5).Value = "SDI";
            worksheet.Cell(3, 6).Value = "SM";
            worksheet.Cell(3, 7).Value = "SR";
            worksheet.Cell(3,8).Value = "Dias Aguinaldo";
            worksheet.Cell(3,9).Value = "Dias Ejercicio";

            worksheet.Cell(3,10).Value = "Primer Dia";
            worksheet.Cell(3,11).Value = "Ultimo Dia";
            worksheet.Cell(3,12).Value = "Faltas";
            worksheet.Cell(3,13).Value = "Faltas Personalizadas";
            worksheet.Cell(3,14).Value = "Dias Trabajados";
            worksheet.Cell(3,15).Value = "Sueldo Mensual";
            worksheet.Cell(3,16).Value = "Proporcion";

            worksheet.Cell(3, 17).Value = "Primer Dia C";
            worksheet.Cell(3, 18).Value = "Dias Trabajados C";
            worksheet.Cell(3, 19).Value = "Proporcion C";

            worksheet.Cell(3, 20).Value = "Dias Aguinaldo Comp";
            worksheet.Cell(3, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3,21).Value = "Tope Exento";
            worksheet.Cell(3,22).Value = "Exento";
            worksheet.Cell(3,23).Value = "Gravado";

            worksheet.Cell(3,24).Value = "Aguinaldo";
            worksheet.Cell(3,25).Value = "ISR";
            worksheet.Cell(3,25).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(3,26).Value = "Pension Alimenticia";
            worksheet.Cell(3,26).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            
            worksheet.Cell(3,27).Value = "Neto";
            worksheet.Cell(3,27).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3,28).Value = "Complemento";
            worksheet.Cell(3,29).Value = "Total";
            worksheet.Cell(3,29).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3,30).Value = "ISN";
            worksheet.Cell(3,30).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3,31).Value = "Factor";
            worksheet.Cell(3,31).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3,32).Value = "Fecha Reg";
            #endregion

            #region CONTENIDO
            int row = 4;
            var rowColor = false;

            listaEmpleados = listaEmpleados.OrderBy(x => x.APaterno).ToList();

            foreach (var itemEmpleado in listaEmpleados)
            {
                //var itemEmpleado = listaEmpleados.FirstOrDefault(x => x.IdEmpleado == item.IdEmpleado);
                var item = listaAguinaldos.FirstOrDefault(x => x.IdEmpleado == itemEmpleado.IdEmpleado);

                if (itemEmpleado == null || item == null) continue;

                var itemN = listaNominas.FirstOrDefault(x => x.IdNomina == item.IdNomina);

                worksheet.Cell(row, 1).Value = item.IdAguinaldo;
                worksheet.Cell(row, 2).Value = item.IdEmpleado;
                worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 3).Value = $"{itemEmpleado.APaterno} {itemEmpleado.AMaterno} {itemEmpleado.Nombres}";
                worksheet.Cell(row, 3).Style.Font.Bold = true;
        

                worksheet.Cell(row, 4).Value = item.SD;
                worksheet.Cell(row, 5).Value = item.SDI;
                worksheet.Cell(row, 6).Value = item.SalarioMinimo;

                worksheet.Cell(row, 7).Value = itemN?.SDReal ?? 0;

                worksheet.Cell(row, 8).Value = item.DiasAguinaldo;
                worksheet.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 9).Value = item.DiasEjercicioFiscal;
                worksheet.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 10).Value = item.FechaPrimerDia;
                worksheet.Cell(row, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 11).Value = item.FechaUltimoDia;
                worksheet.Cell(row, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 12).Value = item.TotalFaltas;
                worksheet.Cell(row, 12).Style.Font.FontColor = XLColor.Red;
                worksheet.Cell(row, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 13).Value = item.FaltasPersonalizadas;
                worksheet.Cell(row, 13).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 14).Value = item.DiasTrabajados;
                worksheet.Cell(row, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                worksheet.Cell(row, 15).Value = item.SueldoMensual;
                worksheet.Cell(row, 16).Value = item.Proporcion;

                worksheet.Cell(row, 17).Value = item.FechaPrimerDiaC;
                worksheet.Cell(row, 18).Value = item.DiasTrabajadosC;
                worksheet.Cell(row, 19).Value = item.ProporcionC;

                worksheet.Cell(row, 20).Value = item.DiasAguinaldoComp >= 0 ? item.DiasAguinaldoComp.ToString() : "-" ;
                worksheet.Cell(row, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 20).Style.Font.FontColor = XLColor.DeepPink;

                worksheet.Cell(row, 21).Value = item.TopeExcento;

                worksheet.Cell(row, 22).Value = item.Exento;
                worksheet.Cell(row, 23).Value = item.Gravado;
                worksheet.Cell(row, 24).Value = item.Aguinaldo;
                worksheet.Cell(row, 24).Style.Font.Bold = true;
                worksheet.Cell(row, 25).Value = item.ISR;
                worksheet.Cell(row, 26).Value = item.PensionAlimenticia;

                worksheet.Cell(row, 27).Value = item.Neto;
                worksheet.Cell(row, 27).Style.Font.Bold = true;

                worksheet.Cell(row, 28).Value = item.Complemento;
                worksheet.Cell(row, 29).Value = item.Total;
                worksheet.Cell(row, 29).Style.Font.Bold = true;

                worksheet.Cell(row, 30).Value = item.ISN3;
                worksheet.Cell(row, 31).Value = item.Factor;
                worksheet.Cell(row, 31).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 32).Value = item.FechaReg;

                if (rowColor)
                {
                    // worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.Lavender;

                    var rangoPintar = "A" + row + ":AE" + row;
                    worksheet.Range(rangoPintar).Style
                   .Fill.SetBackgroundColor(XLColor.Lavender);

                }

                row++;
                rowColor = !rowColor;
            }

            #endregion

            #region SUMATORIA

            string[] letra = new[]
            {
                "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U",
                "V", "W", "X", "Y", "Z", "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM",
                "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", "BA", "BB", "BC", "BD",
                "BE", "BF", "BG", "BH", "BI", "BJ", "BK", "BL", "BM", "BN", "BO", "BP", "BQ", "BR", "BS", "BT", "BU",
                "BV", "BW", "BX", "BY", "BZ", "CA", "CB", "CC", "CD", "CE", "CF", "CG", "CH", "CI", "CJ", "CK", "CL",
                "CM", "CN", "CO", "CP", "CQ", "CR", "CS", "CT", "CU", "CV", "CW", "CX", "CY", "CZ"
            };

            int[] arrayColumnas = new[] {24,26,27,28,29,30};
            int filaInicial = 4;
            int filaFinal = row - 1;
            int filaDelTotal = row;
            for (int i = 0; i < arrayColumnas.Length; i++)
            {
                int c = arrayColumnas[i];

                c -= 1;
                string formula = $"=SUM({letra[c]}{filaInicial}:{letra[c]}{filaFinal})";
                worksheet.Cell(filaDelTotal, c + 1).FormulaA1 = formula;
                //worksheet.Range(filaDelTotal, 1, filaDelTotal, columna).Style.Fill.SetBackgroundColor(XLColor.FromArgb(235, 241, 222));
                //worksheet.Cell("B9").SetFormulaA1("=SUM(B3:B8)");
            }

            worksheet.Range(filaDelTotal, 1, filaDelTotal, 31).Style.Font.SetBold(true);

            #endregion

            #region DISEÑO
            //Fix
            worksheet.SheetView.Freeze(3, 7);

            //Color
            worksheet.Range("A3:AE3").Style
               .Font.SetFontSize(14)
               .Font.SetBold(true)
               .Font.SetFontColor(XLColor.White)
               .Fill.SetBackgroundColor(XLColor.Teal);

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

            #endregion


            //Creamos el folder para guardar el archivo
            var pathUsuario = Utils.ValidarFolderUsuario(idUsuario, pathFolder);

            string nombreArchivo = $"Aguinaldos {periodo.Fecha_Fin.Year} {nombreHoja}.xlsx";

            var fileName = pathUsuario + nombreArchivo;
            //Guarda el archivo
            workbook.SaveAs(fileName);
            //  return fileName;
            return fileName; //pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;
        }



        private string ValidarFolderUsuario(int idUsuario, string pathFolder)
        {
            var pathArchivos = pathFolder;
            if (!Directory.Exists(pathArchivos))
            {
                Directory.CreateDirectory(pathArchivos);
            }
            //Crear folder para el usuario con su id
            string folderUsuario = pathArchivos + "\\" + idUsuario + "\\";
            if (Directory.Exists(folderUsuario))
            {
                //Elimina el contenido del folder
                Array.ForEach(Directory.GetFiles(folderUsuario), File.Delete);

            }
            else
            {
                //Crea el folder con el id del usuario
                Directory.CreateDirectory(folderUsuario);
            }
            return folderUsuario;
        }

        /// <summary>
        /// creacionde un csv de movimiento de personal para el sistema de la empresa odesa
        /// </summary>
        /// <returns></returns>
        public string CrearMoper(string path, int idUsuario, string pathDescarga, NOM_PeriodosPago periodo) {
            
            List<DatosBancarios> listaDatosBancarios = new List<DatosBancarios>();
            List<Empleado> listaEmpleados = new List<Empleado>();
            List<NOM_Nomina> listaNominas = new List<NOM_Nomina>();
            List<Empleado_Contrato> listaContratos = new List<Empleado_Contrato>();
            var nombreArchivo = "";

            using (var context = new RHEntities())
            {
                listaNominas = (from nn in context.NOM_Nomina
                                    where nn.IdPeriodo == periodo.IdPeriodoPago
                                    select nn).ToList();

                var arrayContratosId = listaNominas.Select(x => x.IdContrato).ToArray();

                listaContratos = (from ec in context.Empleado_Contrato
                                  where arrayContratosId.Contains(ec.IdContrato)
                                  select ec).ToList();

                var arrayEnpleadosId = listaContratos.Select(x => x.IdEmpleado).ToArray();

                listaEmpleados = (from e in context.Empleado
                                  where arrayEnpleadosId.Contains(e.IdEmpleado)
                                  select e).ToList();

                listaDatosBancarios = (from db in context.DatosBancarios
                                  where arrayEnpleadosId.Contains(db.IdEmpleado)
                                  select db).ToList();
            }
            var arrayEmpresaFiscal = listaContratos.Select(x => x.IdEmpresaFiscal).Distinct().ToArray();//obtiene los id de empresas fiscales en esta nomina que deberia ser solo una
            var numeroEmpresas = arrayEmpresaFiscal.Count();
            int idEmpresaFiscal;
            if (numeroEmpresas > 1)
            {
                return null;
            } else {
                idEmpresaFiscal = (int)arrayEmpresaFiscal.FirstOrDefault();
            }

            var numeroEmpresaOdesa = UtilsFondoAhorro.claveEmpresa(idEmpresaFiscal);//devuelve el numero de empresa o 0 en caso de no estar en la lista
            if (numeroEmpresas == 0)//la empresa no pertenece a las dadas de alta en osea
            {
                return null;
            }
           
            var tipoNominaMoper = UtilsFondoAhorro.formaPago(periodo.IdTipoNomina);//la periodicidad del periodo para las lineas
            var tipoNominaNombreMoper = UtilsFondoAhorro.formaPagoNombreMoper(periodo.IdTipoNomina);//la periodicidad del periodo para el nombre del archivo

            nombreArchivo = periodo.Fecha_Fin.ToString("yyyyMMdd") + "MP" + numeroEmpresaOdesa + tipoNominaNombreMoper + periodo.IdPeriodoPago + "FA.txt";//fa: fondo de ahorro - Ca: caja de ahorro

            int sumaIdempleados = 0;
            decimal sumaSueldoMensual = 0;
            List<string> lineas = new List<string>();
            lineas.Add("01,REGISTRO,SOCIO,CAMPO4,EMPRESA,PLANTA,C_DEPTO,NOMBRE,PATERNO,MATERNO,F_NACIM,SEXO,E_CIVIL,I_GRUPO,I_EMPRESA,F_PAGO,T_TRAB,R_PAGO,BANCO,CUENTA_B,S_MENSUAL,RFC,NIVEL_DEL_AVAL,AGUINALDO,PRIMA_V,PTU,FONDO_AH,PRESTA_FA,INCAPACIDAD");

            var reg = 1;//para llevar un contador en los renglones del moper
            foreach (var con in listaContratos)
            {
                var itemEmpleado = listaEmpleados.Where(x => x.IdEmpleado == con.IdEmpleado).FirstOrDefault();
                var itemDatoBancario = listaDatosBancarios.Where(x => x.IdEmpleado == con.IdEmpleado).FirstOrDefault();
                var itemNomina = listaNominas.Where(x => x.IdEmpleado == con.IdEmpleado).FirstOrDefault();

                var cadenaLinea = "02,";
                cadenaLinea += reg.ToString() + ",";//REGISTRO
                cadenaLinea += con.IdEmpleado.ToString() + ",";//SOCIO
                    sumaIdempleados += con.IdEmpleado;//para la ultima linea
                cadenaLinea += 0 + ",";//CAMPO4
                cadenaLinea += numeroEmpresaOdesa + ",";//EMPRESA
                cadenaLinea += 1 + ",";//PLANTA
                cadenaLinea += 0 + ",";//C_DEPTO
                cadenaLinea += itemEmpleado.Nombres + ",";//NOMBRE
                cadenaLinea += itemEmpleado.APaterno + ",";//PATERNO
                cadenaLinea += itemEmpleado.AMaterno + ",";//MATERNO
                cadenaLinea += itemEmpleado.FechaNacimiento.ToString("yyyyMMdd") + ",";//F_NACIM
                cadenaLinea += ",";//SEXO
                cadenaLinea += ",";//E_CIVIL
                cadenaLinea += con.FechaReal.ToString("yyyyMMdd") + ",";//I_GRUPO
                cadenaLinea += con.FechaReal.ToString("yyyyMMdd") + ",";//I_EMPRESA
                cadenaLinea += tipoNominaMoper + ",";//F_PAGO
                cadenaLinea += "E,";//T_TRAB //Se usa la "E" como se recomendo por Odesa ya que en el catalogo significa empleado
                cadenaLinea += "0,";//R_PAGO
                cadenaLinea += "\""+UtilsFondoAhorro.claveBanco(itemDatoBancario.IdBanco) + "\",";//BANCO
                var cuentaoclabe = (itemDatoBancario.IdBanco == 2)? itemDatoBancario.CuentaBancaria : itemDatoBancario.Clabe;
                //var cuentaoclabe = (Int32.Parse(itemDatoBancario.CuentaBancaria) != 0)? itemDatoBancario.CuentaBancaria : itemDatoBancario.Clabe;
                cadenaLinea += cuentaoclabe + ",";//CUENTA_B
                var sueldoMensual = Math.Round((con.SD * 30.4M),2);
                cadenaLinea += sueldoMensual.ToString("f2") + ",";//S_MENSUAL
                    sumaSueldoMensual += sueldoMensual;//para la ultima linea
                //var netoNominaDia = itemNomina.TotalNomina / itemNomina.Dias_Laborados;
                //cadenaLinea += Math.Round((netoNominaDia * 30.4M),2).ToString("f2") + ",";//S_NETO // no es necesario siempre y cuando este el sueldo mensual
                //cadenaLinea += itemNomina.TotalNomina + ",";//S_NETO del periodo //verificar
                cadenaLinea += itemEmpleado.RFC+ ",";//RFC
                cadenaLinea += ",";//NIVEL DEL AVAL
                cadenaLinea += ",";//AGUINALDO 
                cadenaLinea += ",";//PRIMA_V 
                cadenaLinea += ",";//PTU 
                cadenaLinea += ",";//FONDO_AH 
                cadenaLinea += ",";//PRESTA_FA 
                cadenaLinea += "";//INCAPACIDAD

                lineas.Add(cadenaLinea);

                reg++;
            }
            //03, numero consecutivo, numero de lineas, sumatoria de idempleados, sumatoria sueldo mensual redondeado a2 por cada suma, una coma por cada casilla para rellenar
            lineas.Add("03," + reg+"," + (reg-1) + "," + sumaIdempleados + "," + sumaSueldoMensual.ToString("f2") + ",,,,,,,,,,,,,,,,,,,,,,,,");

            var pathUsuario2 = Utils.ValidarFolderUsuario(idUsuario, path);
            var archivoMoper = pathUsuario2 + nombreArchivo;
            File.WriteAllLines(archivoMoper, lineas);

            return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;
        }

        /// <summary>
        /// creacionde un csv de confirmacion para el sistema de la empresa odesa de caja de ahorro
        /// </summary>
        /// <returns></returns>
        public string CrearArchivoConfirmacion(string path, int idUsuario, string pathDescarga, NOM_PeriodosPago periodo)
        {
            List<NOM_Nomina_Detalle> listaDetalleNominas = new List<NOM_Nomina_Detalle>();
            List<NOM_Nomina> listaNominas = new List<NOM_Nomina>();
            var nombreArchivo = "";

            using (var context = new RHEntities())
            {

                listaNominas = (from nn in context.NOM_Nomina
                                where nn.IdPeriodo == periodo.IdPeriodoPago
                                select nn).ToList();

                var arrayinominasid = listaNominas.Select(x => x.IdNomina).ToArray();

                int[] arrayConceptosCA = {156, 157, 158, 159, 160, 161, 162 };

                listaDetalleNominas = (from nd in context.NOM_Nomina_Detalle
                                       where arrayinominasid.Contains(nd.IdNomina) &&
                                             arrayConceptosCA.Contains(nd.IdConcepto)
                                       select nd).ToList();

            }
            var arrayEmpresaFiscal = listaNominas.Select(x => x.IdEmpresaFiscal).Distinct().ToArray();//obtiene los id de empresas fiscales en esta nomina que deberia ser solo una
            var numeroEmpresas = arrayEmpresaFiscal.Count();//cuenta las empresas
            int idEmpresaFiscal;
            if (numeroEmpresas > 1)//si es mas de una 
            {
                return null;
            }
            else//si solo es una devuelve el id de la empresa
            {
                idEmpresaFiscal = (int)arrayEmpresaFiscal.FirstOrDefault();
            }

            var numeroEmpresaOdesa = UtilsFondoAhorro.claveEmpresa(idEmpresaFiscal);//devuelve el numero de empresa o 0 en caso de no estar en la lista
            if (numeroEmpresas == 0)//la empresa no pertenece a las dadas de alta en osea
            {
                return null;
            }
            var numeroProceso = UtilsFondoAhorro.numeroProcesoCA(periodo.IdTipoNomina);
            var tipoNomina = UtilsFondoAhorro.formaPagoNombreMoper(periodo.IdTipoNomina);
            var numeroPeriodo = (periodo.NumeroPeriodoCAOdesa != null) ? periodo.NumeroPeriodoCAOdesa.ToString() : "aaaaccc"; // este es unico lugar en el que se usa esta calumna de la base de datos (y al momento de regsitrarlo del archivo de descuentos)

            //nombreArchivo = periodo.Fecha_Fin.ToString("yyyyMMdd") + "CN" + numeroEmpresaOdesa + tipoNomina + periodo.IdPeriodoPago + "CA.txt";//fa: fondo de ahorro - Ca: caja de ahorro
            nombreArchivo = periodo.Fecha_Fin.ToString("yyyyMMdd") + "CN" + numeroEmpresaOdesa + tipoNomina + numeroPeriodo + "CA.txt";//fa: fondo de ahorro - Ca: caja de ahorro

            int sumaIdempleados = 0;
            decimal sumavalorDescuento = 0;
            List<string> lineas = new List<string>();
          
            var reg = 1;//para llevar un contador en los renglones del moper
            foreach (var detalleNomina in listaDetalleNominas)
            {
                var itemNomina = listaNominas.Where(x => x.IdNomina == detalleNomina.IdNomina).FirstOrDefault();

                var cadenaLinea = "02,";//Tipo de Registro
                cadenaLinea += reg.ToString() + ",";//Número de Registro
                cadenaLinea += itemNomina.IdEmpleado.ToString() + ",";//Número de Socio
                    sumaIdempleados += itemNomina.IdEmpleado;//para la ultima linea
                cadenaLinea += "0,";//Campo4
                cadenaLinea += numeroEmpresaOdesa + ",";//Clave de Empresa
                cadenaLinea += periodo.Fecha_Fin.ToString("yyyyMMdd") + ",";//Fecha Movimiento //se tomo en cuenta el fin del periodo por que es la fecha de pago
                cadenaLinea += UtilsFondoAhorro.claveMovimientoDestinoCA(detalleNomina.IdConcepto) + ",";//Clave Movimiento Destino
                    var valorDescuento = Math.Round(detalleNomina.Total, 2);
                cadenaLinea += valorDescuento.ToString("f2") + ",";//Valor del Movimiento
                    sumavalorDescuento += valorDescuento;//para la ultima linea
                cadenaLinea += "0,";//Clave Movimiento Origen
                cadenaLinea += numeroProceso + ",";//Número de Proceso
                cadenaLinea += numeroPeriodo;//Número de Período

                lineas.Add(cadenaLinea);

                reg++;
            }
            //03, numero consecutivo, numero de lineas, sumatoria de idempleados, sumatoria sueldo mensual redondeado a2 por cada suma, una coma por cada casilla para rellenar
            lineas.Add("03," + reg + "," + (reg - 1) + "," + sumaIdempleados + "," + sumavalorDescuento.ToString("f2") + ",,,,,,");

            var pathUsuario2 = Utils.ValidarFolderUsuario(idUsuario, path);
            var archivoMoper = pathUsuario2 + nombreArchivo;
            File.WriteAllLines(archivoMoper, lineas);

            return pathDescarga + "\\" + idUsuario + "\\" + nombreArchivo;
        }
    }
    public class DatosReporteNominas
    {
        public int IdEmpleado { get; set; }
        public int IdNomina { get; set; }
        public string Nombres { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public decimal Percepciones { get; set; }
        public decimal Deducciones { get; set; }
        public decimal Complemento { get; set; }
        public decimal Sueldo { get; set; }
        public decimal SD { get; set; }
        public decimal SDI { get; set; }
        public decimal SBC { get; set; }
        public decimal PensionAlimenticia { get; set; }
        public decimal Subsidio { get; set; }
        public decimal ImpuestoNomina { get; set; }
        public decimal IMSS { get; set; }
        public decimal ISR { get; set; }
        public decimal Infonavit { get; set; }
        public decimal TotalNomina { get; set; }

    }
}
