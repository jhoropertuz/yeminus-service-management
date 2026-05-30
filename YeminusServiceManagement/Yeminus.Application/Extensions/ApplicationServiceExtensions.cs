using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Yeminus.Application.DTOs.Clients;
using Yeminus.Application.DTOs.Orders;
using Yeminus.Application.DTOs.Technicians;
using Yeminus.Application.Services;
using Yeminus.Application.Services.Interfaces;
using Yeminus.Application.Validators.Clients;
using Yeminus.Application.Validators.Orders;
using Yeminus.Application.Validators.Technicians;

namespace Yeminus.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ITechnicianService, TechnicianService>();
        services.AddScoped<IServiceOrderService, ServiceOrderService>();

        services.AddScoped<IValidator<CreateClientRequest>, CreateClientRequestValidator>();
        services.AddScoped<IValidator<UpdateClientRequest>, UpdateClientRequestValidator>();
        services.AddScoped<IValidator<CreateTechnicianRequest>, CreateTechnicianRequestValidator>();
        services.AddScoped<IValidator<UpdateTechnicianRequest>, UpdateTechnicianRequestValidator>();
        services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
        services.AddScoped<IValidator<ChangeOrderStatusRequest>, ChangeOrderStatusRequestValidator>();

        return services;
    }
}
