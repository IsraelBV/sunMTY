using RH.Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utils;
using System.Text.RegularExpressions;

namespace Nomina.BLL
{
    
  public class _Layout
    {
        RHEntities ctx = null;

        public _Layout()
        {
            ctx = new RHEntities();
        }
        public List<DatosLayout> listaEmpleados(int idPeriodo, int idEmpresa)
        {
            try

            { 
          


            var empPeriodo = ctx.NOM_Empleado_PeriodoPago.Where(x => x.IdPeriodoPago == idPeriodo).ToList();
            var nombrePeriodo = ctx.NOM_PeriodosPago.Where(x => x.IdPeriodoPago == idPeriodo).Select(x => x.Descripcion).FirstOrDefault();
            List<DatosLayout> empleados = new List<DatosLayout>();
            foreach(var idemp in empPeriodo)
            {
                var dato = (from emp in ctx.Empleado
                            join datosB in ctx.DatosBancarios
                            on emp.IdEmpleado equals datosB.IdEmpleado
                            join nomina in ctx.NOM_Nomina
                            on emp.IdEmpleado equals nomina.IdEmpleado
                            join contrato in ctx.Empleado_Contrato
                            on emp.IdEmpleado equals contrato.IdEmpleado
                            join empresa in ctx.Empresa
                            on contrato.IdEmpresaFiscal equals empresa.IdEmpresa
                            where nomina.IdPeriodo == idPeriodo && nomina.IdEmpleado == idemp.IdEmpleado && datosB.NoSigaF !=0  && empresa.IdEmpresa == idEmpresa && contrato.FormaPago!=1  /*&& empresa.RazonSocial != null*/
                            select new DatosLayout
                            {
                                IdEmpleado = emp.IdEmpleado,
                                NombrePeriodo = nombrePeriodo,
                                NoSiga1 = datosB.NoSigaF,
                                NoSiga2 = datosB.NoSigaC,
                                CuentaBancaria = datosB.CuentaBancaria,
                                Nombres = emp.Nombres,
                                Paterno = emp.APaterno,
                                Materno = emp.AMaterno,
                                Importe = nomina.TotalNomina,
                                Generado = false,
                                NombreEmpresa = empresa.RazonSocial,
                                NoEmisor = empresa.ClaveEmisora_Banco,
                                IdEmpresa = empresa.IdEmpresa,
                                IsComplemento = false,
                                IdBanco =  datosB.IdBanco,
                            }).FirstOrDefault();
                if(dato != null)
                {
                    empleados.Add(dato);
                }

                var empresaC = ctx.Empleado_Contrato.Where(x => x.IdEmpleado == idemp.IdEmpleado /*&& x.Status == true*/).FirstOrDefault();
                if(empresaC != null)
                {
                    var dato2 = (from emp in ctx.Empleado
                                 join datosB in ctx.DatosBancarios
                                 on emp.IdEmpleado equals datosB.IdEmpleado
                                 join nomina in ctx.NOM_Nomina
                                 on emp.IdEmpleado equals nomina.IdEmpleado
                                 join contrato in ctx.Empleado_Contrato
                                 on emp.IdEmpleado equals contrato.IdEmpleado
                                 join empresa in ctx.Empresa
                                 on contrato.IdEmpresaComplemento equals empresa.IdEmpresa
                                 where nomina.IdPeriodo == idPeriodo && nomina.IdEmpleado == idemp.IdEmpleado && datosB.NoSigaC != 0 && empresa.IdEmpresa == idEmpresa && contrato.FormaPago != 1   /*&& empresa.RazonSocial != null*/
                                 select new DatosLayout
                                 {
                                     IdEmpleado = emp.IdEmpleado,
                                     NombrePeriodo = nombrePeriodo,
                                     NoSiga1 = datosB.NoSigaF,
                                     NoSiga2 = datosB.NoSigaC,
                                     CuentaBancaria = datosB.CuentaBancaria,
                                     Nombres = emp.Nombres,
                                     Paterno = emp.APaterno,
                                     Materno = emp.AMaterno,
                                     Importe = nomina.TotalComplemento,
                                     Generado = false,
                                     NombreEmpresa = empresa.RazonSocial,
                                     NoEmisor = empresa.ClaveEmisora_Banco,
                                     IdEmpresa = empresa.IdEmpresa,
                                     IsComplemento = true,
                                        IdBanco = datosB.IdBanco,
                                 }).FirstOrDefault();
                    if (dato2 != null)
                    {
                        empleados.Add(dato2);
                    }
                }

                var empresaS = ctx.Empleado_Contrato.Where(x => x.IdEmpleado == idemp.IdEmpleado && x.Status == true).FirstOrDefault();
                if (empresaS != null)
                {
                    var dato2 = (from emp in ctx.Empleado
                                 join datosB in ctx.DatosBancarios
                                 on emp.IdEmpleado equals datosB.IdEmpleado
                                 join nomina in ctx.NOM_Nomina
                                 on emp.IdEmpleado equals nomina.IdEmpleado
                                 join contrato in ctx.Empleado_Contrato
                                 on emp.IdEmpleado equals contrato.IdEmpleado
                                 join empresa in ctx.Empresa
                                 on contrato.IdEmpresaSindicato equals empresa.IdEmpresa
                                 where nomina.IdPeriodo == idPeriodo && nomina.IdEmpleado == idemp.IdEmpleado && datosB.NoSigaC != 0 && empresa.IdEmpresa == idEmpresa && contrato.FormaPago != 1  /*&& empresa.RazonSocial != null*/
                                 select new DatosLayout
                                 {
                                     IdEmpleado = emp.IdEmpleado,
                                     NombrePeriodo = nombrePeriodo,
                                     NoSiga1 = datosB.NoSigaF,
                                     NoSiga2 = datosB.NoSigaC,
                                     CuentaBancaria = datosB.CuentaBancaria,
                                     Nombres = emp.Nombres,
                                     Paterno = emp.APaterno,
                                     Materno = emp.AMaterno,
                                     Importe = nomina.TotalNomina,
                                     Generado = false,
                                     NombreEmpresa = empresa.RazonSocial,
                                     NoEmisor = empresa.ClaveEmisora_Banco,
                                     IdEmpresa = empresa.IdEmpresa,
                                     IsComplemento = true,
                                     IdBanco = datosB.IdBanco,
                                 }).FirstOrDefault();
                    if (dato2 != null)
                    {
                        empleados.Add(dato2);
                    }
                }

                var empresaA = ctx.Empleado_Contrato.Where(x => x.IdEmpleado == idemp.IdEmpleado && x.Status == true).FirstOrDefault();
                if (empresaA != null)
                {
                    var dato2 = (from emp in ctx.Empleado
                                join datosB in ctx.DatosBancarios
                                on emp.IdEmpleado equals datosB.IdEmpleado
                                join nomina in ctx.NOM_Nomina
                                on emp.IdEmpleado equals nomina.IdEmpleado
                                join contrato in ctx.Empleado_Contrato
                                on emp.IdEmpleado equals contrato.IdEmpleado
                                join empresa in ctx.Empresa
                                on contrato.IdEmpresaAsimilado equals empresa.IdEmpresa
                                where nomina.IdPeriodo == idPeriodo && nomina.IdEmpleado == idemp.IdEmpleado && datosB.NoSigaF != 0 && empresa.IdEmpresa == idEmpresa && contrato.FormaPago != 1 /*&& empresa.RazonSocial != null*/
                                 select new DatosLayout
                                {
                                    IdEmpleado = emp.IdEmpleado,
                                    NombrePeriodo = nombrePeriodo,
                                    NoSiga1 = datosB.NoSigaF,
                                    NoSiga2 = datosB.NoSigaC,
                                    CuentaBancaria = datosB.CuentaBancaria,
                                    Nombres = emp.Nombres,
                                    Paterno = emp.APaterno,
                                    Materno = emp.AMaterno,
                                    Importe = nomina.TotalNomina,
                                    Generado = false,
                                    NombreEmpresa = empresa.RazonSocial,
                                    NoEmisor = empresa.ClaveEmisora_Banco,
                                    IdEmpresa = empresa.IdEmpresa,
                                    IsComplemento = false,
                                    IdBanco = datosB.IdBanco,
                                }).FirstOrDefault();
                    if (dato2 != null)
                    {
                        empleados.Add(dato2);
                    }
                }


            }
            
            return empleados;


            }
            catch (Exception es)
            {

                return null;
            }


        }

        public DatosLayout listaEmpleadosFiniquito(int idPeriodo, int idEmpresa)
        {
            var empPeriodo = ctx.NOM_Empleado_PeriodoPago.Where(x => x.IdPeriodoPago == idPeriodo).FirstOrDefault();
            var nombrePeriodo = ctx.NOM_PeriodosPago.Where(x => x.IdPeriodoPago == idPeriodo).Select(x => x.Descripcion).FirstOrDefault();

            
            var dato = (from emp in ctx.Empleado
                        join datosB in ctx.DatosBancarios
                        on emp.IdEmpleado equals datosB.IdEmpleado
                        join fin in ctx.NOM_Finiquito
                        on emp.IdEmpleado equals fin.IdEmpleado
                        join contrato in ctx.Empleado_Contrato
                        on emp.IdEmpleado equals contrato.IdEmpleado
                        join empresa in ctx.Empresa
                        on contrato.IdEmpresaFiscal equals empresa.IdEmpresa
                        where fin.IdPeriodo == idPeriodo && fin.IdEmpleado == empPeriodo.IdEmpleado && datosB.NoSigaF != 0 && empresa.IdEmpresa == idEmpresa  /*&& empresa.RazonSocial != null*/
                        select new DatosLayout
                        {
                            IdEmpleado = emp.IdEmpleado,
                            NombrePeriodo = nombrePeriodo,
                            NoSiga1 = datosB.NoSigaF,
                            NoSiga2 = datosB.NoSigaC,
                            CuentaBancaria = datosB.CuentaBancaria,
                            Nombres = emp.Nombres,
                            Paterno = emp.APaterno,
                            Materno = emp.AMaterno,
                            Importe = fin.TOTAL_total,
                            Generado = false,
                            NombreEmpresa = empresa.RazonSocial,
                            NoEmisor = empresa.ClaveEmisora_Banco,
                            IdEmpresa = empresa.IdEmpresa,
                            IsComplemento = false
                        }).FirstOrDefault();

            return dato;
        
        }
        public List<Empresa> empresas (int idSucursal)
        {
            var empresa = (from suc in ctx.Sucursal
                           join suc_emp in ctx.Sucursal_Empresa
                           on suc.IdSucursal equals suc_emp.IdSucursal
                           join emp in ctx.Empresa
                           on suc_emp.IdEmpresa equals emp.IdEmpresa
                           where suc.IdSucursal == idSucursal
                           select emp).ToList();
            return empresa;
        }



        public string [] GenerarLayout(string pathTxt, encabezado Encabezado, int idUsuario, List<detallado> Detalle,List<emisoras> emisoras)
        {
            int count = Detalle == null ? 0: Detalle.Count;
            
            List<int> emi= new List<int>() ;
            
            int j = 0;
            int c = 0;
            
            
            decimal importetotal = Utils.TruncateDecimalesAbc(Encabezado.ImporteTotal,2);
            
            //string imptotal = Regex.Replace(importetotal.ToString(), @".", "");
            string imptotal = importetotal.ToString().Replace(".", "");
            ////validar que el directorio para el txt este creado, sino se crea
            var newruta = ValidarFolderUsuario(idUsuario, pathTxt);
           
            var grupo = emisoras.GroupBy(u => u.NoEmisor).Select(grp => grp.FirstOrDefault()).ToList();
            int countEmi = emisoras == null ? 0 : grupo.Count;
            string[] archivoTxt = new string[countEmi];
            foreach (var emisor in grupo)
            {
                //string[] lines = new string[count + 1];
                List<string> lines = new List<string>();
                //int i = 1;
                
                string encabezadoFinal = Encabezado.TipoRegistro + "" + Encabezado.ClaveServicio + ""+emisor.NoEmisor+""+Encabezado.fecha+""+addCeros((Encabezado.consecutivo+c),2)+""+addCeros(Encabezado.TotalEmpleados,6)+""+addCeros(Convert.ToInt32(imptotal), 15)
                    +""+addCeros(0,6) + "" + addCeros(0, 15) + "" + addCeros(0, 6) + "" + addCeros(0, 15) + "" + addCeros(0, 6)+""+0+"    " + "" + addCeros(0, 8)+"" + addCeros(0, 10) + "" + addCeros(0, 55);
                 archivoTxt[j] = pathTxt + "NI"+emisor.NoEmisor+addCeros((Encabezado.consecutivo+c),2)+".PAG";
                //lines[0] = encabezadoFinal;
                lines.Add(encabezadoFinal);
                foreach(var d in Detalle.Where(x=>x.NoEmisor == emisor.NoEmisor))
                {
                    string impDetalle = Extensores.ToCurrencyFormat(d.Importe);

                    string impDGeneral = impDetalle.ToString().Replace(".", "");
                     impDGeneral = impDGeneral.ToString().Replace(",", "");
                    string detallado = d.TipoRegistro + "" + d.fecha + "" + addCeros(d.NoSiga1, 10) + "                                                                                " + "" + addCeros(Convert.ToInt32(impDGeneral), 15) + "" + addCeros(d.Banco, 3)
                        + "" + addCeros(d.TipoCuenta, 2) + "" + addCeros(d.CuentaBancaria, 18) + "0 " + "" + addCeros(0, 8) + "                  ";
                    //lines[i] = detallado;
                    lines.Add(detallado);
                    //i++;
                }
                File.WriteAllLines(archivoTxt[j], lines.ToArray());
                j++;
                c++;
            }
         


            return archivoTxt;

        }
        private string ValidarFolderUsuario(int idUsuario, string pathFolder)
        {

            //var pathArchivos = @"C:\Sites\Nominas\Nomina.WEB\Files\DownloadRecibos";
            //var pathArchivos = HttpContext.Current.Server.MapPath("~/Files/DownloadRecibos/");
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
       public string addCeros(int n,int length)
        {
            var str = (n > 0 ? n : -n) + "";
            var zeros = "";
            for (var i = length - str.Length; i > 0; i--)
                zeros += "0";
            zeros += str;
            return n >= 0 ? zeros : "-" + zeros;
        }

    }

    public class DatosLayout
    {
        public int IdEmpleado { get; set; }
        public string NombrePeriodo { get; set; }
        public int? NoSiga1 { get; set; }
        public int? NoSiga2 { get; set; }
        public string CuentaBancaria { get; set; }
        public string Nombres { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public decimal Importe { get; set; }
        public bool Generado { get; set; }
        public string NombreEmpresa { get; set; }
        public int IdEmpresa { get; set; }
        public int? NoEmisor { get; set; }
        public bool IsComplemento { get; set; }
        public int IdBanco { get; set; }
    }

    public class encabezado
    {
        public string TipoRegistro { get; set; }
        public string ClaveServicio { get; set; }
        public int fecha { get; set; }
        public int consecutivo { get; set; }
        public decimal ImporteTotal { get; set; }
        public int TotalEmpleados { get; set; }
    }
    public class detallado
    {
        public int NoSiga1 { get; set; }
        public int NoSiga2 { get; set; }
        public decimal Importe { get; set; }
        public int Banco { get; set; }
        public int TipoCuenta { get; set; }
        public string TipoRegistro { get; set; }
        public int CuentaBancaria { get; set; }
        public int fecha { get; set; }
        public int NoEmisor { get; set; }

    }
    public class emisoras
    {
        public int NoEmisor { get; set; }
    }

}
