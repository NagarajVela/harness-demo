using NetArchTest.Rules;
using Xunit;

namespace Shop.ArchitectureTests;

public class LayerRules
{
    [Fact]
    public void ApplicationMustNotUseEfCore()
    {
        var result = Types.InAssembly(typeof(Shop.Application.AssemblyMarker).Assembly)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Application must not use EF Core. Depend on IOrderRepository " +
            "(Application.Abstractions). Offenders: " +
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
