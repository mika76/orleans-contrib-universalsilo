﻿namespace Template.WebApiDirectClient

open Microsoft.Extensions.Hosting
open Microsoft.OpenApi.Models
open System
open Orleans.Contrib.UniversalSilo.Configuration

[<AutoOpen>]
module OpenApiConfiguration =
    let private Version = "v1"                                                                    // revision this appropriately
    let private Title = "My Orleans API"                                                          // title this application
    let private Description = "An application with an Orleans backend and a WebAPI interface"     // describe this application
    let private TermsOfService = new Uri("http://127.0.0.1")                                      // replace with <Your TOS Uri>
    let private Contact_Name = "A Sagacious Developer"                                            // replace with <Your Name>
    let private Contact_Email = "<Your Email>"                                                    // replace with <Your Email>
    let private Contact_Url = new Uri("http://127.0.0.1")                                         // replace with <Your Uri>
    let private License_Name = "A generous license"                                               // replace with <Your License Name>
    let private License_Url = new Uri("http://127.0.01")                                          // replace with <Your License Uri>
    let private Contact = new OpenApiContact(Name = Contact_Name, Email = Contact_Email, Url = Contact_Url)
    let private License = new OpenApiLicense(Name = License_Name, Url = License_Url)

    let ApiInfo =
        OpenApiInfo(
            Version = Version,
            Title = Title,
            Description = Description,
            TermsOfService = TermsOfService,
            Contact = Contact,
            License = License)

/// <summary>
/// Override methods in this class to take over how the web-api host is configured
/// </summary>
type WebApiConfigurator() = class
    inherit Orleans.Contrib.UniversalSilo.WebApiConfigurator(OpenApiConfiguration.ApiInfo)
end

/// <summary>
/// Override methods in this class to take over how the silo is configured
/// </summary>
type SiloConfigurator () = class
    inherit Orleans.Contrib.UniversalSilo.SiloConfigurator(false)
end

module Program =
    let CreateHostBuilderString args =
        let webapiConfigurator : Func<IHostBuilder, IHostBuilder> =
            System.Func<_,_>(fun hb ->
                let wc = new WebApiConfigurator()
                wc.ConfigureWebApiHost hb)

        let siloConfigurator =
            new SiloConfigurator()
            |> (fun sc -> sc.ConfigureSiloHost)

        (Host.CreateDefaultBuilder args)
        |> ApplyAppConfiguration
        |> (fun hb -> ApplyConfiguration hb webapiConfigurator)
        |> (fun hb -> hb.UseOrleans siloConfigurator)


    /// <summary>
    ///
    /// This is the entry point to the silo.
    ///
    /// No changes should normally be needed here to start up a silo and a web-api front-end co-hosted in the same executable
    ///
    /// Provide the configuration of the silo to connect by any combination of
    ///    * Working with the default configuration
    ///    * Setting environment variables,
    ///    * Providing a `clustering.json` file to configure clustering options
    ///    * Providing a `persistence.json` file to configure storage provider options
    ///    * Overriding methods in the `WebApiConfigurator` class
    ///    * Overriding methods in the `SiloConfigurator` class
    ///
    /// </summary>
    [<EntryPoint>]
    let Main args =
       CreateHostBuilderString args
       |> (fun hb -> hb.Build())
       |> (fun hb -> hb.Run())
       0