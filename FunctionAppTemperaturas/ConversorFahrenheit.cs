using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using FunctionAppTemperaturas.Models;

namespace FunctionAppTemperaturas;

public class ConversorFahrenheit
{
    private readonly ILogger _logger;

    public ConversorFahrenheit(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ConversorFahrenheit>();
    }

    [Function(nameof(ConversorFahrenheit))]
    [OpenApiOperation(operationId: "ConversorTemperaturas", tags: new[] { "Temperaturas" })]
    [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
    [OpenApiParameter(name: "temperaturaFahrenheit", In = ParameterLocation.Path, Required = true, Type = typeof(double), Description = "Temperatura em graus Fahrenheit")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(Temperatura), Description = "Resultado da conversão de uma temperatura em Fahrenheit")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(FalhaConversao), Description = "Falha na conversão de uma temperatura em Fahrenheit")]

    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get",
        Route = $"ConversorTemperaturas/Fahrenheit/{{{nameof(temperaturaFahrenheit)}}}")] HttpRequestData req,
        double temperaturaFahrenheit)
    {
        _logger.LogInformation(
            $"Recebida temperatura para conversão: {temperaturaFahrenheit}");

        if (temperaturaFahrenheit >= -459.67) // Temperatura maior ou igual ao zero absoluto
        {
            var resultado = new Temperatura(temperaturaFahrenheit);
            _logger.LogInformation($"{resultado.Fahrenheit} graus Fahrenheit = " +
                $"{resultado.Celsius} graus Celsius = " +
                $"{resultado.Kelvin} graus Kelvin");

            var response = req.CreateResponse();
            response.WriteAsJsonAsync(resultado);
            return response;
        }
        else
        {
            var erro = $"Informe uma temperatura válida! Valor recebido: {temperaturaFahrenheit}";
            _logger.LogError(erro);

            var response = req.CreateResponse();
            response.WriteAsJsonAsync(new FalhaConversao() { Mensagem = erro });
            response.StatusCode = HttpStatusCode.BadRequest;
            return response;
        }
    }
}