# 📦 FinValidator

**FinValidator** es una poderosa librería C# para validaciones financieras, detección de fraude en tiempo real y análisis de riesgo utilizando reglas heurísticas, comportamiento transaccional y lógica dinámica basada en JSON.

## ✨ Características principales

- ✅ Validación de montos, países, IPs y frecuencia
- 🧠 Reglas heurísticas configurables vía JSON
- 📍 Comparación geográfica entre IP y país declarado
- 🛡️ Detección de fraude en sistemas de pago
- ⚖️ Ponderación de riesgo por regla (score dinámico)
- 🔁 Webhooks o métricas exportables (opcional)
- 🧪 Pruebas unitarias integradas con xUnit
- 🚀 Listo para producción y despliegue en NuGet

## 🔍 Validaciones Incluidas

- Monto superior a umbral definido
- IPs en lista negra (blacklist)
- País bloqueado o sospechoso
- Frecuencia anormal de transacciones
- Desajuste entre IP geolocalizada y país declarado
- Lógica compuesta (AND/OR entre reglas)

## 🧰 Ejemplo de uso
```csharp
var tx = new TransactionData
{
    UserId = "cliente123",
    Amount = 12500,
    Country = "MX",
    IPAddress = "203.0.113.55",
    Timestamp = DateTime.UtcNow,
    UserTransactionTimestamps = new() {
        DateTime.UtcNow.AddMinutes(-3),
        DateTime.UtcNow.AddMinutes(-2)
    }
};

var rulesJson = File.ReadAllText("rules.json");
var rules = RuleLoader.LoadFromJson(rulesJson);
var engine = new FinValidatorEngine(rules);

var result = engine.Validate(tx);

if (result.IsFraud)
{
    Console.WriteLine("⚠️ Transacción sospechosa:");
    foreach (var r in result.Triggers) Console.WriteLine($"- {r}");
    Console.WriteLine($"Riesgo: {result.RiskScore}");
}
```

## ⚙️ Instalación

```bash
dotnet add package FinValidator
```

## 📄 Licencia

MIT License — puedes usarla en productos comerciales y personales.

## 🌐 Repositorio

[https://github.com/Tapia05/FinValidator](https://github.com/Tapia05/FinValidator)