using System.Reflection;
using System.Web;
using Gwire.Pages;
using Microsoft.AspNetCore.Components;

namespace Gwire;

public partial class App : ComponentBase
{
    private static readonly Dictionary<string, Type> PageTypes = typeof(App).Assembly.GetTypes()
        .SelectMany(type => type.GetCustomAttributes<RouteAttribute>()
            .Select(route => (Name: route.Template.Trim('/'), Type: type)))
        .Where(page => page.Name.Length > 0)
        .ToDictionary(page => page.Name, page => page.Type, StringComparer.OrdinalIgnoreCase);

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private RouteData ResolvePageRouteData(RouteData routeData)
    {
        // loading from url. This is client side app only, so we can safely assume that the query string is always available.
        var page = HttpUtility.ParseQueryString(new Uri(Navigation.Uri).Query)["page"];
        if (routeData.PageType != typeof(Home) || page is null)
        {
            return routeData;
        }

        var pageType = PageTypes.GetValueOrDefault(page.Trim('/'), typeof(NotFound));
        return new RouteData(pageType, new Dictionary<string, object?>());
    }
}
