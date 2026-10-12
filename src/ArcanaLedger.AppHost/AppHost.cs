var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithInitFiles("../ArcanaLedger.Server/Database/schema.sql")
    .AddDatabase("ArcanaLedger");

var server = builder.AddProject<Projects.ArcanaLedger_Server>("server")
    .WithReference(database)
    .WaitFor(database)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("frontend", "../ArcanaLedger.Client")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
