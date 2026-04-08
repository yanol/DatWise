using Autofac;
using Autofac.Integration.Web;
using SafetyCompliance.Helpers;
using SafetyCompliance.Repositories;
using SafetyCompliance.Services;
using System;

namespace SafetyCompliance
{
    public class Global : System.Web.HttpApplication, IContainerProviderAccessor
    {
        
        static IContainerProvider _containerProvider;
        public IContainerProvider ContainerProvider => _containerProvider;

        protected void Application_Start(object sender, EventArgs e)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SystemLogger>().As<ISystemLogger>().InstancePerRequest();
            builder.RegisterType<ComplianceAgentService>().As<IComplianceAgentService>().InstancePerRequest();
            builder.RegisterType<GroqApiClient>().As<IAiClient>().InstancePerRequest();
            builder.RegisterType<ComplianceRepository>().As<IComplianceRepository>().InstancePerRequest();
                    
            _containerProvider = new ContainerProvider(builder.Build());
        }
    }
}