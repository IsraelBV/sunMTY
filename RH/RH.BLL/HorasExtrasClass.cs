using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utils;
using RH.Entidades;

namespace RH.BLL
{
   public class HorasExtrasClass
    {
        public bool  GuardarHorasExtras(int[] empleados, string[] fechas, int horas, bool tipoSimples, int idUser)
        {
           List<HorasExtrasEmpleado> listaHoras = new List<HorasExtrasEmpleado>();
            int ide = 0;
            string fecha;
            bool result = false;

            if (horas <= 0) return result;

            if (empleados == null || fechas == null ) return result;

            if (empleados.Length <= 0 || fechas.Length <= 0) return result;

            for (int j = 0; j < fechas.Length; j++)
            {
                fecha = fechas[j];

               if (fecha == null) continue;

            for (int i = 0; i < empleados.Length; i++)
                {
                    ide = empleados[i];
                    if (ide == null  ) continue;
                    
                    HorasExtrasEmpleado item = new HorasExtrasEmpleado()
                    {
                        IdHoras = 0,
                        IdEmpleado = ide,
                        Fecha = DateTime.Parse(fecha.ToDiaMesAño()),
                        Horas = horas,
                        Simples = tipoSimples,
                        FechaReg = DateTime.Now,
                        UsuarioReg = idUser,
                        FechaMod = null,
                        UsuarioMod = null
                    };

                    listaHoras.Add(item);
                }
            }
            //Guardamos los datos
            using (var context = new RHEntities())
            {
                context.HorasExtrasEmpleado.AddRange(listaHoras);
                context.SaveChanges();
            }

            return result;
        }

        public List<HorasExtrasEmpleado> GetHorasExtrasEmpleados(int idEmpleado)
        {
            using (var context = new RHEntities())
            {
                var lista = (from h in context.HorasExtrasEmpleado
                    where h.IdEmpleado == idEmpleado
                    select h).ToList();

                return lista;
            }
        }

        public bool DeleteHoraExtra(int idHe)
        {
            using (var context = new RHEntities())
            {
                var item = context.HorasExtrasEmpleado.FirstOrDefault(x => x.IdHoras == idHe);

                context.HorasExtrasEmpleado.Remove(item);

                var result = context.SaveChanges();

                return result > 0 ? true : false;

            }
        }

        public List<EmpleadosHoras> GetHorasEmpleados(int[] idEmpleados)
        {
            List<HorasExtrasEmpleado> lista;
            using (var context = new RHEntities())
            {
                 lista = (from t in context.HorasExtrasEmpleado
                    where idEmpleados.Contains(t.IdEmpleado)
                    select t).ToList();
            }

            List < EmpleadosHoras > listafinal = new List<EmpleadosHoras>();
            foreach (var id in idEmpleados)
            {
                var cont = lista.Where(x=> x.IdEmpleado == id).Sum(x => x.Horas);
                EmpleadosHoras item = new EmpleadosHoras();
                item.IdEmpleado = id;
                item.HorasTotales = cont;

                listafinal.Add(item);
            }

            return listafinal;
        }


        public List<EmpleadosHoras> GetHotasExtrasParaNomina(int[] idEmpleados, NOM_PeriodosPago ppago )
        {
            List<HorasExtrasEmpleado> listaGeneral;
            using (var context = new RHEntities())
            {
                 listaGeneral = (from t in context.HorasExtrasEmpleado
                    where idEmpleados.Contains(t.IdEmpleado)
                          && t.Fecha >= ppago.Fecha_Inicio && t.Fecha <= ppago.Fecha_Fin
                    select t).ToList();
            }

            var listaGr = (from l in listaGeneral
                group l by new {l.IdEmpleado, l.Fecha}
                into gp
                select new EmpleadosHoras()
                {
                    IdEmpleado = gp.Key.IdEmpleado,
                    Fecha = gp.Key.Fecha,
                    StrFecha = gp.Key.Fecha.ToString("ddMM"),
                    HorasTotales = gp.Sum(s=> s.Horas)

                }).ToList();


            //from c in input
            //group c by new
            //{
            //    c.ProductGroup,
            //    c.BusinessDate.Year,
            //    c.BusinessDate.Month,
            //} into grp
            //select new ResultType()
            //{
            //    ProductGroup = grp.Key.ProductGroup,
            //    Month = new DateTime(grp.Key.Year, grp.Key.Month, 1),
            //    Sum = grp.Sum(s => s.Price),
            //};

            return listaGr;
        }
    }

    public class EmpleadosHoras
    {
        public int IdEmpleado { get; set; }
        public int HorasTotales { get; set; }
        public DateTime Fecha { get; set; }
        public string StrFecha { get; set; }
    }
}
