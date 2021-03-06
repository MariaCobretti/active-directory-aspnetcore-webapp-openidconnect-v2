﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using TodoListService.AuthorizationPolicies;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using TodoListService.Infrastructure;

namespace TodoListService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddProtectedWebApi("AzureAdB2C", Configuration, options =>
                {
                    Configuration.Bind("AzureAdB2C", options);

                    options.TokenValidationParameters.NameClaimType = "name";
                    //options.TokenValidationParameters.RoleClaimType = "groups";
                    //options.TokenValidationParameters.RoleClaimType = "roles";
                });

            // This is required to be instantiated before the OpenIdConnectOptions starts getting configured.
            // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
            // 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles'
            // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
            //JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // Adding authorization policies that enforce authorization using Azure AD roles.
            services.AddAuthorization(options =>
            {
                // policy discussion
                // https://stackoverflow.com/questions/31464359/how-do-you-create-a-custom-authorizeattribute-in-asp-net-core/31465227
                options.AddPolicy("ReadScope", policy => policy.Requirements.Add(new ScopesRequirement("tasks.read")));
                options.AddPolicy("EditTaskPolicy", policy => policy.Requirements.Add(new MoveInDateRequirement()));
                options.AddPolicy("AdminOrDispatcher", policy => policy.RequireClaim("groups", "Admin", "Dispatcher"));
                //options.AddPolicy(Constants.AuthorizationPolicies.AssignmentToAdminGroupRequired, policy => policy.Requirements.Add(new GroupsRequirement(Constants.Groups.Admin)));



                //options.AddPolicy(Constants.AuthorizationPolicies.AssignmentToUserReaderRoleRequired, policy => policy.RequireRole(Constants.AppRole.UserReaders));
                //options.AddPolicy(Constants.AuthorizationPolicies.AssignmentToDirectoryViewerRoleRequired, policy => policy.RequireRole(Constants.AppRole.DirectoryViewers));
            });

            services.AddControllers();


            services.AddSingleton<IAuthorizationHandler, EditTaskAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
                // PII hiding in log files is enabled by default for GDPR concerns.
                // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
                // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}