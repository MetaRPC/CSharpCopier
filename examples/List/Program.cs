using MetaRPC.Copier;
using var service = new CopierService("https://copy.mrpc.pro", Environment.GetEnvironmentVariable("MRPC_API_KEY") ?? "");
var result = await service.ListAsync();
if (!result.Ok) throw new Exception(result.Error);
foreach (var copier in result.Copiers) Console.WriteLine($"{copier.Id}: paused={copier.Paused}");
