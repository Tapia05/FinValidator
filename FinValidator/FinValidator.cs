using System;
using System.Collections.Generic;
using System.Linq;

// --- Modelo de Datos ---
public class TransactionData
{
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string IPAddress { get; set; }
    public string Country { get; set; }
    public string DeviceId { get; set; }
    public DateTime Timestamp { get; set; }
    public List<DateTime> UserTransactionTimestamps { get; set; } = new();
}

// --- Resultado de Validación ---
public class ValidationResult
{
    public bool IsFraud { get; set; } = false;
    public int RiskScore { get; set; } = 0;
    public List<string> Triggers { get; set; } = new();
}

// --- Interfaz Base de Regla ---
public interface IValidationRule
{
    int Weight { get; set; }
    bool Evaluate(TransactionData tx, out string reason);
}

// --- Reglas Implementadas ---
public class AmountGreaterThanRule : IValidationRule
{
    public decimal Threshold { get; set; }
    public int Weight { get; set; } = 20;

    public bool Evaluate(TransactionData tx, out string reason)
    {
        if (tx.Amount > Threshold)
        {
            reason = $"Monto supera el umbral de {Threshold}";
            return true;
        }
        reason = null;
        return false;
    }
}

public class CountryNotAllowedRule : IValidationRule
{
    public List<string> BlockedCountries { get; set; } = new();
    public int Weight { get; set; } = 20;

    public bool Evaluate(TransactionData tx, out string reason)
    {
        if (BlockedCountries.Contains(tx.Country))
        {
            reason = $"Transacción desde país bloqueado: {tx.Country}";
            return true;
        }
        reason = null;
        return false;
    }
}

public class BlacklistedIPRule : IValidationRule
{
    public List<string> BlacklistedIPs { get; set; } = new();
    public int Weight { get; set; } = 25;

    public bool Evaluate(TransactionData tx, out string reason)
    {
        if (BlacklistedIPs.Contains(tx.IPAddress))
        {
            reason = $"IP en lista negra: {tx.IPAddress}";
            return true;
        }
        reason = null;
        return false;
    }
}

public class FrequencyLimitRule : IValidationRule
{
    public int MaxTransactionsPerMinute { get; set; } = 3;
    public int Weight { get; set; } = 15;

    public bool Evaluate(TransactionData tx, out string reason)
    {
        var recent = tx.UserTransactionTimestamps.Where(t =>
            (tx.Timestamp - t).TotalMinutes <= 1).ToList();

        if (recent.Count >= MaxTransactionsPerMinute)
        {
            reason = $"Frecuencia alta de transacciones: {recent.Count} en el último minuto.";
            return true;
        }
        reason = null;
        return false;
    }
}

// --- Motor de Validación ---
public class FinValidatorEngine
{
    private readonly List<IValidationRule> _rules;

    public FinValidatorEngine(List<IValidationRule> rules)
    {
        _rules = rules;
    }

    public ValidationResult Validate(TransactionData tx)
    {
        var result = new ValidationResult();

        foreach (var rule in _rules)
        {
            if (rule.Evaluate(tx, out var reason))
            {
                result.IsFraud = true;
                result.Triggers.Add(reason);
                result.RiskScore += rule.Weight;
            }
        }

        return result;
    }
}

// --- Uso de ejemplo ---
public class Program
{
    public static void Main()
    {
        var tx = new TransactionData
        {
            UserId = "user123",
            Amount = 12000,
            Currency = "USD",
            Country = "RU",
            IPAddress = "123.45.67.89",
            Timestamp = DateTime.UtcNow,
            UserTransactionTimestamps = new List<DateTime>
            {
                DateTime.UtcNow.AddSeconds(-30),
                DateTime.UtcNow.AddSeconds(-20),
                DateTime.UtcNow.AddSeconds(-10)
            }
        };

        var rules = new List<IValidationRule>
        {
            new AmountGreaterThanRule { Threshold = 10000, Weight = 30 },
            new CountryNotAllowedRule { BlockedCountries = new() { "RU", "KP" }, Weight = 25 },
            new BlacklistedIPRule { BlacklistedIPs = new() { "123.45.67.89" }, Weight = 20 },
            new FrequencyLimitRule { MaxTransactionsPerMinute = 2, Weight = 15 }
        };

        var engine = new FinValidatorEngine(rules);
        var result = engine.Validate(tx);

        Console.WriteLine(result.IsFraud ? "⚠️ Transacción sospechosa" : "✅ Transacción segura");
        Console.WriteLine("Puntaje de Riesgo: " + result.RiskScore);
        foreach (var reason in result.Triggers)
            Console.WriteLine("- " + reason);
    }
}