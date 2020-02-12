using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using RH.Entidades.GlobalModel;
using RH.Entidades;

namespace Nomina.BLL
{
    public class PrimaVacacionalModulo
    {

        //GET Primas-Empleados
        public List<PrimaVacacionalModelo> 
            GetDatosPrimasByPeriodo(int idPeriodo)
        {
            using (var context = new RHEntities())
            {
                var itemPeriodo = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                if (itemPeriodo == null) return null;

                var lista = (from ep in context.NOM_Empleado_PeriodoPago
                             join em in context.Empleado on ep.IdEmpleado equals em.IdEmpleado
                             where ep.IdPeriodoPago == idPeriodo
                             select new PrimaVacacionalModelo
                             {
                                 IdP = 0,
                                 IdE = em.IdEmpleado,
                                 NombreEmpleado = em.APaterno +" "+ em.AMaterno + " " + em.Nombres ,
                                 Porcentaje = 25
                             }).ToList();

                //busca los contratos
                var arrayIdE = lista.Select(x => x.IdE).ToArray();

                var listaContrato = (from c in context.Empleado_Contrato
                                     where arrayIdE.Contains(c.IdEmpleado)
                                     select c).ToList();

                listaContrato = listaContrato.OrderByDescending(x => x.IdContrato).ToList();

                //consultar los datos guardados
                var listaprimas = context.NOM_Nomina_PrimaVacacional.Where(x => x.IdPeriodo == idPeriodo).ToList();


                foreach(var item in lista)
                {
                    var found = listaprimas.FirstOrDefault(x => x.IdEmpleado == item.IdE);
                    var contrato = listaContrato.FirstOrDefault(x => x.IdEmpleado == item.IdE);

                    if (contrato == null) continue;
                    item.Sd = contrato.SD;

                    if (found == null) continue;

                    item.Dias = found.DiasPrima;
                    item.Porcentaje = found.Porcentaje;
                    item.Gravado = found.Gravado;
                    item.Exento = found.Exento;
                    item.Isn = found.Isn;
                    item.Total = found.Total;
                    item.IdP = found.Id;
                    item.Guardado = true;

                }


                return lista;
            }
        }
        
        //GET Primas
        public List<NOM_Nomina_PrimaVacacional> GetPrimasVacacionesUsuario(int idPeriodo)
        {
            using (var context = new RHEntities())
            {
                var lista = context.NOM_Nomina_PrimaVacacional.Where(x => x.IdPeriodo == idPeriodo).ToList();

                return lista;
            }
        }

        //SAVE Primas
        //solo si el periodo no esta autorizado
        public int SavePrimaVacacional(NOM_Nomina_PrimaVacacional itemNuevo, int idPeriodo)
        {
            using (var context = new RHEntities())
            {
                var item = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                if (item == null) return 0;

                if (item.Autorizado == true) return 0;

                context.NOM_Nomina_PrimaVacacional.Add(itemNuevo);
                var r = context.SaveChanges();

                if (r > 0)
                {
                    return itemNuevo.Id;
                }

                return 0;
            }
        }

        //DELETE Primas
        public bool DeletePrima(int idPrima, int idPeriodo)
        {
            using (var context = new RHEntities())
            {
                var item = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == idPeriodo);

                if (item == null) return false;

                if (item.Autorizado == true) return false;


                //Historial de pagos
                string sqlQuery = $"DELETE [NOM_Nomina_PrimaVacacional] where Id = {idPrima}";
                var r = context.Database.ExecuteSqlCommand(sqlQuery);

                if (r > 0)
                {
                    return true;
                }

                return false;
            }

        }

        //UPDATE Primas
        public bool UpdatePrima(NOM_Nomina_PrimaVacacional itemUp)
        {
            using (var context = new RHEntities())
            {
                var itemPeriodo = context.NOM_PeriodosPago.FirstOrDefault(x => x.IdPeriodoPago == itemUp.IdPeriodo);

                if (itemPeriodo == null) return false;

                if (itemPeriodo.Autorizado == true) return false;

                var item = context.NOM_Nomina_PrimaVacacional.FirstOrDefault(x => x.Id == itemUp.Id);

                if (item == null) return false;

                item.DiasPrima = itemUp.DiasPrima;
                item.Porcentaje = itemUp.Porcentaje;
                item.SD = itemUp.SD;
                item.Gravado = itemUp.Gravado;
                item.Exento = itemUp.Exento;
                item.Isn = itemUp.Isn;
                item.Total = itemUp.Total;

                var r = context.SaveChanges();


                if (r > 0)
                    return true;
                else
                    return false;

            }
        }



    }

    public class PrimaVacacionalModelo
    {
        public int IdP { get; set; }
        public int IdE { get; set; }
        public string NombreEmpleado { get; set; }
        public decimal Dias { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal Gravado { get; set; }
        public decimal Exento { get; set; }
        public decimal Total { get; set; }
        public decimal Isn { get; set; }
        public decimal Sd { get; set; }//Sd Actual
        public bool Guardado { get; set; }


    }
}
