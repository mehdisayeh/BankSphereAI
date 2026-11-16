using BankSphere.Identity.Application;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BankSphere.Identity.Infrastructure;

public sealed class SmtpIdentityNotificationSender(IConfiguration configuration) : IIdentityNotificationSender
{
    public async Task SendAsync(string email, string purpose, string encodedToken, CancellationToken cancellationToken)
    {
        var host = configuration["Identity:Notifications:Host"] ?? throw new InvalidOperationException("Configure the identity SMTP host.");
        var from = configuration["Identity:Notifications:From"] ?? throw new InvalidOperationException("Configure the identity sender.");
        var actionUri = configuration["Identity:Notifications:AccountActionUri"];
        if (!Uri.TryCreate(actionUri, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Configure an HTTPS account action URI.");
        var link = QueryHelpers.AddQueryString(uri.AbsoluteUri, new Dictionary<string, string?>
            { ["email"] = email, ["token"] = encodedToken, ["purpose"] = purpose });
        using var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(from)); message.To.Add(MailboxAddress.Parse(email));
        message.Subject = purpose == "reset-password" ? "Reset your BankSphere password" : "Confirm your BankSphere email";
        message.Body = new TextPart("plain") { Text = $"Complete your requested BankSphere account action:\n{link}\nIf you did not request this, ignore this message." };
        using var smtp = new SmtpClient();
        var port = int.Parse(configuration["Identity:Notifications:Port"] ?? "587", System.Globalization.CultureInfo.InvariantCulture);
        await smtp.ConnectAsync(host, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls, cancellationToken);
        if (configuration["Identity:Notifications:Username"] is { Length: > 0 } username)
            await smtp.AuthenticateAsync(username, configuration["Identity:Notifications:Password"]
                ?? throw new InvalidOperationException("Configure the SMTP password through a secret provider."), cancellationToken);
        await smtp.SendAsync(message, cancellationToken);
        await smtp.DisconnectAsync(true, cancellationToken);
    }
}
