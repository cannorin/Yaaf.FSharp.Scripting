// ----------------------------------------------------------------------------
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
// ----------------------------------------------------------------------------

(*
    This file handles the configuration of the Yaaf.AdvancedBuilding build script.

    The first step is handled in build.sh and build.cmd by restoring either paket dependencies or bootstrapping a NuGet.exe and 
    executing NuGet to resolve all build dependencies (dependencies required for the build to work, for example FAKE).

    The secound step is executing build.fsx which loads this file (for configuration), builds the solution and executes all unit tests.
*)

#if FAKE
#else
// Support when file is opened in Visual Studio
#load "packages/Yaaf.AdvancedBuilding/content/buildConfigDef.fsx"
#endif

open BuildConfigDef
open System.Collections.Generic
open System.IO

open Fake
open Fake.Git
open Fake.FSharpFormatting
open AssemblyInfoFile

if isMono then
    monoArguments <- "--runtime=v4.0 --debug"

let buildConfig =
 // Read release notes document
 let release = ReleaseNotesHelper.parseReleaseNotes (File.ReadLines "doc/ReleaseNotes.md")
 { BuildConfiguration.Defaults with
    ProjectName = "Yaaf.FSharp.Scripting"
    CopyrightNotice = "Yaaf.FSharp.Scripting Copyright © Matthias Dittrich 2015"
    ProjectSummary = "A helper library to easily add F# scripts to your application."
    ProjectDescription = "This library builds on top of the FSharp.Compiler.Service library and provides a nice API for F# script integration. It provides APIs to push values into and to get values from scripts. Additionally it adds some extension methods missing from the FSharp.Compiler.Service API."
    ProjectAuthors = ["Matthias Dittrich"]
    NugetTags =  "fsharp scripting compiler host"
    PageAuthor = "Matthias Dittrich"
    GithubUser = "matthid"
    Version = release.NugetVersion
    NugetPackages =
      [ "Yaaf.FSharp.Scripting.nuspec", (fun config p ->
          { p with
              Version = config.Version
              ReleaseNotes = toLines release.Notes
              Dependencies = 
                [ "FSharp.Compiler.Service"
                  "FSharp.Core" ]
                |> List.map (fun name -> name, (GetPackageVersion "packages" name)) }) ]
    UseNuget = false
    SetAssemblyFileVersions = (fun config ->
      let info =
        [ Attribute.Company config.ProjectName
          Attribute.Product config.ProjectName
          Attribute.Copyright config.CopyrightNotice
          Attribute.Version config.Version
          Attribute.FileVersion config.Version
          Attribute.InformationalVersion config.Version]
      CreateFSharpAssemblyInfo "./src/SharedAssemblyInfo.fs" info)
    EnableDebugSymbolConversion = false
    RestrictReleaseToWindows = false
    BuildTargets =
     [ { BuildParams.WithSolution with
          // The default build
          PlatformName = "Net40"
          SimpleBuildName = "net40" }
       // Test if it actually compiles without YAAF_FSHARP_SCRIPTING_PUBLIC defined.
       { BuildParams.WithSolution with
          // The generated templates
          PlatformName = "IncludeTest"
          SimpleBuildName = "include_test" }
       { BuildParams.WithSolution with
          // The generated templates
          PlatformName = "Net45"
          SimpleBuildName = "net45" } ]
  }