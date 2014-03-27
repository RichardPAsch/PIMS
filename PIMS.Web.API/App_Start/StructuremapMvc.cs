// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructuremapMvc.cs" company="Web Advanced">
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

using System.Web.Http;
using System.Web.Mvc;
using StructureMap;
using PIMS.Web.Api.DependencyResolution;

[assembly: WebActivator.PreApplicationStartMethod(typeof(PIMS.Web.Api.App_Start.StructuremapMvc), "Start")]


namespace PIMS.Web.Api.App_Start {


	public static class StructuremapMvc {

		public static void Start()
		{
		    var connString = @"Data Source=RICHARD-VAIO\RICHARDDB;Initial Catalog='Lighthouse - PIMS - Test';Integrated Security=True"; // use web.config

			IContainer container = IoC.Initialize(connString); // Initialize the container, aka bootstrap
			DependencyResolver.SetResolver(new StructureMapDependencyResolver(container));
			GlobalConfiguration.Configuration.DependencyResolver = new StructureMapDependencyResolver(container);

			// Verify sm container content instances
			//var smContainer = container.WhatDoIHave();

		}


	}
}