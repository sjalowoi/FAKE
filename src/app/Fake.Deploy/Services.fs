﻿module Fake.Services

open Fake
open System
open System.Diagnostics
open System.ServiceProcess
open System.Configuration
open Fake.TraceListener

type FakeDeployService() as self =
    inherit ServiceBase()

    let console = new ConsoleTraceListener(false, colorMap) :> ITraceListener

    let logger (msg, eventLogEntry : EventLogEntryType) = 
        if Environment.UserInteractive
        then 
            match eventLogEntry with
            | EventLogEntryType.Error -> console.Write(ErrorMessage(msg))
            | EventLogEntryType.Information -> console.Write(TraceMessage(msg, true))
            | EventLogEntryType.Warning -> console.Write(ImportantMessage(msg))
            | _ -> console.Write(LogMessage(msg, true))
        else self.EventLog.WriteEntry(msg, eventLogEntry)

    do 
        self.AutoLog <- true
        self.ServiceName <- "Fake Deploy Agent"

    override x.OnStart(args) = 
        DeploymentAgent.start logger (ConfigurationManager.AppSettings.["Port"])

    override x.OnStop() = 
        DeploymentAgent.stop()

    member x.Start(args) =
        if Environment.UserInteractive then 
            x.OnStart(args)
        else 
            ServiceBase.Run(x)

    member x.Stop() = 
        x.OnStop()

let getFakeAgentService() =
    ServiceController.GetServices() 
    |> Array.find (fun (x : ServiceController) -> x.ServiceName = "Fake Deploy Agent")