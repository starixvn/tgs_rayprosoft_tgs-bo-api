using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TGS.Apis.BackendOffice.Attributes;

public class BOTypeFilterAttribute : Attribute, IFilterFactory, IOrderedFilter
{
    private ObjectFactory factory;

    public object[] Arguments { get; set; }

    public Type ImplementationType { get; private set; }

    public int Order { get; set; }

    public bool IsReusable => true;

    public BOTypeFilterAttribute([NotNull] Type type)
    {
        ImplementationType = type;
    }

    public IFilterMetadata CreateInstance([NotNull] IServiceProvider serviceProvider)
    {
        if (factory == null)
        {
            var argumentTypes = Arguments?.Select(a => a.GetType())?.ToArray();

            factory = ActivatorUtilities.CreateFactory(ImplementationType, argumentTypes ?? Type.EmptyTypes);
        }

        return (IFilterMetadata)factory(serviceProvider, Arguments);
    }
}