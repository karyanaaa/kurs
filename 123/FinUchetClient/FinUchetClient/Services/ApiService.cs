using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FinUchetClient.Models;

namespace FinUchetClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private string? _token;

        // ВАЖНО: Используем тот же порт, что и сервер (8080)
        private readonly string _baseUrl = "http://127.0.0.1:8080";

        public ApiService()
        {
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(30);

            // Добавляем заголовки для JSON
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> GetSecurityQuestionAsync(string username)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/security-question?username={username}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return result?["question"] ?? "";
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetSecurityQuestion error: {ex.Message}");
                return "";
            }
        }

        public async Task<bool> ResetPasswordAsync(string username, string securityAnswer, string newPassword)
        {
            try
            {
                var data = new { username, security_answer = securityAnswer, new_password = newPassword };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/reset-password", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResetPassword error: {ex.Message}");
                return false;
            }
        }
        public void SetToken(string token)
        {
            _token = token;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            System.Diagnostics.Debug.WriteLine($"Token set in ApiService: {(token != null ? token.Substring(0, 20) + "..." : "null")}");
        }

        public async Task<bool> RegisterAsync(string username, string password, string securityQuestion, string securityAnswer, string email = "")
        {
            try
            {
                var data = new
                {
                    username,
                    password,
                    security_question = securityQuestion,
                    security_answer = securityAnswer,
                    email
                };
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"Sending to: {_baseUrl}/register");
                Console.WriteLine($"Data: {json}");

                var response = await _client.PostAsync($"{_baseUrl}/register", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response body: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }



        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

                var response = await _client.PostAsync($"{_baseUrl}/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LoginResponse>(json);
                    return result?.access_token;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TransactionModel>?> GetTransactionsAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/transactions");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<TransactionModel>>(json);
                }
                return new List<TransactionModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTransactions error: {ex.Message}");
                return new List<TransactionModel>();
            }
        }

        public async Task<List<CategoryModel>?> GetCategoriesAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/categories");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<CategoryModel>>(json);
                }
                return new List<CategoryModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCategories error: {ex.Message}");
                return new List<CategoryModel>();
            }
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}/transactions/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteTransaction error: {ex.Message}");
                return false;
            }
        }
        // Добавьте эти методы в класс ApiService

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}/categories/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteCategory error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddCategoryAsync(CategoryModel category)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new { name = category.Name, type = category.Type });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/categories", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddCategory error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(CategoryModel category)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new { name = category.Name, type = category.Type });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_baseUrl}/categories/{category.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateCategory error: {ex.Message}");
                return false;
            }
        }



        public async Task<bool> UpdateTransactionAsync(TransactionModel transaction)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    amount = transaction.Amount,
                    description = transaction.Description,
                    type = transaction.Type,
                    category_id = transaction.CategoryId,
                    date = transaction.Date
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_baseUrl}/transactions/{transaction.Id}", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"UpdateTransaction response: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response body: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateTransaction error: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> AddTransactionAsync(TransactionModel transaction)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    amount = transaction.Amount,
                    description = transaction.Description,
                    type = transaction.Type,
                    category_id = transaction.CategoryId,
                    date = transaction.Date
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/transactions", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"AddTransaction response: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response body: {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AddTransaction error: {ex.Message}");
                return false;
            }
        }
        // Инвестиции
        public async Task<List<InvestmentModel>?> GetInvestmentsAsync()
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/investments");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<InvestmentModel>>(json);
                }
                return new List<InvestmentModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetInvestments error: {ex.Message}");
                return new List<InvestmentModel>();
            }
        }

        public async Task<bool> AddInvestmentAsync(InvestmentModel investment)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    name = investment.Name,
                    type = investment.Type,
                    amount = investment.Amount,
                    purchase_price = investment.PurchasePrice,
                    current_price = investment.CurrentPrice,
                    quantity = investment.Quantity,
                    purchase_date = investment.PurchaseDate,
                    currency = investment.Currency,
                    notes = investment.Notes
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{_baseUrl}/investments", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddInvestment error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateInvestmentAsync(InvestmentModel investment)
        {
            try
            {
                var json = JsonConvert.SerializeObject(new
                {
                    name = investment.Name,
                    type = investment.Type,
                    amount = investment.Amount,
                    purchase_price = investment.PurchasePrice,
                    current_price = investment.CurrentPrice,
                    quantity = investment.Quantity,
                    purchase_date = investment.PurchaseDate,
                    currency = investment.Currency,
                    notes = investment.Notes
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_baseUrl}/investments/{investment.Id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateInvestment error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteInvestmentAsync(int id)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_baseUrl}/investments/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeleteInvestment error: {ex.Message}");
                return false;
            }
        }
    }
    public class LoginResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
    }
}