using Microsoft.Extensions.DependencyInjection;
using Typer.Application.Auth.Interfaces;
using Typer.Application.Players.Interfaces;
using Typer.Application.Predictions.Interfaces;
using Typer.Application.Rankings.Interfaces;
using Typer.Application.Teams.Interfaces;

namespace Typer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Implementacje serwisów rejestrowane w Typer.Infrastructure
        return services;
    }
}
