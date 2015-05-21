// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using NHibernate.AspNet.Identity;
using NHibernate.Context;
using PIMS.Core.Models;
using PIMS.Core.Security;
using PIMS.Data.FakeRepositories;
using PIMS.Data.Repositories;
using PIMS.Infrastructure;
using PIMS.Web.Api.Controllers;
using StructureMap;
using NHibernate;
using Microsoft.AspNet.Identity;


namespace PIMS.Web.Api.DependencyResolution {

    public static class IoC
    {
        public static IContainer Initialize(string connString = "")
        {
            ObjectFactory.Initialize(x =>
                        {
                            x.Scan(scan =>
                                    {
                                        scan.TheCallingAssembly();
                                        scan.WithDefaultConventions();
                                        scan.LookForRegistries();
                                        scan.AssemblyContainingType<Asset>();                 // Core
                                        scan.AssemblyContainingType<Class1>();                // InfraStructure; (T to be replaced with real file)
                                        scan.AssemblyContainingType<AssetClassController>();  // Web.Api
                                    });

                            // x.For<IAssetRepository>().Use<AssetRepository>(); //.Ctor<string>("connectionString").EqualToAppSetting("Connection-String");
                            x.For<UserManager<ApplicationUser>>().Use<UserManager<ApplicationUser>>();
                            x.For<IUserStore<ApplicationUser>>().Use<UserStore<ApplicationUser>>();
                            x.For<IPimsIdentityService>().Use<PimsIdentityService>();
                            //x.For<IUserStore<ApplicationUser>>().Use<Core.Security.UserStore<ApplicationUser>>();
                            //x.For<IMembershipAdapter>().Use<MembershipAdapter>();
                            //x.For<IUserMapper>().Use<UserMapper>();

                            // ** -- Domain Constructor injections ---
                            /*  --------------- UNIT TESTING only, via NUnit/Fiddler ---------------------------- */
                            //x.For<IGenericRepository<Asset>>().Use<InMemoryAssetRepository>();
                            //x.For<IGenericRepository<Investor>>().Use<InMemoryInvestorRepository>();
                            //x.For<IGenericRepository<Position>>().Use<InMemoryPositionRepository>();
                            //x.For<IGenericRepository<Income>>().Use<InMemoryIncomeRepository>();
                            //x.For<IGenericRepository<AccountType>>().Use<InMemoryAccountTypeRepository>();
                            //x.For<IGenericRepository<AssetClass>>().Use<InMemoryAssetClassRepository>();
                            /*  --------------------------------------------------------------------------------- */

                            /*  --------------- INTEGRATION/PROD TESTING only, via Fiddler & Web Api client ----- */
                            x.For<IGenericRepository<AssetClass>>().Use<AssetClassRepository>();
                            x.For<IGenericRepository<AccountType>>().Use<AccountTypeRepository>();
                            x.For<IGenericRepository<Asset>>().Use<AssetRepository>();
                            x.For<IGenericRepository<Position>>().Use<PositionRepository>();
                            x.For<IGenericRepository<Income>>().Use<IncomeRepository>();
                            x.For<IGenericRepository<Investor>>().Use<InvestorRepository>();
                            x.For<IGenericRepository<Profile>>().Use<ProfileRepository>();
                            //x.For<IGenericRepository<Profile>>().Use<InMemoryProfileRepository>(); [10/24/14: not needed due to unit test configurations]
                            //x.For<IGenericRepository<Position>>().Use<PositionRepository>();
                            //x.For<IGenericRepository<Income>>().Use<IncomeRepository>();

                         

                            // ISessionFactory is expensive to initialize, so we'll create it as a singleton.
                            x.For<ISessionFactory>()
                                .Singleton()
                                .Use(() => NHibernateConfiguration.CreateSessionFactory(connString));

                            // Cache each ISession per web request. Will need to dispose this!
                            x.For<ISession>()
                                .HttpContextScoped()
                                .Use(context => context.GetInstance<ISessionFactory>().OpenSession());

                            // Ensure that for each web request, only one ISession object is used.
                            x.For<ISession>()
                                .HttpContextScoped()
                                .Use(ctx => NHibernateConfiguration.CheckSession(ctx.GetInstance<ISessionFactory>()));

                        });

            // Diagnostic tools:
            //var debugContainer = ObjectFactory.Container.WhatDoIHave();
            //  This will write out all dependencies that are registered with the container.
            //ObjectFactory.Container.AssertConfigurationIsValid();


            return ObjectFactory.Container;
        }

       


       


        public static void DisposeCurrentSession()
        {
            ISession currentSession =
                CurrentSessionContext.Unbind(ObjectFactory.Container.GetInstance<ISessionFactory>());

            currentSession.Close();
            currentSession.Dispose();
        }



    }
}