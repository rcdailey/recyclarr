using System.Reflection;
using Autofac;
using FluentValidation;
using TrashLib.Config.Services;
using TrashLib.Config.Settings;
using Module = Autofac.Module;

namespace TrashLib.Config;

public class ConfigAutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AsClosedTypesOf(typeof(IValidator<>))
            .AsImplementedInterfaces();

        builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>().InstancePerLifetimeScope();
        builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().InstancePerLifetimeScope();
        builder.RegisterType<YamlSerializerFactory>().As<IYamlSerializerFactory>();
    }
}
