namespace Backend.Configurations
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            return app;
        }

        public static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app, string policyName)
        {
            app.UseCors(policyName);
            return app;
        }

        public static IApplicationBuilder UseSecurityMiddlewares(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseAuthentication(); // Must be before UseAuthorization
            app.UseAuthorization();
            return app;
        }

        public static IApplicationBuilder UseRoutingMiddleware(this IApplicationBuilder app)
        {
            app.UseRouting();
            return app;
        }
    }
}
