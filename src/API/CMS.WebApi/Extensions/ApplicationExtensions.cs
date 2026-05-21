using CMS.Application.Behaviors;
using FluentValidation;
using MediatR;

namespace CMS.WebApi.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Pipeline davranış sırası (dıştan içe):
        // 1. Logging      - her request loglanır
        // 2. TenantResolution - tenant belirlenir (Authorization bunu gerektirir)
        // 3. Language     - dil bağlamı set edilir
        // 4. Authorization - yetki kontrolü (tenant set olduktan sonra)
        // 5. Validation   - yetki geçtikten sonra input doğrulanır
        // 6. Performance  - en içte, gerçek işlem süresini ölçer
        var appAssembly = typeof(Application.Features.Contents.GetContent.GetContentHandler).Assembly;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(appAssembly));

        // Pipeline sırası (dıştan içe):
        // Logging → TenantResolution → Language → Authorization → Validation → SaveVersion → Performance
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantResolutionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LanguageBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SaveVersionBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

        services.AddValidatorsFromAssembly(appAssembly);
        return services;
    }
}
