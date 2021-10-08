using Microsoft.Extensions.DependencyInjection;
using SIO.Domain.Users.CommandHandlers;
using SIO.Domain.Users.Commands;
using SIO.Infrastructure.Commands;

namespace SIO.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddTransient<ICommandHandler<ForgotPasswordCommand>, ForgotPasswordCommandHandler>();
            services.AddTransient<ICommandHandler<LoginCommand>, LoginCommandHandler>();
            services.AddTransient<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
            services.AddTransient<ICommandHandler<RegisterUserCommand>, RegisterUserCommandHandler>();
            services.AddTransient<ICommandHandler<VerifyUserCommand>, VerifyUserCommandHandler>();

            return services;
        }
    }
}
