using ENTITY.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarEmailPedido(ColaEmailDTO emailInfo);
        Task ProcesarColaEmails();
    }
}
