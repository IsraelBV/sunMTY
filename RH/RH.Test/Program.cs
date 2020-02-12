using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RH.BLL;
using RH.Entidades;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Nomina.Procesador;
using Nomina.Procesador.Datos;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using SYA.BLL;
using System.Xml.Serialization;

namespace RH.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //testDecimales();

            //XmlReader();

            // GetHorasExtras();

            //GeneradorXml();
            //TestSHA();

            //GetAño();

            //Micomando();

            //  MicomandoApp();

            Micomando2();

            // UsuarioWindows();

            //ExecuteCommand("echo Oscar");

            //ExecuteCommand("salir");

            Console.ReadKey();

        }


        public static void UsoInterfazClase()
        {

            var emMini = new EnviarMiniMsg();
            var emCorreo = new EnviarCorreo();

            var enviadorMensaje = new EnviadorMensaje(emCorreo);
            enviadorMensaje.Enviar("Juan perez");

            // Usando la factoria

            var emObjeto = FactoriaEnviadorMensaje.Factoria("sms");

            var enviadorMensaje2 = new EnviadorMensaje(emObjeto);
            enviadorMensaje2.Enviar("Juan perez");


        }

        public static string Generico<T>(T dato)
        {
            var serializador = new XmlSerializer(typeof(T));

            using (var sw = new StringWriter())
            {
                using (var escritor = XmlWriter.Create(sw))
                {
                    serializador.Serialize(escritor, dato);
                    return sw.ToString();
                }
            }


            Stack<int> os = new Stack<int>();

            os.Push(8);
            os.Push(6);

            int valor = os.Pop();


            HashSet<int> conjunto1 = new HashSet<int>();

            conjunto1.Add(5);
            conjunto1.Add(6);
            conjunto1.Add(5);// no se inserta porque ya existe


            HashSet<int> conjunto2 = new HashSet<int>();

            conjunto1.Add(5);
            conjunto1.Add(8);

            // se obtiene 5,6 y 8
            HashSet<int> union_set1_set2 = new HashSet<int>(conjunto1);
            union_set1_set2.UnionWith(conjunto2);

            // se obtiene solo  el 5
            HashSet<int> interseccion_set1_set2 = new HashSet<int>(conjunto1);
            interseccion_set1_set2.IntersectWith(conjunto2);


            // se obtiene solo  el 6
            HashSet<int> diferencia_set1_set2 = new HashSet<int>(conjunto1);
            diferencia_set1_set2.ExceptWith(conjunto2);


            // se obtiene solo  el 6 y 8
            HashSet<int> diferencia_simetrica_set1_set2 = new HashSet<int>(conjunto1);
            diferencia_simetrica_set1_set2.SymmetricExceptWith(conjunto2);

            // eliminar numeros repetidos
            List<int> numeros = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4, 5, 6, 7, 8, 9, 12, 34, 5, 6, 8 };
            numeros = new HashSet<int>(numeros).ToList();



            string[] arregloStr = new string[] { "uno", "dos", "tres", "cuatro", "cinco" };

            Array.Resize<string>(ref arregloStr, 6);

            arregloStr[5] = "seis";


            // Arreglo bidimensional
            int[,] matrixi = new int[3, 2]
            {
                {1,2 },
                {3,4 },
                {5,6 }

            };

            string[,] matrix = new string[3, 2]
            {
                {"valor1","valor2" },
                {"valor3","valor4" },
                {"valor5","valor6" },
            };


            var filas = matrix.GetLength(0);
            var columnas = matrix.GetLength(1);

            var str = string.Join(",", matrix);

        }

        public static void RecorrerArregloBidimensional(int[,] matrix)
        {
            var filas = matrix.GetLength(0);
            var columnas = matrix.GetLength(1);

            var sb = new StringBuilder();
            var tempFila = new int[matrix.GetLength(1)];


            for(int i= 0; i< filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    tempFila[j] = matrix[i, j];
                }

                sb.Append(string.Join("\t", tempFila));
                sb.Append("");
            }

            Console.WriteLine(sb.ToString());

        }


        public static void UsuarioWindows()
        {
            var r = WindowsIdentity.GetCurrent();

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;

            String UserName = Environment.UserName;
            String Domain = Environment.UserDomainName;

            var animal = new { animal = "Perro", nombre = "Juan", vida = 1 };

            Console.WriteLine($"name:{0} user{1}", UserName, r.User);
        }

        public static void Micomando()
        {
            Process cmd = new Process();
            //cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("echo Oscar");
            cmd.StandardInput.WriteLine("echo Oscar 2");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit(12000);
      
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static void Micomando2()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "notepad.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.Verb = "runas";
            //cmd.StartInfo.UserName = "Administrador";
            //cmd.StartInfo.PasswordInClearText = "Pa$$w0rd1234#";
            cmd.Start();

           // cmd.StandardInput.WriteLine("net user");
           // cmd.StandardInput.WriteLine("net user admin /active:yes");//net user Administrador /active:yes
            //cmd.StandardInput.WriteLine("net user admin P@ssw0rd");

            //cmd.StandardInput.WriteLine("net user Admin Pa$$w0rd1234# /add");
            //cmd.StandardInput.WriteLine("shutdown /s /t 0");

            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit(12000);

            GuardarRe(cmd.StandardOutput.ReadToEnd());
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());


            
        }

        private static void GuardarRe(string r)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"C:\ABAS\AngR.txt"))
            {
                        file.WriteLine(r);
            }
        }

    


        public static void MicomandoApp()
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = @"C:\Users\Administrador\Downloads\TeamViewer_Setup13.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("echo Oscar");
            cmd.StandardInput.WriteLine("echo Oscar 2");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            cmd.WaitForExit(12000);

            cmd.Close();

            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }
        
        //Lanza un proceso con su ventana oculta y devuelve el resultado
        private static string lanzaProceso(string Proceso, string Parametros)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo(Proceso, Parametros);
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false; //No utiliza RunDLL32 para lanzarlo
                                               //Redirige las salidas y los errores
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            Process proc = Process.Start(startInfo); //Ejecuta el proceso
            proc.WaitForExit(); // Espera a que termine el proceso
            string error = proc.StandardError.ReadToEnd();
            if (error != null && error != "") //Error
                throw new Exception("Se ha producido un error al ejecutar el proceso '" + Proceso + "'\n" + "Detalles:\n" + "Error: " + error);
            else //Éxito
                return proc.StandardOutput.ReadToEnd(); //Devuelve el resultado 
        }

        static void ExecuteCommand(string _Command)
        {
            //Indicamos que deseamos inicializar el proceso cmd.exe junto a un comando de arranque. 
            //(/C, le indicamos al proceso cmd que deseamos que cuando termine la tarea asignada se cierre el proceso).
            //Para mas informacion consulte la ayuda de la consola con cmd.exe /? 
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + _Command);
            // Indicamos que la salida del proceso se redireccione en un Stream
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            //Indica que el proceso no despliegue una pantalla negra (El proceso se ejecuta en background)
            procStartInfo.CreateNoWindow = false;
            //Inicializa el proceso
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            //Consigue la salida de la Consola(Stream) y devuelve una cadena de texto
            string result = proc.StandardOutput.ReadToEnd();
            //Muestra en pantalla la salida del Comando
            Console.WriteLine(result);
        }


        public static void GetAño()
        {
            var dt = DateTime.Now;

            var año = dt.Year;

            Console.WriteLine("Año: {0}", año);
        }

        public static void test()
        {
            Empleados emp = new Empleados();
            Sucursales suc = new Sucursales();
            var datos = suc.ListSucEmp(1);
        }


        public static void testDecimales()
        {
            double uno = 7.7645;
            double dos = 4.4250;



            Console.WriteLine();
            Console.WriteLine(Math.Round(uno));
            Console.WriteLine(Math.Round(dos));


            Console.WriteLine();
            Console.WriteLine(uno.ToString("C"));
            Console.WriteLine(dos.ToString("C"));


            Console.WriteLine();
            Console.WriteLine(uno.ToString("C2"));
            Console.WriteLine(dos.ToString("C2"));

            Console.WriteLine();
            Console.WriteLine(uno.ToString("N"));
            Console.WriteLine(dos.ToString("N"));

            Console.WriteLine();
            Console.WriteLine(uno.ToString("N2"));
            Console.WriteLine(dos.ToString("N2"));





            Console.WriteLine();
            Console.WriteLine(Math.Round(uno, 2));
            Console.WriteLine(Math.Round(dos, 2));

            var a3Uno = Math.Round(uno, 3);
            var a3Dos = Math.Round(dos, 3);

            Console.WriteLine("a tres");
            Console.WriteLine(a3Uno);
            Console.WriteLine(a3Dos);

            var a2Uno = Math.Round(a3Uno, 2);
            var a2Dos = Math.Round(a3Dos, 2);

            Console.WriteLine("de tres a dos");
            Console.WriteLine(a2Uno);
            Console.WriteLine(a2Dos);


        }

        public static void XmlReader()
        {
            #region XML string
            var strX = @"<?xml version=""1.0"" encoding=""utf-8""?>
<cfdi:Comprobante xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""3.3"" Serie=""A"" Folio=""57238"" Fecha=""2018-01-18T17:26:42"" Sello=""bN2TofDY4AhEqWqPti4Yaj5uZwDIcEDgeI/tYqHOSD6pUPFkoRgqHXl2l0P1sKFu5V5QCDZF+ucYKKq/5BMyQUAlPY7cp6ml1QdDXzl6aVtP01n1rCxfH9dG3r09MDbb/0cmYUmCmnh9ry7JBZ6Bi/bpWfYWLvZoyURn5aAcpE1ypFgO7JDRsKVAnyWitctkJA5mmKwus0bEdwbIHjg0XGVNl+0gjDPijlL25j8sCXxcIdNbZnsLxDYe8ZINKyn2E96ESvflSVIqHBmOaWJhg3eYP9xI9EzyBt2I2Q138cS8iOjLpC7nIBhuV85FKwuMRtX7pFPVAmfO0c5raqDnpw=="" FormaPago=""99"" NoCertificado=""00001000000408965494"" Certificado=""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"" SubTotal=""717.18"" Descuento=""16.37"" Moneda=""MXN"" Total=""700.81"" TipoDeComprobante=""N"" MetodoPago=""PUE"" LugarExpedicion=""64710"" xsi:schemaLocation=""http://www.sat.gob.mx/cfd/3 http://www.sat.gob.mx/sitio_internet/cfd/3/cfdv33.xsd http://www.sat.gob.mx/nomina12 http://www.sat.gob.mx/sitio_internet/cfd/nomina/nomina12.xsd "" xmlns:cfdi=""http://www.sat.gob.mx/cfd/3"">
  <cfdi:Emisor Rfc=""AIL130725U89"" Nombre=""ABASTECIMIENTO ILIMITADO SA DE CV"" RegimenFiscal=""601"" />
  <cfdi:Receptor Rfc=""REPC910122HGA"" Nombre=""REYNA PEREZ CARLOS ENRIQUE"" UsoCFDI=""P01"" />
  <cfdi:Conceptos>
    <cfdi:Concepto ClaveProdServ=""84111505"" Cantidad=""1"" ClaveUnidad=""ACT"" Descripcion=""Pago de nómina"" ValorUnitario=""717.18"" Importe=""717.18"" Descuento=""16.37"" />
  </cfdi:Conceptos>
  <cfdi:Complemento>
    <nomina12:Nomina xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""1.2"" TipoNomina=""O"" FechaPago=""2018-01-19"" FechaInicialPago=""2018-01-13"" FechaFinalPago=""2018-01-19"" NumDiasPagados=""7"" TotalPercepciones=""659.82"" TotalDeducciones=""16.37"" TotalOtrosPagos=""57.36"" xsi:schemaLocation=""http://www.sat.gob.mx/nomina12 http://www.sat.gob.mx/informacion_fiscal/factura_electronica/Documents/Complementoscfdi/nomina12.xsd"" xmlns:nomina12=""http://www.sat.gob.mx/nomina12"">
      <nomina12:Emisor RegistroPatronal=""Y3926552104"" RfcPatronOrigen=""AIL130725U89"" />
      <nomina12:Receptor Curp=""REPC910122HNLYRR00"" NumSeguridadSocial=""43089118178"" FechaInicioRelLaboral=""2017-03-01"" Antigüedad=""P46W"" TipoContrato=""01"" TipoJornada=""03"" TipoRegimen=""02"" NumEmpleado=""217"" Departamento=""OPERACIONES"" Puesto=""MESERO"" RiesgoPuesto=""3"" PeriodicidadPago=""02"" Banco=""072"" CuentaBancaria=""0499840493"" SalarioBaseCotApor=""98.52"" SalarioDiarioIntegrado=""98.52"" ClaveEntFed=""NLE"" />
      <nomina12:Percepciones TotalSueldos=""659.82"" TotalGravado=""659.82"" TotalExento=""0.00"">
        <nomina12:Percepcion TipoPercepcion=""001"" Clave=""001"" Concepto=""Sueldos"" ImporteGravado=""659.82"" ImporteExento=""0.00"" />
      </nomina12:Percepciones>
      <nomina12:Deducciones TotalOtrasDeducciones=""16.37"">
        <nomina12:Deduccion TipoDeduccion=""001"" Clave=""042"" Concepto=""Seguridad social"" Importe=""16.37"" />
      </nomina12:Deducciones>
      <nomina12:OtrosPagos>
        <nomina12:OtroPago TipoOtroPago=""002"" Clave=""CACR"" Concepto=""Subsidio"" Importe=""57.36"">
          <nomina12:SubsidioAlEmpleo SubsidioCausado=""93.62"" />
        </nomina12:OtroPago>
      </nomina12:OtrosPagos>
    </nomina12:Nomina>
  <tfd:TimbreFiscalDigital xmlns:tfd=""http://www.sat.gob.mx/TimbreFiscalDigital"" xsi:schemaLocation=""http://www.sat.gob.mx/TimbreFiscalDigital http://www.sat.gob.mx/sitio_internet/cfd/TimbreFiscalDigital/TimbreFiscalDigitalv11.xsd"" Version=""1.1"" UUID=""87EE3AAA-E90F-411F-AD22-23A4DCCE70D1"" FechaTimbrado=""2018-01-18T17:26:53"" RfcProvCertif=""DND070112H92"" SelloCFD=""bN2TofDY4AhEqWqPti4Yaj5uZwDIcEDgeI/tYqHOSD6pUPFkoRgqHXl2l0P1sKFu5V5QCDZF+ucYKKq/5BMyQUAlPY7cp6ml1QdDXzl6aVtP01n1rCxfH9dG3r09MDbb/0cmYUmCmnh9ry7JBZ6Bi/bpWfYWLvZoyURn5aAcpE1ypFgO7JDRsKVAnyWitctkJA5mmKwus0bEdwbIHjg0XGVNl+0gjDPijlL25j8sCXxcIdNbZnsLxDYe8ZINKyn2E96ESvflSVIqHBmOaWJhg3eYP9xI9EzyBt2I2Q138cS8iOjLpC7nIBhuV85FKwuMRtX7pFPVAmfO0c5raqDnpw=="" NoCertificadoSAT=""00001000000405908583"" SelloSAT=""d+/Bmp1mQSDZRxibdYBMve+Zo6JcCywNP6Je0v6+URuY159Mee7rSNlgMkfy9EHkgUSUgXfhHOtbUn0ilhgBiyc8yW6zvLHh0utd8VJaAlt8DhPnAzoUDV9twvgBgXWjaVxyAWaxCsIGrn194fegJggRemLzHvpGmiQHhYhG0xqiJqeUGt/fzbrGh63OGRLZA1vqL2VE4bgWp/OveiVtvpixcPgQjmVr+GVBCxUoViWT416lZ/gtxR0u5MLJcT35P0iHFljdtR3pwh2fhseH9mCMP+FigHREWJ5nHhbccDoV8aq97JH39VwMoFG6VX9vkT6JFqzxzSlNSci0VbtMyg=="" />
  </cfdi:Complemento>
</cfdi:Comprobante>";

            #endregion

            DataSet ds = new DataSet();

            //Carga el contenido del XML en el DATASET
            StringReader streamReader = new StringReader(strX);
            // ds.ReadXml(streamReader);
            //   XPathDocument doc = new XPathDocument(streamReader);

            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(strX);


            XElement xr = XElement.Load(streamReader);

            var name = from nm in xr.Descendants("Comprobante")
                           //  where (string)nm.Element("Sex") == "Female"
                       select nm;




            IEnumerable<XElement> marketing =
        from el in xr.Elements("cfdi:Comprobante")
        where (string)el.Attribute("Version") == "3.3"
        select el;

            foreach (XElement el in marketing)
                Console.WriteLine(el);






            XmlNodeList xnList1 = doc2.SelectNodes("/Comprobante [@]Version");

            XmlNodeList xnList = doc2.SelectNodes("/cfdi:Comprobante/");

            XmlNodeList xnList2 = doc2.SelectNodes("/cfdi:Comprobante");

            XmlNodeList xnList3 = doc2.SelectNodes("/cfdi:Comprobante");

            foreach (XmlNode xn in xnList)
            {
                Console.WriteLine(xn.InnerText);
            }



            XPathNavigator nav = doc2.CreateNavigator();
            XPathExpression expr;
            expr = nav.Compile("//cfdi:Comprobante [@]Version");
            XPathNodeIterator iterator = nav.Select(expr);

            //Root
            while (iterator.MoveNext())
            {
                XPathNavigator nav2 = iterator.Current.Clone();
                var root = nav2.Value;

                Console.WriteLine($"Vaersion {root}");

            }



        }


        public static void GetHorasExtras()
        {
            int[] empleados = new[] { 62, 65, 72 };

            NOM_PeriodosPago pp = new NOM_PeriodosPago();
            pp.Fecha_Fin = new DateTime(2018, 01, 31);
            pp.Fecha_Inicio = new DateTime(2018, 01, 16);

            HorasExtrasClass ob = new HorasExtrasClass();
            var lista = ob.GetHotasExtrasParaNomina(empleados, pp);

            foreach (var item in lista)
            {
                Console.WriteLine($" {item.IdEmpleado} {item.Fecha} {item.HorasTotales}");
            }
        }

        private static void GeneradorXml()
        {
            int[] arrayNominas = new[] {58137,
58138,
58139,
58140,
58143,
58144,
58145,
58146,
58147,
58148,
58149,
58150,
58152,
58153,
58154,
58155,
58156,
58157,
58158,
58159,
58160,
58161,
58162,
58163,
58164,
58165,
58166,
58167,
58168,
58170,
58172,
58174,
58175,
58176,
58177,
58178,
58179,
58180,
58181,
58182,
58183,
58184,
58185,
58186,
58187,
58188,
58189,
58190,
58191,
58192,
58193,
58194,
58195,
58196,
58197,
58198,
58199,
58200,
58202};

            int[] arrayNominas1 = new[] { 58147 };

            TimbradoDao objDao = new TimbradoDao();
            var ppago = objDao.GetPeriodoPagoById(2026);

            GlobalConfig gconfig = new GlobalConfig();
            var servidorConfig = gconfig.GetGlobalConfig();

            //TimbradoCore tc = new TimbradoCore(arrayNominas, ppago, ppago.IdEjercicio,false);
            TimbradoCoreV2 tcv2 = new TimbradoCoreV2();

            //Debug.WriteLine("========== V1 ===============================");
            //Debug.WriteLine($"Inicio v1 {DateTime.Now}");
            //var nominasTimbradas = tc.CrearXmlyTimbrado(4, ppago, true);
            //Debug.WriteLine($"Fin v1 {DateTime.Now}");

            Debug.WriteLine("========== V2 ===============================");
            Debug.WriteLine($"Inicio v2 {DateTime.Now}");
            var nominasTimbradasv2 = tcv2.CrearXml(4, ppago, servidorConfig, arrayNominas, false);
            Debug.WriteLine($"Fin v2 {DateTime.Now}");

            Debug.WriteLine($"Inicio Timbrado v2 {DateTime.Now}");
            var listaTim = tcv2.Timbrado33_v2(ppago, arrayNominas, servidorConfig);
            Debug.WriteLine($"Fin Timbrado v2 {DateTime.Now}");


            Console.WriteLine("Fin de la ejecucion...");


        }

        private static void TestSHA()
        {

            string r = HashHelper.SHA1("Hola mundo");
            Console.WriteLine(r);


            string r1 = HashHelper.SHA1(r);
            Console.WriteLine(r1);

        }



        public void metodo1()
        {
        
            int longitudArray = 10;
            double PROF = 0;

            double NTR = 0;
            double[] XL = Enumerable.Repeat(0.00, longitudArray).ToArray();
            double[] DVAR = Enumerable.Repeat(0.00, longitudArray).ToArray();
            double[] WR = Enumerable.Repeat(0.00, longitudArray).ToArray();
            

            Console.WriteLine("seleccione una sarta");
            var dato1 = Console.ReadLine();
            int NVAR = Int32.Parse(dato1);

            Console.WriteLine("seleccione el DIAM EMBOLO");
            var dato2 = Console.ReadLine();
            double DEMB = double.Parse(dato2);


            //SWITCH INICIAL
            switch (NVAR)
            {
                case 44:
                    NTR = 1;
                    XL[0] = PROF;
                    DVAR[0] = 0.5;
                    WR[0] = 0.726;
                    break;
                case 54:
                    //al llegar a este case 54
                    //llamamos al metodo que hace el calculo
                    //QUE SE ENCARGARA DE HACER LA SELECCION INTERNA DENTRO DEL SWITCH
                    var objResultado = SeleccionarDEMB54(DEMB, PROF);
                    
                    //despues de obtener el resultado
                    //guardamos cada propiedad del objeto en su respectiva variable

                    //Pero primero validamos que el objeto no sea null
                    //si es null entonces no hacemos nada

                    if (objResultado != null)
                    {
                        NTR = objResultado.ntr;

                        XL[0] = objResultado.xl1; //
                        XL[1] = objResultado.xl2;

                        DVAR[0] = objResultado.dvar1;
                        DVAR[1] = objResultado.dvar2;

                        WR[0] = objResultado.wr1;
                        WR[1] = objResultado.wr2;
                    }

                    break;
                case 55:

                    break;
                case 64:

                    break;
            }

        }


        //Este será el meotodo que hara como segunda comparacion dentro del switch
        public ResultadoDemb SeleccionarDEMB54(double demb, double prof)
        {
            //Creamos un objeto donde vamos a guardar los valores
            ResultadoDemb objResultado = new ResultadoDemb();
            

            if (demb == 1.0026)
            {
                objResultado.ntr = 2;
                objResultado.xl1 = 0.44 * prof;
                objResultado.xl2 = 0.544 * prof;
                objResultado.dvar1 = 0.625;
                objResultado.dvar2 = 0.5;
                objResultado.wr1 = 1.135;
                objResultado.wr2 = 0.726;
            }
            else if (demb == 1.25)
            {
                //hacer algo
            }
            else if (demb == 1.5)
            {
               //hacer esto
            }
            else if (demb == 1.75)
            {
               //hacer esto
            }


            //Al final retornamos el objeto con los resultado
            return objResultado;

        }

        //Esta Clase solo nos servirá como auxiliar pra guardar varios datos al mismo tiempo
    public   class ResultadoDemb
        {
            public int ntr { get; set; }
            public double xl1 { get; set; }
            public double xl2 { get; set; }
            public double dvar1 { get; set; }
            public double dvar2 { get; set; }
            public double wr1 { get; set; }
            public double wr2 { get; set; }
        }



    }
}
