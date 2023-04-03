using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AuthenticationTokenServiceAPI
{
    public static class MvcOptionsExtension
    {
        public static void UseCentralRoutePrefix(this MvcOptions options, IRouteTemplateProvider routeAttribute)
        {
            options.Conventions.Insert(0, new RouteConvention(routeAttribute));
        }

        public class RouteConvention : IApplicationModelConvention
        {
            private readonly AttributeRouteModel _centralPrefix;

            public RouteConvention(IRouteTemplateProvider routeTemplateProvider)
            {
                _centralPrefix = new AttributeRouteModel(routeTemplateProvider);
            }

            public void Apply(ApplicationModel application)
            {
                foreach (var controller in application.Controllers)
                {
                    var matchedSelectors = controller.Selectors.Where(x => x.AttributeRouteModel != null).ToList();
                    if (matchedSelectors.Any())
                    {
                        foreach (var selectorModel in matchedSelectors)
                        {
                            selectorModel.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(_centralPrefix,
                                selectorModel.AttributeRouteModel);
                        }
                    }
                }
            }
        }
    }
}
