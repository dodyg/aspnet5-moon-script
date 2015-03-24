using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Runtime;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MoonScriptWeb
{
    public static class IReadableStringCollectionExtensions {

        public static IReadOnlyDictionary<string, string> ToDictionary(this IReadableStringCollection self)
        {
            var q = new Dictionary<string, string>();

            foreach (var x in self)
            {
                q.Add(x.Key, string.Join(",", x.Value));
            }

            return q;
        }
    }

    public class Environments
    {
        public IHostingEnvironment Hosting { get; set; }

        public IApplicationEnvironment Application { get; set; }
    }

    public class MoonSharpMiddleware
    {
        RequestDelegate _next;
        Script _script;
        Environments _envs;

        public MoonSharpMiddleware(RequestDelegate next, Script s, Environments envs)
        {
            _next = next;
            _script = s;
            _envs = envs;
        }

        public async Task Invoke(HttpContext context)
        {
            var baseApp = Path.Combine(_envs.Application.ApplicationBasePath, "App");

            var requestPath = context.Request.Path.ToString();

            var script = @"return """" .. path";

            if (requestPath.Equals("/"))
            {
                var scriptFile = Path.Combine(baseApp, "Index.lua");
                if (File.Exists(scriptFile))
                {
                    script = File.ReadAllText(scriptFile);
                }
            }
            else
            {
                var scriptFile = Path.Combine(baseApp, requestPath.ToString().Trim(new[] { '/' }) + ".lua");
                if (File.Exists(scriptFile))
                {
                    script = File.ReadAllText(scriptFile);
                }
            }

            _script.Globals["path"] = requestPath.ToString();
            _script.Globals["querystring"] = context.Request.Query.ToDictionary();
            _script.Globals["cookies"] = context.Request.Cookies.ToDictionary();

            if (context.Request.HasFormContentType)
            {
                _script.Globals["form"] = context.Request.Form.ToDictionary();
            }

            _script.Globals["root"] = _envs.Application.ApplicationBasePath;

            DynValue res = _script.DoString(script);

            await context.Response.WriteAsync(res.String);

            //pre
            await _next(context);
            //post
        }
    }

    public static class MoonSharpMiddlewareExtensions
    {
        /// <summary>
        /// This makes the usage of this middleware must nicer, e.g. app.UseMoondSharp()
        /// </summary>
        /// <param name="app"></param>
        /// <param name="s"></param>
        /// <param name="hostingEnv"></param>
        /// <param name="appEnv"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMoonSharp(this IApplicationBuilder app, Script s, IHostingEnvironment hostingEnv, IApplicationEnvironment appEnv)
        {
            return app.Use(next => new MoonSharpMiddleware(next, s, new Environments { Application = appEnv, Hosting = hostingEnv }).Invoke);
        }
    }
}