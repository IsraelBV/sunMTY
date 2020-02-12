using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using Common.Utils;
using Nomina.Procesador.Datos;
using Nomina.Procesador.Modelos;
using RH.Entidades;

namespace Nomina.Procesador.Metodos
{
    public static class MNominas
    {
        static readonly NominasDao _nominasDao = new NominasDao();
        static readonly RHDAO _rhDao = new RHDAO();

        //public static Task<TotalIsrSubsidio> CalculoDeIsr(NOM_Nomina nomina, decimal salarioBase, int tipoNomina, int diasPeriodo)
        //{

        //    var t = Task.Factory.StartNew(() =>
        //    {
        //        IsrSubsidio objCalculoSubsidio;

        //        if (tipoNomina == 16) //Asimilado
        //        {
        //            objCalculoSubsidio = CalculoIsrAsimilado(salarioBase, nomina.SD, diasPeriodo, 4);//5 tarifa mensual// se cambio a 4 por peticion de rodolfo y sergio
        //        }
        //        else
        //        {
        //            objCalculoSubsidio = CalculoIsrSubsidio(salarioBase, nomina.SD, diasPeriodo, tipoNomina);
        //        }

        //        if (objCalculoSubsidio != null)
        //        {
        //            //SE crea el objeto total del calculo
        //            var totalConcepto = new TotalIsrSubsidio
        //            {
        //                SubsidioCausado = objCalculoSubsidio.Subsidio,
        //                SubsidioEntregado = 0,
        //                IsrAntesSubsidio = objCalculoSubsidio.Isr,
        //                IsrCobrado = 0
        //            };


        //            //Si Resultado es ISR, Subsidio se guarda en Cero
        //            if (objCalculoSubsidio.EsISR)
        //            {
        //                totalConcepto.IsrCobrado = objCalculoSubsidio.ResultadoIsrOSubsidio;
        //                GuardarConcepto(nomina.IdNomina, 43, objCalculoSubsidio.ResultadoIsrOSubsidio, 0, 0, 0, 0);
        //                GuardarConcepto(nomina.IdNomina, 144, 0, 0, 0, 0, 0);
        //            }
        //            else  //Si Resultado es Subsidio, ISR se guarda en Cero
        //            {
        //                totalConcepto.SubsidioEntregado = objCalculoSubsidio.ResultadoIsrOSubsidio;
        //                GuardarConcepto(nomina.IdNomina, 144, (objCalculoSubsidio.ResultadoIsrOSubsidio), 0, 0, 0, 0);
        //                GuardarConcepto(nomina.IdNomina, 43, 0, 0, 0, 0, 0);
        //            }


        //            return totalConcepto;
        //        }
        //        else
        //        {
        //            var totalConcepto = new TotalIsrSubsidio
        //            {
        //                SubsidioCausado = 0,
        //                SubsidioEntregado = 0,
        //                IsrAntesSubsidio = 0,
        //                IsrCobrado = 0
        //            };

        //            return totalConcepto;
        //        }

        //    });

        //    return t;
        //}


            //usado para el calculo del isr de la nomina - 2 metodo nomina - son 2 porque ultimo se fuerza a que sea la tabla mensual
            //usando para el calculo isr del asimilado - 1
        public static TotalIsrSubsidio CalculoDeIsr(NOM_Nomina nomina, decimal salarioBase, int tipoNomina, int diasPeriodo)
        {


            IsrSubsidio objCalculoSubsidio;

            if (tipoNomina == 16) //Asimilado
            {
                objCalculoSubsidio = CalculoIsrAsimilado(salarioBase, nomina.SD, diasPeriodo, 4);//5 tarifa mensual// se cambio a 4 por peticion de rodolfo y sergio
            }
            else
            {
                
               //objCalculoSubsidio = CalculoIsrSubsidioFin(nomina, salarioBase, nomina.SD, diasPeriodo, tipoNomina);
                objCalculoSubsidio = CalculoIsrSubsidio304(nomina, salarioBase, nomina.SD, diasPeriodo, tipoNomina);//factor
               
            }

            if (objCalculoSubsidio != null)
            {
                //SE crea el objeto total del calculo
                var totalConcepto = new TotalIsrSubsidio
                {
                    SubsidioCausado = objCalculoSubsidio.Subsidio, //GetSubsidioCausadoByTipoNomina(tipoNomina,salarioBase),
                    SubsidioEntregado = 0,
                    IsrAntesSubsidio = objCalculoSubsidio.IsrAntesDeSub,
                    IsrCobrado = 0
                };


                //Si Resultado es ISR, Subsidio se guarda en Cero
                if (objCalculoSubsidio.EsISR)
                {
                    totalConcepto.IsrCobrado = objCalculoSubsidio.ResultadoIsrOSubsidio;
                    GuardarConcepto(nomina.IdNomina, 43, objCalculoSubsidio.ResultadoIsrOSubsidio, 0, 0, 0, 0);
                    GuardarConcepto(nomina.IdNomina, 144, 0, 0, 0, 0, 0);
                }
                else  //Si Resultado es Subsidio, ISR se guarda en Cero
                {
                    totalConcepto.SubsidioEntregado = objCalculoSubsidio.ResultadoIsrOSubsidio;
                    GuardarConcepto(nomina.IdNomina, 144, (objCalculoSubsidio.ResultadoIsrOSubsidio), 0, 0, 0, 0);
                    GuardarConcepto(nomina.IdNomina, 43, 0, 0, 0, 0, 0);
                }


                return totalConcepto;
            }
            else
            {
                var totalConcepto = new TotalIsrSubsidio
                {
                    SubsidioCausado = 0,
                    SubsidioEntregado = 0,
                    IsrAntesSubsidio = 0,
                    IsrCobrado = 0
                };

                return totalConcepto;
            }

        }

        private static void GuardarConcepto(int idNomina = 0, int idConcepto = 0, decimal total = 0, decimal gravaIsr = 0, decimal excentoIsr = 0, decimal integraImss = 0, decimal impuestoNomina = 0)
        {
            var nd = new NOM_Nomina_Detalle()
            {
                Id = 0,
                IdNomina = idNomina,
                IdConcepto = idConcepto,
                Total = total,
                GravadoISR = gravaIsr,
                ExentoISR = excentoIsr,
                IntegraIMSS = integraImss,
                ExentoIMSS = 0,
                ImpuestoSobreNomina = impuestoNomina
            };

            _nominasDao.AddDetalleNomina(nd);
        }

        /// <summary>
        /// Método que realiza el cálculo del IRS/SUBSIDIO Retorna un objeto "IsrSubsidio" con los valores del cálculo.
        /// tipoTarifa: Diario, semanal, quincenal, etc..
        /// 1.- Usado en Detalle de la nomina 
        /// 2.- Fininiquito - 
        /// 3.- finiquito F
        /// 4.- ultimo sueldo
        /// 5- en este mismo archivo linea 96
        /// </summary>
        /// <param name="nomina"></param>
        /// <param name="baseGravable"></param>
        /// <param name="diasPeriodo"></param>
        /// <param name="tipoTarifa"></param>
        /// <returns></returns>
        public static IsrSubsidio CalculoIsrSubsidioFin(NOM_Nomina nomina,decimal baseGravable, decimal SD, int diasPeriodo, int tipoTarifa, bool incluirTablas = false)
        {
            ////ASimilados a salario
            //if (tipoTarifa == 16) //Asimilado a Salarios - tipo Tarifa viene con el valor de Periodicidad de Pago
            //{
            //    if (diasPeriodo > 15)//Aqui habria que asegurarse de que tarifa quieren aplicar para el calculo
            //    {
            //        tipoTarifa = 5;//Lo cambiamos al tipo de tarifa mensual    
            //    }
            //    else if (diasPeriodo == 15)
            //    {
            //        tipoTarifa = 4; //Lo cambiamos al tipo de tarifa quincenal 
            //    }
            //    else
            //    {
            //        tipoTarifa = 2; //Lo cambiamos al tipo de tarifa semanal 
            //    }
            //}

            if (baseGravable <= 0) return null;
            if (tipoTarifa <= 0) return null;

            bool esISR = false;
            double variableMensual = 30.40;
            double nuevaBaseGravable = 0.00;
            double holdBaseGravable = 0.00;
            decimal subsidio14 = 0;
            decimal isr14 = 0;

            //Regla 14 dias - para nominas catorcenales
            if (diasPeriodo == 14)
            {
                //Se obtiene el sueldo mensual
                nuevaBaseGravable = ((double)SD * variableMensual);
                holdBaseGravable = (double)baseGravable;
                baseGravable = (decimal)nuevaBaseGravable;
                tipoTarifa = 5;//mensual
            }

            List<C_NOM_Tabla_ISR> tablaisrcompleta = null;
            List<C_NOM_Tabla_Subsidio> tablasubsidiocompleta = null;

            var tablaIsr = NominasDao.GeTablaIsrByTipoNomina(tipoTarifa, baseGravable);
            var tablaSubsidio = NominasDao.GetTablaSubsidioByTipoNomina(tipoTarifa, baseGravable);

            if (tablaIsr == null || tablaSubsidio == null) return null;

            if (incluirTablas)
            {
                tablaisrcompleta = NominasDao.GetAllTablaIsr(tipoTarifa);
                tablasubsidiocompleta = NominasDao.GetAllTablaSubsidios(tipoTarifa);
            }

            //1) Buscar el rango de limite inferior
            decimal limiteInferior = tablaIsr.Limite_Inferior;

            //2) Restar Ingreso - Limite Inferior = BASE
            decimal _base = baseGravable - limiteInferior;

            //3) Tomar el porcentaje del Rango %
            decimal porcentaje = tablaIsr.Porcentaje;

            //4) Multiplicar el % por la BASE
            decimal resultado = _base * (porcentaje / 100);

            //5)Tomar la cuota fija del rango
            decimal cuotaFija = tablaIsr.Cuota_Fija;

            //6 Sumar 4) + 5) = ISR
            decimal isr = resultado + cuotaFija;

            // 7) buscar en la tabla de subsidio en que rango esta el Salario Gravable
            decimal subsidioAlEmpleo = tablaSubsidio.Subsidio;

            //7.1 proporcional para el periodo de 14 dias 
            if (diasPeriodo == 14)
            {
                isr14 = (isr / (decimal)variableMensual) * diasPeriodo;
                subsidio14 = (subsidioAlEmpleo / (decimal)variableMensual) * diasPeriodo;
                baseGravable = (decimal)holdBaseGravable;
            }


            //8) Resta del 6) - 7) = ISR o Subsidio
            decimal isrOSubsidio = 0;
            if (isr > subsidioAlEmpleo)
            {
                isrOSubsidio = (isr - subsidioAlEmpleo);

                if (diasPeriodo == 14)
                {
                    isrOSubsidio = (isr14 - subsidio14);
                }
            }
            else
            {
                isrOSubsidio = (subsidioAlEmpleo - isr);

                if (diasPeriodo == 14)
                {
                    isrOSubsidio = (subsidio14 - isr14);
                }
            }


            //9) Neto a pagar Salario Gravable - 8)
            decimal netoPagar = 0;
            esISR = isr > subsidioAlEmpleo;

            if (esISR)
            {
                netoPagar = baseGravable - (isrOSubsidio);
            }
            else
            {
                netoPagar = baseGravable + (isrOSubsidio);
            }

            esISR = isr > subsidioAlEmpleo;

            var item = new IsrSubsidio()
            {

                BaseGravable = baseGravable,
                BaseGravableMensual = (decimal)nuevaBaseGravable,
                LimiteInferior = limiteInferior,
                Base = _base,
                Tasa = porcentaje,
                IsrAntesDeSub = isr,
                Subsidio = subsidioAlEmpleo,
                NetoAPagar = netoPagar,
                ResultadoIsrOSubsidio =Common.Utils.Utils.TruncateDecimales(isrOSubsidio),
                IdTablaIsr = tablaIsr.IdISR,
                IdTablaSubsidio = tablaSubsidio.IdSubsidio,
                Tablaisr = incluirTablas ? tablaisrcompleta : null,
                Tablasubsidio = incluirTablas ? tablasubsidiocompleta : null,
                EsISR = esISR
            };

            return item;
        }

        //Version 2 - 30.4
        //Method Name - CalculoIsrSubsidio304
        //
        public static IsrSubsidio CalculoIsrSubsidio304(NOM_Nomina nomina, decimal baseGravable, decimal SD, int diasPeriodo, int tipoTarifa, bool incluirTablas = false)
        {


            if (baseGravable <= 0) return null;
            //if (tipoTarifa <= 0) return null;

            bool esISR = false;
            decimal factor304 = 30.40M;
            decimal nuevaBaseGravable = 0;
            decimal holdBaseGravable = 0;
            decimal subsidio304 = 0;
            decimal isr304 = 0;

            //Regla 14 dias - para nominas catorcenales***********************************************************
            //version 30.4
            //Se obtiene el sueldo mensual

            //nuevaBaseGravable = (SD * variableMensual);//Obtenemos el sueldo mensual SD * 30.4
            //holdBaseGravable = baseGravable; //Guardamos la baseGravable anterior
            //baseGravable = (decimal)nuevaBaseGravable;


            nuevaBaseGravable = (baseGravable / nomina.Dias_Laborados) * factor304;//nueva base a 30.4
            holdBaseGravable = baseGravable; //Guardamos la baseGravable anterior
            baseGravable = (decimal)nuevaBaseGravable;// establecemos como base de calculo la nueva base encontrada
            //******************************************************************************************************


            tipoTarifa = 5;//mensual


            List<C_NOM_Tabla_ISR> tablaisrcompleta = null;
            List<C_NOM_Tabla_Subsidio> tablasubsidiocompleta = null;

            var tablaIsr = NominasDao.GeTablaIsrByTipoNomina(tipoTarifa, baseGravable);
            var tablaSubsidio = NominasDao.GetTablaSubsidioByTipoNomina(tipoTarifa, baseGravable);

            if (tablaIsr == null || tablaSubsidio == null) return null;

            if (incluirTablas)
            {
                tablaisrcompleta = NominasDao.GetAllTablaIsr(tipoTarifa);
                tablasubsidiocompleta = NominasDao.GetAllTablaSubsidios(tipoTarifa);
            }

            //1) Buscar el rango de limite inferior
            decimal limiteInferior = tablaIsr.Limite_Inferior;

            //2) Restar Ingreso - Limite Inferior = BASE
            decimal _base = baseGravable - limiteInferior;

            //3) Tomar el porcentaje del Rango %
            decimal porcentaje = tablaIsr.Porcentaje;

            //4) Multiplicar el % por la BASE
            decimal resultado = _base * (porcentaje / 100);

            //5)Tomar la cuota fija del rango
            decimal cuotaFija = tablaIsr.Cuota_Fija;

            //6 Sumar 4) + 5) = ISR
            decimal isr = resultado + cuotaFija;

            // 7) buscar en la tabla de subsidio en que rango esta el Salario Gravable
            decimal subsidioAlEmpleo = tablaSubsidio.Subsidio;

            //7.1 proporcional para el periodo de 14 dias 

            //version 30.4
            isr304 = Utils.TruncateDecimales((isr / (decimal)factor304) * nomina.Dias_Laborados);
            subsidio304 = Utils.TruncateDecimales((subsidioAlEmpleo / (decimal)factor304) * nomina.Dias_Laborados);
            baseGravable = (decimal)holdBaseGravable;



            //8) Resta del 6) - 7) = ISR o Subsidio
            decimal isrOSubsidio = 0;
            if (isr > subsidioAlEmpleo)
            {
                isrOSubsidio = (isr - subsidioAlEmpleo);

                //version 30.4
                isrOSubsidio = (isr304 - subsidio304);
                
            }
            else
            {
                isrOSubsidio = (subsidioAlEmpleo - isr);

                //version 30.4
                isrOSubsidio = (subsidio304 - isr304);
                
            }


            //9) Neto a pagar Salario Gravable - 8)
            decimal netoPagar = 0;
            esISR = isr > subsidioAlEmpleo;

            if (esISR)
            {
                netoPagar = baseGravable - (isrOSubsidio);
            }
            else
            {
                netoPagar = baseGravable + (isrOSubsidio);
            }

            esISR = isr > subsidioAlEmpleo;

            var item = new IsrSubsidio()
            {

                BaseGravable = baseGravable,
                BaseGravableMensual = (decimal)nuevaBaseGravable,
                LimiteInferior = limiteInferior,
                Base = _base,
                Tasa = porcentaje,
                IsrAntesDeSub = isr304,
                Subsidio = subsidio304,//subsidioAlEmpleo
                NetoAPagar = Utils.TruncateDecimales(netoPagar),
                ResultadoIsrOSubsidio = Common.Utils.Utils.TruncateDecimales(isrOSubsidio),
                IdTablaIsr = tablaIsr.IdISR,
                IdTablaSubsidio = tablaSubsidio.IdSubsidio,
                Tablaisr = incluirTablas ? tablaisrcompleta : null,
                Tablasubsidio = incluirTablas ? tablasubsidiocompleta : null,
                EsISR = esISR
            };

            return item;
        }

        public static IsrSubsidio CalculoIsrAsimilado(decimal baseGravable, decimal SD, int diasPeriodo, int tipoTarifa, bool incluirTablas = false)
        {


            if (baseGravable <= 0) return null;
            if (tipoTarifa <= 0) return null;

            bool esISR = false;

            double nuevaBaseGravable = 0.00;

            List<C_NOM_Tabla_ISR> tablaisrcompleta = null;
            List<C_NOM_Tabla_Subsidio> tablasubsidiocompleta = null;

            var tablaIsr = NominasDao.GeTablaIsrByTipoNomina(tipoTarifa, baseGravable);
            //  var tablaSubsidio = NominasDao.GetTablaSubsidioByTipoNomina(tipoTarifa, baseGravable);

            if (tablaIsr == null) return null;

            if (incluirTablas)
            {
                tablaisrcompleta = NominasDao.GetAllTablaIsr(tipoTarifa);
                tablasubsidiocompleta = NominasDao.GetAllTablaSubsidios(tipoTarifa);
            }

            //1) Buscar el rango de limite inferior
            decimal limiteInferior = tablaIsr.Limite_Inferior;

            //2) Restar Ingreso - Limite Inferior = BASE
            decimal _base = baseGravable - limiteInferior;

            //3) Tomar el porcentaje del Rango %
            decimal porcentaje = tablaIsr.Porcentaje;

            //4) Multiplicar el % por la BASE
            decimal resultado = _base * (porcentaje / 100);

            //5)Tomar la cuota fija del rango
            decimal cuotaFija = tablaIsr.Cuota_Fija;

            //6 Sumar 4) + 5) = ISR
            decimal isr = resultado + cuotaFija;

            // 7) buscar en la tabla de subsidio en que rango esta el Salario Gravable
            decimal subsidioAlEmpleo = 0; // tablaSubsidio.Subsidio;


            //8) Resta del 6) - 7) = ISR o Subsidio
            decimal isrOSubsidio = 0;

            //9) Neto a pagar Salario Gravable - 8)
            decimal netoPagar = baseGravable - isr;

            isrOSubsidio = isr;
            esISR = true;

            var item = new IsrSubsidio()
            {
                BaseGravable = baseGravable,
                BaseGravableMensual = (decimal)nuevaBaseGravable,
                LimiteInferior = limiteInferior,
                Base = _base,
                Tasa = porcentaje,
                IsrAntesDeSub = isr,
                Subsidio = subsidioAlEmpleo,
                NetoAPagar = netoPagar,
                ResultadoIsrOSubsidio = Utils.TruncateDecimales(isrOSubsidio),
                IdTablaIsr = tablaIsr.IdISR,
                IdTablaSubsidio = 0,//tablaSubsidio.IdSubsidio,
                Tablaisr = incluirTablas ? tablaisrcompleta : null,
                Tablasubsidio = incluirTablas ? tablasubsidiocompleta : null,
                EsISR = esISR
            };

            return item;
        }
        public static int GetInasistenciasByIdEmpleado(int idEmpleado)
        {
            return 0; //idEmpleado <= 0 ? 0 : _rhDao.GetInasistenciasByIdEmpleado(idEmpleado);
        }

        public static decimal GetSubsidioCausadoByTipoNomina(int tipoNomina, decimal sbc)
        {
            if (tipoNomina == 16) { return 0; }
            var tablaSubsidio = NominasDao.GetTablaSubsidioByTipoNomina(tipoNomina, sbc);

            return tablaSubsidio.Subsidio;
        }
    }
}
