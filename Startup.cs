using System;
using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Hosting;
using MoonSharp.Interpreter;
using Microsoft.Framework.Runtime;

namespace MoonScriptWeb
{
    /// <summary>
    /// The exciting part about aspnet5 is not MVC 6. It's everything else that MVC 6 utilitizes from the basic stack.
    /// This project creates a mini Lua powered 'web framework' in less than 100 LOC.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Take care of configuration of things to be made available on this application
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Create MoonSharp interpreter everytime a request was made.
            services.AddScoped(typeof(Script), x =>
            {
                var s = new Script();
                s.Globals["now"] = DateTime.UtcNow.ToString();
                return s;
            });
        }

        /// <summary>
        /// Check the parameters in this method. They are all injected automagically by aspnet. We only configure Script. There rest is already there.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="hostingEnv"></param>
        /// <param name="appEnv"></param>
        /// <param name="ss"></param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment hostingEnv, IApplicationEnvironment appEnv, Script ss)
        {
            app.UseMoonSharp(ss, hostingEnv, appEnv);
        }
    }
}
