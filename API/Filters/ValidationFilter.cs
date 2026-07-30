using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Models;

namespace API.Filters
{
    /// <summary>
    /// Filtro para validación automática de DTOs usando FluentValidation
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor del filtro
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios</param>
        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Ejecuta el filtro de validación
        /// </summary>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (next == null) throw new ArgumentNullException(nameof(next));

            // Verificar si hay argumentos para validar
            if (context.ActionArguments.Count == 0)
            {
                await next();
                return;
            }

            // ✅ Verificar si hay algún argumento que NO sea primitivo
            bool hasDto = context.ActionArguments.Values
                .Any(arg => arg != null &&
                            !arg.GetType().IsPrimitive &&
                            arg.GetType() != typeof(string) &&
                            arg.GetType() != typeof(decimal));

            // ✅ Si no hay DTO en absoluto, continuar
            if (!hasDto)
            {
                await next();
                return;
            }

            // ✅ BUSCAR EL PRIMER ARGUMENTO QUE SEA UN DTO (NO PRIMITIVO)
            object? requestArgument = context.ActionArguments.Values
                .FirstOrDefault(arg => arg != null &&
                                       !arg.GetType().IsPrimitive &&
                                       arg.GetType() != typeof(string) &&
                                       arg.GetType() != typeof(decimal));

            // ✅ Si hay DTO pero es null, devolver BadRequest
            if (requestArgument == null)
            {
                context.Result = new BadRequestObjectResult(new { Message = "Request body cannot be null" });
                return;
            }

            // Obtener el tipo del argumento
            Type argumentType = requestArgument.GetType();

            // Buscar el validador para este tipo usando el tipo genérico IValidator<T>
            Type validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
            object? validator = _serviceProvider.GetService(validatorType);

            if (validator == null)
            {
                // No hay validador registrado, continuar
                await next();
                return;
            }

            // Obtener el método ValidateAsync con CancellationToken
            System.Reflection.MethodInfo? validateMethod = validatorType.GetMethod("ValidateAsync", new[] { argumentType, typeof(CancellationToken) });

            if (validateMethod == null)
            {
                await next();
                return;
            }

            // Invocar el método ValidateAsync y obtener el resultado tipado
            // Convertimos el resultado a Task<ValidationResult> y luego obtenemos el ValidationResult
            object? invokeResult = validateMethod.Invoke(validator, new object[] { requestArgument, CancellationToken.None });

            if (invokeResult == null)
            {
                await next();
                return;
            }

            // El resultado es un Task, así que esperamos a que termine
            Task<ValidationResult> validationTask = (Task<ValidationResult>)invokeResult;
            ValidationResult validationResult = await validationTask;

            // Verificar si la validación es válida
            if (!validationResult.IsValid)
            {
                string errorMessage = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                string errorDetail = string.Join("\n", validationResult.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));

                ErrorResponse errorResponse = new ErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = errorMessage,
                    Detail = errorDetail,
                    Timestamp = DateTime.UtcNow
                };

                context.Result = new BadRequestObjectResult(errorResponse);
                return;
            }

            await next();
        }
    }
}