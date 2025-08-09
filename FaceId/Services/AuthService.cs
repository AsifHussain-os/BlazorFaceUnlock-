// Services/AuthService.cs - Use This Simple Version
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Plugin.Maui.Biometric;

namespace FaceId.Services;

public class AuthService
{
    const string PinKey = "app_pin_hash";
    const string PasswordKey = "app_password_hash";
    const string BiometricKey = "app_biometric_enabled";

    // No session storage at all - user must authenticate every time

    public async Task<bool> HasCredentialsAsync()
    {
        var pin = await SecureStorage.GetAsync(PinKey);
        var pwd = await SecureStorage.GetAsync(PasswordKey);
        return !string.IsNullOrEmpty(pin) || !string.IsNullOrEmpty(pwd);
    }

    public async Task SetCredentialsAsync(string pin, string password)
    {
        if (!string.IsNullOrEmpty(pin))
            await SecureStorage.SetAsync(PinKey, Hash(pin));
        if (!string.IsNullOrEmpty(password))
            await SecureStorage.SetAsync(PasswordKey, Hash(password));
    }

    public async Task<bool> ValidatePinAsync(string pin)
    {
        var stored = await SecureStorage.GetAsync(PinKey);
        if (string.IsNullOrEmpty(stored)) return false;
        return stored == Hash(pin);
    }

    public async Task<bool> ValidatePasswordAsync(string password)
    {
        var stored = await SecureStorage.GetAsync(PasswordKey);
        if (string.IsNullOrEmpty(stored)) return false;
        return stored == Hash(password);
    }

    public async Task EnableBiometricsAsync(bool enabled)
    {
        if (enabled)
            await SecureStorage.SetAsync(BiometricKey, "true");
        else
            SecureStorage.Remove(BiometricKey);
    }

    public async Task<bool> IsBiometricsEnabledAsync()
    {
        // Just check if user enabled it in app settings
        var s = await SecureStorage.GetAsync(BiometricKey);
        return s == "true";
    }

    public async Task<bool> TryBiometricUnlockAsync()
    {
        try
        {
            var result = await BiometricAuthenticationService.Default.AuthenticateAsync(
                new AuthenticationRequest
                {
                    Title = "Unlock to Use My App — FaceId",
                    //Subtitle = "Use fingerprint to continue",
                    Description = "Place finger on sensor to unlock",
                    NegativeText = "Cancel"
                },
                CancellationToken.None
            );
            return result.Status == BiometricResponseStatus.Success;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    // No SignInAsync method - no sessions to create

    public void SignOut()
    {
        // Nothing to clear since there's no session
    }

    public async Task ClearCredentialsAsync()
    {
        SecureStorage.Remove(PinKey);
        SecureStorage.Remove(PasswordKey);
        SecureStorage.Remove(BiometricKey);
    }

    // Always return false - no persistent sessions
    public Task<bool> IsSignedInAsync()
    {
        return Task.FromResult(false);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}