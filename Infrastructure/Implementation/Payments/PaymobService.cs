using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Tamkeen.Infrastructure.Setting;

namespace Tamkeen.Infrastructure.Implementation.Payments
{
    public class PaymobService
    {
        private readonly HttpClient _http;
        private readonly PaymobSettings _settings;

        public PaymobService(HttpClient http, IOptions<PaymobSettings> settings)
        {
            _http = http;
            _settings = settings.Value;
        }

        // ── خطوة 1: Auth → بنجيب auth token ──────────────
        public async Task<string> GetAuthTokenAsync()
        {
            var response = await _http.PostAsJsonAsync(
                $"{_settings.BaseUrl}/auth/tokens",
                new { api_key = _settings.ApiKey }
            );

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()!;
        }

        // ── خطوة 2: Order → بنسجل الأوردر ────────────────
        public async Task<string> RegisterOrderAsync(
            string authToken, decimal amountInPiasters, string merchantOrderId)
        {
            var response = await _http.PostAsJsonAsync(
                $"{_settings.BaseUrl}/ecommerce/orders",
                new
                {
                    auth_token = authToken,
                    delivery_needed = false,
                    amount_cents = (int)amountInPiasters,  // بالقروش مش الجنيه!
                    currency = "EGP",
                    merchant_order_id = merchantOrderId,
                    items = new object[] { }
                }
            );

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetInt32().ToString();
        }

        // ── خطوة 3: Payment Key → بنجيب token للـ iframe ─
        public async Task<string> GetPaymentKeyAsync(
            string authToken, string orderId,
            decimal amountInPiasters, string integrationId,
            string tenantEmail, string tenantName, string? walletNumber = null, string? callbackUrl = null)
        {
            var billingData = new
            {
                first_name = tenantName,
                last_name = ".",
                email = tenantEmail,
                phone_number = walletNumber ?? "01000000000",
                street = "NA",
                building = "NA",
                floor = "NA",
                apartment = "NA",
                city = "NA",
                country = "EG",
                state = "NA",
                postal_code = "NA"
            };

            var response = await _http.PostAsJsonAsync(
                $"{_settings.BaseUrl}/acceptance/payment_keys",
                new
                {
                    auth_token = authToken,
                    amount_cents = (int)amountInPiasters,
                    expiration = 3600,             // صالح لساعة
                    order_id = orderId,
                    billing_data = billingData,
                    currency = "EGP",
                    integration_id = int.Parse(integrationId),
                    lock_order_when_paid = true,
                    redirection_url = callbackUrl ?? "http://localhost:4200/payment/callback"
                }
            );

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()!;
        }

        // ── Wallet: بنطلب الدفع مباشرة ────────────────────
        public async Task<string> RequestWalletPaymentAsync(
            string paymentToken, string walletNumber)
        {
            var response = await _http.PostAsJsonAsync(
                $"{_settings.BaseUrl}/acceptance/payments/pay",
                new
                {
                    source = new
                    {
                        identifier = walletNumber,
                        subtype = "WALLET"
                    },
                    payment_token = paymentToken
                }
            );

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            // بيرجع redirect url الـ tenant يفتحه على موبايله
            return result.GetProperty("redirect_url").GetString()!;
        }
        // في PaymobService.cs — أضف الـ method دي
        public async Task<bool> CheckTransactionSuccessAsync(string paymobOrderId)
        {
            var authToken = await GetAuthTokenAsync();

            var response = await _http.GetAsync(
                $"{_settings.BaseUrl}/ecommerce/orders/{paymobOrderId}?auth_token={authToken}"
            );

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();

            // Paymob بيرجع payment_status: "paid" لو اتدفع
            try
            {
                var paymentStatus = result.GetProperty("payment_status").GetString();
                return paymentStatus?.ToLower() == "paid";
            }
            catch
            {
                return false;
            }
        }
    }
}

