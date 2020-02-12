using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RH.Test
{
    //INTERFAZ
    public interface IEnviadorM
    {
        void EnviarMensaje(string msg);
    }

    // CLASE 1 QUE IMPLMENTA LA INTERFAZ
    public class EnviarMiniMsg : IEnviadorM
    {
        public void EnviarMensaje(string msg)
        {
            Console.WriteLine("Mini mensaje enviado : " + msg);
        }

    }

    // CLASE 2 QUE IMPLEMENTA LA INTERFAZ
    public class EnviarCorreo : IEnviadorM
    {
        public void EnviarMensaje(string msg)
        {
            Console.WriteLine("Correo enviado :" + msg);
        }
    }

    // CLASE INTERMEDIA 
    public class EnviadorMensaje
    {
        private IEnviadorM _enviadorMensaje;

        public EnviadorMensaje(IEnviadorM enviador)
        {
            _enviadorMensaje = enviador;
        }

        public void Enviar(string msg)
        {
            _enviadorMensaje.EnviarMensaje(msg);
        }

    }


    // CLASE QUE UTILIZA LA CLASE INTERMEDIA

    //CLASE FACtoria
    // para que el metodo en particular no tenga que instancias los tipos de clases
    public static class FactoriaEnviadorMensaje
    {
        public  static IEnviadorM Factoria(string parametro)
        {
            if(parametro == "sms")
            {
                return new EnviarMiniMsg();
            }
            else if( parametro == "correo")
            {
                return new EnviarCorreo();
            }

            throw new ApplicationException();
        }
    }

}



