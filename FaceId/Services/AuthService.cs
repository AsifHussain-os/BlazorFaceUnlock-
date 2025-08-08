
// Services/AuthService.cs
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
    const string SessionKey = "app_session_token";

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
            // biometric API may throw on some devices — treat as failure
            return false;
        }
    }

    public async Task SignInAsync()
    {
        await SecureStorage.SetAsync(SessionKey, Guid.NewGuid().ToString());
    }

    public void SignOut()
    {
        SecureStorage.Remove(SessionKey);
    }

    public async Task ClearCredentialsAsync()
    {
        SecureStorage.Remove(PinKey);
        SecureStorage.Remove(PasswordKey);
        SecureStorage.Remove(BiometricKey);
        SecureStorage.Remove(SessionKey);
    }

    public async Task<bool> IsSignedInAsync()
    {
        var s = await SecureStorage.GetAsync(SessionKey);
        return !string.IsNullOrEmpty(s);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);
        // safe hex string:
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}


