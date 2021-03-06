﻿using Autofac;
using AutoMapper;
using DataAccess.IdentityAccessor;
using Services.Common.AutoMapper.ApplicationUserResolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Services.Common.AutoMapper
{
    public class AutoMapperModule : Autofac.Module
    {
        private readonly IEnumerable<Assembly> assembliesToScan;

        public AutoMapperModule(IEnumerable<Assembly> assembliesToScan) => this.assembliesToScan = assembliesToScan;

        public AutoMapperModule(params Assembly[] assembliesToScan) : this((IEnumerable<Assembly>)assembliesToScan) { }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            var assembliesToScan = this.assembliesToScan as Assembly[] ?? this.assembliesToScan.ToArray();

            TypeInfo[] allTypes;
            try
            {
                allTypes = assembliesToScan
                             .Where(a => !a.IsDynamic && a.GetName().Name != nameof(AutoMapper))
                             .Distinct() // avoid AutoMapper.DuplicateTypeMapConfigurationException
                             .SelectMany(a => a.DefinedTypes)
                             .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                List<string> messages = new List<string>();
                foreach (var item in ex.LoaderExceptions)
                {
                    messages.Add(item.Message);
                }

                throw new Exception(string.Join(". ", messages), ex);
            }
            

            var openTypes = new[] {
                            typeof(IValueResolver<,,>),
                            typeof(IMemberValueResolver<,,,>),
                            typeof(ITypeConverter<,>),
                            typeof(IValueConverter<,>),
                            typeof(IMappingAction<,>)
            };

            foreach (var type in openTypes.SelectMany(openType =>
                 allTypes.Where(t => t.IsClass && !t.IsAbstract && ImplementsGenericInterface(t.AsType(), openType))))
            {
                builder.RegisterType(type.AsType()).InstancePerDependency();
            }

            builder.Register<IConfigurationProvider>(ctx => new MapperConfiguration(cfg => cfg.AddMaps(assembliesToScan)));

            builder.Register<IMapper>(ctx => new Mapper(ctx.Resolve<IConfigurationProvider>(), ctx.Resolve<IComponentContext>().Resolve)).InstancePerDependency();
        }

        private static bool ImplementsGenericInterface(Type type, Type interfaceType)
                  => IsGenericType(type, interfaceType) || type.GetTypeInfo().ImplementedInterfaces.Any(@interface => IsGenericType(@interface, interfaceType));

        private static bool IsGenericType(Type type, Type genericType)
                   => type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType;
    }
}
