using CAPSI.Sante.Application.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace CAPSI.Sante.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendReactivationEmailAsync(string toEmail, string patientName, string reactivationToken)
        {
            try
            {
                var reactivationLink = $"{_configuration["AppSettings:BaseUrl"]}/api/patient/confirm-reactivation/{reactivationToken}";

                var subject = "Demande de Réactivation de Compte Patient - CAPSI SANTÉ";
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #2c5aa0; color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; }}
                            .button {{ display: inline-block; padding: 12px 30px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                            .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>CAPSI SANTÉ</h1>
                                <h2>Réactivation de Compte Patient</h2>
                            </div>
                            <div class='content'>
                                <p>Bonjour <strong>{patientName}</strong>,</p>
                                <p>Une demande de réactivation a été faite pour votre compte patient dans notre système CAPSI SANTÉ.</p>
                                <p>Si vous êtes à l'origine de cette demande, cliquez sur le bouton ci-dessous pour confirmer la réactivation :</p>
                                <p style='text-align: center;'>
                                    <a href='{reactivationLink}' class='button'>RÉACTIVER MON COMPTE</a>
                                </p>
                                <p><strong>Important :</strong></p>
                                <ul>
                                    <li>Ce lien expire dans <strong>7 jours</strong></li>
                                    <li>Si vous n'êtes pas à l'origine de cette demande, ignorez cet email</li>
                                    <li>Pour toute question, contactez notre support</li>
                                </ul>
                                <p>Cordialement,<br>L'équipe CAPSI SANTÉ</p>
                            </div>
                            <div class='footer'>
                                <p>Cet email a été envoyé automatiquement, merci de ne pas y répondre.</p>
                                <p>© {DateTime.Now.Year} CAPSI SANTÉ - Tous droits réservés</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'email de réactivation à {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendReactivationConfirmationAsync(string toEmail, string patientName)
        {
            try
            {
                var subject = "Compte Réactivé avec Succès - CAPSI SANTÉ";
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; }}
                            .success {{ background-color: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>CAPSI SANTÉ</h1>
                                <h2>✅ Compte Réactivé</h2>
                            </div>
                            <div class='content'>
                                <p>Bonjour <strong>{patientName}</strong>,</p>
                                <div class='success'>
                                    <p><strong>Bonne nouvelle !</strong> Votre compte patient a été réactivé avec succès.</p>
                                </div>
                                <p>Vous pouvez maintenant :</p>
                                <ul>
                                    <li>Vous connecter à votre compte</li>
                                    <li>Prendre des rendez-vous</li>
                                    <li>Consulter votre dossier médical</li>
                                    <li>Accéder à tous nos services</li>
                                </ul>
                                <p>Nous sommes heureux de vous retrouver parmi nos patients !</p>
                                <p>Cordialement,<br>L'équipe CAPSI SANTÉ</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'email de confirmation à {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendAdminNotificationAsync(string patientName, string requestReason)
        {
            try
            {
                var adminEmail = _configuration["Email:AdminEmail"];
                if (string.IsNullOrEmpty(adminEmail))
                {
                    _logger.LogWarning("Email admin non configuré");
                    return false;
                }

                var subject = $"Nouvelle Demande de Réactivation - {patientName}";
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; }}
                            .info {{ background-color: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>CAPSI SANTÉ - ADMIN</h1>
                                <h2>🔔 Nouvelle Demande de Réactivation</h2>
                            </div>
                            <div class='content'>
                                <div class='info'>
                                    <p><strong>Patient :</strong> {patientName}</p>
                                    <p><strong>Motif :</strong> {requestReason ?? "Non spécifié"}</p>
                                    <p><strong>Date :</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                                </div>
                                <p>Une nouvelle demande de réactivation de compte patient a été soumise.</p>
                                <p>Connectez-vous à l'interface d'administration pour traiter cette demande.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(adminEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification admin");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();

                // Expéditeur
                message.From.Add(new MailboxAddress(
                    _configuration["Email:FromName"] ?? "CAPSI SANTÉ",
                    _configuration["Email:FromAddress"]
                ));

                // Destinataire
                message.To.Add(new MailboxAddress("", toEmail));

                // Sujet
                message.Subject = subject;

                // Corps du message
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = bodyBuilder.ToMessageBody();

                // Envoi via SMTP
                using var client = new SmtpClient();

                // Connexion au serveur SMTP
                await client.ConnectAsync(
                    _configuration["Email:SmtpHost"],
                    int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                // Authentification
                await client.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                // Envoi
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email envoyé avec succès à {Email} - Sujet: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'email à {Email}", toEmail);
                return false;
            }
        }
    }
}
