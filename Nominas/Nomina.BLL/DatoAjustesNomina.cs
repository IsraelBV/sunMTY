using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using RH.Entidades;
using System.Data;
using Common.Utils;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace Nomina.BLL
{
    public class DatoAjustesNomina
    {
        public byte[] CrearLayoutAjuste(int idPeriodoPago)
        {
            //Guarda el archivo en la memoria
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //Crea el libro y la hoja para el Layout
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ajustes");
            var wc = workbook.Worksheets.Add("Conceptos");

            //Crea los Header del layout
            worksheet.Cell(1, 1).Value = "IdE";
            worksheet.Cell(1, 2).Value = "Nombre del Empleado";
            worksheet.Cell(1, 3).Value = "Tipo | IdConcepto | Clave SAT | Tipo Otro Pago | Descripcion";
            worksheet.Cell(1, 4).Value = "Total Concepto";
            worksheet.Cell(1, 5).Value = "Gravado ISR";
            worksheet.Cell(1, 6).Value = "Exento ISR";
            worksheet.Cell(1, 7).Value = "Integra Imss";
            worksheet.Cell(1, 8).Value = "Impuesto Sobre Nomina";

            //Establece un estilo al header
            worksheet.Range("A1:H1").Style
            .Font.SetFontSize(13)
            .Font.SetBold(true)
            .Font.SetFontColor(XLColor.White)
            .Fill.SetBackgroundColor(XLColor.Awesome);



            //Obtiene los datos del empleado
            var ids = GetIdEmpleadosByIdPeriodo(idPeriodoPago);

            if (ids == null) return null;

            var datosEmpleados = GetEmpleadosById(ids);

            if (datosEmpleados == null) return null;

            //Agrega los datos a la hoja
            int row = 2;
            int col = 1;

            //Agregamos la hoja de conceptos
            var rc = GetConceptos(wc);

            foreach (var item in datosEmpleados)
            {
                StringBuilder strb = new StringBuilder();
                strb.Append(item.APaterno);
                strb.Append(" ");
                strb.Append(item.AMaterno);
                strb.Append(" ");
                strb.Append(item.Nombres);
                var colaborador = strb.ToString();

                worksheet.Cell(row, col).Value = item.IdEmpleado;
                worksheet.Cell(row, col + 1).Value = colaborador;

                //agregar validacion
                InsertValidation(rc, 3, worksheet);

                row++;
                col = 1;
            }

            //Agregar validacion a la columna de la cantidad

            //Dar formato adicional al layout
            //Ajustar el header al contenido
            worksheet.Column(1).AdjustToContents();
            worksheet.Column(2).AdjustToContents();
            worksheet.Column(3).AdjustToContents();
            worksheet.Column(4).AdjustToContents();
            worksheet.Column(5).AdjustToContents();
            worksheet.Column(6).AdjustToContents();
            worksheet.Column(7).AdjustToContents();
            worksheet.Column(8).AdjustToContents();




            workbook.SaveAs(ms, false);
            return ms.ToArray();


        }
        public void ImportarDatosAjustes(DataTable dataT, int idPeriodo)
        {
            if (dataT == null || idPeriodo <= 0) return;


            List<NOM_Nomina_Ajuste> listaDeAjustes = new List<NOM_Nomina_Ajuste>();
            int cont = 0;
            int idEmpleado = 0;
            decimal total = 0;
            int idConcepto = 0;
            decimal gravado = 0;
            decimal excento = 0;
            decimal integraimss = 0;
            decimal impuestosn = 0;

            var idEmpArray = GetIdEmpleadosByIdPeriodo(idPeriodo);

            //int[] s = { 1, 2, 3, 3, 4 };
            //int[] q = s.Distinct().ToArray();

            foreach (DataRow row in dataT.Rows)
            {
                idEmpleado = 0;
                total = 0;
                idConcepto = 0;
                gravado = 0;
                excento = 0;
                integraimss = 0;
                impuestosn = 0;

                //valida la columna id empleado
                if (row[0].ToString().Trim() == "") continue;
                int.TryParse(row[0].ToString(), out idEmpleado);
                if (idEmpleado <= 0) continue;

                //Valida columna concepto
                if (row[2].ToString().Trim() == "") continue;

                var concepto = row[2].ToString();

                var arrayConcepto = concepto.Split('|');

                int.TryParse(arrayConcepto[1], out idConcepto);

                if (idConcepto <= 0) continue;

                //Validar total
                if (row[3].ToString().Trim() == "") continue;

                total = decimal.Parse(row[3].ToString());
                gravado = decimal.Parse(row[4].ToString());
                excento = decimal.Parse(row[5].ToString());

                decimal intimss = 0;
                decimal.TryParse(row[6].ToString(), out intimss);
                integraimss = intimss;

                decimal varisn = 0;
                decimal.TryParse(row[7].ToString(), out varisn);
                impuestosn = varisn;

                //Buscamos que el idEmpleado este en el array
                if (!BuscarInArray(idEmpArray, idEmpleado)) continue;

                //Validamos que el total sea igual a la suma del gravado mas exento
                if(total != (gravado+excento)) continue;
                

                NOM_Nomina_Ajuste itemAsimilados = new NOM_Nomina_Ajuste()
                {
                    IdAjuste = 0,
                    IdPeriodo = idPeriodo,
                    IdEmpleado = idEmpleado,
                    IdConcepto = idConcepto,
                    Total = total,
                    GravadoIsr = gravado,
                    ExentoIsr = excento,
                    IntegraImss = integraimss,
                    ImpuestoSobreNomina = impuestosn,
                    FechaReg = DateTime.Now
                };

                listaDeAjustes.Add(itemAsimilados);
                cont++;
            }

            //Borrar anterior
            int[] arrayIdEmpleados = listaDeAjustes.Select(x => x.IdEmpleado).ToArray();

           // BorrarDatoAnteriorDeAjuste(arrayIdEmpleados, idPeriodo);


            //Agregar el dato nuevo
            InsertarRegistrosDeAjuste(listaDeAjustes);

        }
        private int[] GetIdEmpleadosByIdPeriodo(int idPeriodoPago)
        {
            using (var context = new RHEntities())
            {
                var lista = (from t in context.NOM_Empleado_PeriodoPago
                             where t.IdPeriodoPago == idPeriodoPago
                             select t.IdEmpleado).ToArray();
                return lista;
            }
        }
        private List<Empleado> GetEmpleadosById(int[] IdEmpleadosArray)
        {
            if (IdEmpleadosArray == null) return null;

            using (var context = new RHEntities())
            {
                var lista = (from t in context.Empleado
                             where IdEmpleadosArray.Contains(t.IdEmpleado)
                             select t).ToList();

                var nuevaLista = lista.OrderBy(x => x.APaterno).ToList();
                return nuevaLista;
            }
        }
        public List<ModeloAjuste> GetDatosAjuste(int idPeriodo)
        {
            if (idPeriodo <= 0) return null;

            using (var context = new RHEntities())
            {
                var lista = (from na in context.NOM_Nomina_Ajuste
                             join emp in context.Empleado on na.IdEmpleado equals emp.IdEmpleado
                             join con in context.C_NOM_Conceptos on na.IdConcepto equals con.IdConcepto
                             where na.IdPeriodo == idPeriodo
                             select new ModeloAjuste
                             {
                                 IdAjuste = na.IdAjuste,
                                 IdPeriodo = na.IdPeriodo,
                                 IdEmpleado = na.IdEmpleado,
                                 Paterno = emp.APaterno,
                                 Materno = emp.AMaterno,
                                 Nombres = emp.Nombres,
                                 Tipo = con.TipoConcepto,
                                 StrTipo = con.TipoConcepto == 1 ? "P" : "D",
                                 IdConcepto = na.IdConcepto,
                                 Concepto = con.DescripcionCorta,
                                 Total = na.Total,
                                 Gravado = na.GravadoIsr,
                                 Exento = na.ExentoIsr,
                                 IntegraImss = na.IntegraImss,
                                 Isn = na.ImpuestoSobreNomina
                             }).ToList();

                if (lista.Count > 0)
                {
                    return lista.OrderBy(x => x.Paterno).ToList();
                }
            }

            return null;
        }
        public bool BuscarInArray(int[] array, int elemento)
        {
            bool result = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == elemento)
                {
                    result = true;
                }
            }

            return result;
        }
        private void BorrarDatoAnteriorDeAjuste(int[] idEmpleados, int idPeriodo)
        {

            if (idEmpleados == null) return;
            if (idEmpleados.Length <= 0) return;
            //eliminar de nominas
            var idsE = string.Join(",", idEmpleados);
            using (var context = new RHEntities())
            {
                string sqlQuery1 = "DELETE [NOM_Nomina_Ajuste] WHERE IdEmpleado in  (" + idsE + ") and IdPeriodo = @p0";
                context.Database.ExecuteSqlCommand(sqlQuery1, idPeriodo);
            }
        }
        private void InsertarRegistrosDeAjuste(List<NOM_Nomina_Ajuste> lista)
        {
            if (lista.Count > 0)
            {
                using (var context = new RHEntities())
                {
                    context.NOM_Nomina_Ajuste.AddRange(lista);
                    context.SaveChanges();
                }

            }
        }
        public void EliminarAjustes(int[] arrayAjuste, int idPeriodo)
        {
            if (arrayAjuste == null) return;

            if (arrayAjuste.Length <= 0) return;

            var ids = string.Join(",", arrayAjuste);

            using (var context = new RHEntities())
            {

                var periodo = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                if (periodo == null) return;

                if (periodo.Autorizado == true) return;

                var sqlQuery = "DELETE [NOM_Nomina_Ajuste] WHERE [IdAjuste] in  (" + ids + ")";
                context.Database.ExecuteSqlCommand(sqlQuery);
            }

        }

        public IXLRange GetConceptos(IXLWorksheet ws)
        {
            List<C_NOM_Conceptos> listaConceptos;
            using (var context = new RHEntities())
            {
                listaConceptos = context.C_NOM_Conceptos.ToList();
            }

            if (listaConceptos == null) return null;


            List<string> conceptosExcel = new List<string>();

            ws.Cell(1, 1).Value = "Conceptos";
            ws.Cell(1, 2).Value = "Id Concepto";
            ws.Cell(1, 3).Value = "Descripcion";
            ws.Cell(1, 4).Value = "Clave SAT";
            ws.Cell(1, 5).Value = "Tipo Otros Pagos";

            int row = 2;
            string tipo = "+";

            foreach (var item in listaConceptos)
            {

                tipo = item.TipoConcepto == 1 ? "P" : "D";

                conceptosExcel.Add($"{tipo} | {item.IdConcepto} | {item.Clave} | {item.IdTipoOtroPago.ToString().PadLeft(2,'0')} | {item.DescripcionCorta}");

                ws.Cell(row, 2).Value = item.IdConcepto;
                ws.Cell(row, 3).Value = item.Clave;
                ws.Cell(row, 4).Value = item.IdTipoOtroPago;
                ws.Cell(row, 5).Value = item.Descripcion;

                row++;
            }

           
            var rangeConceptos = ws.Cell(2, 1).InsertData(conceptosExcel);

            ws.Column(1).Hide();
            return rangeConceptos;
        }


        private void InsertValidation(IXLRange range, int column, IXLWorksheet ws)
        {
            if (range != null)
            {
                for (int row = 2; row < 102; row++)
                {
                    ws.Cell(row, column).DataValidation.List(range);
                }
            }
        }

    }

    public class ModeloAjuste
    {
        public int IdAjuste { get; set; }
        public int IdPeriodo { get; set; }
        public int IdEmpleado { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Nombres { get; set; }
        public int IdConcepto { get; set; }
        public int Tipo { get; set; }
        public string StrTipo { get; set; }
        public string Concepto { get; set; }
        public decimal Total { get; set; }
        public decimal Gravado { get; set; }
        public decimal Exento { get; set; }
        public decimal IntegraImss { get; set; }
        public decimal Isn { get; set; }
    }
}
