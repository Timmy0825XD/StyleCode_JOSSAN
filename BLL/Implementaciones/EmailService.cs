using BLL.Interfaces;
using DAL.Interfaces;
using ENTITY.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Text;

namespace BLL.Implementaciones
{
    public class EmailService : IEmailService
    {
        private readonly IEmailDAO _emailDAO;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IEmailDAO emailDAO,
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            _emailDAO = emailDAO;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarEmailPedido(ColaEmailDTO emailInfo)
        {
            try
            {
                // Obtener datos completos del pedido
                var responseDatos = await _emailDAO.ObtenerDatosEmailPedido(emailInfo.IdPedido);
                if (!responseDatos.IsSuccess || responseDatos.Object == null)
                {
                    _logger.LogError($"No se pudieron obtener datos del pedido {emailInfo.IdPedido}");
                    return false;
                }

                var datosPedido = responseDatos.Object;

                // Crear mensaje
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"] ?? "StyleCode",
                    _configuration["Email:FromAddress"] ?? "noreply@stylecode.com"
                ));
                message.To.Add(MailboxAddress.Parse(emailInfo.Destinatario));
                message.Subject = emailInfo.Asunto;

                // Generar HTML según tipo de email
                string htmlBody = emailInfo.TipoEmail switch
                {
                    "PEDIDO_CREADO" => GenerarHtmlPedidoCreado(datosPedido),
                    "PEDIDO_CONFIRMADO" => GenerarHtmlPedidoConfirmado(datosPedido),
                    "PEDIDO_ENVIADO" => GenerarHtmlPedidoEnviado(datosPedido),
                    "PEDIDO_ENTREGADO" => GenerarHtmlPedidoEntregado(datosPedido),
                    "PEDIDO_CANCELADO" => GenerarHtmlPedidoCancelado(datosPedido),
                    _ => GenerarHtmlGenerico(datosPedido)
                };

                message.Body = new TextPart("html") { Text = htmlBody };

                // Enviar email
                using var client = new SmtpClient();

                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPass = _configuration["Email:SmtpPassword"];

                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email enviado exitosamente a {emailInfo.Destinatario} - Tipo: {emailInfo.TipoEmail} - Pedido: {datosPedido.NumeroPedido}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar email para pedido {emailInfo.IdPedido}");
                return false;
            }
        }

        public async Task ProcesarColaEmails()
        {
            try
            {
                var responseEmails = await _emailDAO.ObtenerEmailsPendientes();
                if (!responseEmails.IsSuccess || responseEmails.ListObject == null)
                {
                    return;
                }

                _logger.LogInformation($"Procesando {responseEmails.ListObject.Count} emails pendientes");

                foreach (var email in responseEmails.ListObject)
                {
                    bool enviado = await EnviarEmailPedido(email);

                    if (enviado)
                    {
                        await _emailDAO.MarcarEmailEnviado(email.IdEmail);
                    }
                    else
                    {
                        await _emailDAO.MarcarEmailError(email.IdEmail, "Error al enviar email");
                    }

                    // Pequeña pausa entre emails para no saturar
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar cola de emails");
            }
        }

        #region Plantillas HTML

        private string GenerarHtmlPedidoCreado(DatosEmailPedidoDTO datos)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { margin: 0; padding: 0; font-family: 'Arial', sans-serif; background: #f3f4f6; }
        .container { max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; }
        .header h1 { color: white; margin: 0; font-size: 28px; }
        .emoji-icon { font-size: 80px; margin-bottom: 10px; }
        .content { padding: 30px; }
        .pedido-number { font-size: 18px; color: #667eea; font-weight: bold; text-align: center; margin-bottom: 20px; }
        .section { margin: 20px 0; }
        .section-title { font-size: 16px; font-weight: bold; color: #1f2937; margin-bottom: 10px; padding-bottom: 8px; border-bottom: 2px solid #e5e7eb; }
        .producto-item { display: flex; gap: 15px; padding: 15px; background: #f9fafb; border-radius: 8px; margin-bottom: 10px; }
        .producto-img { width: 80px; height: 80px; object-fit: cover; border-radius: 8px; }
        .producto-info { flex: 1; }
        .producto-nombre { font-weight: bold; color: #1f2937; margin: 0 0 5px 0; }
        .producto-detalles { font-size: 13px; color: #6b7280; margin: 3px 0; }
        .producto-precio { font-weight: bold; color: #667eea; margin-top: 5px; }
        .totales { background: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .total-linea { display: flex; justify-content: space-between; padding: 8px 0; }
        .total-linea.final { font-size: 20px; font-weight: bold; color: #667eea; border-top: 2px solid #e5e7eb; padding-top: 15px; margin-top: 10px; }
        .info-box { background: #eff6ff; padding: 15px; border-radius: 8px; border-left: 4px solid #667eea; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #6b7280; font-size: 13px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='emoji-icon'>🎉</div>
            <h1>¡Pedido Recibido!</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>" + datos.NombreCliente + @"</strong>,</p>
            <p>Hemos recibido tu pedido exitosamente. Lo estamos procesando y pronto lo confirmaremos.</p>
            <div class='pedido-number'>Pedido #" + datos.NumeroPedido + @"</div>");

            // Productos
            sb.Append("<div class='section'><div class='section-title'>📦 Productos</div>");
            foreach (var producto in datos.Productos)
            {
                sb.Append($@"
                <div class='producto-item'>
                    {(string.IsNullOrEmpty(producto.UrlImagen)
                        ? "<div style='width:80px;height:80px;background:#e5e7eb;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:30px;'>👕</div>"
                        : $"<img src='{producto.UrlImagen}' class='producto-img' alt='Producto'/>")}
                    <div class='producto-info'>
                        <p class='producto-nombre'>{producto.NombreProducto}</p>
                        <p class='producto-detalles'>{producto.Marca}</p>
                        <p class='producto-detalles'>Talla: {producto.Talla} | Color: {producto.Color}</p>
                        <p class='producto-detalles'>Cantidad: {producto.Cantidad}</p>
                        <p class='producto-precio'>${producto.SubtotalLinea:N0} COP</p>
                    </div>
                </div>");
            }
            sb.Append("</div>");

            // Totales
            sb.Append($@"
            <div class='totales'>
                <div class='total-linea'><span>Subtotal:</span><span>${datos.Subtotal:N0} COP</span></div>
                <div class='total-linea'><span>Envío:</span><span>${datos.CostoEnvio:N0} COP</span></div>
                <div class='total-linea'><span>IVA:</span><span>${datos.Impuesto:N0} COP</span></div>
                <div class='total-linea final'><span>Total:</span><span>${datos.Total:N0} COP</span></div>
            </div>");

            sb.Append($@"
            <div class='info-box'>
                <p><strong>⏳ Próximos pasos:</strong></p>
                <p style='margin:5px 0;'>1. Verificaremos tu pedido y disponibilidad de productos</p>
                <p style='margin:5px 0;'>2. Te enviaremos una confirmación cuando esté listo</p>
                <p style='margin:5px 0;'>3. Procesaremos el envío a tu dirección</p>
            </div>
            <p style='text-align:center; color:#6b7280; margin-top:20px;'>
                Te notificaremos de cualquier actualización en tu pedido.
            </p>
        </div>
        <div class='footer'>
            <p>Gracias por tu compra en StyleCode<br/>
            © 2025 StyleCode. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }

        private string GenerarHtmlPedidoConfirmado(DatosEmailPedidoDTO datos)
        {
            var sb = new StringBuilder();
            sb.Append(@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body { margin: 0; padding: 0; font-family: 'Arial', sans-serif; background: #f3f4f6; }
        .container { max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.1); }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 40px 30px; text-align: center; }
        .header h1 { color: white; margin: 0; font-size: 28px; }
        .checkmark { width: 80px; height: 80px; border-radius: 50%; background: white; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; font-size: 40px; }
        .content { padding: 30px; }
        .pedido-number { font-size: 18px; color: #667eea; font-weight: bold; text-align: center; margin-bottom: 20px; }
        .section { margin: 20px 0; }
        .section-title { font-size: 16px; font-weight: bold; color: #1f2937; margin-bottom: 10px; padding-bottom: 8px; border-bottom: 2px solid #e5e7eb; }
        .producto-item { display: flex; gap: 15px; padding: 15px; background: #f9fafb; border-radius: 8px; margin-bottom: 10px; }
        .producto-img { width: 80px; height: 80px; object-fit: cover; border-radius: 8px; }
        .producto-info { flex: 1; }
        .producto-nombre { font-weight: bold; color: #1f2937; margin: 0 0 5px 0; }
        .producto-detalles { font-size: 13px; color: #6b7280; margin: 3px 0; }
        .producto-precio { font-weight: bold; color: #667eea; margin-top: 5px; }
        .totales { background: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .total-linea { display: flex; justify-content: space-between; padding: 8px 0; }
        .total-linea.final { font-size: 20px; font-weight: bold; color: #667eea; border-top: 2px solid #e5e7eb; padding-top: 15px; margin-top: 10px; }
        .info-box { background: #eff6ff; padding: 15px; border-radius: 8px; border-left: 4px solid #667eea; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #6b7280; font-size: 13px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='checkmark'>✓</div>
            <h1>¡Pedido Confirmado!</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>" + datos.NombreCliente + @"</strong>,</p>
            <p>Tu pedido ha sido confirmado exitosamente y está siendo preparado para envío.</p>
            <div class='pedido-number'>Pedido #" + datos.NumeroPedido + @"</div>");

            // Productos
            sb.Append("<div class='section'><div class='section-title'>📦 Productos Confirmados</div>");
            foreach (var producto in datos.Productos)
            {
                sb.Append($@"
                <div class='producto-item'>
                    {(string.IsNullOrEmpty(producto.UrlImagen)
                        ? "<div style='width:80px;height:80px;background:#e5e7eb;border-radius:8px;display:flex;align-items:center;justify-content:center;font-size:30px;'>👕</div>"
                        : $"<img src='{producto.UrlImagen}' class='producto-img' alt='Producto'/>")}
                    <div class='producto-info'>
                        <p class='producto-nombre'>{producto.NombreProducto}</p>
                        <p class='producto-detalles'>{producto.Marca}</p>
                        <p class='producto-detalles'>Talla: {producto.Talla} | Color: {producto.Color}</p>
                        <p class='producto-detalles'>Cantidad: {producto.Cantidad}</p>
                        <p class='producto-precio'>${producto.SubtotalLinea:N0} COP</p>
                    </div>
                </div>");
            }
            sb.Append("</div>");

            // Dirección de envío
            sb.Append($@"
            <div class='section'>
                <div class='section-title'>🚚 Dirección de Envío</div>
                <p><strong>{datos.DireccionCompleta}</strong><br/>
                {datos.Barrio}, {datos.Ciudad}<br/>
                {datos.Departamento}</p>
            </div>");

            // Totales
            sb.Append($@"
            <div class='totales'>
                <div class='total-linea'><span>Subtotal:</span><span>${datos.Subtotal:N0} COP</span></div>
                <div class='total-linea'><span>Envío:</span><span>${datos.CostoEnvio:N0} COP</span></div>
                <div class='total-linea'><span>IVA:</span><span>${datos.Impuesto:N0} COP</span></div>
                <div class='total-linea final'><span>Total:</span><span>${datos.Total:N0} COP</span></div>
            </div>");

            sb.Append($@"
            <div class='info-box'>
                <strong>💳 Método de Pago:</strong> {datos.MetodoPago}
            </div>
            <p>Te notificaremos cuando tu pedido sea enviado.</p>
        </div>
        <div class='footer'>
            <p>Gracias por tu compra en StyleCode<br/>
            © 2025 StyleCode. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>");

            return sb.ToString();
        }

        private string GenerarHtmlPedidoEnviado(DatosEmailPedidoDTO datos)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; background: #f3f4f6; }}
        .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 40px 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .pedido-number {{ font-size: 18px; color: #10b981; font-weight: bold; text-align: center; margin: 20px 0; }}
        .info-box {{ background: #ecfdf5; padding: 20px; border-radius: 8px; border-left: 4px solid #10b981; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div style='font-size:60px;margin-bottom:10px;'>🚚</div>
            <h1>¡Tu Pedido Está en Camino!</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{datos.NombreCliente}</strong>,</p>
            <p>Tu pedido ha sido enviado y está en camino a tu dirección.</p>
            <div class='pedido-number'>Pedido #{datos.NumeroPedido}</div>
            <div class='info-box'>
                <p><strong>📍 Dirección de entrega:</strong><br/>
                {datos.DireccionCompleta}<br/>
                {datos.Barrio}, {datos.Ciudad}, {datos.Departamento}</p>
            </div>
            <p style='text-align:center; color:#6b7280;'>
                Recibirás tu pedido en los próximos días hábiles.<br/>
                Te notificaremos cuando sea entregado.
            </p>
        </div>
        <div class='footer'>
            <p>Gracias por tu compra en StyleCode<br/>
            © 2025 StyleCode. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlPedidoEntregado(DatosEmailPedidoDTO datos)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; background: #f3f4f6; }}
        .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #10b981 0%, #059669 100%); padding: 40px 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .pedido-number {{ font-size: 18px; color: #10b981; font-weight: bold; text-align: center; margin: 20px 0; }}
        .info-box {{ background: #ecfdf5; padding: 20px; border-radius: 8px; border-left: 4px solid #10b981; margin: 20px 0; }}
        .rating-section {{ background: #f9fafb; padding: 20px; border-radius: 8px; text-align: center; margin: 20px 0; }}
        .rating-section h3 {{ margin: 0 0 10px 0; color: #1f2937; }}
        .rating-section p {{ color: #6b7280; font-size: 14px; margin: 5px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div style='font-size:80px;margin-bottom:10px;'>📦</div>
            <h1>¡Pedido Entregado!</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{datos.NombreCliente}</strong>,</p>
            <p>Tu pedido ha sido entregado exitosamente en tu dirección.</p>
            <div class='pedido-number'>Pedido #{datos.NumeroPedido}</div>
            <div class='info-box'>
                <p><strong>📍 Entregado en:</strong><br/>
                {datos.DireccionCompleta}<br/>
                {datos.Barrio}, {datos.Ciudad}, {datos.Departamento}</p>
            </div>
            <div class='rating-section'>
                <h3>¿Cómo fue tu experiencia?</h3>
                <p>Nos encantaría conocer tu opinión sobre tu compra</p>
                <p style='margin-top:15px;'>⭐⭐⭐⭐⭐</p>
            </div>
            <p style='text-align:center; color:#6b7280;'>
                Esperamos que disfrutes tu compra. ¡Gracias por confiar en StyleCode!
            </p>
        </div>
        <div class='footer'>
            <p>Gracias por tu compra en StyleCode<br/>
            © 2025 StyleCode. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlPedidoCancelado(DatosEmailPedidoDTO datos)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ margin: 0; padding: 0; font-family: Arial, sans-serif; background: #f3f4f6; }}
        .container {{ max-width: 600px; margin: 40px auto; background: white; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%); padding: 40px 30px; text-align: center; color: white; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .pedido-number {{ font-size: 18px; color: #ef4444; font-weight: bold; text-align: center; margin: 20px 0; }}
        .info-box {{ background: #fef2f2; padding: 20px; border-radius: 8px; border-left: 4px solid #ef4444; margin: 20px 0; }}
        .totales {{ background: #f9fafb; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .total-linea {{ display: flex; justify-content: space-between; padding: 8px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div style='font-size:80px;margin-bottom:10px;'>❌</div>
            <h1>Pedido Cancelado</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{datos.NombreCliente}</strong>,</p>
            <p>Tu pedido ha sido cancelado según lo solicitado.</p>
            <div class='pedido-number'>Pedido #{datos.NumeroPedido}</div>
            <div class='info-box'>
                <p><strong>💰 Reembolso</strong></p>
                <p>Si realizaste algún pago, el reembolso será procesado en los próximos 5-10 días hábiles al método de pago original.</p>
            </div>
            <div class='totales'>
                <div class='total-linea'><span>Total a reembolsar:</span><span>${datos.Total:N0} COP</span></div>
            </div>
            <p style='text-align:center; color:#6b7280;'>
                Lamentamos que no hayamos podido completar tu pedido.<br/>
                Si tienes alguna duda, no dudes en contactarnos.
            </p>
        </div>
        <div class='footer'>
            <p>StyleCode - Siempre a tu servicio<br/>
            © 2025 StyleCode. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerarHtmlGenerico(DatosEmailPedidoDTO datos)
        {
            return GenerarHtmlPedidoConfirmado(datos);
        }

        #endregion
    }
}