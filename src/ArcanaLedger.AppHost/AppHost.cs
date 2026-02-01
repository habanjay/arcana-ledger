var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ArcanaPortfolio_API>("arcanaportfolio-api");

builder.Build().Run();
