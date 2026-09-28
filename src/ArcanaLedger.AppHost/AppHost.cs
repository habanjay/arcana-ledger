var builder = DistributedApplication.CreateBuilder(args);

var server = builder.AddProject<Projects.ArcanaLedger_Server>("server")
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("frontend", "../ArcanaLedger.Client")
    .WithReference(server)
    .WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
