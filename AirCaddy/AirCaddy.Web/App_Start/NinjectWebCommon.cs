using AirCaddy.Data.Repositories;
using AirCaddy.Domain.Services;
using AirCaddy.Domain.Services.Privileges;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(AirCaddy.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(AirCaddy.App_Start.NinjectWebCommon), "Stop")]

namespace AirCaddy.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using System.Configuration;
    using AirCaddy.Domain.Services.GolfCourses;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            BindDomainServices(kernel);
            BindDataRepositories(kernel);
        }

        private static void BindDomainServices(IKernel kernel)
        {
            kernel.Bind<ISessionMapperService>().To<SessionMapperService>();
            kernel.Bind<IPrivilegeRequestHandlerService>().To<PrivilegeRequestHandlerService>()
                .WithConstructorArgument("uspsUserId", ConfigurationManager.AppSettings["USPS_User_ID"].ToString());
            kernel.Bind<IGolfCourseService>().To<GolfCourseService>();
            kernel.Bind<IYelpGolfCourseReviewService>().To<YelpGolfCourseReviewService>()
                .WithConstructorArgument("yelpApiKey", ConfigurationManager.AppSettings["Yelp_API_Key"].ToString());
        }

        private static void BindDataRepositories(IKernel kernel)
        {
            kernel.Bind<IUserRepository>().To<UserRepository>();
            kernel.Bind<IGolfCourseRepository>().To<GolfCourseRepository>();
            kernel.Bind<IPrivilegeRepository>().To<PrivilegeRepository>();
        }
    }
}
