using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

            // Tomar el primer argumento (asumimos que es el DTO)
            object? requestArgument = context.ActionArguments.Values.FirstOrDefault();

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
                // Construir errores de validación usando tipos explícitos
                List<object> errorsList = new List<object>();

                foreach (ValidationFailure error in validationResult.Errors)
                {
                    object errorItem = new
                    {
                        Property = error.PropertyName,
                        Message = error.ErrorMessage,
                        AttemptedValue = error.AttemptedValue
                    };

                    errorsList.Add(errorItem);
                }

                object response = new
                {
                    Message = "Validation failed",
                    Errors = errorsList
                };

                context.Result = new BadRequestObjectResult(response);
                return;
            }

            await next();
        }
    }
}