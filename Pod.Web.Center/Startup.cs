#region Licence
/****************************************************************
 *  Filename: Startup.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Pod.Data;
using Pod.Data.Config;
using Pod.Data.Models.Interfaces;
using Pod.Data.Models.Servers;
using Pod.Data.Models.Users;
using Pod.Grpc.Base.Server;
using Pod.Grpc.ConnectHost.Server.Services;
using Pod.Grpc.ShellHost.Server.Services;
using Pod.LetsEncrypt;
using Pod.LetsEncrypt.Config;
using Pod.MailEngine;
using Pod.Services;
using Pod.Services.Accountant;
using Pod.Services.Administrator;
using Pod.Services.Applications;
using Pod.Services.Authentication;
using Pod.Services.ConnectHost;
using Pod.Services.Customer;
using Pod.Services.Email;
using Pod.Services.Health;
using Pod.Services.ServerManager;
using Pod.Services.ShellHost;
using Pod.Services.Station;
using Pod.Services.Support;
using Pod.Services.System;
using Pod.Services.User;
using Pod.Web.Authentication.ApiKeySecret;
using Pod.Web.Center.Authentication;
using Pod.Web.Center.Config;
using Pod.Web.Center.ServicesHosted;
using Pod.Web.Center.Swagger;
using Pod.Web.Center.Swagger.Examples;
using Pod.Web.Center.TokenProvider;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Pod.Web.Center
{
    public partial class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register Configuration Instances
            services.AddOptions();
            services.AddSingleton(_configuration);


            //Register RateLimiter Configurations
            services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(_configuration.GetSection("IpRateLimitPolicies"));

            //Register Lets Encrypt Config
            services.Configure<LetsEncryptOptions>(_configuration.GetSection(nameof(LetsEncryptOptions)));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<LetsEncryptOptions>>().Value);
            var letsEncryptOptions = _configuration.GetSection(nameof(LetsEncryptOptions)).Get<LetsEncryptOptions>();
            if(letsEncryptOptions.IsEnabled)
            {
                services.AddLetsEncrypt();
            }

            //Register specific configuration instances
            services.Configure<DbContextFactoryConfig>(_configuration.GetSection(nameof(DbContextFactoryConfig)));
            services.Configure<ConfigSuperuser>(_configuration.GetSection(nameof(ConfigSuperuser)));
            services.Configure<ConfigShellServer>(_configuration.GetSection(nameof(ConfigShellServer)));
            services.Configure<GrpcServerConfig>(_configuration.GetSection(nameof(GrpcServerConfig)));

            //Config of Server Info
            services.Configure<WebAppConfig>(_configuration.GetSection(nameof(WebAppConfig)));
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<WebAppConfig>>().Value);

            //Register/Get the configuration for authentication
            services.Configure<AuthConfig>(_configuration.GetSection(nameof(AuthConfig)));
            services.Configure<JwtIssuerOptionsConfig>(_configuration.GetSection(nameof(JwtIssuerOptionsConfig)));

            //Register Config for TokenProviders
            services.Configure<PasswordResetTokenProviderOptions>(_configuration.GetSection(nameof(PasswordResetTokenProviderOptions)));
            services.Configure<EmailConfirmationTokenProviderOptions>(_configuration.GetSection(nameof(EmailConfirmationTokenProviderOptions)));
            services.Configure<RefreshAccessTokenProviderOptions>(_configuration.GetSection(nameof(RefreshAccessTokenProviderOptions)));

            //Add Config used in Services/Controllers
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AuthConfig>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<RefreshAccessTokenProviderOptions>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtIssuerOptionsConfig>>().Value);
            services.AddSingleton<JwtIssuerOptions>();
            services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<DbContextFactoryConfig>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ConfigSuperuser>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<ConfigShellServer>>().Value);
            services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<GrpcServerConfig>>().Value);
            services.AddSingleton<ShellServer>(
                    provider =>
                    {
                        using(var scope = provider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<PodDbContext>();
                            var server = dbContext.Servers.FirstOrDefault();
                            if(server == null)
                                throw new NotSupportedException(
                                        "There must be at least one ShellServer available to run this service");
                            return server;
                        }
                    });


            //Register DB-Context for Identity
            //Register with resolver as class has also a empty constructor for commandline utils
            services.AddSingleton<PodDbContextFactory>(resolver => new PodDbContextFactory(
                                                               resolver.GetRequiredService<IConfiguration>(),
                                                               resolver.GetRequiredService<DbContextFactoryConfig>(),
                                                               resolver.GetRequiredService<ILoggerFactory>()));
            services.AddSingleton<IDesignTimeDbContextFactory<PodDbContext>, PodDbContextFactory>();
            services.AddScoped<ContextInitializer>();
            services.AddScoped<PodDbContext>(resolver => resolver.GetService<PodDbContextFactory>().Create());
            services.AddScoped<PodUserClaimsPrincipalFactory>();

            //Register DB Setup Tasks
            services.AddScoped<IDbSetupTask, DbSetupUsers>();
            services.AddScoped<IDbSetupTask, DbSetupShellServer>();
            services.AddScoped<IDbSetupTask, DbSetupEmail>();
            services.AddScoped<DbSetupUsers>();
            services.AddScoped<DbSetupShellServer>();
            services.AddScoped<DbSetupEmail>();


            //Provider Keys
            var refreshTokenProviderKey = _configuration.GetSection(nameof(RefreshAccessTokenProviderOptions))["Name"];
            var emailConfirmationTokenProviderKey = _configuration.GetSection(nameof(EmailConfirmationTokenProviderOptions))["Name"];
            var passwordResetTokenProviderKey = _configuration.GetSection(nameof(PasswordResetTokenProviderOptions))["Name"];

            //Add Identity for API without Cookies (AddIdentityCore vs AddIdentity)
            IdentityBuilder identityBuilder = services.AddIdentityCore<ApplicationUser, ApplicationRole>(
                    options =>
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireNonAlphanumeric = true;
                        options.Password.RequireUppercase = true;
                        options.Password.RequiredLength = 10;
                        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                        options.Lockout.MaxFailedAccessAttempts = 4;
                        options.Lockout.AllowedForNewUsers = false;
                        options.SignIn.RequireConfirmedEmail = true;

                        //This set the Token Providers for Emails and Password Reset to Token with a configurable lifespan
                        options.Tokens.EmailConfirmationTokenProvider = emailConfirmationTokenProviderKey;
                        options.Tokens.PasswordResetTokenProvider = passwordResetTokenProviderKey;
                    }
            );

            identityBuilder.AddEntityFrameworkStores<PodDbContext>();
            // SignInManager needs IUserConfirmation<TUser>; AddIdentityCore (unlike AddIdentity)
            // doesn't auto-register either, so the operator login endpoint 500s without these.
            // AddSignInManager wires SignInManager itself; the explicit TryAddScoped below
            // supplies the IUserConfirmation dependency that AddSignInManagerDeps still leaves
            // unset under AddIdentityCore.
            identityBuilder.AddSignInManager();
            services.TryAddScoped<
                Microsoft.AspNetCore.Identity.IUserConfirmation<ApplicationUser>,
                Microsoft.AspNetCore.Identity.DefaultUserConfirmation<ApplicationUser>>();

            //Allows to use Tokens for Reset Password/Change Email .....
            identityBuilder.AddDefaultTokenProviders();

            //Allows to Add/Remove/Validate Tokens through UserManager
            identityBuilder.AddTokenProvider(refreshTokenProviderKey,typeof(RefreshAccessTokenProvider<ApplicationUser>));
            identityBuilder.AddTokenProvider(emailConfirmationTokenProviderKey, typeof(EmailConfirmationTokenProvider<ApplicationUser>));
            identityBuilder.AddTokenProvider(passwordResetTokenProviderKey, typeof(PasswordResetTokenProvider<ApplicationUser>));

            //Authentication: register all three schemes in a single chain.
            //JWT for human operators, "amx" HMAC for REST station traffic, "grpc-station" metadata for kiosk gRPC.
            services.AddAuthentication(
                             options =>
                             {
                                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                             }).
                     AddJwtBearer().
                     AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeySecretHandler>(
                             ApiKeySecretHandler.AuthenticationScheme,
                             _ => { }).
                     AddGrpcStationMetadata();

            services.AddScoped<IApiKeySecretValidator, ApiKeySecretValidator>();
            services.AddScoped<StationApiKeyService>();

            //Adds Authorization with user claim policy
            services.AddAuthorization();


            //Required by RateLimiter and APIKey Handler
            services.AddMemoryCache();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddInMemoryRateLimiting();

            // Health checks - reports liveness + DB connectivity at /health
            services.AddHealthChecks()
                    .AddDbContextCheck<PodDbContext>("postgres");

            // System Settings (currently not persistent)
            services.AddSingleton<SystemSettingsService>();

            // gRPC server (Grpc.AspNetCore - mapped via endpoints below)
            services.AddGrpc();
            services.AddSingleton<ShellHostServiceGrpc>();
            services.AddSingleton<ShellApplicationServiceGrpc>();
            services.AddSingleton<ConnectHostServiceGrpc>();
            services.AddSingleton(typeof(PublisherHub<>));
            services.AddSingleton(typeof(StationResponseHub));
            services.AddHostedService<GrpcHostedServer>();

            //Services
            services.AddHostedService<ConnectionHealthServiceHosted>();
            services.AddHostedService<SendEmailServiceHosted>();
            services.AddScoped<ShellService>();
            services.AddScoped<ConnectService>();
            services.AddScoped<ShellApplicationService>();
            services.AddScoped<ConnectionHealthService>();

            services.AddScoped<IUniqueAppFactory, UniqueAppFactory>();
            services.AddScoped<AdminService>();
            services.AddScoped<ServerManagerService>();
            services.AddScoped<AccountantService>();
            services.AddScoped<UserService>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<StationService>();
            services.AddScoped<CustomerSubscriptionService>();
            services.AddScoped<CustomerSupportService>();
            services.AddScoped<StationSupportService>();
            services.AddScoped<EMailService>();
            services.AddSingleton<EMailTemplateSenderFactory>();
            services.AddSingleton<IVariableParser, VariableParser>();

            //Lower Case Urls e.g. Controller Names
            services.AddRouting(options => options.LowercaseUrls = true);

            //Controllers + Razor Pages + Newtonsoft JSON
            services.AddControllers(c => c.Conventions.Add(new ApiExplorerGroupPerVersionConvention())).
                     AddNewtonsoftJson(options =>
                                       {
                                           options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                                           options.SerializerSettings.Formatting = Formatting.Indented;
                                           options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                                           options.SerializerSettings.Converters.Add(new StringEnumConverter());
                                       });
            services.AddRazorPages();


            services.AddSwaggerExamplesFromAssemblyOf<RequestLoginModelDtoExample>();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(
                    c =>
                    {
                        //Lowercase routes for swagger documentation
                        c.DocumentFilter<LowercaseDocumentFilter>();
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Leap Play", Version = "v1" });
                        c.SwaggerDoc("v1_internal", new OpenApiInfo { Title = "Leap Play - Internal", Version = "v1" });
                        c.ExampleFilters();

                        // OpenAPI 3 security definitions
                        c.AddSecurityDefinition(
                                JwtBearerDefaults.AuthenticationScheme,
                                new OpenApiSecurityScheme
                                {
                                        In = ParameterLocation.Header,
                                        Description = @"Please enter ""bearer "" + ""your access token""",
                                        Name = "Authorization",
                                        Type = SecuritySchemeType.ApiKey
                                });
                        c.AddSecurityDefinition(
                                ApiKeySecretHandler.AuthenticationScheme,
                                new OpenApiSecurityScheme
                                {
                                        In = ParameterLocation.Header,
                                        Description = @"Please enter ""amx "" + ""your signature""",
                                        Name = "Authorization",
                                        Type = SecuritySchemeType.ApiKey
                                });
                        //Adds a Security requirement only for the Methods decorated with an Authorize attribute
                        c.OperationFilter<AuthorizationOperationFilter>(JwtBearerDefaults.AuthenticationScheme);
                        c.OperationFilter<AuthorizationOperationFilter>(ApiKeySecretHandler.AuthenticationScheme);
                        //Modifies the names of the model e.g. to remove ViewModel or DTO suffix
                        c.CustomSchemaIds(SchemaIdStrategy.RemoveModelSufixStrategy);

                        //Applies sorting for Models
                        c.DocumentFilter<OnlyApiResponseAndRequestFilterOrdered>();

                        // Set the comments path for the Swagger JSON and UI.
                        // Requires that generate xml documentation is enabled in project
                        // and will be used to create the documentation for the routes
                        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        if(File.Exists(xmlPath))
                        {
                            c.IncludeXmlComments(xmlPath);
                        }

                        //Extension of Summary needs to be after Xml Comments
                        c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                        //Enables Annotations like [SwaggerTag]
                        c.EnableAnnotations();
                    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, LetsEncryptOptions letsEncrypt, ContextInitializer dbInitializer)
        {
            //As first use RateLimiter then other Middleware
            //otherwise RateLimiter could be not working for each route
            app.UseIpRateLimiting();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(
                        builder =>
                        {
                            builder.Run(
                                    async context =>
                                    {
                                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                        context.Response.Headers["Access-Control-Allow-Origin"] = "*";

                                        var error = context.Features.Get<IExceptionHandlerFeature>();
                                        if(error != null)
                                        {
                                            context.Response.Headers["Application-Error"] = Regex.Replace(
                                                            error.Error.Message,
                                                            @"\p{C}+",
                                                            string.Empty);
                                            // CORS
                                            context.Response.Headers["access-control-expose-headers"] = "Application-Error";
                                            await context.Response.WriteAsync(error.Error.Message).
                                                          ConfigureAwait(false);
                                        }
                                    });
                        });
                app.UseHsts();
            }

            //If Lets Encrypt is enabled we do need to accept Challenge Requests through Http,
            //and want to redirect everything else to Https
            if(letsEncrypt.IsEnabled)
            {
                WithLetsEncrypt(app);
            }
            else
            {
                AddDefault(app);
            }

            // DB migrations + seed (Users, ShellServer, Email templates) ran in Program.Main
            // BEFORE host.RunAsync() so they finish before the hosted services (notably
            // ConnectionHealthService) start querying the DB. Leaving the parameter on the
            // signature so Pod.Data.ContextInitializer stays resolvable here for tests that
            // want to re-run it.
            _ = dbInitializer;
        }

        private void WithLetsEncrypt(IApplicationBuilder app)
        {
            //Everything that is not an ACME Challenge
            app.MapWhen(
                    httpContext => !httpContext.Request.Path.StartsWithSegments(LetsEncryptConst.ChallengePath),
                    appBuilder =>
                    {
                        appBuilder.UseHttpsRedirection();
                        AddDefault(appBuilder);
                    }
            );
            //For ACME Challenges only
            app.MapWhen(
                    httpContext => httpContext.Request.Path.StartsWithSegments(LetsEncryptConst.ChallengePath),
                    appBuilder =>
                    {
                        appBuilder.UseLetsEncrypt();
                    }
            );
        }

        private void AddDefault(IApplicationBuilder app)
        {
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(
                    c =>
                    {
                        //Directly Main ;
                        c.RoutePrefix = String.Empty;
                        //Replaces the Index.Html with an custom one we can manipulate
                        c.IndexStream = () => GetType()
                                              .GetTypeInfo()
                                              .Assembly
                                              .GetManifestResourceStream("Pod.Web.Center.Swagger.index.html");
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Public API");
                        c.SwaggerEndpoint("/swagger/v1_internal/swagger.json", "V1 Internal API");
                        c.ConfigObject.ShowCommonExtensions = true;

                        //The Models and Routes are not expanded when page is loaded
                        c.DocExpansion(DocExpansion.None);
                        //Adds an custom css file allowing for further customization of the layout
                        c.InjectStylesheet("/swagger-ui/custom.css");
                    });
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger(c =>
                           {
                               c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                                                         {
                                                             swaggerDoc.Servers = new List<OpenApiServer>
                                                             {
                                                                 new OpenApiServer { Url = $"https://{httpReq.Host.Value}" }
                                                             };
                                                             swaggerDoc.Info.Description = "PlayOnDemand operator API. https://github.com/<org>/PlayOnDemand";
                                                         });
                           });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapRazorPages();
                endpoints.MapHealthChecks("/health");
                endpoints.MapGrpcService<ConnectHostServiceGrpc>();
                endpoints.MapGrpcService<ShellHostServiceGrpc>();
                endpoints.MapGrpcService<ShellApplicationServiceGrpc>();
            });
        }
    }

}
