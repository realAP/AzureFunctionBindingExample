using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

FunctionsApplication.CreateBuilder(args)
.ConfigureFunctionsWebApplication()
.Build().Run();
