using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureWatcher.Function.Domain.Entities;
using AzureWatcher.Function.Domain.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureWatcher.Function.Infrastructure.Email;

/// <summary>
/// Implements email notifications using standard SMTP (e.g. Gmail).
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendNewOffersEmailAsync(IEnumerable<JobOffer> newOffers, CancellationToken cancellationToken = default)
    {
        if (!newOffers.Any()) return;

        var host = _configuration["SmtpHost"] ?? "smtp.gmail.com";
        var portStr = _configuration["SmtpPort"] ?? "587";
        var username = _configuration["SmtpUsername"];
        var password = _configuration["SmtpPassword"];
        var fromAddress = _configuration["EmailFromAddress"];
        var toAddress = _configuration["EmailToAddress"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _logger.LogError("SMTP credentials are not configured.");
            return;
        }

        if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(toAddress))
        {
            _logger.LogError("Email from/to address is not configured.");
            return;
        }

        var subject = $"New Job Offers Found! ({newOffers.Count()})";

        var sb = new StringBuilder();
        sb.AppendLine("<h2>New Job Offers</h2>");
        sb.AppendLine("<ul>");
        
        foreach (var offer in newOffers)
        {
            sb.AppendLine($"<li><a href='{offer.Link}'>{offer.Title}</a></li>");
        }
        sb.AppendLine("</ul>");

        var htmlContent = sb.ToString();

        using var client = new SmtpClient(host)
        {
            Port = int.TryParse(portStr, out var port) ? port : 587,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        using var message = new MailMessage(fromAddress, toAddress)
        {
            Subject = subject,
            Body = htmlContent,
            IsBodyHtml = true
        };

        try
        {
            _logger.LogInformation("Sending email via SMTP...");
            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via SMTP.");
        }
    }
}
