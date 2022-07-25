using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utils
{
    public static class UtilsFondoAhorro
    {
        /// <summary>
        /// Recibe el indice del tipo de periodo y devuelve la descripcion dada por ODESA
        /// </summary>
        /// <param name="idTiponomina"></param>
        /// <returns></returns>
        public static string formaPago(int idTiponomina) {
            switch (idTiponomina) 
            {
                case 2:
                    return "SEM";
                case 14:
                    return "DEC";
                case 4:
                    return "QUI";
                case 3:
                    return "CAT";
                case 5:
                    return "MEN";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Recibe el indice del tipo de periodo y devuelve la descripcion dada por ODESA para el nombre de el archivo
        /// </summary>
        /// <param name="idTiponomina"></param>
        /// <returns></returns>
        public static string formaPagoNombreMoper(int idTiponomina)
        {
            switch (idTiponomina)
            {
                case 2:
                    return "S";
                case 14:
                    return "D";
                case 4:
                    return "Q";
                case 3:
                    return "C";
                case 5:
                    return "M";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Recibe el indice del tipo de periodo y devuelve la descripcion dada por ODESA para numero de proceso en archivo de confirmacion
        /// </summary>
        /// <param name="idTiponomina"></param>
        /// <returns></returns>
        public static int numeroProcesoCA(int idTiponomina)
        {
            switch (idTiponomina)
            {
                case 2:
                    return 01;
                case 14:
                    return 06;
                case 4:
                    return 02;
                case 3:
                    return 03;
                case 5:
                    return 04;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Recibe el indice de banco y devuelve la descripcion dada por ODESA ya con los espacios en blanco para 10 caracteres
        /// </summary>
        /// <param name="idBanco"></param>
        /// <returns></returns>

        public static string claveBanco(int idBanco)
        {
            switch (idBanco)
            {
                case 1:
                    return "BANAMEX   ";
                case 2:
                    return "BANORTE S.";
                case 3:
                    return "BITAL     ";
                case 4:
                    return "SERFIN    ";
                case 5:
                    return "BBVA-BANCO";
                case 6:
                    return "127       ";
                case 7:
                    return "INBURSA   ";
                case 8:
                    return "BANREGIO  ";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Recibe el indice de banco y devuelve la descripcion dada por ODESA ya con los espacios en blanco para 10 caracteres
        /// </summary>
        /// <param name="idEmpresaFiscal"></param>
        /// <returns></returns>
        public static int claveEmpresa(int? idEmpresaFiscal)
        {
            switch (idEmpresaFiscal)
            {
                case 6://ARRENDADORA DATA MOVIL S.A DE C.V 
                    return 4654;
                case 8://AUTOS DATAMOVIL SA DE CV
                    return 4653;
                default:
                    return 0;
            }
        }

        public static string claveMovimientoDestinoCA(int? idEmpresaFiscal)
        {
            switch (idEmpresaFiscal)
            {
                case 158:// Caja de Ahorro: Ahorro Vista
                    return "001";
                case 159://Caja de Ahorro: Ahorro a Plazo
                    return "010";
                case 160://Caja de Ahorro: Ahorro al Retiro
                    return "040";
                case 161://Caja de Ahorro: Pago de Préstamo
                    return "060";
                case 162://Caja de Ahorro: Pago de Seguros
                    return "070";
                default:
                    return null;
            }
        }

        public static string claveMovimientoArchivoDescuentoCA(string idEmpresaFiscal)
        {
            switch (idEmpresaFiscal)
            {
                case "001":// Caja de Ahorro: Ahorro Vista
                    return "158";
                case "010"://Caja de Ahorro: Ahorro a Plazo
                    return "159";
                case "040"://Caja de Ahorro: Ahorro al Retiro
                    return "160";
                case "060"://Caja de Ahorro: Pago de Préstamo
                    return "161";
                case "070"://Caja de Ahorro: Pago de Seguros
                    return "162";
                default:
                    return null;
            }
        }
    }
}
