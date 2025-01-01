﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nomina.Procesador.Datos;
using RH.Entidades;
using Common.Enums;
using Common.Utils;
using RH.BLL;//para obtener las inasistencias
using RH.Entidades.GlobalModel;

namespace Nomina.Procesador.Metodos
{
    public static class MAguinaldo
    {
        static readonly NominasDao _NominasDao = new NominasDao();
        //static readonly NominasDao _nominasDao = new NominasDao();
        public static Task<List<NotificationSummary>> GenerarAguinaldo(int[] arrayIdEmpleado, string[] faltasCapturadas, bool[] generarPensionAlimenticia, int idPeriodo, int idCliente, int idSucursal, int idUsuario, string[] dac)
        {
            List<NotificationSummary> summaryList = new List<NotificationSummary>();

            var t = Task.Factory.StartNew(() => CalcularAguinaldoA(arrayIdEmpleado, faltasCapturadas, generarPensionAlimenticia, idPeriodo, idCliente, idSucursal, idUsuario, dac));

            return t;
        }
        private static List<NotificationSummary> CalcularAguinaldoA(int[] arrayIdEmpleado, string[] faltasCapturadas, bool[] generarPensionAlimenticia, int idPeriodo, int idCliente, int idSucursal, int idUsuario, string[] dac)
        {
            List<NotificationSummary> summaryList = new List<NotificationSummary>();
         
            //VARIABLES
            decimal sm = 0;
            int dias = 30;
            decimal sd = 0;
            decimal sdi = 0;
            decimal sdr = 0;
            int diasAguinaldo = 15;//default
            int diasEjercicio = 365;
            DateTime primerDia;
            DateTime fechaUltimoDia;
            DateTime fechaIngreso = DateTime.Now;
            int faltas = 0;
            int faltaPersonalizada = -1;
            int diasTrabajados = 0;
            decimal sueldoMensual = 0;
            decimal proporcion = 0;
            decimal topeExento = 0;
            decimal aguinaldo = 0;
            decimal parteExento = 0;
            decimal parteGravado = 0;
            decimal isr = 0;
            decimal neto = 0;
            // decimal complemento = 0;
            decimal total = 0;
            decimal iSobreNomina = 0;
            decimal factor = 0;
            decimal antiguedad = 0;
            decimal IngresoGravable = 0;
            decimal IngresoGravable304 = 0;
            decimal parteComplemento = 0;
            decimal proporcionComplemento = 0;
            int diasTrabajadosComplemento = 0;
            DateTime primerDiaC;

            List<NOM_Nomina> listaNominasGeneradas = new List<NOM_Nomina>();

            //Obtenemos los contratos
            List<Empleado_Contrato> listaContratos = new List<Empleado_Contrato>();
            List<C_FactoresIntegracion> listaFactorIntegracion = new List<C_FactoresIntegracion>();
            ZonaSalario itemZonaSalario = new ZonaSalario();
            ParametrosConfig itemConfigISN = new ParametrosConfig();
            NOM_PeriodosPago periodoPago = new NOM_PeriodosPago();

            List<Incapacidades> listaIncapacidades = new List<Incapacidades>();
            List<Inasistencias> listaInasistencias = new List<Inasistencias>();
            List<Permisos> listaPermisos = new List<Permisos>();

            List<NOM_Empleado_Complemento> listaDatosComplementosDelPeriodo = null;

            Sucursal itemSucursal = new Sucursal();

            if (arrayIdEmpleado == null)
                return null;

            if (arrayIdEmpleado.Length <= 0)
                return null;

            //VALIDACION TIMBRADOS

            #region VALIDA QUE EL EMPLEADO YA TENGA SU NOMINA TIMBRADA EN ESTE PERIODO - para no conciderar en el procesado estas nominas

            NominasDao _nominasDao = new NominasDao();

            var arrayidEmpleadosCfdiGenerados = _nominasDao.GetEmpleadosIdCfdiGenerados(arrayIdEmpleado, idPeriodo);

            if (arrayidEmpleadosCfdiGenerados.Length > 0)
            {
                var idEmpSumm = string.Join(",", arrayidEmpleadosCfdiGenerados);

                summaryList.Add(new NotificationSummary() { Reg = 0, Msg1 = $"Los empleados con id: {idEmpSumm} tienen su nominas timbradas en este periodo. \n Ya no se pueden volver a procesar.", Msg2 = "" });

                //Filtra los empleados que ya se timbraron
                var arrayFiltro = Utils.ElimarEnArrarDeOtroArray(arrayIdEmpleado, arrayidEmpleadosCfdiGenerados);
                arrayIdEmpleado = arrayFiltro;
            }
            #endregion

            if (arrayIdEmpleado.Length <= 0)
                return null;

            //Buscamos los id de la nomina de los empleados, 
            //para eliminar las nominas que hayan sido procesadas con ese id de empleado en el mismo periodo
            #region SE ELIMINA LOS REGISTROS DEL PROCESADO ANTERIOR

            var arrayIdNominas = _nominasDao.GetNominasIdByEmpleados(arrayIdEmpleado, idPeriodo);

            //si la nomina fue procesada anteriormente, se eliminan sus registros, para guardar sus nuevos valores.
            if (arrayIdNominas.Length > 0)
                NominasDao.EliminarAguinaldosProcesados(arrayIdNominas);


            #endregion

            #region GET DATA

            using (var context = new RHEntities())
            {
                periodoPago = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                //VALIDACIONES
                if (periodoPago == null)
                {
                    summaryList.Add(new NotificationSummary() { Reg = idPeriodo, Msg1 = $"No se encontró el periodo {idPeriodo}.", Msg2 = "" });
                    return null;
                }


                itemSucursal = context.Sucursal.FirstOrDefault(x => x.IdSucursal == periodoPago.IdSucursal);

                listaDatosComplementosDelPeriodo = _nominasDao.GetDatosComplementoDelPeriodo(idPeriodo);


                var fechaInicial = new DateTime(periodoPago.Fecha_Inicio.Year, periodoPago.Fecha_Inicio.Month, periodoPago.Fecha_Inicio.Day, 05, 00, 0);
                var fechaFinal = new DateTime(periodoPago.Fecha_Fin.Year, periodoPago.Fecha_Fin.Month, periodoPago.Fecha_Fin.Day, 23, 50, 0);


                listaContratos = (from c in context.Empleado_Contrato
                                  where arrayIdEmpleado.Contains(c.IdEmpleado)
                                  //&& c.Status == true // ya no porque empleados pueden estar dados de baja
                                  select c).ToList();

                listaIncapacidades = (from i in context.Incapacidades
                                      where arrayIdEmpleado.Contains(i.IdEmpleado)
                                      && i.FechaInicio >= fechaInicial && i.FechaInicio <= fechaFinal
                                      && (i.IdIncapacidadesSat == 2 || i.IdIncapacidadesSat == 3)
                                      select i).ToList(); //tipo 2,3 - idEmpleado - fi y ff

                listaInasistencias = (from i in context.Inasistencias
                                      where arrayIdEmpleado.Contains(i.IdEmpleado)
                                      && i.Fecha >= fechaInicial && i.Fecha <= fechaFinal
                                      && (i.IdTipoInasistencia == 3 || i.IdTipoInasistencia == 4 || i.IdTipoInasistencia == 5 || i.IdTipoInasistencia == 8 || i.IdTipoInasistencia == 9 || i.IdTipoInasistencia == 10 || i.IdTipoInasistencia == 11 || i.IdTipoInasistencia == 16)
                                      select i).ToList();// tipo 3,4,5,8,9,10,11,16 - idEmpleado - fecha-fechafin

                listaPermisos = (from p in context.Permisos
                                 where arrayIdEmpleado.Contains(p.IdEmpleado)
                                 && p.FechaInicio >= fechaInicial && p.FechaInicio <= fechaFinal
                                 select p).ToList();// id Empleado - FechaInicio-Fechafin

                itemZonaSalario = context.ZonaSalario.FirstOrDefault(x => x.Status == true);

                listaFactorIntegracion = context.C_FactoresIntegracion.ToList();

                //  var strNumerador = ((ParametrosDeConfiguracion)intNumerador).ToString().ToUpper();

                int idConfig = (int)ParametrosDeConfiguracion.ISN;

                itemConfigISN = context.ParametrosConfig.FirstOrDefault(x => x.IdConfig == idConfig);
            }


            #endregion

            //primerDia = periodoPago.Fecha_Inicio;
            fechaUltimoDia = periodoPago.Fecha_Fin;

            if (itemZonaSalario != null)
                sm = itemZonaSalario.UMA;// se calcula con el UMA, antes se calculaba con SM 06-12-2023


            diasEjercicio = Utils.GetDiasDelAño(fechaUltimoDia);
            topeExento = sm * dias;

            //Listas
            List<NOM_Nomina_Detalle> listaDetalles = new List<NOM_Nomina_Detalle>();
            List<NOM_Aguinaldo> listaAguinaldos = new List<NOM_Aguinaldo>();

            //Por cada empleado
            int index = 0;
            foreach (var idArrayEmpleado in arrayIdEmpleado)
            {
                bool calcularPensionA = true;
                decimal pensionAlimenticia = 0;

                primerDia = periodoPago.Fecha_Inicio;//Inicio del Periodo
                primerDiaC = periodoPago.Fecha_Inicio;
                var itemContrato = listaContratos.OrderByDescending(x => x.IdContrato).FirstOrDefault(x => x.IdEmpleado == idArrayEmpleado);

                //Validar si el contrato es null
                if (itemContrato == null)
                {
                    summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"No se encontró contrato para el empleado: {idArrayEmpleado}.", Msg2 = "" });

                    index++;
                    continue;
                }

                //Valida que el SD o SDI sea mayor a cero
                if (itemContrato.SD == 0 || itemContrato.SDI == 0)
                {
                    summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"El contrato {itemContrato.IdContrato} tiene los valores SD: {itemContrato.SD}, SDI:{itemContrato.SDI}", Msg2 = "" });

                    //index++;
                    //continue;
                }

                if (itemContrato.FechaIMSS != null)
                {
                    fechaIngreso = itemContrato.FechaIMSS.Value;

                    if (itemContrato.FechaIMSS > periodoPago.Fecha_Inicio)
                    {
                        primerDia = itemContrato.FechaIMSS.Value;
                    }
                }
                else
                {
                    summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"El contrato {itemContrato.IdContrato} no tiene fecha imss", Msg2 = "" });
                }


                if (itemContrato.FechaReal != null)
                {
                    if (itemContrato.FechaReal > periodoPago.Fecha_Inicio)
                    {
                        primerDiaC = itemContrato.FechaReal;
                    }
                }
                else
                {
                    summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"El contrato {itemContrato.IdContrato} no tiene fecha real", Msg2 = "" });
                }


                #region PROCESO CALCULO

                sd = itemContrato.SD;
                sdi = itemContrato.SDI;
                sdr = itemContrato.SalarioReal;
                sueldoMensual = 0;
                diasTrabajados = 0;
                //obtenemos los años de antiguedad
                if(itemContrato.FechaIMSS != null)
                antiguedad = itemContrato.FechaIMSS.Value.Year - fechaUltimoDia.Year;

                if (antiguedad == 0)
                    antiguedad = 1;
                //buscamos en la lista de factores los dias de aguinaldo
                var itemFactorIntegracion = listaFactorIntegracion.FirstOrDefault(x => x.Antiguedad == antiguedad);

                if (itemFactorIntegracion != null)
                    diasAguinaldo = itemFactorIntegracion.DiasAguinaldo;
                    diasAguinaldo = 30;// itemFactorIntegracion.DiasAguinaldo;


                //FALTAS
                faltas = 0;
                faltaPersonalizada = GetIntDataFromString(faltasCapturadas[index]);

                faltas = faltaPersonalizada >= 0 ? faltaPersonalizada : GetFaltas(idArrayEmpleado, listaIncapacidades, listaInasistencias, listaPermisos);

                //Fiscal
                diasTrabajados = Utils.GetDiasEntreDosFechas(primerDia, fechaUltimoDia) - faltas;// diasEjercicio - faltas;
                //Complemento
                diasTrabajadosComplemento = Utils.GetDiasEntreDosFechas(primerDiaC, fechaUltimoDia) - faltas;

                sueldoMensual = dias * sd;

                if (diasEjercicio > 0)
                {
                    //Fiscal
                    proporcion = ((decimal) diasAguinaldo * (decimal) diasTrabajados) / (decimal) diasEjercicio;
                    //Complemento
                    proporcionComplemento = ((decimal) diasAguinaldo * (decimal) diasTrabajadosComplemento) /
                                            (decimal) diasEjercicio;
                }
                else
                {
                    summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"Dias del ejercicio: {diasEjercicio}.", Msg2 = "" });
                }

                //Evitamos el calculo fiscal y se pase al calculo complento
                if (sd > 0 && sdi > 0) 
                {
                    aguinaldo = Utils.TruncateDecimales(sd * proporcion);

                    if (aguinaldo <= topeExento)
                    {
                        parteExento = aguinaldo;
                        parteGravado = 0;
                    }
                    else
                    {
                        parteExento = topeExento;
                        parteGravado = aguinaldo - topeExento;
                    }

                    IngresoGravable = sueldoMensual;

                    if (diasTrabajados > 0)
                    {
                        IngresoGravable304 = ((parteGravado / diasTrabajados) * (decimal) 30.40) + sueldoMensual;
                    }
                    else
                    {
                        summaryList.Add(new NotificationSummary() { Reg = idArrayEmpleado, Msg1 = $"Dias trabajados: {diasTrabajados}.", Msg2 = "" });
                    }

                    #region ISR - NETO

                    var result1 = MNominas.CalculoIsrSubsidioFin(null, IngresoGravable, sd, 0, 5, false);

                    var result2 = MNominas.CalculoIsrSubsidioFin(null, IngresoGravable304, sd, 0, 5, false);

                    decimal difBase = IngresoGravable304 - IngresoGravable;

                    decimal difIsr = result2.IsrAntesDeSub - result1.IsrAntesDeSub;

                    if (difIsr < (decimal) 0.00001)
                    {
                        factor = 0;
                    }
                    else
                    {
                        factor = (difIsr / difBase); //
                    }

                    isr = (parteGravado * factor);
                    neto = Utils.TruncateDecimales(aguinaldo - isr);

                    #endregion

                    //PENSION ALIMENTICIA

                    #region PENSION ALIMENTICIA

                    if (generarPensionAlimenticia != null)
                        calcularPensionA = generarPensionAlimenticia[index];

                    if (calcularPensionA)
                    {
                        if (itemContrato.PensionAlimenticiaPorcentaje != null)
                        {
                            decimal porcentaje = 0;

                            porcentaje = itemContrato.PensionAlimenticiaPorcentaje.Value;

                            if (porcentaje > 0)
                            {
                                pensionAlimenticia = (neto * (porcentaje / 100));

                                //Se le descuenta al neto
                                if (pensionAlimenticia > 0)
                                {
                                    neto = (neto - pensionAlimenticia);
                                }
                            }
                        }
                    }

                    #endregion

                    //parte de complemento
                    //complemento = 0;

                    #region IMPUESTO SOBRE NOMINA

                    var porcentajeSobreNomina = (decimal) 0.03;

                    if (itemConfigISN != null)
                    {
                        porcentajeSobreNomina = itemConfigISN.ValorDecimal;
                    }

                    iSobreNomina = (sd * diasAguinaldo) * porcentajeSobreNomina; // mty


                    iSobreNomina = iSobreNomina.TruncateDecimal(2);

                    #endregion
                }
                //COMPLEMENTO
                parteComplemento = 0;
                bool dataCompleto = false;

                #region NUEVO CÁLCULO DE COMPLEMENTO

                decimal diasAComp = -1;
                var sumaTotalComplemento = 0.00M;

                if (dac != null)
                {
                    //Buscamos en el array los dias de aguinaldo  complemento
                    diasAComp = GetDecimalDataFromString(dac[index]);

                    if (diasAComp >= 0)
                    {
                        sumaTotalComplemento = diasAComp * sdr;
                    }
                    else //sino busca el dato cargado por layout y el campo lo asigna a -1
                    {
                        //si la lista esta vacia buscamos en listaDatosComplementosDelPeriodo

                        //Busca en la lista de Datos de Complemento lo correspondiente al colaborador
                        //Obtenemos una lista con los concentos de complemento
                        var complementoEmpleado =
                            listaDatosComplementosDelPeriodo.Where(x => x.IdEmpleado == itemContrato.IdEmpleado)
                                .ToList();

                        //valida que la lista no este vacia
                        if (complementoEmpleado?.Count > 0)
                        {
                            //Complemento que se subio en el Layout
                            sumaTotalComplemento = complementoEmpleado.Select(x => x.Cantidad).Sum();
                        }
                    }

                    //Regla se aplico en monterrey por causa de descuento de lado de complemento
                    //donde el complemento se queda menor que el fiscal y eso genera negativo
                    //para no aplicar negativo se pondra cero
                    if (neto > sumaTotalComplemento)
                    {
                        parteComplemento = 0;
                    }
                    else
                    {
                        parteComplemento = (sumaTotalComplemento - neto.TruncateDecimal(2));
                    }

                }

                #endregion


                total = parteComplemento + neto;


                #endregion

                #region item Nomina
                //1.-Crear un item para Nominas
                //1) Crear el registro de nomina --------------------------------------------
                NOM_Nomina itemNomina = new NOM_Nomina();
                itemNomina.IdNomina = 0;
                itemNomina.IdCliente = idCliente;
                itemNomina.IdSucursal = idSucursal;
                itemNomina.IdEjercicio = periodoPago.IdEjercicio;
                itemNomina.IdPeriodo = idPeriodo;
                itemNomina.IdEmpleado = itemContrato.IdEmpleado;
                itemNomina.IdContrato = itemContrato.IdContrato;
                itemNomina.FechaReg = DateTime.Now;
                itemNomina.IdUsuario = idUsuario;
                itemNomina.CFDICreado = false;
                itemNomina.TotalNomina = neto;
                itemNomina.TotalPercepciones = 0;
                itemNomina.TotalDeducciones = 0;
                itemNomina.TotalOtrosPagos = 0;
                itemNomina.TotalComplemento = parteComplemento;
                itemNomina.TotalImpuestoSobreNomina = iSobreNomina;
                itemNomina.TotalImpuestoSobreNomina_Complemento = 0;
                itemNomina.TotalObligaciones = 0;
                itemNomina.TotalCuotasIMSS = 0;
                itemNomina.TotalCuotasIMSSComplemento = 0;
                itemNomina.SubsidioCausado = 0;
                itemNomina.SubsidioEntregado = 0;
                itemNomina.ISRantesSubsidio = isr;
                itemNomina.SD = itemContrato.SD;
                itemNomina.SDI = itemContrato.SDI;
                itemNomina.SBC = itemContrato.SBC;
                itemNomina.SDReal = itemContrato.SalarioReal;
                itemNomina.IdEmpresaFiscal = itemContrato.IdEmpresaFiscal;
                itemNomina.IdEmpresaComplemento = itemContrato.IdEmpresaComplemento;
                itemNomina.IdEmpresaAsimilado = itemContrato.IdEmpresaAsimilado;
                itemNomina.IdEmpresaSindicato = itemContrato.IdEmpresaSindicato;
                itemNomina.IdMetodo_Pago = itemContrato.FormaPago;
                itemNomina.Dias_Laborados = diasTrabajados;
                itemNomina.Faltas = faltas;
                itemNomina.Prima_Riesgo = 0;
                itemNomina.TipoTarifa = 5;//mensual
                itemNomina.SBCotizacionDelProcesado = 0;
                itemNomina.XMLSinTimbre = null;
                itemNomina.SMGV = itemZonaSalario.SMG;
                itemNomina.UMA = itemZonaSalario.UMA;


                _NominasDao.CrearNomina(itemNomina);
                listaNominasGeneradas.Add(itemNomina);
                #endregion

                #region item Detalle Nomina


                //2.- Crear un item para Detalle de nomina
                NOM_Nomina_Detalle itemNominaDetalle = new NOM_Nomina_Detalle();
                itemNominaDetalle.Id = 0;
                itemNominaDetalle.IdNomina = itemNomina.IdNomina;
                itemNominaDetalle.IdConcepto = 2;
                itemNominaDetalle.Total = aguinaldo;
                itemNominaDetalle.GravadoISR = parteGravado;
                itemNominaDetalle.ExentoISR = parteExento;
                itemNominaDetalle.IntegraIMSS = 0;
                itemNominaDetalle.ExentoIMSS = 0;
                itemNominaDetalle.ImpuestoSobreNomina = iSobreNomina;
                itemNominaDetalle.Complemento = false;
                itemNominaDetalle.IdPrestamo = 0;

                listaDetalles.Add(itemNominaDetalle);

                if (isr > 0)
                {
                    NOM_Nomina_Detalle itemDetalleIsr = new NOM_Nomina_Detalle();
                    itemDetalleIsr.Id = 0;
                    itemDetalleIsr.IdNomina = itemNomina.IdNomina;
                    itemDetalleIsr.IdConcepto = 43;
                    itemDetalleIsr.Total = isr;
                    itemDetalleIsr.GravadoISR = 0;
                    itemDetalleIsr.ExentoISR = 0;
                    itemDetalleIsr.IntegraIMSS = 0;
                    itemDetalleIsr.ExentoIMSS = 0;
                    itemDetalleIsr.ImpuestoSobreNomina = 0;
                    itemDetalleIsr.Complemento = false;
                    itemDetalleIsr.IdPrestamo = 0;

                    listaDetalles.Add(itemDetalleIsr);
                }

                if (pensionAlimenticia > 0)
                {
                    NOM_Nomina_Detalle itemPension = new NOM_Nomina_Detalle()
                    {
                        Id = 0,
                        IdNomina = itemNomina.IdNomina,
                        IdConcepto = 48,
                        Total = pensionAlimenticia,
                        GravadoISR = pensionAlimenticia,
                        ExentoISR = 0,
                        IntegraIMSS = 0,
                        ImpuestoSobreNomina = 0,
                        Complemento = false

                    };

                    listaDetalles.Add(itemPension);
                }

                #endregion

                #region item Aguinaldo


                //3.- Creo un item para Aguinaldo

                NOM_Aguinaldo itemAguinaldo = new NOM_Aguinaldo();

                itemAguinaldo.IdAguinaldo = 0;
                itemAguinaldo.IdEjercicio = periodoPago.IdEjercicio;
                itemAguinaldo.IdPeriodo = periodoPago.IdPeriodoPago;
                itemAguinaldo.IdContrato = itemContrato.IdContrato;
                itemAguinaldo.IdEmpleado = itemContrato.IdEmpleado;
                itemAguinaldo.IdNomina = itemNomina.IdNomina;
                itemAguinaldo.SD = sd;
                itemAguinaldo.SDI = sdi;
                itemAguinaldo.SalarioMinimo = sm;
                itemAguinaldo.DiasAguinaldo = diasAguinaldo;
                itemAguinaldo.DiasEjercicioFiscal = diasEjercicio;
                itemAguinaldo.FechaIngreso = fechaIngreso;
                itemAguinaldo.FechaPrimerDia = primerDia;
                itemAguinaldo.FechaUltimoDia = fechaUltimoDia;
                itemAguinaldo.TotalFaltas = faltas;
                int? personalF = null;

                if (faltaPersonalizada >= 0)
                {
                    personalF = faltaPersonalizada;
                }

                itemAguinaldo.FaltasPersonalizadas = personalF;
                itemAguinaldo.Incapacidades = 0;
                itemAguinaldo.FaltasInjustificadas = 0;
                itemAguinaldo.PermisoSinGose = 0;
                itemAguinaldo.DiasTrabajados = diasTrabajados;
                itemAguinaldo.SueldoMensual = sueldoMensual;
                itemAguinaldo.Proporcion = proporcion;
                itemAguinaldo.TopeExcento = topeExento;
                itemAguinaldo.Aguinaldo = aguinaldo;
                itemAguinaldo.Exento = parteExento;
                itemAguinaldo.Gravado = parteGravado;
                itemAguinaldo.ISR = isr;
                itemAguinaldo.Neto = neto;
                itemAguinaldo.Complemento = parteComplemento;
                itemAguinaldo.Total = total;
                itemAguinaldo.ISN3 = iSobreNomina;
                itemAguinaldo.Factor = factor;
                itemAguinaldo.FechaReg = DateTime.Now;
                itemAguinaldo.CalcularPensionAlimenticia = calcularPensionA;
                itemAguinaldo.PensionAlimenticia = pensionAlimenticia;
                itemAguinaldo.DiasAguinaldoComp = diasAComp;
                itemAguinaldo.SDR = sdr;
                itemAguinaldo.FechaPrimerDiaC = primerDiaC;
                itemAguinaldo.ProporcionC = proporcionComplemento;
                itemAguinaldo.DiasTrabajadosC = diasTrabajadosComplemento;
                #endregion


                listaAguinaldos.Add(itemAguinaldo);

                index++;
            }//-> fin for

            NominasDao.GuardarDetallesNomina(listaDetalles, null, 0);//Aguinaldo - no aplica agregar incapacidades
            _NominasDao.AddRangeAguinaldos(listaAguinaldos);


            //Obtener las empresas fiscales
            var listaNominaA = _nominasDao.GetDatosNominas(idPeriodo);
            var listaAguinaldo = _nominasDao.GetAguinaldosByIdPeriodo(idPeriodo);

            var empresaFiscal = listaNominaA.Select(x => x.IdEmpresaFiscal).Distinct().ToArray();
            var empresaComplemento = listaNominaA.Select(x => x.IdEmpresaComplemento).Distinct().ToArray();

            empresaFiscal = empresaFiscal.Distinct().ToArray();
            empresaComplemento = empresaComplemento.Distinct().ToArray();

            var porcentajeIva = _nominasDao.PorcentajeIVA();

            foreach (var itemFiscal in empresaFiscal)
            {
                decimal isn = 0;
                decimal totalPercepcion = 0;
                decimal totalFiscal = 0;
                decimal totalPension = 0;
                decimal relativo = 0;

                isn = listaNominaA.Where(x => x.IdEmpresaFiscal == itemFiscal).Sum(x => x.TotalImpuestoSobreNomina);

                totalPercepcion = listaNominaA.Where(x => x.IdEmpresaFiscal == itemFiscal).Sum(x => x.TotalNomina);

                var arrayIdNom = listaNominaA.Select(x => x.IdNomina).ToArray();

                var listaA = (from p in listaAguinaldo where arrayIdNom.Contains(p.IdNomina) select p).ToList();

                totalPension = listaA.Sum(x => x.PensionAlimenticia);

                relativo = isn + totalPension;

                totalFiscal = relativo + totalPercepcion;

                _nominasDao.CrearFactura(idPeriodo, itemFiscal.Value, 0, isn, 0, 0, totalPension, relativo, totalPercepcion, totalFiscal, porcentajeIva);
            }

            foreach (var itemComp in empresaComplemento)
            {
                if (itemComp == null) continue;

                decimal totalPercepcionesC = 0;

                decimal relativoC = 0;
                decimal subTotalC = 0;
                decimal totalIVAC = 0;
                decimal totalComple = 0;
                decimal porcentajeServicio = 0;
                decimal totalServicio = 0;

                decimal isn = 0;

                isn = listaNominaA.Where(x => x.IdEmpresaComplemento == itemComp).Sum(x => x.TotalImpuestoSobreNomina);

                totalPercepcionesC = listaNominaA.Where(x => x.IdEmpresaComplemento == itemComp).Sum(x => x.TotalComplemento);


                //resultado complemento de la factura
                porcentajeServicio = itemSucursal.PorcentajeServicio;//  _nominasDao.PorcentajeServicioHold(ppago.IdSucursal);
                porcentajeServicio = porcentajeServicio / 100;
                totalServicio = totalPercepcionesC * porcentajeServicio;

                relativoC = isn;
                subTotalC = relativoC + totalPercepcionesC + totalServicio;

                totalIVAC = (porcentajeIva / 100) * subTotalC;
                totalComple = totalIVAC + subTotalC;

                _nominasDao.CrearFacturaComplemento(idPeriodo, itemComp.Value, totalPercepcionesC, porcentajeServicio, totalServicio, 0, isn, relativoC, subTotalC, totalIVAC, totalComple);
            }


            return summaryList;


        }
        private static int GetFaltas(int idEmpleado, List<Incapacidades> listaIncapacidades, List<Inasistencias> listaInasistencias, List<Permisos> listaPermisos)
        {
            int totalFaltas = 0;
            int totalIncapacidades = 0;
            int totalInasistencias = 0;
            int totalPermisos = 0;

            //Get Incapacidades
            var incapacidades = listaIncapacidades.Where(x => x.IdEmpleado == idEmpleado).ToList();
            var inasistencias = listaInasistencias.Where(x => x.IdEmpleado == idEmpleado).ToList();
            var permisos = listaPermisos.Where(x => x.IdEmpleado == idEmpleado).ToList();

            //Validar las fechas que la fecha fin sea menor o igual a la fecha maxima del periodo - en este caso 31-12-año

            foreach (var item in incapacidades)
            {
                totalIncapacidades += item.Dias;
            }

            foreach (var item in inasistencias)
            {
                totalInasistencias += item.Dias;
            }

            foreach (var item in permisos)
            {
                totalPermisos += item.Dias ?? 0;
            }

            totalFaltas = (totalIncapacidades + totalInasistencias + totalPermisos);

            return totalFaltas;
        }
        private static int GetIntDataFromString(string valor)
        {
            int result = -1;

            if (valor.Trim() == "") return result;


            bool esnum = int.TryParse(valor, out result);

            if (esnum == false)
            {
                return -1;
            }
            else
            {
                return result;
            }

        }
        private static decimal GetDecimalDataFromString(string valor)
        {
            decimal result = -1;

            if (valor.Trim() == "") return result;


            bool esnum = decimal.TryParse(valor, out result);

            if (esnum == false)
            {
                return -1;
            }
            else
            {
                return result;
            }

        }

    }
}
